﻿using System;
using System.IO;

namespace Etrea2.Interfaces
{
    internal class LoggingProvider : ILoggingProvider
    {
        private readonly object _lock = new object();

        public void LogMessage(string msg, LogLevel logLevel, bool writeToScreen)
        {
            try
            {
                lock (_lock)
                {
                    string logName = $"{DateTime.UtcNow:yyyy}-{DateTime.UtcNow:MM}-{DateTime.UtcNow:dd}-{logLevel}.log";
                    string basePath = AppDomain.CurrentDomain.BaseDirectory;
                    string logPath = Path.Combine(basePath, "logs");
                    if (!Directory.Exists(logPath))
                    {
                        Directory.CreateDirectory(logPath);
                    }
                    string logFile = Path.Combine(logPath, logName);
                    string logEntry = $"{DateTime.UtcNow:G}: {msg}";
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
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Cannot write to {logLevel} log: {ex.Message}");
            }
        }
    }
}
