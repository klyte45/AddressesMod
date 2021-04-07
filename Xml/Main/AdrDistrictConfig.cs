using ColossalFramework;
using Klyte.Addresses.ModShared;
using Klyte.Commons.Utils;
using System;
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
        public AdrRoadQualifierConfig RoadConfig { get; set; } = new AdrRoadQualifierConfig();

        [XmlIgnore]
        public ushort PostalCodePrefix
        {
            get => m_postalCodePrefix ?? (byte)(Id == 0 ? 0 : 128 - Id); set
            {
                m_postalCodePrefix = value;
                AdrFacade.TriggerPostalCodeChanged();
            }
        }

        [XmlAttribute("zipcodePrefix")]
        [Obsolete("Serialization only!", true)]
        public ushort? StoredPostalCodePrefix
        {
            get => m_postalCodePrefix; set
            {
                m_postalCodePrefix = value;
                AdrFacade.TriggerPostalCodeChanged();

            }
        }

        [XmlIgnore]
        public bool IsDefaultCode => m_postalCodePrefix == null;
        private ushort? m_postalCodePrefix;

        [XmlIgnore]
        public Color DistrictColor { get => m_cachedColor; set => SetDistrictColor(value); }
        [XmlIgnore]
        private Color m_cachedColor;

        [XmlAttribute("color")]
        public string DistrictColorStr { get => m_cachedColor == default ? null : ColorExtensions.ToRGB(DistrictColor); set => SetDistrictColor(value.IsNullOrWhiteSpace() ? default : (Color)ColorExtensions.FromRGB(value)); }

        private void SetDistrictColor(Color c)
        {
            m_cachedColor = c;
            AdrFacade.TriggerDistrictChanged();
        }
        public void ResetDistrictCode()
        {
            m_postalCodePrefix = null;
            AdrFacade.TriggerDistrictChanged();
            AdrFacade.TriggerPostalCodeChanged();
        }
    }
}

