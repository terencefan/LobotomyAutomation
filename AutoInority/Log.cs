using System;
using System.IO;

using UnityEngine;

namespace AutoInority
{
    internal class Log
    {
        public static void Error(string message)
        {
            File.AppendAllText(Application.dataPath + "/BaseMods/Error.txt", message + "\n");
        }

        public static void Error(Exception e)
        {
            Error(e.Message);
            Error(e.StackTrace);
        }

        public static void Info(string message) => Info("info", message);

        public static void Info(string file, string message)
        {
            File.AppendAllText($"{Application.dataPath}/BaseMods/{file}.txt", message + "\n");
        }
    }
}