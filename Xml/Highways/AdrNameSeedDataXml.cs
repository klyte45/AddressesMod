using ColossalFramework;
using Klyte.Addresses.ModShared;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

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
            foreach (var item in m_nameSeedConfigs.Where(x => !CurrentSavegameSeeds.ContainsKey((ushort)x.Key)))
            {
                _ = m_nameSeedConfigs - item.Value;
            }
        }

        public override void BeforeSerialize()
        {
            base.BeforeSerialize();
            EraseSeedCache();
            EnsureValidSeedsOnly();
        }

        private void EraseParentCaches()
        {
            foreach (var x in m_nameSeedConfigs)
            {
                x.Value.CleanCacheParent();
            }
        }
        public override void LoadDefaults()
        {
            base.LoadDefaults();
            AdrShared.Instance.EventHighwaysChanged += EraseParentCaches;
        }
    }

    public class AdrNameSeedConfig : IIdentifiable
    {
        private string highwayParentName;
        private AdrHighwayParentXml cachedParent;

        [XmlIgnore]
        public AdrHighwayParentXml HighwayParent
        {
            get
            {
                if (cachedParent == null && highwayParentName != null)
                {
                    cachedParent = AdrHighwayParentLibDataXml.Instance.Get(highwayParentName);
                    if (cachedParent == null)
                    {
                        highwayParentName = null;
                    }
                }
                return cachedParent;
            }
        }


        [XmlAttribute("highwayParent")]
        public string HighwayParentName
        {
            get => HighwayParent?.SaveName;
            set
            {
                highwayParentName = value;
                cachedParent = null;
                AdrShared.TriggerHighwaySeedChanged((ushort)Id);
            }
        }

        [XmlAttribute("highwayIdentifier")]
        public string HighwayIdentifier
        {
            get => highwayIdentifier; set
            {
                highwayIdentifier = value;
                AdrShared.TriggerHighwaySeedChanged((ushort)Id);
            }
        }

        [XmlAttribute("forcedName")]
        public string ForcedName
        {
            get => forcedName; set
            {
                forcedName = value;
                AdrShared.TriggerHighwaySeedChanged((ushort)Id);
            }
        }

        [XmlAttribute("mileageOffset")]
        public uint MileageOffset
        {
            get => mileageOffset; set
            {
                mileageOffset = value;
                AdrShared.TriggerHighwaySeedChanged((ushort)Id);
            }
        }

        [XmlAttribute("invertMileageStart")]
        public bool InvertMileageStart
        {
            get => invertMileageStart; set
            {
                invertMileageStart = value;
                AdrShared.TriggerHighwaySeedChanged((ushort)Id);
            }
        }

        [XmlIgnore]
        public Color HighwayColor { get => m_cachedHighwayColor; set => SetHighwayColor(value); }
        [XmlIgnore]
        private Color m_cachedHighwayColor;
        private string highwayIdentifier;
        private string forcedName;
        private uint mileageOffset;
        private bool invertMileageStart;

        [XmlAttribute("color")]
        public string HighwayColorStr { get => m_cachedHighwayColor == default ? null : ColorExtensions.ToRGB(HighwayColor); set => SetHighwayColor(value.IsNullOrWhiteSpace() ? default : (Color)ColorExtensions.FromRGB(value)); }

        [XmlAttribute("seedId")]
        public long? Id { get; set; }

        internal void CleanCacheParent()
        {
            highwayParentName = HighwayParent?.SaveName;
            cachedParent = null;
        }

        private void SetHighwayColor(Color c)
        {
            m_cachedHighwayColor = c;
            AdrShared.TriggerHighwaySeedChanged((ushort)Id);
        }
    }
}

