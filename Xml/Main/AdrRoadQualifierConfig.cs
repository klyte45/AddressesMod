using Klyte.Addresses.ModShared;
using System.Xml.Serialization;

namespace Klyte.Addresses.Xml
{
    public class AdrRoadQualifierConfig
    {
        private string m_namesFile;
        private string m_qualifierFile;

        [XmlAttribute("namesFile")]
        public string NamesFile
        {
            get => m_namesFile; set
            {
                m_namesFile = value;
                AdrFacade.TriggerRoadNamingChange();
            }
        }

        [XmlAttribute("qualifierFile")]
        public string QualifierFile
        {
            get => m_qualifierFile; set
            {
                m_qualifierFile = value;
                AdrFacade.TriggerRoadNamingChange();
            }
        }
    }
}

