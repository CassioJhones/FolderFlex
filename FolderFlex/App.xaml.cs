using System.Windows;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace FolderFlex;
public partial class App : Application
{
    private void Application_Startup(object sender, System.Windows.StartupEventArgs e)
    {
        bool hasNewVersion = AtualizacaoChecker.VerificarAtualizacaoAsync().Result;

        if (hasNewVersion)
        {
            MessageBoxResult resultado = MessageBox.Show(
                    "Uma nova versão está disponível. Deseja atualizar agora?",
                    "Atualização Disponível",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
                AtualizacaoChecker.StartUpdaterAndExit();
        }
    }
}

