﻿using ColossalFramework;
using Klyte.Addresses.ModShared;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using System.Xml.Serialization;
using UnityEngine;
using static Klyte.Commons.Utils.SegmentUtils;

namespace Klyte.Addresses.Xml
{
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
                AdrFacade.TriggerHighwaySeedChanged((ushort)Id);
            }
        }

        [XmlAttribute("highwayIdentifier")]
        public string HighwayIdentifier
        {
            get => highwayIdentifier; set
            {
                highwayIdentifier = value;
                AdrFacade.TriggerHighwaySeedChanged((ushort)Id);
            }
        }

        [XmlAttribute("forcedName")]
        public string ForcedName
        {
            get => forcedName; set
            {
                forcedName = value;
                AdrFacade.TriggerHighwaySeedChanged((ushort)Id);
            }
        }

        [XmlAttribute("mileageOffset")]
        public uint MileageOffset
        {
            get => mileageOffset; set
            {
                mileageOffset = value;
                AdrFacade.TriggerHighwaySeedChanged((ushort)Id);
            }
        }

        [XmlAttribute("mileageStartSrc")]
        public MileageStartSource MileageStartSrc
        {
            get => mileageStartSource; set
            {
                mileageStartSource = value;
                AdrFacade.TriggerHighwaySeedChanged((ushort)Id);
            }
        }

        [XmlAttribute("highwayAxis")]
        public MileageStartSource HighwayAxis
        {
            get => highwayAxis; set
            {
                highwayAxis = value;
                AdrFacade.TriggerHighwaySeedChanged((ushort)Id);
            }
        }

        [XmlIgnore]
        public Color HighwayColor { get => m_cachedHighwayColor; set => SetHighwayColor(value); }
        [XmlIgnore]
        private Color m_cachedHighwayColor;
        private string highwayIdentifier;
        private string forcedName;
        private uint mileageOffset;
        private MileageStartSource mileageStartSource;
        private MileageStartSource highwayAxis;
        private ushort id;

        [XmlAttribute("color")]
        public string HighwayColorStr { get => m_cachedHighwayColor == default ? null : ColorExtensions.ToRGB(HighwayColor); set => SetHighwayColor(value.IsNullOrWhiteSpace() ? default : (Color)ColorExtensions.FromRGB(value)); }

        [XmlAttribute("seedId")]
        public long? Id { get => id; set => id = (ushort)(value ?? 0); }

        internal void CleanCacheParent()
        {
            highwayParentName = HighwayParent?.SaveName;
            cachedParent = null;
        }

        private void SetHighwayColor(Color c)
        {
            m_cachedHighwayColor = c;
            AdrFacade.TriggerHighwaySeedChanged(id);
        }
    }
}

