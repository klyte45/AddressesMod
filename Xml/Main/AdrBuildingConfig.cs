using System.Xml.Serialization;

namespace Klyte.Addresses.Xml
{
    [XmlRoot("adrBuildingConfig")]
    public class AdrBuildingConfig
    {
        [XmlElement("stationsNameGeneration")]
        public AdrStationNamesGenerationConfig StationsNameGenerationConfig { get; set; } = new AdrStationNamesGenerationConfig();

        [XmlElement("ricoNameGeneration")]
        public AdrRicoNamesGenerationConfig RicoNamesGenerationConfig { get; set; } = new AdrRicoNamesGenerationConfig();
    }
}

