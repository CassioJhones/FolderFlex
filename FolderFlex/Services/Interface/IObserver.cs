using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderFlex.Services.Interface
{
    public interface IObserver
    {
       void Update(string message);
    }
}
