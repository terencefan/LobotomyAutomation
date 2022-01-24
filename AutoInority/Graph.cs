using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace AutoInority
{
    internal class Graph
    {
        private static readonly int[][] _sefiraDistance = new int[][]
                {
            new int[]{ 0, 1, 1, 1, 2, 3, 3, 3, 4, 4}, // Control
            new int[]{ 1, 0, 1, 1, 2, 3, 3, 3, 4, 4}, // Information
            new int[]{ 1, 1, 0, 2, 2, 3, 3, 3, 4, 4}, // Training
            new int[]{ 1, 1, 2, 0, 2, 3, 3, 3, 4, 4}, // Safety
            new int[]{ 2, 1, 2, 2, 0, 1, 1, 1, 2, 2}, // Central 1
            new int[]{ 3, 2, 3, 3, 1, 0, 1, 1, 2, 2}, // Central 2
            new int[]{ 3, 2, 3, 3, 1, 1, 0, 2, 1, 3}, // Disciplinary
            new int[]{ 3, 2, 3, 3, 1, 1, 2, 0, 3, 1}, // Welfare
            new int[]{ 4, 3, 4, 4, 2, 2, 1, 3, 0, 4}, // Extraction
            new int[]{ 4, 3, 4, 4, 2, 2, 3, 1, 4, 0}, // Record
        };

        private static readonly Dictionary<string, int> _sid2id = new Dictionary<string, int>
        {
        };

        private static float[,] _distance = new float[,]
        {
        };

        public static int Distance(Sefira a, Sefira b)
        {
            return Distance(a.sefiraEnum, b.sefiraEnum);
        }

        public static int Distance(SefiraEnum a, SefiraEnum b)
        {
            return SefiraDistance((int)a, (int)b);
        }

        public static float Distance(AgentModel agent, CreatureModel creature)
        {
            var node1 = agent.GetCurrentNode() ?? agent.GetCurrentEdge().node1;
            var node2 = creature.GetWorkspaceNode();
            return NodeDistance(node1, node2);
        }

        public static void Initialize()
        {
            Assembly a = Assembly.GetExecutingAssembly();

            foreach (var name in a.GetManifestResourceNames())
            {
                Log.Info(name);
            }

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

            Log.Info("Graph module initialized.");
        }

        private static int GetNodeId(MapNode node)
        {
            var sid = node.GetId().Split('@')[0];
            return _sid2id[sid];
        }

        private static float NodeDistance(MapNode node1, MapNode node2)
        {
            if (node1 == null || node2 == null)
            {
                return float.MaxValue;
            }
            var id1 = GetNodeId(node1);
            var id2 = GetNodeId(node2);
            return _distance[id1, id2];
        }

        private static int SefiraDistance(int a, int b)
        {
            if (a > 9 || b > 9)
            {
                return 5;
            }
            return _sefiraDistance[a][b];
        }
    }
}