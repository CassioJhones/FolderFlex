using FolderFlexCommon.Settings.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FolderFlexCommon.Settings
{
    public class IniFileParameterStore : IParameterStorage
    {
        private readonly Dictionary<string, Dictionary<string, string>> _data = new Dictionary<string, Dictionary<string, string>>();

        private readonly string _filePath;
        public IniFileParameterStore(string iniFile) { 
            _filePath = iniFile;

            if (!File.Exists(_filePath))
            {
                using (var file = File.Create(_filePath)) { }
            }

            Load(_filePath);
        }
        public string GetParameter(string section, string key)
        {
            if (_data.ContainsKey(section) && _data[section].ContainsKey(key))
            {
                return _data[section][key];
            }
            return null;
        }

        public void SetParameter(string section, string key, string value)
        {
            if (!_data.ContainsKey(section))
            {
                _data[section] = new Dictionary<string, string>();
            }
            _data[section][key] = value;

            Save();
        }

        public void Load(string filePath)
        {
            _data.Clear();
            foreach (var line in File.ReadAllLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith(";") || line.StartsWith("#")) continue;

                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    var section = line.Trim('[', ']');
                    if (!_data.ContainsKey(section))
                    {
                        _data[section] = new Dictionary<string, string>();
                    }

                    continue;
                }
               
                var parts = line.Split(new[] { '=' }, 2);

                if (parts.Length == 2)
                {
                    var section = _data.Keys.Last();
                    _data[section][parts[0].Trim()] = parts[1].Trim();
                }
                
            }
        }

        private void Save()
        {
            var sb = new StringBuilder();
            foreach (var section in _data)
            {
                sb.AppendLine($"[{section.Key}]");
                foreach (var kvp in section.Value)
                {
                    sb.AppendLine($"{kvp.Key}={kvp.Value}");
                }
            }
            File.WriteAllText(_filePath, sb.ToString());
        }
    }
}
