using Klyte.Addresses.ModShared;
using Klyte.Addresses.Utils;
using Klyte.Commons.Utils;
using System;
using System.Xml.Serialization;

namespace Klyte.Addresses.Xml
{
    [XmlRoot("adrAddressingConfig")]
    public class AdrAddressingConfig
    {
        [XmlAttribute("zipcodeFormat")]
        [Obsolete("Legacy < 3.0",true)]
        public string Old_zipCodeFormat
        {
            get => null; set => PostalCodeFormat = ConvertFromV2ToV3PostalCode(value);
        }

        [XmlAttribute("postalCodeFormat")]
        public string PostalCodeFormat
        {
            get => TokenizedPostalCodeFormat.AsPostalCodeString(); set
            {
                TokenizedPostalCodeFormat = value.TokenizeAsPostalCode();
                AdrFacade.TriggerPostalCodeChanged();
            }
        }
        [XmlAttribute("zipcodeCityPrefix")]
        public int ZipcodeCityPrefix
        {
            get => m_zipcodeCityPrefix; set
            {
                m_zipcodeCityPrefix = value;
                AdrFacade.TriggerPostalCodeChanged();
            }
        }

        [XmlElement("addressLine1")]
        public string AddressLine1
        {
            get => m_addressLine1; set
            {
                m_addressLine1 = value;
                AdrFacade.TriggerPostalCodeChanged();
            }
        }
        [XmlElement("addressLine2")]
        public string AddressLine2
        {
            get => m_addressLine2; set
            {
                m_addressLine2 = value;
                AdrFacade.TriggerPostalCodeChanged();
            }
        }
        [XmlElement("addressLine3")]
        public string AddressLine3
        {
            get => m_addressLine3; set
            {
                m_addressLine3 = value;
                AdrFacade.TriggerPostalCodeChanged();
            }
        }
        [XmlElement("districts")]
        public AdrDistrictQualifierConfig DistrictsConfig { get; set; } = new AdrDistrictQualifierConfig();

        [XmlAttribute("zeroMarkBuildingId")]
        public ushort ZeroMarkBuilding
        {
            get => m_savedZeroMarkBuilding; set
            {
                if (m_savedZeroMarkBuilding != value)
                {
                    AdrFacade.TriggerZeroMarkerBuildingChange();
                }
                m_savedZeroMarkBuilding = value;
            }
        }

        private const string DEFAULT_POSTAL_CODE_FORMAT = "LMNOP-BCE";

        [XmlIgnore]
        private ushort m_savedZeroMarkBuilding;
        [XmlIgnore]
        public PostalCodeTokenContainer[] TokenizedPostalCodeFormat { get; private set; } = DEFAULT_POSTAL_CODE_FORMAT.TokenizeAsPostalCode();

        private string m_addressLine1 = "B A";
        private string m_addressLine2 = "[D - ]C";
        private string m_addressLine3 = "E";
        private int m_zipcodeCityPrefix;

        private string ConvertFromV2ToV3PostalCode(string v2)
        {
            var result = "";
            foreach (var letter in v2)
            {
                switch (letter)
                {
                    case 'A': result += "OP"; break;
                    case 'B': result += "NOP"; break;
                    case 'C': result += "X"; break;
                    case 'D': result += "Y"; break;
                    case 'E': result += "x"; break;
                    case 'F': result += "y"; break;
                    case 'G': result += "M"; break;
                    case 'H': result += "LM"; break;
                    case 'I': result += "KLM"; break;
                    case 'J': result += "J"; break;
                    case 'K': result += "IJ"; break;
                    case 'L': result += "HIJ"; break;
                    default: result += $"{("MNOPXxYy\\\"".Contains(letter.ToString()) ? "\\" : "")}{letter}"; break;
                }
            }
            LogUtils.DoLog($"Convert CEP: from {v2} => {result}");
            return result;
        }
    }
}

