using System;
using System.IO;
using System.Configuration;

namespace Etrea3.Core
{
    public static class Logger
    {
        private static readonly object lockObject = new object();
        private static string basePath = AppDomain.CurrentDomain.BaseDirectory;
        private static string logPath = Path.Combine(basePath, "logs");
        private static bool logToFile = bool.Parse(ConfigurationManager.AppSettings["LogToFile"]);
        private static bool logToDatabase = bool.Parse(ConfigurationManager.AppSettings["LogToDatabase"]);

        public static void LogMessage(string message, LogLevel level)
        {
            try
            {
                string logEntry = $"{DateTime.UtcNow:G}: {message}";
                Console.WriteLine(logEntry);
                if (logToFile)
                {
                    lock (lockObject)
                    {
                        string logName = $"{DateTime.UtcNow:yyyy}-{DateTime.UtcNow:MM}-{DateTime.UtcNow:dd}-{level}.log";
                        if (!Directory.Exists(logPath))
                        {
                            Directory.CreateDirectory(logPath);
                        }
                        string logFile = Path.Combine(logPath, logName);
                        using (TextWriter tw = new StreamWriter(logFile, true))
                        {
                            tw.WriteLine(logEntry);
                            tw.Flush();
                            tw.Close();
                        }
                    }
                }
                if (logToDatabase)
                {
                    DatabaseManager.AddLogEntry(message, level);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Cannot write to {level} log: {ex.Message}");
            }
        }
    }
}
