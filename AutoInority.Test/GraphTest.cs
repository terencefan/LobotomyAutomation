using System.Reflection;

using Xunit;

namespace AutoInority.Test
{
    /// <summary>合成图钉住运行时激活子图语义：锁节点绕路/不可达、指纹翻位检测、
    /// 部门表激活感知（全锁部门整行 MaxValue 含对角）。节点/边经 GameBridge 跳构造器
    /// 分配 + 注字段（MapNode.id/_activate、MapEdge.node1/node2/cost），不走引擎。</summary>
    public class GraphTest
    {
        private static MapNode Node(string id, bool active)
        {
            var node = GameBridge.Bare<MapNode>();
            GameBridge.Set(node, "id", id);
            GameBridge.Set(node, "_activate", active);
            return node;
        }

        private static MapEdge Edge(MapNode a, MapNode b, float cost)
        {
            var edge = GameBridge.Bare<MapEdge>();
            GameBridge.Set(edge, "node1", a);
            GameBridge.Set(edge, "node2", b);
            GameBridge.Set(edge, "cost", cost);
            return edge;
        }

        [Fact]
        public void LockedNodeForcesDetour()
        {
            // malkuth-netzach-hod 链 + malkuth-hod 直连：中点锁定走直连绕路，重开恢复
            var a = Node("sefira-malkuth-1", true);
            var b = Node("sefira-netzach-1", true);
            var c = Node("sefira-hod-1", true);
            var nodes = new[] { a, b, c };
            var edges = new[] { Edge(a, b, 10f), Edge(b, c, 10f), Edge(a, c, 25f) };

            Graph.Rebuild(nodes, edges);
            Assert.Equal(20f, Graph.Distance(SefiraEnum.MALKUT, SefiraEnum.HOD));
            Assert.Equal(10f, Graph.Distance(SefiraEnum.MALKUT, SefiraEnum.NETZACH));

            GameBridge.Set(b, "_activate", false);
            Graph.Rebuild(nodes, edges);
            Assert.Equal(25f, Graph.Distance(SefiraEnum.MALKUT, SefiraEnum.HOD));
            Assert.Equal(float.MaxValue, Graph.Distance(SefiraEnum.MALKUT, SefiraEnum.NETZACH));

            GameBridge.Set(b, "_activate", true);
            Graph.Rebuild(nodes, edges);
            Assert.Equal(20f, Graph.Distance(SefiraEnum.MALKUT, SefiraEnum.HOD));
        }

        [Fact]
        public void DisconnectedComponentsYieldMaxValue()
        {
            var a = Node("sefira-malkuth-1", true);
            var b = Node("sefira-binah-1", true);
            Graph.Rebuild(new[] { a, b }, new MapEdge[0]);

            Assert.Equal(float.MaxValue, Graph.Distance(SefiraEnum.MALKUT, SefiraEnum.BINAH));
            Assert.Equal(float.MaxValue, Graph.Distance(SefiraEnum.BINAH, SefiraEnum.MALKUT));
            Assert.Equal(0f, Graph.Distance(SefiraEnum.MALKUT, SefiraEnum.MALKUT));
            Assert.Equal(0f, Graph.Distance(SefiraEnum.BINAH, SefiraEnum.BINAH));
        }

        [Fact]
        public void FingerprintDetectsActivationFlip()
        {
            var nodes = new[] { Node("sefira-malkuth-1", true), Node("sefira-hod-1", true) };
            var fingerprint = Graph.Fingerprint(nodes);

            // 翻激活位必变；翻回必复原
            GameBridge.Set(nodes[1], "_activate", false);
            Assert.NotEqual(fingerprint, Graph.Fingerprint(nodes));
            GameBridge.Set(nodes[1], "_activate", true);
            Assert.Equal(fingerprint, Graph.Fingerprint(nodes));

            // LoadMap 重建：同内容新数组（引用全变）指纹必变——日切换自愈的依据
            var rebuilt = new[] { Node("sefira-malkuth-1", true), Node("sefira-hod-1", true) };
            Assert.NotEqual(fingerprint, Graph.Fingerprint(rebuilt));
        }

        [Fact]
        public void SefiraTableActivationAware()
        {
            // hod 组有节点但全锁：整行 MaxValue（含对角），不能被当作零距离目标
            var a = Node("sefira-malkuth-1", true);
            var h = Node("sefira-hod-1", false);
            Graph.Rebuild(new[] { a, h }, new[] { Edge(a, h, 5f) });

            Assert.Equal(float.MaxValue, Graph.Distance(SefiraEnum.MALKUT, SefiraEnum.HOD));
            Assert.Equal(float.MaxValue, Graph.Distance(SefiraEnum.HOD, SefiraEnum.HOD));
            Assert.Equal(0f, Graph.Distance(SefiraEnum.MALKUT, SefiraEnum.MALKUT));
        }

        [Fact]
        public void ActivatedDomainSymmetricZeroDiagonal()
        {
            var a = Node("sefira-malkuth-1", true);
            var b = Node("sefira-tessod-1", true);
            var c = Node("sefira-hod-1", true);
            Graph.Rebuild(new[] { a, b, c }, new[] { Edge(a, b, 10f), Edge(b, c, 10f) });

            var departments = new[]
            {
                SefiraEnum.MALKUT, SefiraEnum.YESOD, SefiraEnum.HOD, SefiraEnum.NETZACH,
                SefiraEnum.TIPERERTH1, SefiraEnum.TIPERERTH2, SefiraEnum.GEBURAH, SefiraEnum.CHESED,
                SefiraEnum.BINAH, SefiraEnum.CHOKHMAH, SefiraEnum.KETHER, SefiraEnum.DAAT, SefiraEnum.DUMMY,
            };
            foreach (var x in departments)
            {
                foreach (var y in departments)
                {
                    Assert.Equal(Graph.Distance(x, y), Graph.Distance(y, x));
                }
            }
            Assert.Equal(0f, Graph.Distance(SefiraEnum.MALKUT, SefiraEnum.MALKUT));
            Assert.Equal(0f, Graph.Distance(SefiraEnum.YESOD, SefiraEnum.YESOD));
            // 无节点/未激活部门对角保持 MaxValue
            Assert.Equal(float.MaxValue, Graph.Distance(SefiraEnum.NETZACH, SefiraEnum.NETZACH));
            Assert.Equal(float.MaxValue, Graph.Distance(SefiraEnum.DUMMY, SefiraEnum.DUMMY));
        }

        [Fact]
        public void EmptyGraphYieldsMaxValue()
        {
            // bootstrap 语义：图未就绪（场景未载）时一切查询安全降级为不可达
            Graph.Rebuild(new MapNode[0], new MapEdge[0]);
            Assert.Equal(float.MaxValue, Graph.Distance(SefiraEnum.MALKUT, SefiraEnum.MALKUT));
        }

        [Fact]
        public void HeapGrowthBeyondCapacityStaysCorrect()
        {
            // 懒删除重复入堆必超初始堆容量 n+1：6 条递减平行边 = 6 次 push（容量 3），
            // 触发堆翻倍扩容，最短边须胜出
            var a = Node("sefira-malkuth-1", true);
            var b = Node("sefira-hod-1", true);
            var edges = new[] { Edge(a, b, 6f), Edge(a, b, 5f), Edge(a, b, 4f), Edge(a, b, 3f), Edge(a, b, 2f), Edge(a, b, 1f) };
            Graph.Rebuild(new[] { a, b }, edges);

            Assert.Equal(1f, Graph.Distance(SefiraEnum.MALKUT, SefiraEnum.HOD));
        }

        [Fact]
        public void InactiveSourceRowYieldsMaxValue()
        {
            // 未激活源行必须整行 MaxValue（数组零初始化陷阱：漏填则表现为处处可达 0）
            var locked = Node("sefira-hod-1", false);
            var open = Node("sefira-malkuth-1", true);
            Graph.Rebuild(new[] { locked, open }, new[] { Edge(locked, open, 5f) });

            var method = typeof(Graph).GetMethod("NodeDistance", BindingFlags.NonPublic | BindingFlags.Static);
            var distance = (float)method.Invoke(null, new object[] { locked, open });
            Assert.Equal(float.MaxValue, distance);
        }

        [Fact]
        public void UnknownNodeFallsBackToMaxValue()
        {
            var known = Node("sefira-malkuth-1", true);
            Graph.Rebuild(new[] { known }, new MapEdge[0]);
            var unknown = Node("definitely-not-a-node", true);

            var method = typeof(Graph).GetMethod("NodeDistance", BindingFlags.NonPublic | BindingFlags.Static);
            var distance = (float)method.Invoke(null, new object[] { known, unknown });
            Assert.Equal(float.MaxValue, distance);
        }

        [Fact]
        public void DummyEnumYieldsMaxValue()
        {
            Graph.Rebuild(new[] { Node("sefira-malkuth-1", true) }, new MapEdge[0]);
            Assert.Equal(float.MaxValue, Graph.Distance(SefiraEnum.DUMMY, SefiraEnum.MALKUT));
            Assert.Equal(float.MaxValue, Graph.Distance(SefiraEnum.MALKUT, SefiraEnum.DUMMY));
        }
    }
}
