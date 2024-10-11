using FolderFlexCommon.Settings.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FolderFlexCommon.Settings;

public class IniFileParameterStore : IParameterStorage
{
    private readonly Dictionary<string, Dictionary<string, string>> _data = new();

    private readonly string _filePath;
    public IniFileParameterStore(string iniFile)
    {
        _filePath = iniFile;

        if (!File.Exists(_filePath))
        {
            using FileStream file = File.Create(_filePath);
        }

        Load(_filePath);
    }
    public string GetParameter(string section, string key) => _data.ContainsKey(section) && _data[section].ContainsKey(key) ? _data[section][key] : null;

    public void SetParameter(string section, string key, string value)
    {
        if (!_data.ContainsKey(section))
            _data[section] = new Dictionary<string, string>();
        _data[section][key] = value;

        Save();
    }

    public void Load(string filePath)
    {
        _data.Clear();
        foreach (string line in File.ReadAllLines(filePath))
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith(";") || line.StartsWith("#")) continue;

            if (line.StartsWith("[") && line.EndsWith("]"))
            {
                string section = line.Trim('[', ']');
                if (!_data.ContainsKey(section))
                    _data[section] = new Dictionary<string, string>();

                continue;
            }

            string[] parts = line.Split(new[] { '=' }, 2);

            if (parts.Length == 2)
            {
                string section = _data.Keys.Last();
                _data[section][parts[0].Trim()] = parts[1].Trim();
            }
        }
    }

    private void Save()
    {
        StringBuilder sb = new();
        foreach (KeyValuePair<string, Dictionary<string, string>> section in _data)
        {
            sb.AppendLine($"[{section.Key}]");
            foreach (KeyValuePair<string, string> kvp in section.Value)
                sb.AppendLine($"{kvp.Key}={kvp.Value}");
        }
        File.WriteAllText(_filePath, sb.ToString());
    }
}
