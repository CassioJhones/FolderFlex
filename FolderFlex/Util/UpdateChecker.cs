using FolderFlex.Services;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text.Json.Serialization;

namespace FolderFlex.Util;

internal sealed record GitHubRelease([property: JsonPropertyName("tag_name")] string? TagName);

[JsonSerializable(typeof(GitHubRelease))]
internal sealed partial class GitHubReleaseJsonContext : JsonSerializerContext;

public static class UpdateChecker
{
    private const string GitHubApiUrl = "https://api.github.com/repos/CassioJhones/FolderFlex/releases/latest";

    private static readonly HttpClient Client = CreateHttpClient();

    private static HttpClient CreateHttpClient()
    {
        HttpClient client = new(new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(2)
        })
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        string version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("FolderFlex", version));
        return client;
    }

    public static async Task<bool> CheckUpdateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                ErrorLogService.Shared.AddError("Conexão com a Internet não disponível.");
                return false;
            }

            Version? currentAssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
            if (currentAssemblyVersion is null)
                return false;

            Version currentVersion = new(
                Math.Max(currentAssemblyVersion.Major, 0),
                Math.Max(currentAssemblyVersion.Minor, 0),
                Math.Max(currentAssemblyVersion.Build, 0));

            GitHubRelease? release = await Client.GetFromJsonAsync(
                GitHubApiUrl,
                GitHubReleaseJsonContext.Default.GitHubRelease,
                cancellationToken);

            string? tagName = release?.TagName;
            if (string.IsNullOrWhiteSpace(tagName))
            {
                ErrorLogService.Shared.AddError("Versão do GitHub não disponível.");
                return false;
            }

            ReadOnlySpan<char> versionSpan = tagName.AsSpan().TrimStart(['v', 'V']);
            return Version.TryParse(versionSpan, out Version? latestVersion) && latestVersion > currentVersion;
        }
        catch (HttpRequestException ex)
        {
            ErrorLogService.Shared.AddError($"Erro na requisição: {ex.Message}");
            return false;
        }
        catch (TaskCanceledException)
        {
            ErrorLogService.Shared.AddError("A requisição foi cancelada devido ao timeout.");
            return false;
        }
        catch (Exception ex)
        {
            ErrorLogService.Shared.AddError($"Erro inesperado: {ex.Message}");
            return false;
        }
    }
}
