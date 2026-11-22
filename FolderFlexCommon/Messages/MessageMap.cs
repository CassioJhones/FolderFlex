using FolderFlexCommon.Settings;
using FolderFlexCommon.Settings.ApplicationSettings;
using System.Collections.Frozen;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FolderFlexCommon.Messages;

[JsonSerializable(typeof(Dictionary<string, Dictionary<string, string>>))]
internal sealed partial class MessageJsonContext : JsonSerializerContext;

public static class MessageMap
{
    private const string FallbackLanguage = "pt";
    private const string ResourceName = "FolderFlexCommon.Messages.Resources.messages.json";

    private static readonly FrozenDictionary<string, FrozenDictionary<string, string>> Messages;
    private static readonly FrozenDictionary<string, string> Languages;
    private static volatile string _defaultLanguage;

    static MessageMap()
    {
        Messages = LoadMessagesFromJson();

        Languages = Messages
            .Where(pair => pair.Value.ContainsKey("language_description"))
            .ToFrozenDictionary(
                pair => pair.Key,
                pair => pair.Value["language_description"],
                StringComparer.OrdinalIgnoreCase);

        string configuredLanguage = ApplicationSettings.New(new IniFileParameterStore("config.flx")).Language;
        _defaultLanguage = Messages.ContainsKey(configuredLanguage) ? configuredLanguage : FallbackLanguage;
    }

    private static FrozenDictionary<string, FrozenDictionary<string, string>> LoadMessagesFromJson()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        using Stream stream = assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{ResourceName}' was not found.");

        Dictionary<string, Dictionary<string, string>> raw = JsonSerializer.Deserialize(
            stream,
            MessageJsonContext.Default.DictionaryStringDictionaryStringString)
            ?? [];

        return raw.ToFrozenDictionary(
            outer => outer.Key,
            outer => outer.Value.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase),
            StringComparer.OrdinalIgnoreCase);
    }

    public static string GetMessage(string key, string? language = null)
    {
        string targetLanguage = language ?? _defaultLanguage;

        if (Messages.TryGetValue(targetLanguage, out FrozenDictionary<string, string>? langMessages) &&
            langMessages.TryGetValue(key, out string? message))
            return message;

        if (Messages.TryGetValue(_defaultLanguage, out FrozenDictionary<string, string>? defaultMessages) &&
            defaultMessages.TryGetValue(key, out string? defaultMessage))
            return defaultMessage;

        if (Messages.TryGetValue(FallbackLanguage, out FrozenDictionary<string, string>? fallbackMessages) &&
            fallbackMessages.TryGetValue(key, out string? fallbackMessage))
            return fallbackMessage;

        return key;
    }

    public static string FormatMessage(string key, params ReadOnlySpan<object?> args)
        => string.Format(CultureInfo.CurrentCulture, GetMessage(key), args);

    public static void SetDefaultLanguage(string language)
    {
        if (!Messages.ContainsKey(language))
            throw new ArgumentException($"Language '{language}' is not supported.", nameof(language));

        _defaultLanguage = language;
    }

    public static IReadOnlyDictionary<string, string> ListLanguages() => Languages;

    public static string? GetLanguageKeyByDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return null;

        foreach ((string key, string value) in Languages)
        {
            if (string.Equals(value, description, StringComparison.OrdinalIgnoreCase))
                return key;
        }

        return null;
    }
}
