using System.IO;

using UnityEngine;

namespace AutoInority
{
    internal class Log
    {
        internal static bool DebugEnabled { get; set; } = false;

        public static void Debug(string message)
        {
            if (DebugEnabled)
            {
                Info(message);
            }
        }

        public static void Error(System.Exception e)
        {
            Error($"{e.GetType().Name}: {e.Message}");
            Error(e.StackTrace);
        }

        public static void Info(string message) => Info("info", message);

        public static void Info(string file, string message)
        {
            File.AppendAllText($"{Application.dataPath}/BaseMods/{file}.txt", message + "\n");
        }

        private static void Error(string message)
        {
            File.AppendAllText(Application.dataPath + "/BaseMods/Error.txt", message + "\n");
        }
    }
}