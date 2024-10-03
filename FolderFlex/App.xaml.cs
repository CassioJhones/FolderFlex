using System.Diagnostics;
using System.IO;
using Application = System.Windows.Application;

namespace FolderFlex;
public partial class App : Application
{
    private void Application_Startup(object sender, System.Windows.StartupEventArgs e)
    {
#if (DEBUG == false)
        bool hasNewVersion = AtualizacaoChecker.VerificarAtualizacaoAsync().Result;

        if (hasNewVersion)
        {
            AtualizacaoChecker.StartUpdaterAndExit();
        }
#endif
    }
}

