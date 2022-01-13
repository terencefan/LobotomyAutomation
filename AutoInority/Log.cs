using System.IO;

using UnityEngine;

namespace AutoInority
{
    class Log
    {
        public static void Warning(string message)
        {
            File.AppendAllText(Application.dataPath + "/BaseMods/info.txt", message + "\n");
        }

        public static void Info(string message)
        {
            File.AppendAllText(Application.dataPath + "/BaseMods/info.txt", message + "\n");
        }
    }
}
