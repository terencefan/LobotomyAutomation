using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AutoInority
{
    /// <summary>运行时激活子图全对距离（取代离线 Graph.xml 快照）。数据源 MapGraph.instance：
    /// 节点激活位是唯一通行性状态（LoadMap 全置 false，日切换/开部门时恢复；隔离不改激活位），
    /// Poll 每 tick 比对激活位指纹，变化才在手写堆 Dijkstra 上重算激活子图全对矩阵，
    /// 部门级 13×13 表随同一次重算派生——未开部门整行 MaxValue，不再被全图快照低估。</summary>
    internal class Graph
    {
        /// <summary>节点名前缀 → SefiraEnum 数值。两个地图拼写坑：Yesod 拼作 tessod；
        /// keter 节点无连字符（sefira-keter1..10）；sefira-extra-* 是 DAAT（第 11 质点）。</summary>
        private static readonly Dictionary<string, int> _prefix2sefira = new Dictionary<string, int>
        {
            { "sefira-malkuth", 0 },     // MALKUT
            { "sefira-tessod", 1 },      // YESOD
            { "sefira-hod", 2 },         // HOD
            { "sefira-netzach", 3 },     // NETZACH
            { "sefira-tiphereth1", 4 },  // TIPERERTH1
            { "sefira-tiphereth2", 5 },  // TIPERERTH2
            { "sefira-geburah", 6 },     // GEBURAH
            { "sefira-chesed", 7 },      // CHESED
            { "sefira-binah", 8 },       // BINAH
            { "sefira-chokhmah", 9 },    // CHOKHMAH
            { "sefira-keter", 10 },      // KETHER
            { "sefira-extra", 11 },      // DAAT
        };

        private static Dictionary<string, int> _sid2id = new Dictionary<string, int>();

        private static float[,] _distance = new float[0, 0];

        private static readonly float[,] _sefiraDistance = new float[13, 13];

        private static long _fingerprint;

        // Dijkstra 复用草稿区（游戏主循环单线程，按节点数随重建重新分配）
        private static float[] _dist = new float[0];
        private static bool[] _settled = new bool[0];
        private static int[] _heapNode = new int[0];
        private static float[] _heapDist = new float[0];
        private static int _heapSize;

        public static float Distance(SefiraEnum a, SefiraEnum b)
        {
            return _sefiraDistance[(int)a, (int)b];
        }

        public static float Distance(AgentModel agent, CreatureModel creature)
        {
            var node1 = agent.GetCurrentNode() ?? agent.GetCurrentEdge().node1;
            var node2 = creature.state == CreatureState.ESCAPE ? (creature.GetCurrentNode() ?? creature.GetCurrentEdge().node1) : creature.GetWorkspaceNode();
            return NodeDistance(node1, node2);
        }

        public static void Initialize()
        {
            Poll();
            Log.Info("Graph module initialized.");
        }

        /// <summary>主循环每 tick 轮询：激活位指纹不变零成本返回，变化才全量重算
        /// （量级 = 日切换/开部门各一次）。instance 惰性创建永不为 null，就绪判 loaded
        /// （LoadMap 末尾置 true，场景卸载置 false）；未就绪保持现表不动。</summary>
        public static void Poll()
        {
            var graph = MapGraph.instance;
            if (graph == null || !graph.loaded)
            {
                return;
            }
            var nodes = graph.GetGraphNodes();
            var fingerprint = Fingerprint(nodes);
            if (fingerprint == _fingerprint)
            {
                return;
            }
            Rebuild(nodes, graph.GetGraphEdges());
            _fingerprint = fingerprint; // 重算成功才落账：中途异常下一 tick 重试
        }

        /// <summary>指纹 = 节点数 + 逐节点对象引用与激活位。LoadMap 重建字典时 MapNode 全新
        /// （引用即变），部门开合只翻激活位——两条变化路径都收敛到本指纹。</summary>
        internal static long Fingerprint(MapNode[] nodes)
        {
            unchecked
            {
                var hash = 14695981039346656037UL;
                hash = (hash ^ (ulong)nodes.Length) * 1099511628211UL;
                foreach (var node in nodes)
                {
                    hash = (hash ^ (ulong)RuntimeHelpers.GetHashCode(node)) * 1099511628211UL;
                    hash = (hash ^ (node.activate ? 3UL : 7UL)) * 1099511628211UL;
                }
                return (long)hash;
            }
        }

        /// <summary>产品虚缝（测试直调，绕开 MapGraph 引擎耦合）：从节点/边数组重建激活子图
        /// 全对矩阵与部门表。节点按 GetId() 归一化索引，边双向展开，端点未知的边跳过。</summary>
        internal static void Rebuild(MapNode[] nodes, MapEdge[] edges)
        {
            var n = nodes.Length;
            _sid2id = new Dictionary<string, int>(n);
            var active = new bool[n];
            var groups = new List<int>[_prefix2sefira.Count];
            for (var i = 0; i < groups.Length; i++)
            {
                groups[i] = new List<int>();
            }
            for (var i = 0; i < n; i++)
            {
                var raw = nodes[i].GetId();
                if (raw == null)
                {
                    continue;
                }
                var name = raw.Split('@')[0];
                active[i] = nodes[i].activate;
                // 重名归一化取首个（真实地图唯一；兜底行为确定即可）
                if (!_sid2id.ContainsKey(name))
                {
                    _sid2id[name] = i;
                }
                var sefira = MatchSefira(name);
                if (sefira >= 0)
                {
                    groups[sefira].Add(i);
                }
            }

            var adjacency = new List<int>[n];
            var adjacencyCost = new List<float>[n];
            for (var i = 0; i < n; i++)
            {
                adjacency[i] = new List<int>();
                adjacencyCost[i] = new List<float>();
            }
            foreach (var edge in edges)
            {
                if (edge == null || edge.node1 == null || edge.node2 == null || edge.cost < 0f)
                {
                    continue;
                }
                var u = 0;
                var v = 0;
                if (!TryGetNodeId(edge.node1, out u) || !TryGetNodeId(edge.node2, out v) || u == v)
                {
                    continue;
                }
                adjacency[u].Add(v);
                adjacencyCost[u].Add(edge.cost);
                adjacency[v].Add(u);
                adjacencyCost[v].Add(edge.cost);
            }

            _distance = new float[n, n];
            for (var i = 0; i < n; i++)
            {
                for (var j = 0; j < n; j++)
                {
                    _distance[i, j] = float.MaxValue; // new 数组零初始化，不填满则未激活源行默认 0（=处处可达）
                }
            }
            if (n > 0)
            {
                EnsureScratch(n);
                for (var s = 0; s < n; s++)
                {
                    if (!active[s])
                    {
                        continue; // 未激活源：整行保持 MaxValue（不可作为出发点）
                    }
                    Dijkstra(s, n, active, adjacency, adjacencyCost);
                    for (var j = 0; j < n; j++)
                    {
                        _distance[s, j] = _dist[j];
                    }
                }
            }
            DeriveSefiraDistance(groups, active);
        }

        /// <summary>部门间距离 = 两组 sefira 节点在激活子图矩阵上的最小路径代价。
        /// 部门无激活节点时整行保持 MaxValue（含对角）——未开部门不可达亦不可作为目标。</summary>
        private static void DeriveSefiraDistance(List<int>[] groups, bool[] active)
        {
            for (var i = 0; i < 13; i++)
            {
                for (var j = 0; j < 13; j++)
                {
                    _sefiraDistance[i, j] = float.MaxValue;
                }
            }
            for (var a = 0; a < groups.Length; a++)
            {
                var anyActive = false;
                foreach (var idx in groups[a])
                {
                    if (active[idx])
                    {
                        anyActive = true;
                        break;
                    }
                }
                if (!anyActive)
                {
                    continue;
                }
                _sefiraDistance[a, a] = 0f;
                for (var b = a + 1; b < groups.Length; b++)
                {
                    var min = MinPairDistance(groups[a], groups[b]);
                    _sefiraDistance[a, b] = min;
                    _sefiraDistance[b, a] = min;
                }
            }
        }

        private static int MatchSefira(string name)
        {
            foreach (var pair in _prefix2sefira)
            {
                if (name.StartsWith(pair.Key))
                {
                    return pair.Value;
                }
            }
            return -1;
        }

        private static float MinPairDistance(List<int> xs, List<int> ys)
        {
            var min = float.MaxValue;
            foreach (var x in xs)
            {
                foreach (var y in ys)
                {
                    if (_distance[x, y] < min)
                    {
                        min = _distance[x, y];
                    }
                }
            }
            return min;
        }

        private static bool TryGetNodeId(MapNode node, out int id)
        {
            var raw = node.GetId();
            if (raw == null)
            {
                id = 0;
                return false;
            }
            return _sid2id.TryGetValue(raw.Split('@')[0], out id);
        }

        private static float NodeDistance(MapNode node1, MapNode node2)
        {
            if (node1 == null || node2 == null)
            {
                return float.MaxValue;
            }
            var id1 = 0;
            var id2 = 0;
            if (!TryGetNodeId(node1, out id1) || !TryGetNodeId(node2, out id2))
            {
                return float.MaxValue;
            }
            return _distance[id1, id2];
        }

        private static void EnsureScratch(int n)
        {
            if (_dist.Length != n)
            {
                _dist = new float[n];
                _settled = new bool[n];
                _heapNode = new int[n + 1];
                _heapDist = new float[n + 1];
            }
        }

        /// <summary>二叉堆 Dijkstra（懒删除：重复入堆，弹出遇已定着点跳过），只走激活节点。</summary>
        private static void Dijkstra(int source, int n, bool[] active, List<int>[] adjacency, List<float>[] adjacencyCost)
        {
            for (var i = 0; i < n; i++)
            {
                _dist[i] = float.MaxValue;
                _settled[i] = false;
            }
            _heapSize = 0;
            _dist[source] = 0f;
            HeapPush(source, 0f);
            while (_heapSize > 0)
            {
                var u = HeapPop();
                if (_settled[u])
                {
                    continue;
                }
                _settled[u] = true;
                var neighbors = adjacency[u];
                var costs = adjacencyCost[u];
                for (var k = 0; k < neighbors.Count; k++)
                {
                    var v = neighbors[k];
                    if (!active[v] || _settled[v])
                    {
                        continue;
                    }
                    var candidate = _dist[u] + costs[k];
                    if (candidate < _dist[v])
                    {
                        _dist[v] = candidate;
                        HeapPush(v, candidate);
                    }
                }
            }
        }

        private static void HeapPush(int node, float dist)
        {
            if (_heapSize == _heapNode.Length)
            {
                // 最坏情形松弛次数超预留（懒删除重复入堆），就地翻倍
                var nodes = new int[_heapSize * 2 + 1];
                var dists = new float[_heapSize * 2 + 1];
                _heapNode.CopyTo(nodes, 0);
                _heapDist.CopyTo(dists, 0);
                _heapNode = nodes;
                _heapDist = dists;
            }
            var i = _heapSize++;
            _heapNode[i] = node;
            _heapDist[i] = dist;
            while (i > 0)
            {
                var parent = (i - 1) >> 1;
                if (_heapDist[parent] <= _heapDist[i])
                {
                    break;
                }
                Swap(i, parent);
                i = parent;
            }
        }

        private static int HeapPop()
        {
            var top = _heapNode[0];
            _heapSize--;
            if (_heapSize > 0)
            {
                _heapNode[0] = _heapNode[_heapSize];
                _heapDist[0] = _heapDist[_heapSize];
                var i = 0;
                while (true)
                {
                    var left = i * 2 + 1;
                    if (left >= _heapSize)
                    {
                        break;
                    }
                    var child = left;
                    var right = left + 1;
                    if (right < _heapSize && _heapDist[right] < _heapDist[left])
                    {
                        child = right;
                    }
                    if (_heapDist[i] <= _heapDist[child])
                    {
                        break;
                    }
                    Swap(i, child);
                    i = child;
                }
            }
            return top;
        }

        private static void Swap(int a, int b)
        {
            var n = _heapNode[a];
            _heapNode[a] = _heapNode[b];
            _heapNode[b] = n;
            var d = _heapDist[a];
            _heapDist[a] = _heapDist[b];
            _heapDist[b] = d;
        }
    }
}
