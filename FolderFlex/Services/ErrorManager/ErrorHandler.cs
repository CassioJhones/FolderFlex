using FolderFlex.Services.Interface;

namespace FolderFlex.Services;

public class ErrorHandler : ISubject
{
    private readonly List<IObserver> observers = [];
    private readonly List<string> errors = [];

    public void Attach(IObserver observer) => observers.Add(observer);

    public void Detach(IObserver observer) => observers.Remove(observer);

    public void Notify(string errorMessage)
    {
        foreach (IObserver observer in observers)
        {
            observer.Update(errorMessage);
        }
    }

    public void AddError(string errorMessage)
    {
        errors.Add(errorMessage);
        Notify(errorMessage);
    }

    public List<string> GetErrors() => errors;

    public void ClearErrors() => errors.Clear();
}
