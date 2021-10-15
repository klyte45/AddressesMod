using System.Xml.Serialization;

namespace Klyte.Addresses.Xml
{
    [XmlRoot("adrGlobalConfig")]
    public class AdrGlobalConfig
    {
        [XmlElement("addressing")]
        public AdrAddressingConfig AddressingConfig { get; set; } = new AdrAddressingConfig();

        [XmlElement("neighbor")]
        public AdrNeighborhoodConfig NeighborhoodConfig { get; set; } = new AdrNeighborhoodConfig();

        [XmlElement("buildings")]
        public AdrBuildingConfig BuildingConfig { get; set; } = new AdrBuildingConfig();

        [XmlElement("citizen")]
        public AdrCitizenConfig CitizenConfig { get; set; } = new AdrCitizenConfig();

        [XmlElement("football")]
        public AdrFootballConfig FootballConfig { get; set; } = new AdrFootballConfig();
    }
}

