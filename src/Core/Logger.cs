using System;
using System.IO;

namespace Etrea3.Core
{
    public static class Logger
    {
        private static readonly object lockObject = new object();

        public static void LogMessage(string message, LogLevel level, bool writeToScreen)
        {
            try
            {
                lock (lockObject)
                {
                    string logName = $"{DateTime.UtcNow:yyyy}-{DateTime.UtcNow:MM}-{DateTime.UtcNow:dd}-{level}.log";
                    string basePath = AppDomain.CurrentDomain.BaseDirectory;
                    string logPath = Path.Combine(basePath, "logs");
                    if (!Directory.Exists(logPath))
                    {
                        Directory.CreateDirectory(logPath);
                    }
                    string logFile = Path.Combine(logPath, logName);
                    string logEntry = $"{DateTime.UtcNow:G}: {message}";
                    using (TextWriter tw = new StreamWriter(logFile, true))
                    {
                        tw.WriteLine(logEntry);
                        tw.Flush();
                        tw.Close();
                    }
                    if (writeToScreen)
                    {
                        Console.WriteLine(logEntry);
                    }
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
