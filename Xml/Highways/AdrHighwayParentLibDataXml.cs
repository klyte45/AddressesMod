﻿using ColossalFramework;
using ColossalFramework.Globalization;
using ICities;
using Klyte.Commons;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using static Klyte.Commons.Utils.XmlUtils;

namespace Klyte.Addresses.Xml
{
    [XmlRoot("HighwayData")]
    public class AdrHighwayParentLibDataXml : DataExtensionLibBase<AdrHighwayParentLibDataXml, AdrHighwayParentXml>
    {
        [XmlIgnore]
        public override string SaveId => "K45_ADR_HW_PARENTDATA";

        [XmlElement("descriptorsData")]
        public override ListWrapper<AdrHighwayParentXml> SavedDescriptorsSerialized
        {
            get => new ListWrapper<AdrHighwayParentXml>() { listVal = m_savedDescriptorsSerialized.Where(x => x.Value.m_configurationSource == ConfigurationSource.CITY).Select(x => x.Value).ToList() };
            set => ReloadGlobalConfigurations(value);
        }
        public void ReloadGlobalConfigurations() => ReloadGlobalConfigurations(null);
        private void ReloadGlobalConfigurations(ListWrapper<AdrHighwayParentXml> fromCity)
        {
            m_savedDescriptorsSerialized = fromCity?.listVal?.Select(x => { x.m_configurationSource = ConfigurationSource.CITY; return x; }).ToDictionary(x => x.SaveName, x => x) ?? m_savedDescriptorsSerialized.Where(x => x.Value.m_configurationSource == ConfigurationSource.CITY).ToDictionary(x => x.Key, x => x.Value);
            var errorList = new List<string>();
            foreach (string filename in Directory.GetFiles(AddressesMod.HighwayConfigurationFolder, "*.xml").OrderBy(x => !x.EndsWith(AddressesMod.DefaultFileGlobalXml)).ThenBy(x => x))
            {
                try
                {
                    if (CommonProperties.DebugMode)
                    {
                        LogUtils.DoLog($"Trying deserialize {filename}:\n{File.ReadAllText(filename)}");
                    }
                    using (FileStream stream = File.OpenRead(filename))
                    {
                        LoadDescriptorsFromXml(stream, filename);
                    }
                }
                catch (Exception e)
                {
                    LogUtils.DoWarnLog($"Error Loading file \"{filename}\" ({e.GetType()}): {e.Message}\n{e}");
                    errorList.Add($"Error Loading file \"{filename}\" ({e.GetType()}): {e.Message}");
                }
            }

            if (errorList.Count > 0)
            {
                K45DialogControl.ShowModal(new K45DialogControl.BindProperties
                {
                    title = "Addresses - Errors loading Highway prefab files",
                    message = string.Join("\r\n", errorList.ToArray()),
                    useFullWindowWidth = true,
                    showButton1 = true,
                    textButton1 = "Okay...",
                    showClose = true

                }, (x) => true);
            }
        }


        private void LoadDescriptorsFromXml(FileStream stream, string filename)
        {
            var serializer = new XmlSerializer(typeof(ListWrapper<AdrHighwayParentXml>));
            if (serializer.Deserialize(stream) is ListWrapper<AdrHighwayParentXml> config)
            {
                var result = new List<AdrHighwayParentXml>();
                foreach (var item in config.listVal)
                {
                    item.m_configurationSource = ConfigurationSource.GLOBAL;
                    result.Add(item);
                }
                m_savedDescriptorsSerialized = m_savedDescriptorsSerialized.Select(x => x.Value).Union(result)
                .GroupBy(p => p.SaveName)
                .Select(g => g.OrderBy(x => -1 * (int)x.m_configurationSource).First())
                .ToDictionary(x => x.SaveName, x => x);
            }
            else
            {
                LogUtils.DoErrorLog($"The file wasn't recognized as a valid descriptor! ({filename})");
            }
        }

        public void FilterBy(string input, Wrapper<string[]> result) =>
         result.Value = m_savedDescriptorsSerialized
         .Where((x) => (input.IsNullOrWhiteSpace() ? true : LocaleManager.cultureInfo.CompareInfo.IndexOf(x.Key, input, CompareOptions.IgnoreCase) >= 0))
         .OrderBy((x) => ((int)(3 - x.Value.m_configurationSource)) + x.Key)
         .Select(x => x.Key)
         .ToArray();

        public override void LoadDefaults(ISerializableData serializableData) => ReloadGlobalConfigurations();
    }
}

