using System.Reflection;

using Xunit;

namespace AutoInority.Test
{
    public class GraphTest
    {
        static GraphTest()
        {
            Graph.LoadGraph();
        }

        private static readonly SefiraEnum[] Departments = new SefiraEnum[]
        {
            SefiraEnum.MALKUT, SefiraEnum.YESOD, SefiraEnum.HOD, SefiraEnum.NETZACH,
            SefiraEnum.TIPERERTH1, SefiraEnum.TIPERERTH2, SefiraEnum.GEBURAH, SefiraEnum.CHESED,
            SefiraEnum.BINAH, SefiraEnum.CHOKHMAH, SefiraEnum.KETHER, SefiraEnum.DAAT,
        };

        [Fact]
        public void DerivedSefiraTableMatchesKnownValues()
        {
            // 钉值来自 Graph.xml 节点集最小路径代价（同一数据的独立核算）
            Assert.InRange(Graph.Distance(SefiraEnum.TIPERERTH2, SefiraEnum.GEBURAH), 29.3f, 30.3f);
            Assert.InRange(Graph.Distance(SefiraEnum.MALKUT, SefiraEnum.HOD), 34.6f, 35.6f);
            Assert.InRange(Graph.Distance(SefiraEnum.CHESED, SefiraEnum.CHOKHMAH), 36.3f, 37.3f);
            Assert.InRange(Graph.Distance(SefiraEnum.BINAH, SefiraEnum.CHOKHMAH), 125.4f, 126.4f);
        }

        [Fact]
        public void DerivedTableIsSymmetricWithZeroDiagonal()
        {
            foreach (var a in Departments)
            {
                Assert.Equal(0f, Graph.Distance(a, a));
                foreach (var b in Departments)
                {
                    Assert.Equal(Graph.Distance(a, b), Graph.Distance(b, a));
                }
            }
        }

        [Fact]
        public void BottomDepartmentsFormCycle()
        {
            // 旧硬编码矩阵错把 malkuth↔yesod 记为相邻（hop=1）；真实拓扑是环
            // malkuth-hod-yesod-netzach，对角相邻更近。此断言钉住该历史错误。
            Assert.True(Graph.Distance(SefiraEnum.MALKUT, SefiraEnum.YESOD) > Graph.Distance(SefiraEnum.MALKUT, SefiraEnum.HOD));
        }

        [Fact]
        public void DummyEnumYieldsMaxValue()
        {
            Assert.Equal(float.MaxValue, Graph.Distance(SefiraEnum.DUMMY, SefiraEnum.MALKUT));
            Assert.Equal(float.MaxValue, Graph.Distance(SefiraEnum.MALKUT, SefiraEnum.DUMMY));
        }

        [Fact]
        public void UnknownNodeFallsBackToMaxValue()
        {
            var known = GameBridge.Bare<MapNode>();
            GameBridge.Set(known, "id", "sefira-malkuth-1");
            var unknown = GameBridge.Bare<MapNode>();
            GameBridge.Set(unknown, "id", "definitely-not-a-node");

            var method = typeof(Graph).GetMethod("NodeDistance", BindingFlags.NonPublic | BindingFlags.Static);
            var distance = (float)method.Invoke(null, new object[] { known, unknown });

            Assert.Equal(float.MaxValue, distance);
        }
    }
}
