using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FolderFlexCommon.Settings.ApplicationSettings;
using FolderFlexCommon.Settings;

namespace FolderFlexCommon.Messages
{
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
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "FolderFlexCommon.Messages.Resources.messages.json";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string jsonContent = reader.ReadToEnd();
                _messages = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(jsonContent);
            }
        }

        public static string GetMessage(string key, string language = null)
        {
            language = language ?? _defaultLanguage;

            if (_messages.ContainsKey(language) && _messages[language].ContainsKey(key))
            {
                return _messages[language][key];
            }

            if (_messages.ContainsKey(_defaultLanguage) && _messages[_defaultLanguage].ContainsKey(key))
            {
                return _messages[_defaultLanguage][key];
            }
           
             return null;
          
        }

        public static void SetDefaultLanguage(string language)
        {
            if (_messages.ContainsKey(language))
            {
                _defaultLanguage = language;
            }
            else
            {
                throw new ArgumentException($"Language '{language}' not supported.");
            }
        }

        public static Dictionary<string, string> ListLanguages()
        {
            var languageList = new Dictionary<string, string>();

            foreach (var language in _messages.Keys)
            {
                if (_messages[language].ContainsKey("language_description"))
                {
                    languageList.Add(language, _messages[language]["language_description"]);
                }
            }

            return languageList;
        }
    }
}
