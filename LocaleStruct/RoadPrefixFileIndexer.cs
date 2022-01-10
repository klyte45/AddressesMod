using ColossalFramework;
using ColossalFramework.Math;
using Klyte.Addresses.Xml;
using Klyte.Commons.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static Klyte.Commons.Utils.SegmentUtils;

namespace Klyte.Addresses.LocaleStruct
{
    internal class RoadPrefixFileIndexer
    {
        private readonly List<RoadPrefixFileItem> m_prefixes = new List<RoadPrefixFileItem>();

        private RoadPrefixFileIndexer() { }

        public object Length => m_prefixes.Count;

        public static RoadPrefixFileIndexer Parse(string[] file)
        {
            RoadPrefixFileIndexer result = new RoadPrefixFileIndexer();
            foreach (string line in file)
            {
                RoadPrefixFileItem item = RoadPrefixFileItem.Parse(line);
                if (item != null)
                {
                    result.m_prefixes.Add(item);
                }
            }
            return result;
        }

        public string GetPrefix(PrefabAI ai, bool isOneWay, bool isSymmetric, float width, byte lanes, Randomizer rand, ushort segmentId, ushort seedId)
        {
            Highway highway = Highway.ANY;
            RoadType type = RoadType.NONE;
            if (ai is RoadBaseAI baseAi)
            {
                if (baseAi is DamAI)
                {
                    type = RoadType.DAM;
                }
                else if (baseAi is RoadBridgeAI)
                {
                    type = RoadType.BRIDGE;
                }
                else if (baseAi.IsUnderground())
                {
                    type = RoadType.TUNNEL;
                }
                else
                {
                    type = RoadType.GROUND;
                }
                highway = baseAi.m_highwayRules ? Highway.TRUE : Highway.FALSE;
            }
            if (type == RoadType.NONE)
            {
                LogUtils.DoLog($"AI NAO IDENTIFICADA: {ai}");
                return null;
            }

            OneWay wayVal = isOneWay ? OneWay.TRUE : OneWay.FALSE;
            Symmetry symVal = isSymmetric ? Symmetry.TRUE : Symmetry.FALSE;
            LinkingType linking = LinkingType.NO_LINKING;
            SeedConfiguration seedConfiguration = 
                (AdrNameSeedDataXml.Instance.NameSeedConfigs.TryGetValue(seedId, out AdrNameSeedConfig seedConf) && !(seedConf.HighwayParent is null) ? SeedConfiguration.HAS_HIGHWAY_TYPE : SeedConfiguration.NO_HIGHWAY_TYPE)
                | ((seedConf?.ForcedName).IsNullOrWhiteSpace()? SeedConfiguration.NO_FORCED_NAME : SeedConfiguration.HAS_FORCED_NAME);
            bool hasStart = true;
            bool hasEnd = true;
            if (wayVal == OneWay.TRUE && lanes <= 2)
            {
                SegmentUtils.GetSegmentRoadEdges(segmentId, true, true, true, out ComparableRoad startRef, out ComparableRoad endRef, out _, out _, out _);
                LogUtils.DoLog($"OneWay s={startRef}; e= {endRef}");
                if (startRef.segmentReference == 0 || endRef.segmentReference == 0)
                {
                    hasStart = startRef.segmentReference != 0;
                    hasEnd = endRef.segmentReference != 0;
                }
                else if ((NetManager.instance.m_segments.m_buffer[startRef.segmentReference].Info.GetAI() is RoadBaseAI baseAiSource && baseAiSource.m_highwayRules) ||
                  (NetManager.instance.m_segments.m_buffer[endRef.segmentReference].Info.GetAI() is RoadBaseAI baseAiTarget && baseAiTarget.m_highwayRules))

                {
                    switch (startRef.CompareTo(endRef))
                    {
                        case 1:
                            linking = LinkingType.FROM_BIG;
                            break;
                        case 0:
                            linking = LinkingType.SAME_SIZE;
                            break;
                        case -1:
                            linking = LinkingType.FROM_SMALL;
                            break;
                    }
                }
            }

            List<RoadPrefixFileItem> filterResult = m_prefixes.Where(x =>
                (x.roadType & type) == type
                && (x.oneWay & wayVal) == wayVal
                && (x.symmetry & symVal) == symVal
                && (x.highway & highway) == highway
                && (wayVal == OneWay.FALSE || ((x.linking & linking) == linking))
                && (hasStart || !x.requireSource)
                && (hasEnd || !x.requireTarget)
                && x.minWidth <= width + 0.99f
                && width + 0.99f < x.maxWidth
                && x.minLanes <= lanes
                && lanes < x.maxLanes
                && (x.seedConfig & seedConfiguration) == seedConfiguration
            ).ToList();
            if (filterResult.Count == 0 && linking != LinkingType.NO_LINKING)
            {
                filterResult = m_prefixes.Where(x =>
                   (x.roadType & type) == type
                   && (x.oneWay & wayVal) == wayVal
                   && (x.symmetry & symVal) == symVal
                   && (x.highway & highway) == highway
                   && (wayVal == OneWay.FALSE || ((x.linking & LinkingType.NO_LINKING) == LinkingType.NO_LINKING))
                   && (hasStart || !x.requireSource)
                   && (hasEnd || !x.requireTarget)
                   && x.minWidth <= width + 0.99f
                   && width + 0.99f < x.maxWidth
                   && x.minLanes <= lanes
                   && lanes < x.maxLanes
                   && (x.seedConfig & seedConfiguration) == seedConfiguration
               ).ToList();
            }
            LogUtils.DoLog($"Results for: {type} O:{wayVal} S:{symVal} H:{highway} W:{width} L:{lanes} Lk:{linking} Hs:{hasStart} He:{hasEnd} Sc:{seedConfiguration} = {filterResult?.Count}");
            if (filterResult?.Count == 0)
            {
                return null;
            }

            return filterResult[rand.Int32((uint)(filterResult.Count))].name;
        }



        private class RoadPrefixFileItem
        {

            public static RoadPrefixFileItem Parse(string line)
            {
                string[] kv = line?.Split('=');
                if (kv?.Length != 2)
                {
                    return null;
                }
                RoadPrefixFileItem item = new RoadPrefixFileItem
                {
                    name = kv[1]?.Trim()
                };

                if (kv[0].Contains("h"))
                {
                    item.highway = Highway.TRUE;
                }
                else if (kv[0].Contains("H"))
                {
                    item.highway = Highway.FALSE;
                }

                MatchCollection matchesLinkingType = Regex.Matches(kv[0], @"[fie]");
                if (matchesLinkingType?.Count > 0)
                {
                    item.oneWay = OneWay.TRUE;

                    item.linking = LinkingType.NONE;
                    foreach (Match m in matchesLinkingType)
                    {
                        switch (m.Value)
                        {
                            case "f":
                                item.linking |= LinkingType.FROM_BIG;
                                break;
                            case "i":
                                item.linking |= LinkingType.FROM_SMALL;
                                break;
                            case "e":
                                item.linking |= LinkingType.SAME_SIZE;
                                break;
                        }
                    }
                }
                else
                {
                    item.linking = LinkingType.NO_LINKING;
                    if (kv[0].Contains("w") || kv[1].Contains("{1}") || kv[1].Contains("{2}") || kv[1].Contains("{3}") || kv[1].Contains("{4}") || kv[1].Contains("{7}"))
                    {
                        item.oneWay = OneWay.TRUE;

                    }
                    else if (kv[0].Contains("W"))
                    {
                        item.oneWay = OneWay.FALSE;
                    }

                    if ((item.oneWay & OneWay.FALSE) != 0)
                    {
                        if (kv[0].Contains("s"))
                        {
                            item.symmetry = Symmetry.TRUE;
                        }
                        else if (kv[0].Contains("S"))
                        {
                            item.symmetry = Symmetry.FALSE;
                        }
                    }
                }
                item.requireSource = kv[1].Contains("{1}") || kv[1].Contains("{3}") || kv[1].Contains("{4}");
                item.requireTarget = kv[1].Contains("{2}");

                MatchCollection matchesRoad = Regex.Matches(kv[0], @"[GBTD]");
                if (matchesRoad?.Count > 0)
                {
                    item.roadType = RoadType.NONE;
                    foreach (Match m in matchesRoad)
                    {
                        switch (m.Value)
                        {
                            case "G":
                                item.roadType |= RoadType.GROUND;
                                break;
                            case "B":
                                item.roadType |= RoadType.BRIDGE;
                                break;
                            case "T":
                                item.roadType |= RoadType.TUNNEL;
                                break;
                            case "D":
                                item.roadType |= RoadType.DAM;
                                break;
                        }
                    }

                }
                MatchCollection matchesWidth = Regex.Matches(kv[0], @"\[([0-9]{0,3}),([0-9]{0,3})\]");
                if (matchesWidth?.Count > 0)
                {
                    Match m = matchesWidth[0];
                    if (ushort.TryParse(m.Groups[1].Value, out ushort minWidth))
                    {
                        item.minWidth = minWidth;
                    }
                    if (ushort.TryParse(m.Groups[2].Value, out ushort maxWidth))
                    {
                        item.maxWidth = maxWidth;
                    }
                }
                MatchCollection matchesWidthSingle = Regex.Matches(kv[0], @"\[([0-9]{0,3})\]");
                if (matchesWidthSingle?.Count > 0)
                {
                    Match m = matchesWidth[0];
                    if (ushort.TryParse(m.Groups[1].Value, out ushort minWidth))
                    {
                        item.minWidth = minWidth;
                        item.maxWidth = ++minWidth;
                    }
                }
                MatchCollection matchesLanes = Regex.Matches(kv[0], @"\(([0-9]{0,3}),([0-9]{0,3})\)");
                if (matchesLanes?.Count > 0)
                {
                    Match m = matchesLanes[0];
                    if (byte.TryParse(m.Groups[1].Value, out byte minLanes))
                    {
                        item.minLanes = minLanes;
                    }
                    if (byte.TryParse(m.Groups[2].Value, out byte maxLanes))
                    {
                        item.maxLanes = maxLanes;
                    }
                }
                MatchCollection matchesLanesSingle = Regex.Matches(kv[0], @"\(([0-9]{0,3})\)");
                if (matchesLanesSingle?.Count > 0)
                {
                    Match m = matchesLanesSingle[0];
                    if (byte.TryParse(m.Groups[1].Value, out byte minLanes))
                    {
                        item.minLanes = minLanes;
                        item.maxLanes = ++minLanes;
                    }
                }
                if (kv[0].Contains("p") || kv[1].Contains("{8}") || kv[1].Contains("{10}"))
                {
                    item.seedConfig = SeedConfiguration.HAS_HIGHWAY_TYPE;
                }
                else if (kv[0].Contains("P"))
                {
                    item.seedConfig = SeedConfiguration.NO_HIGHWAY_TYPE;
                }
                else
                {
                    item.seedConfig = SeedConfiguration.ANY_HIGHWAY_TYPE;
                }

                if (kv[0].Contains("n") || kv[1].Contains("{9}"))
                {
                    item.seedConfig |= SeedConfiguration.HAS_FORCED_NAME;
                }
                else if (kv[0].Contains("N"))
                {
                    item.seedConfig |= SeedConfiguration.NO_FORCED_NAME;
                }
                else
                {
                    item.seedConfig |= SeedConfiguration.ANY_FORCED_NAME;
                }

                return item;
            }

            public ushort minWidth = 0;
            public ushort maxWidth = 255;
            public Symmetry symmetry = Symmetry.ANY;
            public OneWay oneWay = OneWay.ANY;
            public byte minLanes = 0;
            public byte maxLanes = 255;
            public RoadType roadType = RoadType.ANY;
            public Highway highway = Highway.ANY;
            public LinkingType linking = LinkingType.ANY;
            public SeedConfiguration seedConfig = SeedConfiguration.ANY;
            public bool requireSource = false;
            public bool requireTarget = false;
            public string name;
        }

        private enum Symmetry : byte
        {
            ANY = 3,
            TRUE = 1,
            FALSE = 2
        }

        private enum OneWay : byte
        {
            ANY = 3,
            TRUE = 1,
            FALSE = 2
        }

        private enum Highway : byte
        {
            ANY = 3,
            TRUE = 1,
            FALSE = 2
        }

        private enum RoadType : byte
        {
            NONE = 0,
            GROUND = 1,
            BRIDGE = 2,
            TUNNEL = 4,
            DAM = 8,
            ANY = GROUND | BRIDGE | TUNNEL | DAM
        }

        private enum SeedConfiguration : byte
        {
            NONE = 0,
            HAS_HIGHWAY_TYPE = 1,
            NO_HIGHWAY_TYPE = 2,
            ANY_HIGHWAY_TYPE = 3,
            HAS_FORCED_NAME = 4,
            NO_FORCED_NAME = 8,
            ANY_FORCED_NAME = 12,
            ANY = ANY_HIGHWAY_TYPE | ANY_FORCED_NAME
        }

        private enum LinkingType : byte
        {
            NONE = 0,
            FROM_SMALL = 1,
            FROM_BIG = 2,
            SAME_SIZE = 4,
            NO_LINKING = 0x80,
            ANY = FROM_SMALL | FROM_BIG | SAME_SIZE,
        }
    }

}
