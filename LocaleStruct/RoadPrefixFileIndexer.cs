using ColossalFramework.Math;
using Klyte.Addresses.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Klyte.Addresses.LocaleStruct
{
    internal class RoadPrefixFileIndexer
    {
        List<RoadPrefixFileItem> prefixes = new List<RoadPrefixFileItem>();

        private RoadPrefixFileIndexer() { }

        public object Length => prefixes.Count;

        public static RoadPrefixFileIndexer Parse(string[] file)
        {
            RoadPrefixFileIndexer result = new RoadPrefixFileIndexer();
            foreach (var line in file)
            {
                var item = RoadPrefixFileItem.Parse(line);
                if (item != null)
                {
                    result.prefixes.Add(item);
                }
            }
            return result;
        }

        public string getPrefix(PrefabAI ai, bool isOneWay, bool isSymmetric, float width, byte lanes, Randomizer rand)
        {
            Highway highway = Highway.ANY;
            RoadType type = RoadType.NONE;
            if (ai is RoadBaseAI baseAi)
            {
                if (baseAi.WorksAsBuilding())
                {
                    type = RoadType.DAM;
                }
                else if (baseAi.BuildOnWater())
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
                AdrUtils.doLog($"AI NAO IDENTIFICADA: {ai}");
                return null;
            }

            OneWay wayVal = isOneWay ? OneWay.TRUE : OneWay.FALSE;
            Symmetry symVal = isSymmetric ? Symmetry.TRUE : Symmetry.FALSE;
            var filterResult = prefixes.Where(x =>
                (x.roadType & type) != 0
                && (x.oneWay & wayVal) != 0
                && (x.symmetry & symVal) != 0
                && (x.highway & highway) != 0
                && x.minWidth <= width + 0.99f
                && width + 0.99f < x.maxWidth
                && x.minLanes <= lanes
                && lanes < x.maxLanes
            ).ToList();
            //AdrUtils.doLog($"Results for: {type} O:{wayVal} S:{symVal} H:{highway} W:{width} L:{lanes} = {filterResult?.Count}");
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
                    name = kv[1]
                };

                if (kv[0].Contains("h"))
                {
                    item.highway = Highway.TRUE;
                }
                else if (kv[0].Contains("H"))
                {
                    item.highway = Highway.FALSE;
                }


                if (kv[0].Contains("w"))
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

                var matchesRoad = Regex.Matches(kv[0], @"[GBTD]");
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
                var matchesWidth = Regex.Matches(kv[0], @"\[([0-9]{0,3}),([0-9]{0,3})\]");
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
                var matchesLanes = Regex.Matches(kv[0], @"\(([0-9]{0,3}),([0-9]{0,3})\)");
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
            public string name;
        }

        enum Symmetry : byte
        {
            ANY = 3,
            TRUE = 1,
            FALSE = 2
        }
        enum OneWay : byte
        {
            ANY = 3,
            TRUE = 1,
            FALSE = 2
        }
        enum Highway : byte
        {
            ANY = 3,
            TRUE = 1,
            FALSE = 2
        }
        enum RoadType : byte
        {
            NONE = 0,
            GROUND = 1,
            BRIDGE = 2,
            TUNNEL = 4,
            DAM = 8,
            ANY = GROUND | BRIDGE | TUNNEL | DAM
        }
    }

}
