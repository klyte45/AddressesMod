using ColossalFramework;
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
        [XmlAttribute("fixedName")]
        public string FixedName
        {
            get => m_fixedName;
            set
            {
                m_fixedName = value.IsNullOrWhiteSpace() ? null : value.Trim();
                AdrNeighborConfigTab.Instance?.MarkDirty();
            }
        }
        [XmlIgnore]
        private uint m_seed;
        [XmlIgnore]
        private string m_fixedName;
        [XmlIgnore]
        private ushort m_azimuth;
    }
}

