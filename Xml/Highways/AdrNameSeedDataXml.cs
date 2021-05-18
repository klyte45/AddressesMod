using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Klyte.Addresses.Xml
{
    [XmlRoot("NameSeedXml")]
    public class AdrNameSeedDataXml : DataExtensionBase<AdrNameSeedDataXml>
    {

        [XmlElement("Seeds")]
        public NonSequentialList<AdrNameSeedConfig> NameSeedConfigs
        {
            get => m_nameSeedConfigs;

            set => m_nameSeedConfigs = value;
        }

        [XmlIgnore]
        public override string SaveId => "K45_ADR_NAMESEED";

        [XmlIgnore]
        private NonSequentialList<AdrNameSeedConfig> m_nameSeedConfigs = new NonSequentialList<AdrNameSeedConfig>();
        private Dictionary<ushort, HashSet<ushort>> currentSavegameSeeds;
        [XmlIgnore]
        public Dictionary<ushort, HashSet<ushort>> CurrentSavegameSeeds
        {
            get
            {
                if (currentSavegameSeeds == null && NetManager.exists)
                {
                    currentSavegameSeeds = NetManager.instance.m_segments.m_buffer.Select((x, i) => Tuple.New(x.m_nameSeed, i)).GroupBy(x => x.First).ToDictionary(x => x.Key, x => new HashSet<ushort>(x.Select(y => (ushort)y.Second)));
                }
                return currentSavegameSeeds;
            }
        }

        public void EraseSeedCache() => currentSavegameSeeds = null;

        public void EnsureValidSeedsOnly()
        {
            var values = m_nameSeedConfigs.Where(x => !CurrentSavegameSeeds.ContainsKey((ushort)x.Key)).Select(x => x.Value).ToArray();
            foreach (var item in values)
            {
                _ = m_nameSeedConfigs - item;
            }
        }

        public override void BeforeSerialize()
        {
            base.BeforeSerialize();
            EraseSeedCache();
            EnsureValidSeedsOnly();
        }

        internal void EraseParentCaches()
        {
            foreach (var x in m_nameSeedConfigs)
            {
                x.Value.CleanCacheParent();
            }
        }
        public override void LoadDefaults() => base.LoadDefaults();
    }
}

