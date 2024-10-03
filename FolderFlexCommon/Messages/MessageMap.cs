using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace FolderFlexCommon.Messages
{
    public static class MessageMap
    {
        private static Dictionary<string, Dictionary<string, string>> _messages;

        // TODO: deve carregar das configuracoes .ini ou xml
        private static string _defaultLanguage = "pt";

        static MessageMap()
        {
            _messages = new Dictionary<string, Dictionary<string, string>>();
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
            else if (_messages.ContainsKey(_defaultLanguage) && _messages[_defaultLanguage].ContainsKey(key))
            {
                return _messages[_defaultLanguage][key];
            }
            else
            {
                return $"Message key '{key}' not found.";
            }
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
    }
}
