using ColossalFramework.Globalization;
using Klyte.Addresses.ModShared;
using Klyte.Addresses.UI;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using static Klyte.Commons.Utils.XmlUtils;

namespace Klyte.Addresses.Xml
{
    public class AdrHighwayParentXml : ILibable
    {
        private string m_dettachedPrefix;
        private string m_shortPrefix;
        private string m_fullPrefix;
        private bool m_invertShortConcatenationOrder;
        private bool m_invertLongConcatenationOrder;
        private bool m_addSpaceBetweenTermsShort;
        private bool m_addSpaceBetweenTermsLong;

        [XmlIgnore]
        internal ConfigurationSource m_configurationSource = ConfigurationSource.CITY;

        [XmlAttribute("dettachedPrefix")]
        public string DettachedPrefix
        {
            get => m_dettachedPrefix; set
            {
                m_dettachedPrefix = value;
                AdrShared.TriggerHighwaysChanged();
            }
        }
        [XmlAttribute("shortPrefix")]
        public string ShortPrefix
        {
            get => m_shortPrefix; set
            {
                m_shortPrefix = value;
                AdrShared.TriggerHighwaysChanged();
            }
        }
        [XmlAttribute("fullPrefix")]
        public string LongPrefix
        {
            get => m_fullPrefix; set
            {
                m_fullPrefix = value;
                AdrShared.TriggerHighwaysChanged();
            }
        }
        [XmlAttribute("invertShort")]
        public bool InvertShortConcatenationOrder
        {
            get => m_invertShortConcatenationOrder; set
            {
                m_invertShortConcatenationOrder = value;
                AdrShared.TriggerHighwaysChanged();
            }
        }
        [XmlAttribute("invertLong")]
        public bool InvertLongConcatenationOrder
        {
            get => m_invertLongConcatenationOrder; set
            {
                m_invertLongConcatenationOrder = value;
                AdrShared.TriggerHighwaysChanged();
            }
        }

        [XmlAttribute("id")]
        public string SaveName { get; set; }
        [XmlAttribute("spaceOnShort")]
        public bool AddSpaceBetweenTermsShort
        {
            get => m_addSpaceBetweenTermsShort; set
            {
                m_addSpaceBetweenTermsShort = value;
                AdrShared.TriggerHighwaysChanged();
            }
        }
        [XmlAttribute("spaceOnLong")]
        public bool AddSpaceBetweenTermsLong
        {
            get => m_addSpaceBetweenTermsLong; set
            {
                m_addSpaceBetweenTermsLong = value;
                AdrShared.TriggerHighwaysChanged();
            }
        }

        public string GetShortValue(string id) => InvertShortConcatenationOrder ? $"{id}{(m_addSpaceBetweenTermsShort ? " " : "")}{m_shortPrefix}" : $"{m_shortPrefix}{(m_addSpaceBetweenTermsShort ? " " : "")}{id}";
        public string GetLongValue(string id) => InvertLongConcatenationOrder ? $"{id}{(m_addSpaceBetweenTermsLong ? " " : "")}{m_fullPrefix}" : $"{m_fullPrefix}{(m_addSpaceBetweenTermsLong ? " " : "")}{id}";

        public void ExportAsGlobal()
        {
            FileUtils.EnsureFolderCreation(AddressesMod.HighwayConfigurationFolder);

            ListWrapper<AdrHighwayParentXml> currentFile = File.Exists(AddressesMod.HighwayConfigurationDefaultGlobalFile)
                ? DefaultXmlDeserialize<ListWrapper<AdrHighwayParentXml>>(File.ReadAllText(AddressesMod.HighwayConfigurationDefaultGlobalFile), (x, y) => currentFile = new ListWrapper<AdrHighwayParentXml>())
                : new ListWrapper<AdrHighwayParentXml>();

            var targetLayoutName = SaveName;
            var exportableLayouts = new ListWrapper<AdrHighwayParentXml>
            {
                listVal = currentFile.listVal
                .Where(x => x.SaveName != targetLayoutName)
                .Union(new AdrHighwayParentXml[] { CloneViaXml(this) }.Select(x => { x.SaveName = targetLayoutName; return x; }))
                .ToList()
            };
            File.WriteAllText(AddressesMod.HighwayConfigurationDefaultGlobalFile, DefaultXmlSerialize(exportableLayouts));

            AdrHighwayParentEditorTab.Instance.OnRefresh();

            K45DialogControl.ShowModal(
              new K45DialogControl.BindProperties
              {
                  title = AddressesMod.Instance.SimpleName,
                  message = string.Format(Locale.Get("K45_ADR_SAVESUCCESSFULLAT"), AddressesMod.HighwayConfigurationDefaultGlobalFile),
                  showButton1 = true,
                  textButton1 = Locale.Get("K45_CMNS_GOTO_FILELOC"),
                  showButton2 = true,
                  textButton2 = Locale.Get("EXCEPTION_OK"),
              }
           , (x) =>
           {
               if (x == 1)
               {
                   ColossalFramework.Utils.OpenInFileBrowser(AddressesMod.HighwayConfigurationDefaultGlobalFile);
                   return false;
               }
               return true;
           });

        }
    }
}

