using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderFlexCommon.Settings.Model
{
    public interface IParameterStorage
    {
        string GetParameter(string section, string key);
        void SetParameter(string section, string key, string value);
        void Load(string filePath);
    }
}
