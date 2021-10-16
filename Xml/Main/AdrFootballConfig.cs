using Klyte.Commons.Utils;
using System.Xml.Serialization;

namespace Klyte.Addresses.Xml
{
    [XmlRoot("adrFootballConfig")]
    public class AdrFootballConfig
    {
        [XmlAttribute("opponentNamesFile")]
        public string OpponentNamesFile { get; set; }
        [XmlElement("localTeams")]
        public SimpleNonSequentialList<string> localTeamsNames = new SimpleNonSequentialList<string>();
    }
}

