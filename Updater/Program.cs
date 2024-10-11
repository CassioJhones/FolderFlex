using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Text.Json.Nodes;

public class Updater
{
    private const string GitHubApiUrl = "https://api.github.com/repos/CassioJhones/FolderFlex/releases/latest";

    private readonly static string AppName = "FolderFlex.exe";
    private static JsonNode? GithubRepoResponse { get; set; }
    private static string? AppPath { get; set; }

    private static string TempPath = $"{Path.Combine(Path.GetTempPath())}%0%";

    private static string ExtractionPath = $"{Path.Combine(Path.GetTempPath())}%0%";
    public static async Task Main(string[] args)
    {
        HttpClient client = new();
        try
        {
            string appDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            AppPath = Path.Combine(appDirectory, AppName);

            client.DefaultRequestHeaders.UserAgent.TryParseAdd("request");

            string response = await client.GetAsync(GitHubApiUrl).Result.Content.ReadAsStringAsync();

            GithubRepoResponse = JsonNode.Parse(response);

            await Download();

            UnzipFiles();

            ReplaceFiles();

        }
        catch (Exception)
        {
            StartApplication();

            Environment.Exit(0);
        }
        finally
        {
            StartApplication();

            Environment.Exit(0);
        }
    }

    private static async Task Download()
    {
        using HttpClient client = new();

        string downloadLink = GithubRepoResponse["assets"][0]["browser_download_url"].ToString();

        string fileName = Path.GetFileName(downloadLink);
        HttpResponseMessage response = await client.GetAsync(downloadLink);

        if (response.IsSuccessStatusCode)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), fileName);

            int bufferSize = 8192;
            byte[] buffer = new byte[bufferSize];

            await using FileStream fs = new(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, useAsync: true);

            await using Stream contentStream = await response.Content.ReadAsStreamAsync();
            long totalBytesRead = 0;
            long totalBytes = response.Content.Headers.ContentLength ?? -1;

            int bytesRead;
            Console.ForegroundColor = ConsoleColor.Green;
            while ((bytesRead = await contentStream.ReadAsync(buffer.AsMemory(0, bufferSize))) > 0)
            {
                await fs.WriteAsync(buffer.AsMemory(0, bytesRead));

                totalBytesRead += bytesRead;

                if (totalBytes > 0)
                {
                    double progress = (double)totalBytesRead / totalBytes * 100;
                    Console.Clear();
                    Console.WriteLine($"Progress: {progress:F2}%");
                }
            }
            Console.ForegroundColor = ConsoleColor.White;
        }
    }

    private static void ReplaceFiles()
    {
        string[] arquivos = Directory.GetFiles(ExtractionPath, "*.*", SearchOption.AllDirectories);

        foreach (string arquivo in arquivos)
        {
            string destino = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.GetFileName(arquivo));
            File.Copy(arquivo, destino, true);
        }
    }

    private static void UnzipFiles() => ZipFile.ExtractToDirectory(TempPath, ExtractionPath, true);

    private static void StartApplication()
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = Path.GetFileName(AppPath),
            WindowStyle = ProcessWindowStyle.Normal
        };

        Process.Start(Path.GetFileName(AppPath));
    }
}
