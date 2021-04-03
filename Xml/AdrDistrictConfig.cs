using ColossalFramework;
using Klyte.Addresses.ModShared;
using Klyte.Commons.Utils;
using System.Xml.Serialization;
using UnityEngine;

namespace Klyte.Addresses.Xml
{
    [XmlRoot("adrDistrictConfig")]
    public class AdrDistrictConfig
    {
        [XmlAttribute("id")]
        public ushort Id { get; set; }

        [XmlElement("roads")]
        public AdrGeneralQualifierConfig RoadConfig { get; set; } = new AdrGeneralQualifierConfig();

        [XmlAttribute("zipcodePrefix")]
        public int? ZipcodePrefix
        {
            get => m_zipcodePrefix; set
            {
                m_zipcodePrefix = value;
                AdrShared.TriggerPostalCodeChanged();
            }
        }

        [XmlIgnore]
        public Color DistrictColor { get => m_cachedColor; set => SetDistrictColor(value); }
        [XmlIgnore]
        private Color m_cachedColor;
        private int? m_zipcodePrefix;

        [XmlAttribute("color")]
        public string DistrictColorStr { get => m_cachedColor == default ? null : ColorExtensions.ToRGB(DistrictColor); set => SetDistrictColor(value.IsNullOrWhiteSpace() ? default : (Color)ColorExtensions.FromRGB(value)); }

        private void SetDistrictColor(Color c)
        {
            m_cachedColor = c;
            AdrShared.TriggerDistrictChanged();
        }
    }
}

