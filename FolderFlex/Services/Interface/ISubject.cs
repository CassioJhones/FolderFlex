namespace FolderFlex.Services.Interface;

public interface ISubject
{
    void Attach(IObserver observer);
    void Detach(IObserver observer);
    void Notify(string errorMessage);
}
