using System.Globalization;

namespace FolderFlexCommon.Extensions;

public static class FormattingExtensions
{
    private const long Kilobyte = 1024;
    private const long Megabyte = Kilobyte * 1024;
    private const long Gigabyte = Megabyte * 1024;
    private const long Terabyte = Gigabyte * 1024;

    extension(TimeSpan elapsed)
    {
        public string ToFormattedDuration() => elapsed switch
        {
            { TotalSeconds: < 1 } => $"{elapsed.Milliseconds}ms",
            { TotalMinutes: < 1 } => $"{elapsed.Seconds}s {elapsed.Milliseconds}ms",
            { TotalHours: < 1 } => $"{elapsed.Minutes}m {elapsed.Seconds}s {elapsed.Milliseconds}ms",
            _ => $"{(int)elapsed.TotalHours}h {elapsed.Minutes}m {elapsed.Seconds}s"
        };
    }

    extension(long byteCount)
    {
        public string ToFileSizeLabel() => byteCount switch
        {
            < Kilobyte => $"{byteCount} B",
            < Megabyte => $"{byteCount / Kilobyte} KB",
            < Gigabyte => string.Create(CultureInfo.InvariantCulture, $"{byteCount / (double)Megabyte:0.#} MB"),
            < Terabyte => string.Create(CultureInfo.InvariantCulture, $"{byteCount / (double)Gigabyte:0.##} GB"),
            _ => string.Create(CultureInfo.InvariantCulture, $"{byteCount / (double)Terabyte:0.##} TB")
        };
    }

    extension(string path)
    {
        public int DirectoryDepth => path.Count(c => c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar);

        public string TruncatedFileName(int maxLength = 35)
        {
            string fileName = Path.GetFileName(path);
            return fileName.Length > maxLength ? $"{fileName[..maxLength]}..." : fileName;
        }
    }
}
