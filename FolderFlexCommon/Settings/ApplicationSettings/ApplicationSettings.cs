using FolderFlexCommon.Settings.Model;
using System.Globalization;

namespace FolderFlexCommon.Settings.ApplicationSettings;

public sealed class ApplicationSettings
{
    private const string SectionName = "Settings";
    private static readonly Lock InstanceLock = new();
    private static ApplicationSettings? _instance;

    private readonly IParameterStorage _storage;

    private ApplicationSettings(IParameterStorage storage) => _storage = storage;

    public static ApplicationSettings New(IParameterStorage storage)
    {
        ArgumentNullException.ThrowIfNull(storage);

        if (_instance is not null)
            return _instance;

        lock (InstanceLock)
        {
            return _instance ??= new ApplicationSettings(storage);
        }
    }

    public string Theme
    {
        get => _storage.GetParameter(SectionName, nameof(Theme)) ?? "LightTheme";
        set => _storage.SetParameter(SectionName, nameof(Theme), value);
    }

    public string Language
    {
        get => _storage.GetParameter(SectionName, nameof(Language)) ?? "pt";
        set => _storage.SetParameter(SectionName, nameof(Language), value);
    }

    public DateTimeOffset? LastUpdateCheck
    {
        get
        {
            string? raw = _storage.GetParameter(SectionName, nameof(LastUpdateCheck));
            return DateTimeOffset.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTimeOffset parsed)
                ? parsed
                : null;
        }
        set
        {
            if (value is { } date)
                _storage.SetParameter(SectionName, nameof(LastUpdateCheck), date.ToString("O", CultureInfo.InvariantCulture));
        }
    }
}
