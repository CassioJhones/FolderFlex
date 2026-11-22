using FolderFlex.Services;
using FolderFlex.Util;
using FolderFlexCommon.Settings;
using FolderFlexCommon.Settings.ApplicationSettings;
using System.Net.NetworkInformation;
using System.Windows;

namespace FolderFlex;

public partial class App : Application
{
    private static readonly TimeSpan UpdateCheckInterval = TimeSpan.FromDays(5);

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ApplicationSettings settings = ApplicationSettings.New(new IniFileParameterStore("config.flx"));
        ThemeService.ApplyTheme(settings.Theme);

        _ = Dispatcher.InvokeAsync(() => CheckForUpdatesAsync(settings));
    }

    private static async Task CheckForUpdatesAsync(ApplicationSettings settings)
    {
        DateTimeOffset now = TimeProvider.System.GetUtcNow();
        DateTimeOffset lastCheck = settings.LastUpdateCheck ?? now - TimeSpan.FromDays(6);

        if (now - lastCheck < UpdateCheckInterval || !NetworkInterface.GetIsNetworkAvailable())
            return;

        MessageBoxResult shouldCheck = MessageBox.Show(
            "Deseja verificar se existe uma nova versão disponível?",
            "Atualização",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (shouldCheck == MessageBoxResult.Yes && await UpdateChecker.CheckUpdateAsync())
        {
            MessageBoxResult shouldUpdate = MessageBox.Show(
                "Uma nova versão está disponível. Deseja atualizar agora?",
                "Atualização Disponível",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (shouldUpdate == MessageBoxResult.Yes)
                FileService.OpenLink("https://github.com/CassioJhones/FolderFlex/releases");
        }

        settings.LastUpdateCheck = now;
    }
}
