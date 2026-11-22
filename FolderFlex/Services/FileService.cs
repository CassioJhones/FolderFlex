using System.Diagnostics;
using System.IO;

namespace FolderFlex.Services;

internal static class FileService
{
    public static void OpenFile(string filePath)
    {
        if (!File.Exists(filePath))
            return;

        Process.Start(new ProcessStartInfo
        {
            FileName = filePath,
            UseShellExecute = true
        });
    }

    public static void OpenFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath))
            return;

        Process.Start(new ProcessStartInfo
        {
            FileName = folderPath,
            UseShellExecute = true
        });
    }

    public static void OpenLink(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = uri.AbsoluteUri,
            UseShellExecute = true
        });
    }

    public static string GetUniqueFilePath(string originalPath, ISet<string>? reservedPaths = null)
    {
        string folder = Path.GetDirectoryName(originalPath) ?? string.Empty;
        string fileName = Path.GetFileNameWithoutExtension(originalPath);
        string ext = Path.GetExtension(originalPath);

        int count = 1;
        string candidate;

        do
        {
            candidate = Path.Combine(folder, $"{fileName} ({count}){ext}");
            count++;
        }
        while (File.Exists(candidate) || (reservedPaths is not null && reservedPaths.Contains(candidate)));

        reservedPaths?.Add(candidate);
        return candidate;
    }
}
