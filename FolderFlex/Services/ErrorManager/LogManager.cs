using System;
using System.Diagnostics;
using System.IO;

namespace FolderFlex.Services.ErrorManager
{
    public class LogManager
    {
        private const string logFilePath = "error.log";

        public LogManager()
        {
            if (!File.Exists(logFilePath))
            {
                using FileStream fs = File.Create(logFilePath);
            }
        }
        public void WriteLog(string message)
        {
            string logEntry = $"{DateTime.Now}: {message}";

            try
            {
                using StreamWriter writer = new(logFilePath, true);

                writer.WriteLine(logEntry);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to write to log file: {ex.Message}");
            }
        }
    }
}