using FolderFlex.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderFlex.Services
{
    public class ErrorHandler : ISubject
    {
        private readonly List<IObserver> observers = [];
        private readonly List<string> errors = [];

        public void Attach(IObserver observer)
        {
            observers.Add(observer);
        }

        public void Detach(IObserver observer)
        {
            observers.Remove(observer);
        }

        public void Notify(string errorMessage)
        {
            foreach (var observer in observers)
            {
                observer.Update(errorMessage);
            }
        }

        public void AddError(string errorMessage)
        {
            errors.Add(errorMessage);
            Notify(errorMessage);
        }

        public List<string> GetErrors()
        {
            return errors;
        }

        public void ClearErrors()
        {
            errors.Clear();
        }
    }
}
