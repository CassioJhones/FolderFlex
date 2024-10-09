using FolderFlexCommon.Settings.Model;

namespace FolderFlexCommon.Settings.ApplicationSettings;

public class ApplicationSettings
{
    private static ApplicationSettings _instance;
    private IParameterStorage _storage;

    private ApplicationSettings(IParameterStorage storage) => _storage = storage;

    public static ApplicationSettings New(IParameterStorage storage)
    {
        _instance ??= new ApplicationSettings(storage);
        return _instance;
    }

    private const string SectionName = "Settings";
    public string Theme
    {
        get => _storage.GetParameter(SectionName, "Theme");
        set => _storage.SetParameter(SectionName, "Theme", value);
    }

    public string Language
    {
        get => _storage.GetParameter(SectionName, "Language") ?? "pt";
        set => _storage.SetParameter(SectionName, "Language", value);
    }
}
