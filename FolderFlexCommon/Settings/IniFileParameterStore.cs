using FolderFlexCommon.Settings.Model;
using System.Text;

namespace FolderFlexCommon.Settings;

public sealed class IniFileParameterStore : IParameterStorage
{
    private const string DefaultSection = "Settings";

    private readonly Lock _syncRoot = new();
    private readonly Dictionary<string, Dictionary<string, string>> _data = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _filePath;

    public IniFileParameterStore(string iniFile)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(iniFile);
        _filePath = iniFile;

        if (!File.Exists(_filePath))
        {
            string? directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            using FileStream _ = File.Create(_filePath);
        }

        Load(_filePath);
    }

    public string? GetParameter(string section, string key)
    {
        lock (_syncRoot)
        {
            return _data.TryGetValue(section, out Dictionary<string, string>? sectionData) &&
                   sectionData.TryGetValue(key, out string? value)
                ? value
                : null;
        }
    }

    public void SetParameter(string section, string key, string value)
    {
        lock (_syncRoot)
        {
            if (!_data.TryGetValue(section, out Dictionary<string, string>? sectionData))
            {
                sectionData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                _data[section] = sectionData;
            }

            sectionData[key] = value;
            SaveUnsafe();
        }
    }

    public void Load(string filePath)
    {
        lock (_syncRoot)
        {
            _data.Clear();
            string currentSection = DefaultSection;

            foreach (string rawLine in File.ReadLines(filePath))
            {
                ReadOnlySpan<char> line = rawLine.AsSpan().Trim();
                if (line.IsEmpty || line[0] is ';' or '#')
                    continue;

                if (line.Length >= 2 && line[0] == '[' && line[^1] == ']')
                {
                    currentSection = line[1..^1].Trim().ToString();
                    _data.TryAdd(currentSection, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
                    continue;
                }

                int separatorIndex = line.IndexOf('=');
                if (separatorIndex <= 0)
                    continue;

                string key = line[..separatorIndex].Trim().ToString();
                string value = line[(separatorIndex + 1)..].Trim().ToString();

                if (!_data.TryGetValue(currentSection, out Dictionary<string, string>? sectionData))
                {
                    sectionData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    _data[currentSection] = sectionData;
                }

                sectionData[key] = value;
            }
        }
    }

    private void SaveUnsafe()
    {
        StringBuilder sb = new();
        foreach ((string sectionName, Dictionary<string, string> values) in _data)
        {
            sb.Append('[').Append(sectionName).AppendLine("]");
            foreach ((string key, string value) in values)
                sb.Append(key).Append('=').AppendLine(value);
        }

        File.WriteAllText(_filePath, sb.ToString());
    }
}
