using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json.Nodes;

public static class AtualizacaoChecker
{
    private const string GitHubApiUrl = "https://api.github.com/repos/CassioJhones/FolderFlex/releases/latest";

    public static async Task<bool> VerificarAtualizacaoAsync()
    {
        try
        {
            Version? versaoAntiga = Assembly.GetExecutingAssembly().GetName().Version;

            using HttpClient client = new();

            client.DefaultRequestHeaders.UserAgent.TryParseAdd("request");

            HttpResponseMessage response = client.GetAsync(GitHubApiUrl).Result;

            if (response.IsSuccessStatusCode)
            {
                string? apiResponse = await response.Content.ReadAsStringAsync();

                JsonNode? JsonResult = JsonObject.Parse(apiResponse);

                string? versaoGit = JsonResult?["tag_name"]?.ToString();

                Version.TryParse(versaoGit, out Version novaVersao);

                return (novaVersao > versaoAntiga);
            }

            return false;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
    public static void StartUpdaterAndExit()
    {
        var updaterPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Updater.exe");

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "Updater.exe",
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true,
        };

        Process.Start(startInfo);

        Environment.Exit(0);
    }
}
