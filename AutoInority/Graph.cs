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

        public static int Distance(Sefira a, Sefira b)
        {
            return Distance(a.sefiraEnum, b.sefiraEnum);
        }

        public static int Distance(SefiraEnum a, SefiraEnum b)
        {
            return SefiraDistance((int)a, (int)b);
        }

        public static int SefiraDistance(int a, int b)
        {
            if (a > 9 || b > 9)
            {
                return 5;
            }
            return _sefiraDistance[a][b];
        }
    }
}