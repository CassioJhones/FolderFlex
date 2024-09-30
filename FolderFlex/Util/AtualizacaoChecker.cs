using System.Net.Http;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Windows;

public static class AtualizacaoChecker
{
    private const string GitHubApiUrl = "https://api.github.com/repos/CassioJhones/FolderFlex/releases/latest";

    public static async Task VerificarAtualizacaoAsync()
    {
        try
        {
            Version? versaoAntiga = Assembly.GetExecutingAssembly().GetName().Version;
            using HttpClient client = new();
            client.DefaultRequestHeaders.UserAgent.TryParseAdd("request");
            HttpResponseMessage response = await client.GetAsync(GitHubApiUrl);
            if (response.IsSuccessStatusCode)
            {
                string? apiResponse = await response.Content.ReadAsStringAsync();
                JsonNode? JsonResult = JsonObject.Parse(apiResponse);
                string? versaoGit = JsonResult?["tag_name"]?.ToString();

                if (Version.TryParse(versaoGit, out Version novaVersao) && novaVersao > versaoAntiga)
                {
                    System.Windows.MessageBox.Show($"Uma nova versão ({versaoGit}) está disponível. Atualize seu aplicativo.", "Atualização Disponível", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        catch (Exception ex)
        {

        }
    }
}
