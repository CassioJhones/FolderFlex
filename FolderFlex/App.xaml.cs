using FolderFlex.Services;
using FolderFlexCommon.Settings;
using FolderFlexCommon.Settings.ApplicationSettings;
using System.IO;
using System.Windows;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace FolderFlex;
public partial class App : Application
{
    private void Application_Startup(object sender, System.Windows.StartupEventArgs e)
    {
        var settings = ApplicationSettings.New(new IniFileParameterStore("config.flx"));
        ThemeService.ApplyTheme(settings.Theme ?? "LightTheme");

        string lastCheckDateFilePath = "lastCheckDate.txt";

        DateTime lastCheckDate;

        if (File.Exists(lastCheckDateFilePath))
        {
            string savedDate = File.ReadAllText(lastCheckDateFilePath);
            if (DateTime.TryParse(savedDate, out lastCheckDate))
            {
                if (lastCheckDate == DateTime.MinValue)
                    lastCheckDate = DateTime.Now.AddDays(-6);  
            }
            else
                lastCheckDate = DateTime.Now.AddDays(-6);  
        }
        else
            lastCheckDate = DateTime.Now.AddDays(-6);

        bool hasConnection = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();

        if ((DateTime.Now - lastCheckDate).TotalDays >= 5 && hasConnection)
        {
            MessageBoxResult resultado = MessageBox.Show(
                "Deseja verificar se existe uma nova versão disponível?",
                "Atualização",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                bool hasNewVersion = UpdateChecker.CheckUpdateAsync().Result;
                if (hasNewVersion)
                {
                    resultado = MessageBox.Show(
                        "Uma nova versão está disponível. Deseja atualizar agora?",
                        "Atualização Disponível",
                        MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (resultado == MessageBoxResult.Yes)
                        FileService.OpenLink("https://github.com/CassioJhones/FolderFlex/releases");
                }
            }

            File.WriteAllText(lastCheckDateFilePath, DateTime.Now.ToString());
        }
    }
}

