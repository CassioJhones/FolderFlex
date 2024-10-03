using FolderFlexCommon.Settings.Model;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace FolderFlexCommon.Settings
{
    internal class XmlParameterStore : IParameterStorage
    {
        private XElement _xmlData;

        private readonly string _filePath;
        public XmlParameterStore(string xmlFile)
        {
            _filePath = xmlFile;
        }
        public string GetParameter(string section, string key)
        {
            var sectionElement = _xmlData?.Element(section);
            if (sectionElement != null)
            {
                var keyElement = sectionElement.Element(key);
                return keyElement?.Value;
            }
            return null;
        }

        public void SetParameter(string section, string key, string value)
        {
            var sectionElement = _xmlData.Element(section);
            if (sectionElement == null)
            {
                sectionElement = new XElement(section);
                _xmlData.Add(sectionElement);
            }
            var keyElement = sectionElement.Element(key);
            if (keyElement == null)
            {
                keyElement = new XElement(key, value);
                sectionElement.Add(keyElement);

                return;
            }
           
            keyElement.Value = value;

            Save();
        }

        public void Load(string filePath)
        {
            _xmlData = XElement.Load(filePath);
        }

        private void Save()
        {
            _xmlData.Save(_filePath);
        }
    }
}
