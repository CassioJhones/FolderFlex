using FolderFlex.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderFlex.Services.ErrorManager
{
    public class ErrorLogger : IObserver
    {
        private readonly LogManager logManager;
        public ErrorLogger()
        {
            logManager = new LogManager();
        }

        public void Update(string errorMessage)
        {
            logManager.WriteLog(errorMessage);
        }
    }
}
