namespace FolderFlexCommon.Settings.Model;

public interface IParameterStorage
{
    string GetParameter(string section, string key);
    void SetParameter(string section, string key, string value);
    void Load(string filePath);
}
