using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace AutoInority
{
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

        private static readonly Dictionary<string, int> _sid2id = new Dictionary<string, int>
        {
        };

        private static float[,] _distance = new float[,]
        {
        };

        private static readonly float[,] _sefiraDistance = new float[13, 13];

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
            LoadGraph();
            Log.Info("Graph module initialized.");
        }

        /// <summary>加载与派生不含日志播报——Log.Info 链上 Application.dataPath 是引擎 icall，
        /// 离线测试宿主不可 JIT（产品虚缝：测试直调本方法）。</summary>
        internal static void LoadGraph()
        {
            Assembly a = Assembly.GetExecutingAssembly();

            Stream s = a.GetManifestResourceStream("AutoInority.Graph.xml");

            XmlDocument doc = new XmlDocument();
            XmlTextReader reader = new XmlTextReader(s);
            doc.Load(reader);

            foreach (XmlNode node in doc.SelectNodes("/root/nodes/node"))
            {
                var name = node.Attributes["name"]?.InnerText;
                var id = int.Parse(node.Attributes["id"]?.InnerText);
                _sid2id[name] = id;
            }

            _distance = new float[_sid2id.Count, _sid2id.Count];
            foreach (XmlNode node in doc.SelectNodes("/root/items/item"))
            {
                var from = int.Parse(node.Attributes["from"]?.InnerText);
                var to = int.Parse(node.Attributes["to"]?.InnerText);
                var distance = float.Parse(node.Attributes["distance"]?.InnerText);
                _distance[from, to] = distance;
            }

            DeriveSefiraDistance();
        }

        /// <summary>部门间距离 = 两组 sefira 节点在节点距离矩阵上的最小路径代价，
        /// 取代与真实拓扑弱相关的旧硬编码 hop 矩阵。无节点的组合（DUMMY 等）保持 MaxValue。</summary>
        private static void DeriveSefiraDistance()
        {
            for (var i = 0; i < 13; i++)
            {
                for (var j = 0; j < 13; j++)
                {
                    _sefiraDistance[i, j] = float.MaxValue;
                }
            }
            var groups = new List<int>[_prefix2sefira.Count];
            for (var i = 0; i < groups.Length; i++)
            {
                groups[i] = new List<int>();
            }
            foreach (var pair in _sid2id)
            {
                var sefira = MatchSefira(pair.Key);
                if (sefira >= 0)
                {
                    groups[sefira].Add(pair.Value);
                }
            }
            for (var a = 0; a < groups.Length; a++)
            {
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
            var sid = node.GetId().Split('@')[0];
            return _sid2id.TryGetValue(sid, out id);
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
    }
}
