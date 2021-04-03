using Klyte.Addresses.ModShared;
using System.Xml.Serialization;

namespace Klyte.Addresses.Xml
{
    [XmlRoot("adrAddressingConfig")]
    public class AdrAddressingConfig
    {
        [XmlAttribute("zipcodeFormat")]
        public string ZipcodeFormat
        {
            get => m_zipcodeFormat; set
            {
                m_zipcodeFormat = value;
                AdrShared.TriggerPostalCodeChanged();
            }
        }
        [XmlAttribute("zipcodeCityPrefix")]
        public int ZipcodeCityPrefix
        {
            get => m_zipcodeCityPrefix; set
            {
                m_zipcodeCityPrefix = value;
                AdrShared.TriggerPostalCodeChanged();
            }
        }

        [XmlElement("addressLine1")]
        public string AddressLine1
        {
            get => m_addressLine1; set
            {
                m_addressLine1 = value;
                AdrShared.TriggerPostalCodeChanged();
            }
        }
        [XmlElement("addressLine2")]
        public string AddressLine2
        {
            get => m_addressLine2; set
            {
                m_addressLine2 = value;
                AdrShared.TriggerPostalCodeChanged();
            }
        }
        [XmlElement("addressLine3")]
        public string AddressLine3
        {
            get => m_addressLine3; set
            {
                m_addressLine3 = value;
                AdrShared.TriggerPostalCodeChanged();
            }
        }
        [XmlElement("districts")]
        public AdrGeneralQualifierConfig DistrictsConfig { get; set; } = new AdrGeneralQualifierConfig();

        [XmlAttribute("zeroMarkBuildingId")]
        public ushort ZeroMarkBuilding
        {
            get => m_savedZeroMarkBuilding; set
            {
                if (m_savedZeroMarkBuilding != value)
                {
                    AdrShared.TriggerZeroMarkerBuildingChange();
                }
                m_savedZeroMarkBuilding = value;
            }
        }

        [XmlIgnore]
        private ushort m_savedZeroMarkBuilding;
        private string m_zipcodeFormat = "GCEDF-AJ";
        private string m_addressLine1 = "A, B";
        private string m_addressLine2 = "[D - ]C";
        private string m_addressLine3 = "E";
        private int m_zipcodeCityPrefix;
    }
}

