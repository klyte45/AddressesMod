using Klyte.Addresses.ModShared;
using Klyte.Commons.Interfaces;
using System.Xml.Serialization;

namespace Klyte.Addresses.Xml
{
    public class AdrHighwayParentXml : ILibable
    {
        private string m_shortPrefix;
        private string m_fullPrefix;
        private bool m_invertShortConcatenationOrder;
        private bool m_invertFullConcatenationOrder;
        private bool m_addSpaceBetweenTermsShort;
        private bool m_addSpaceBetweenTermsFull;

        [XmlIgnore]
        internal ConfigurationSource m_configurationSource = ConfigurationSource.CITY;

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
        public string FullPrefix
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
        public bool InvertFullConcatenationOrder
        {
            get => m_invertFullConcatenationOrder; set
            {
                m_invertFullConcatenationOrder = value;
                AdrShared.TriggerHighwaysChanged();
            }
        }

        [XmlAttribute("id")]
        public string SaveName { get; set; }
        public bool AddSpaceBetweenTermsShort
        {
            get => m_addSpaceBetweenTermsShort; set
            {
                m_addSpaceBetweenTermsShort = value;
                AdrShared.TriggerHighwaysChanged();
            }
        }
        public bool AddSpaceBetweenTermsFull
        {
            get => m_addSpaceBetweenTermsFull; set
            {
                m_addSpaceBetweenTermsFull = value;
                AdrShared.TriggerHighwaysChanged();
            }
        }

        public string GetShortValue(string id) => InvertShortConcatenationOrder ? $"{id}{(m_addSpaceBetweenTermsShort?" ":"")}{m_shortPrefix}" : $"{m_shortPrefix}{(m_addSpaceBetweenTermsShort ? " " : "")}{id}";
        public string GetLongValue(string id) => InvertFullConcatenationOrder ? $"{id}{(m_addSpaceBetweenTermsFull ? " " : "")}{m_fullPrefix}" : $"{m_fullPrefix}{(m_addSpaceBetweenTermsFull ? " " : "")}{id}";
    }
}

