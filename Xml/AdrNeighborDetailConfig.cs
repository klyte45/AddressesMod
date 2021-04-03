using Klyte.Addresses.UI;
using System.Xml.Serialization;

namespace Klyte.Addresses.Xml
{
    public class AdrNeighborDetailConfig
    {
        [XmlAttribute("nameSeed")]
        public uint Seed
        {
            get => m_seed;
            set
            {
                m_seed = value;
                AdrNeighborConfigTab.Instance?.MarkDirty();
            }
        }
        [XmlAttribute("azimuth")]
        public ushort Azimuth
        {
            get => m_azimuth;
            set
            {
                m_azimuth = value;
                AdrNeighborConfigTab.Instance?.MarkDirty();
            }
        }

        [XmlIgnore]
        private uint m_seed;
        [XmlIgnore]
        private ushort m_azimuth;
    }
}

