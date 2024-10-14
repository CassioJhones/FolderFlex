using FolderFlex.Services;
using FolderFlex.Services.ErrorManager;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json.Nodes;

public static class AtualizacaoChecker
{
    private const string GitHubApiUrl = "https://api.github.com/repos/CassioJhones/FolderFlex/releases/latest";
    private static readonly ErrorHandler errorHandler;
    static AtualizacaoChecker()
    {
        errorHandler = new ErrorHandler();
        errorHandler.Attach(new ErrorLogger());
    }
        public static async Task<bool> VerificarAtualizacaoAsync()
        {
            try
            {
                bool hasConnection = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
                if (!hasConnection)
                {
                    errorHandler.AddError("Conexão com a Internet não disponível.");
                    return false;
                }

                Version? versaoAntiga = Assembly.GetExecutingAssembly().GetName().Version;
                string versaoAntigaFormatada = $"{versaoAntiga.Major}.{versaoAntiga.Minor}.{versaoAntiga.Build}";

                using HttpClient client = new();
                client.DefaultRequestHeaders.UserAgent.TryParseAdd("request");
                client.Timeout = TimeSpan.FromSeconds(10);

                HttpResponseMessage response = client.GetAsync(GitHubApiUrl).Result;
                response.EnsureSuccessStatusCode();

                string? apiResponse = await response.Content.ReadAsStringAsync();
                JsonNode? JsonResult = JsonNode.Parse(apiResponse);

                string? versaoGit = JsonResult?["tag_name"]?.ToString();
                if (string.IsNullOrEmpty(versaoGit))
                {
                    errorHandler.AddError("Versão do GitHub não disponível.");
                    return false;
                }

                if (versaoGit.StartsWith("v",StringComparison.CurrentCultureIgnoreCase))
                    versaoGit = versaoGit[1..];

                Version.TryParse(versaoGit, out Version? novaVersao);
                Version.TryParse(versaoAntigaFormatada, out Version? versaoAtual);

                return novaVersao > versaoAtual;
            }
            catch (HttpRequestException e)
            {
                errorHandler.AddError($"Erro na requisição: {e.Message}");
                return false;
            }
            catch (TaskCanceledException)
            {
                errorHandler.AddError("A requisição foi cancelada devido ao timeout.");
                return false;
            }
            catch (Exception ex)
            {
                errorHandler.AddError($"Erro inesperado: {ex.Message}");
                return false;
            }
        }

    public static void StartUpdaterAndExit()
    {
        try
        {
            string updaterPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Updater.exe");

            ProcessStartInfo startInfo = new()
            {
                FileName = "Updater.exe",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
            };

            Process.Start(startInfo);
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            errorHandler.AddError($"Erro inesperado: {ex.Message}");
            return;
        }
    }
}
