using FolderFlex.Services.Interface;

namespace FolderFlex.Services.ErrorManager;

public class ErrorLogger : IObserver
{
    private readonly LogManager logManager;
    public ErrorLogger() => logManager = new LogManager();

    public void Update(string errorMessage) => logManager.WriteLog(errorMessage);
}
