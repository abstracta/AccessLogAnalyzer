using System.Collections.Generic;
using System.IO;

namespace Abstracta.AccessLogAnalyzer
{
    public class Logger
    {
        private static volatile Logger _instance;

        private static readonly object Lock = new object();

        private readonly List<string> _logs = new List<string>();

        internal bool Verbose { get; set; }

        internal static Logger GetInstance()
        {
            if (_instance == null)
            {
                lock (Lock)
                {
                    if (_instance == null)
                    {
                        _instance = new Logger();
                    }
                }
            }

            return _instance;
        }

        internal void AddLog(string log)
        {
            if (Verbose)
            {
                _logs.Add(log);
            }
        }

        internal void SaveLogsToFile()
        {
            SaveLogsToFile(System.DateTime.Now.ToString("yyyy.MM.dd-hh:mm:ss") + "-DefaultLogsFileName.log");
        }

        internal void SaveLogsToFile(string fileName)
        {
            if (!Verbose)
            {
                return;
            }

            using (var writer = new StreamWriter(fileName))
            {
                foreach (var log in _logs)
                {
                    writer.WriteLine(log);
                }
            }
        }
    }
}