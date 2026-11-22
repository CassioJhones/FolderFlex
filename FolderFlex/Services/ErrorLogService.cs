using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;

namespace FolderFlex.Services;

public sealed class ErrorLogService(string logFilePath = "error.log")
{
    private readonly Lock _fileLock = new();
    private readonly ConcurrentQueue<string> _errors = [];

    public static ErrorLogService Shared { get; } = new();

    public void AddError(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            return;

        _errors.Enqueue(errorMessage);
        WriteLog(errorMessage);
    }

    public string[] DrainErrors()
    {
        List<string> drained = [];
        while (_errors.TryDequeue(out string? error))
            drained.Add(error);

        return [.. drained];
    }

    private void WriteLog(string message)
    {
        string logEntry = $"{TimeProvider.System.GetLocalNow():yyyy-MM-dd HH:mm:ss}: {message}{Environment.NewLine}";

        lock (_fileLock)
        {
            try
            {
                File.AppendAllText(logFilePath, logEntry);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to write to log file: {ex.Message}");
            }
        }
    }
}
