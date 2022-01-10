using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using ColossalFramework.UI;
using Klyte.Addresses.Tools;
using Klyte.Addresses.UI;
using Klyte.Addresses.Xml;
using Klyte.Commons;
using Klyte.Commons.Extensions;
using Klyte.Commons.Utils;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using static Klyte.Commons.Utils.SegmentUtils;

namespace Klyte.Addresses.Overrides
{
    internal class NetManagerOverrides : MonoBehaviour, IRedirectable
    {
        private static readonly MethodInfo m_roadBaseAiGenerateName = typeof(RoadBaseAI).GetMethod("GenerateStreetName", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.GetField | BindingFlags.GetProperty);

        #region Mod

        public static bool GenerateSegmentName(ushort segmentID, ref string __result)
        {
            List<ushort> path = new List<ushort>();
            return GenerateSegmentNameInternal(segmentID, ref __result, ref path, false);
        }

        public static bool GetStreetNameForStation(ushort segmentID, ref string __result)
        {
            List<ushort> path = new List<ushort>();
            return GenerateSegmentNameInternal(segmentID, ref __result, ref path, true);
        }

        private class NameVariablesContainer
        {
            public string genName = "";
            public string sourceRoad = "";
            public string targetRoad = "";
            public string sourceKm = "";
            public string sourceKmWithDecimal = "";
            public string sourceDistrict = "";
            public string targetDistrict = "";
            public string direction = "";
            public string longCodeHw = "";
            public string forcedNameHw = "";
            public string shortCodeHw = "";

            public string[] ToArray() => new string[]
                {
                    genName,
                    sourceRoad,
                    targetRoad,
                    sourceKm ,
                    sourceKmWithDecimal,
                    sourceDistrict ,
                    targetDistrict,
                    direction,
                    longCodeHw,
                    forcedNameHw,
                    shortCodeHw
                };
        }

        public static bool GenerateSegmentNameInternal(ushort segmentID, ref string __result, ref List<ushort> usedQueue, bool removePrefix)
        {
            LogUtils.DoLog($"[START {segmentID}]" + __result);
            if ((NetManager.instance.m_segments.m_buffer[segmentID].m_flags & NetSegment.Flags.CustomName) != 0)
            {
                if (usedQueue.Count == 0)
                {
                    return true;
                }
                else
                {
                    InstanceID id = default;
                    id.NetSegment = segmentID;
                    __result = Singleton<InstanceManager>.instance.GetName(id);
                    return false;
                }
            }
            ref NetSegment segment = ref NetManager.instance.m_segments.m_buffer[segmentID];
            NetInfo info = segment.Info;
            PrefabAI ai = info.GetAI();
            string format = GetRoadNameFormat(segmentID, removePrefix, ref segment, info, ai, out ushort district);

            if (format.IsNullOrWhiteSpace())
            {
                return true;
            }

            var variables = new NameVariablesContainer();

            if (format.Contains("{8}") || format.Contains("{9}") || format.Contains("{10}"))
            {
                var seed = segment.m_nameSeed;
                if (AdrNameSeedDataXml.Instance.NameSeedConfigs.TryGetValue(seed, out AdrNameSeedConfig seedConf))
                {
                    if (!(seedConf.HighwayParent is null))
                    {
                        if (format.Contains("{8}"))
                        {
                            variables.longCodeHw = seedConf.HighwayParent.GetLongValue(seedConf.HighwayIdentifier);
                        }
                        if (format.Contains("{10}"))
                        {
                            variables.shortCodeHw = seedConf.HighwayParent.GetShortValue(seedConf.HighwayIdentifier);
                        }
                    }
                    if (format.Contains("{9}"))
                    {
                        variables.forcedNameHw = seedConf.ForcedName;
                    }
                }
            }

            if (format.Contains("{0}"))
            {
                GetGeneratedRoadName(ref segment, ai, district, out variables.genName);
                if (variables.genName == null)
                {
                    return true;
                }
            }

            if (format.Contains("{1}") || format.Contains("{2}") || format.Contains("{3}") || format.Contains("{4}") || format.Contains("{7}"))
            {
                SegmentUtils.GetSegmentRoadEdges(segmentID, true, true, true, out ComparableRoad startRef, out ComparableRoad endRef, out ComparableRoad beforeStartRef, out ComparableRoad afterEndRef, out _);

                ushort sourceSeg = beforeStartRef.segmentReference;
                ushort targetSeg = afterEndRef.segmentReference;

                if (format.Contains("{1}"))
                {
                    if (!usedQueue.Contains(sourceSeg))
                    {
                        usedQueue.Add(sourceSeg);
                        GenerateSegmentNameInternal(sourceSeg, ref variables.sourceRoad, ref usedQueue, false);
                    }
                }
                if (format.Contains("{2}"))
                {
                    if (!usedQueue.Contains(targetSeg))
                    {
                        usedQueue.Add(targetSeg);
                        GenerateSegmentNameInternal(targetSeg, ref variables.targetRoad, ref usedQueue, false);
                    }
                }
                if (format.Contains("{3}") || format.Contains("{4}"))
                {
                    var seed = NetManager.instance.m_segments.m_buffer[beforeStartRef.nodeReference].m_nameSeed;
                    MileageStartSource src = MileageStartSource.DEFAULT;
                    var offsetMeters = 0;
                    if (AdrNameSeedDataXml.Instance.NameSeedConfigs.TryGetValue(seed, out AdrNameSeedConfig seedConf))
                    {
                        src = seedConf.MileageStartSrc;
                        offsetMeters = (int)seedConf.MileageOffset;
                    }

                    float km = GetNumberAt(NetManager.instance.m_segments.m_buffer[beforeStartRef.segmentReference].m_startNode == startRef.nodeReference ? 1 : 0, beforeStartRef.segmentReference, src, offsetMeters, out _) / 1000f;
                    variables.sourceKm = km.ToString("0");
                    variables.sourceKmWithDecimal = km.ToString("0.0");
                }
                if (format.Contains("{7}"))//direction
                {
                    var seed = segment.m_nameSeed;
                    MileageStartSource axis = MileageStartSource.DEFAULT;
                    if (AdrNameSeedDataXml.Instance.NameSeedConfigs.TryGetValue(seed, out AdrNameSeedConfig seedConf))
                    {
                        axis = seedConf.HighwayAxis;
                    }
                    int cardinalDirection = SegmentUtils.GetCardinalDirection(startRef, endRef, axis);

                    variables.direction = Locale.Get("K45_CARDINAL_POINT_SHORT", cardinalDirection.ToString());
                }
            }
            if (format.Contains("{5}") || format.Contains("{6}"))
            {
                GetSegmentRoadEdges(segmentID, false, false, false, out _, out _, out ComparableRoad startRef, out ComparableRoad endRef, out _);
                if (format.Contains("{5}"))//source district
                {
                    variables.sourceDistrict = GetDistrictAt(startRef);
                }
                if (format.Contains("{6}"))//target district
                {
                    variables.targetDistrict = GetDistrictAt(endRef);
                }
            }
            if (AddressesMod.DebugMode)
            {
                __result = $"[{segmentID}] " + StringUtils.SafeFormat(format, variables.ToArray())?.Trim();
            }
            else
            {
                __result = StringUtils.SafeFormat(format, variables.ToArray())?.Trim();
            }
            LogUtils.DoLog($"[END {segmentID}]" + __result);
            return false;

        }

        internal static string GetRoadNameFormat(ushort segmentID, bool removePrefix, ref NetSegment segment, NetInfo info, PrefabAI ai, out ushort district)
        {
            string format = null;
            var randomizer = new Randomizer(segment.m_nameSeed);
            district = (ushort)(DistrictManager.instance.GetDistrict(segment.m_middlePosition) & 0xFF);
            Xml.AdrDistrictConfig districtConfig = AdrController.CurrentConfig.GetConfigForDistrict(district);
            Xml.AdrDistrictConfig cityConfig = AdrController.CurrentConfig.GetConfigForDistrict(0);

            if ((info.m_vehicleTypes & VehicleInfo.VehicleType.Car) != VehicleInfo.VehicleType.None)
            {
                string filenamePrefix = districtConfig.RoadConfig?.QualifierFile ?? "";
                ;
                if ((filenamePrefix == null || !AdrController.LoadedLocalesRoadPrefix.ContainsKey(filenamePrefix)) && district > 0)
                {
                    filenamePrefix = cityConfig.RoadConfig?.QualifierFile;
                }

                if (filenamePrefix != null && AdrController.LoadedLocalesRoadPrefix.ContainsKey(filenamePrefix))
                {
                    LocaleStruct.RoadPrefixFileIndexer currentPrefixFile = AdrController.LoadedLocalesRoadPrefix[filenamePrefix];
                    format = currentPrefixFile.GetPrefix(
                        ai,
                        info.m_forwardVehicleLaneCount == 0 || info.m_backwardVehicleLaneCount == 0,
                        info.m_forwardVehicleLaneCount == info.m_backwardVehicleLaneCount, info.m_halfWidth * 2,
                        (byte)(info.m_forwardVehicleLaneCount + info.m_backwardVehicleLaneCount),
                        randomizer,
                        segmentID,
                        segment.m_nameSeed);
                }
                LogUtils.DoLog("selectedPrefix = {0}", format);
                if (format == null)
                {
                    string key = DefaultPrefix(info, ai);
                    uint rangeFormat = Locale.Count(key);
                    format = Locale.Get(key, randomizer.Int32(rangeFormat));
                }
            }

            if (removePrefix)
            {
                format = format?.Replace("\\]", "\0") ?? "";
                format = Regex.IsMatch(format, @"(?<=\[)(?<!\\\[).+?(?<!\\\])(?=\])")
                    ? Regex.Matches(format, @"(?<=\[)(?<!\\\[).+?(?<!\\\])(?=\])")[0].Groups[0].Value
                    : Regex.Replace(format ?? "", "(?!\\{)(\\w+|\\.)(?!\\})", "");
                format = Regex.Replace(format, @"(?<!\\)(\[|\])", "");
                format = Regex.Replace(format, @"(?<=\\\\)(\[|\])", "");
                format = Regex.Replace(format, @"(\\)(\[|\])", "$2");
                format = Regex.Replace(format, @"\\\\", "\\");
                format = format.Replace("\0", "]");
            }
            else
            {
                format = Regex.Replace(format ?? "", @"(?<!\\)(\[|\])", "");
                format = Regex.Replace(format, @"(?<=\\\\)(\[|\])", "");
                format = Regex.Replace(format, @"(\\)(\[|\])", "$2");
                format = Regex.Replace(format, @"\\\\", "\\");
            }

            return format;
        }

        private static string GetDistrictAt(ComparableRoad refer)
        {
            string sourceDistrict;
            NetNode node = NetManager.instance.m_nodes.m_buffer[refer.nodeReference];
            LogUtils.DoLog($"[Node {refer.nodeReference}] Flags => " + NetManager.instance.m_nodes.m_buffer[refer.nodeReference].m_flags);
            if ((NetManager.instance.m_nodes.m_buffer[refer.nodeReference].m_flags & NetNode.Flags.Outside) != 0)
            {
                float angle = Vector2.zero.GetAngleToPoint(VectorUtils.XZ(node.m_position));
                LogUtils.DoLog($"[Node {refer.nodeReference}] angle => {angle}, pos => {node.m_position} ");

                sourceDistrict = OutsideConnectionAIOverrides.GetNameBasedInAngle(angle, out _);
            }
            else
            {
                int districtId = Singleton<DistrictManager>.instance.GetDistrict(node.m_position);
                if (districtId == 0)
                {
                    sourceDistrict = SimulationManager.instance.m_metaData.m_CityName;
                }
                else
                {
                    sourceDistrict = DistrictManager.instance.GetDistrictName(districtId);
                }
            }

            return sourceDistrict;
        }

        private static void GetGeneratedRoadName(ref NetSegment segment, PrefabAI ai, ushort district, out string genName)
        {
            string filename = AdrController.CurrentConfig.GetConfigForDistrict(district).RoadConfig?.NamesFile ?? "";
            if (string.IsNullOrEmpty(filename) || !AdrController.LoadedLocalesRoadName.ContainsKey(filename))
            {
                filename = AdrController.CurrentConfig.GetConfigForDistrict(0).RoadConfig?.NamesFile;
                if (string.IsNullOrEmpty(filename) || !AdrController.LoadedLocalesRoadName.ContainsKey(filename))
                {
                    filename = null;
                }
            }

            Randomizer randomizer = new Randomizer(segment.m_nameSeed);
            if (filename != null)
            {
                int range = AdrController.LoadedLocalesRoadName[filename]?.Length ?? 0;
                if (range == 0)
                {
                    genName = null;
                    return;
                }
                genName = AdrController.LoadedLocalesRoadName[filename][randomizer.Int32((uint)range)];
            }
            else
            {
                genName = m_roadBaseAiGenerateName.Invoke(ai, new object[] { randomizer })?.ToString();
            }
        }

        private static string DefaultPrefix(NetInfo info, PrefabAI ai)
        {
            string text = null;
            if ((info.m_vehicleTypes & VehicleInfo.VehicleType.Car) != VehicleInfo.VehicleType.None)
            {
                if (ai is RoadTunnelAI roadTunnel)
                {
                    if ((!roadTunnel.m_highwayRules || info.m_forwardVehicleLaneCount + info.m_backwardVehicleLaneCount >= 2))
                    {
                        text = "TUNNEL_NAME_PATTERN";
                    }
                }

                if (ai is RoadAI roadAi)
                {
                    if (roadAi.m_enableZoning)
                    {
                        if ((info.m_setVehicleFlags & Vehicle.Flags.OnGravel) != 0)
                        {
                            text = "ROAD_NAME_PATTERN";
                        }
                        else if (info.m_halfWidth >= 12f)
                        {
                            text = "AVENUE_NAME_PATTERN";
                        }
                        else
                        {
                            text = "STREET_NAME_PATTERN";
                        }
                    }
                    else if (roadAi.m_highwayRules)
                    {
                        if (info.m_hasForwardVehicleLanes && info.m_hasBackwardVehicleLanes)
                        {
                            text = "ROAD_NAME_PATTERN";
                        }
                        else if (info.m_forwardVehicleLaneCount >= 2 || info.m_backwardVehicleLaneCount >= 2)
                        {
                            text = "HIGHWAY_NAME_PATTERN";
                        }
                    }
                }

                if (ai is DamAI)
                {
                    if ((info.m_vehicleTypes & VehicleInfo.VehicleType.Car) != VehicleInfo.VehicleType.None)
                    {
                        text = "ROAD_NAME_PATTERN";
                    }
                }

                if (ai is RoadBridgeAI bridgeAi)
                {
                    if ((info.m_vehicleTypes & VehicleInfo.VehicleType.Car) != VehicleInfo.VehicleType.None && (!bridgeAi.m_highwayRules || info.m_forwardVehicleLaneCount + info.m_backwardVehicleLaneCount >= 2))
                    {
                        text = "BRIDGE_NAME_PATTERN";
                    }
                }
            }
            return text;
        }

        public static void AfterEndOverlay(RenderManager.CameraInfo cameraInfo)
        {
            if (AdrHighwaySeedNameDataTab.Instance?.component?.isVisible == true)
            {
                if (AdrHighwaySeedNameDataTab.Instance.CurrentSeedId != 0
                && AdrNameSeedDataXml.Instance.CurrentSavegameSeeds.TryGetValue(AdrHighwaySeedNameDataTab.Instance.CurrentSeedId, out HashSet<ushort> segmentsSelected))
                {
                    var targetColor = new Color32(22, (byte)(127 + ((UIView.GetAView().framesRendered * 2) & 0x7f)), 22, 255);
                    foreach (var id in segmentsSelected)
                    {
                        RenderOverlayUtils.RenderNetSegmentOverlay(cameraInfo, AdrHighwaySeedNameDataTab.Instance.CurrentSourceSegmentId == id ? Color.yellow : (Color)targetColor, id);
                    }
                }

                if (ToolsModifierControl.toolController.CurrentTool is RoadSegmentTool rst
                    && rst.CurrentHoverSegment > 0
                    && NetManager.instance.m_segments.m_buffer[rst.CurrentHoverSegment].m_nameSeed != AdrHighwaySeedNameDataTab.Instance.CurrentSeedId
                    && AdrNameSeedDataXml.Instance.CurrentSavegameSeeds.TryGetValue(NetManager.instance.m_segments.m_buffer[rst.CurrentHoverSegment].m_nameSeed, out HashSet<ushort> segmentsSelected2))
                {
                    var targetColor = new Color(rst.singleSelectMode ? 0 : 1, 1, 0, 1);
                    foreach (var id in segmentsSelected2)
                    {
                        RenderOverlayUtils.RenderNetSegmentOverlay(cameraInfo, targetColor, id);
                    }
                }
            }
        }

        #endregion

        #region Hooking
        public Redirector RedirectorInstance { get; } = new Redirector();

        public void Awake()
        {
            LogUtils.DoLog("Loading NetManager Overrides");
            #region RoadBaseAI Hooks
            MethodInfo preRename = typeof(NetManagerOverrides).GetMethod("GenerateSegmentName", RedirectorUtils.allFlags);

            RedirectorInstance.AddRedirect(typeof(NetManager).GetMethod("GenerateSegmentName", RedirectorUtils.allFlags), preRename);
            RedirectorInstance.AddRedirect(typeof(NetManager).GetMethod("IRenderableManager.EndOverlay", RedirectorUtils.allFlags), null, typeof(NetManagerOverrides).GetMethod("AfterEndOverlay", RedirectorUtils.allFlags));
            #endregion
        }

        #endregion

    }
}
