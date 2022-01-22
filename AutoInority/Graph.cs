using System;
using System.Collections.Generic;

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

        private static float[,] _distance;

        private static Dictionary<string, int> _sid2id = new Dictionary<string, int>();

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
            var nodes = MapGraph.instance.GetGraphNodes();
            var edges = MapGraph.instance.GetGraphEdges();

            var count = nodes.Length;

            var f = new float[count, count];
            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < count; j++)
                {
                    f[i, j] = float.MaxValue / 10;
                }
            }
            var id = 0;
            foreach (var node in nodes)
            {
                _sid2id[node.GetId()] = id;
                id++;
            }

            foreach (var edge in edges)
            {
                var id1 = GetNodeId(edge.node1);
                var id2 = GetNodeId(edge.node2);
                var length = Math.Abs(edge.node1.GetPosition().x - edge.node2.GetPosition().x);
                f[id1, id2] = length;
                f[id2, id1] = length;
            }

            for (int k = 0; k < count; k++)
            {
                for (int i = 0; i < count; i++)
                {
                    for (int j = 0; j < count; j++)
                    {
                        if (f[i, j] > f[i, k] + f[j, k])
                        {
                            f[i, j] = f[i, k] + f[j, k];
                        }
                    }
                }
            }
            _distance = f;
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