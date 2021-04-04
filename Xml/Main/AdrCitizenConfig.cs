using System.Xml.Serialization;

namespace Klyte.Addresses.Xml
{
    [XmlRoot("adrCitizenConfig")]
    public class AdrCitizenConfig
    {
        [XmlAttribute("maleNamesFile")]
        public string MaleNamesFile { get; set; }

        [XmlAttribute("femleNamesFile")]
        public string FemaleNamesFile { get; set; }

        [XmlAttribute("surnamesFile")]
        public string SurnamesFile { get; set; }

    }
}

