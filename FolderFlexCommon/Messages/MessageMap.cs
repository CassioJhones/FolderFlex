using FolderFlexCommon.Settings;
using FolderFlexCommon.Settings.ApplicationSettings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace FolderFlexCommon.Messages;

public static class MessageMap
{
    private static Dictionary<string, Dictionary<string, string>> _messages;

    private static string _defaultLanguage;

    static MessageMap()
    {
        _messages = new Dictionary<string, Dictionary<string, string>>();

        _defaultLanguage = ApplicationSettings.New(new IniFileParameterStore("config.flx")).Language ?? "pt";

        LoadMessagesFromJson();
    }

    private static void LoadMessagesFromJson()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        string resourceName = "FolderFlexCommon.Messages.Resources.messages.json";

        using Stream stream = assembly.GetManifestResourceStream(resourceName);
        using StreamReader reader = new(stream);
        string jsonContent = reader.ReadToEnd();
        _messages = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(jsonContent);
    }

    public static string GetMessage(string key, string language = null)
    {
        language ??= _defaultLanguage;

        if (_messages.ContainsKey(language) && _messages[language].ContainsKey(key))
        {
            return _messages[language][key];
        }

        return _messages.ContainsKey(_defaultLanguage) && _messages[_defaultLanguage].ContainsKey(key)
            ? _messages[_defaultLanguage][key]
            : null;
    }

    public static void SetDefaultLanguage(string language) => _defaultLanguage = _messages.ContainsKey(language) ? language : throw new ArgumentException($"Language '{language}' not supported.");

    public static Dictionary<string, string> ListLanguages()
    {
        Dictionary<string, string> languageList = new();

        foreach (string language in _messages.Keys)
        {
            if (_messages[language].ContainsKey("language_description"))
            {
                languageList.Add(language, _messages[language]["language_description"]);
            }
        }

        return languageList;
    }
}
