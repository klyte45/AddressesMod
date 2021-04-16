using System.Xml.Serialization;

namespace Klyte.Addresses.Xml
{
    public class AdrRicoNamesGenerationConfig
    {
        [XmlAttribute("industry")]
        public GenerationMethod Industry { get; set; }
        [XmlAttribute("commerce")]
        public GenerationMethod Commerce { get; set; }
        [XmlAttribute("office")]
        public GenerationMethod Office { get; set; }
        [XmlAttribute("residential")]
        public GenerationMethod Residence { get; set; }

        public enum GenerationMethod
        {
            NONE,
            ADDRESS
        }
    }
}

