using ColossalFramework;
using ColossalFramework.Math;
using Klyte.Addresses.Overrides;
using Klyte.Addresses.Utils;
using Klyte.Addresses.Xml;
using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Klyte.Addresses.ModShared
{
    public class AdrFacade : MonoBehaviour
    {
        public static AdrFacade Instance => AddressesMod.Controller?.FacadeInstance;
        public event Action EventZeroMarkerBuildingChange;
        public event Action EventRoadNamingChange;
        public event Action EventDistrictChanged;
        public event Action EventBuildingNameStrategyChanged;
        public event Action EventPostalCodeChanged;
        public event Action EventHighwaysChanged;
        public event Action<ushort> EventHighwaySeedChanged;

        internal static void TriggerPostalCodeChanged() => Instance?.EventPostalCodeChanged?.Invoke();
        internal static void TriggerZeroMarkerBuildingChange() => Instance?.EventZeroMarkerBuildingChange?.Invoke();
        internal static void TriggerRoadNamingChange() => Instance?.EventRoadNamingChange?.Invoke();
        internal static void TriggerDistrictChanged() => Instance?.EventDistrictChanged?.Invoke();
        internal static void TriggerBuildingNameStrategyChanged() => Instance?.EventBuildingNameStrategyChanged?.Invoke();
        internal static void TriggerHighwaysChanged()
        {
            SegmentUtils.UpdateSegmentNamesView();
            Instance?.EventHighwaysChanged?.Invoke();
        }

        internal static void TriggerHighwaySeedChanged(ushort seed)
        {
            if (seed == 0)
            {
                return;
            }

            SegmentUtils.UpdateSegmentNamesView();
            Instance?.EventHighwaySeedChanged?.Invoke(seed);
        }

        public static bool GetStreetAndNumber(Vector3 sidewalk, Vector3 midPosBuilding, out string streetName, out int number) => AdrUtils.GetStreetAndNumber(sidewalk, midPosBuilding, out streetName, out number);

        public static string GetStreetSuffix(ushort idx)
        {
            string result = "";
            var usedQueue = new List<ushort>();
            NetManagerOverrides.GenerateSegmentNameInternal(idx, ref result, ref usedQueue, true);
            var qualifier = GetStreetQualifier(idx);
            return qualifier.IsNullOrWhiteSpace()
                ? GetStreetFull(idx).Trim()
                : GetStreetFull(idx).Replace(qualifier, "").Trim();
        }
        [Obsolete("Use version with mileage source and highway axis", true)]
        public static void GetMileageSeedConfig(ushort seedId, out bool invertStart, out int offsetMeters)
        {
            invertStart = false;
            offsetMeters = 0;
        }

        public static void GetMileageSeedConfig(ushort seedId, out int offsetMeters, out int mileageSource, out int highwayAxis)
        {
            mileageSource = 0;
            highwayAxis = 0;
            offsetMeters = 0;
            if (AdrNameSeedDataXml.Instance.NameSeedConfigs.TryGetValue(seedId, out AdrNameSeedConfig seedConf))
            {
                mileageSource = (int)seedConf.MileageStartSrc;
                highwayAxis = (int)seedConf.HighwayAxis;
                offsetMeters = (int)seedConf.MileageOffset;
            }
        }
        public static bool GetHighwayTypeParameters(string layoutName, out string detachedStr, out string shortCode, out string longCode)
        {
            var cachedParent = AdrHighwayParentLibDataXml.Instance.Get(layoutName);
            if (cachedParent is null)
            {
                shortCode = null;
                longCode = null;
                detachedStr = null;
                return false;
            }
            GetHighwayTypeData(out detachedStr, out shortCode, out longCode, cachedParent, "XXX");
            return true;
        }
        public static bool GetSeedHighwayParameters(ushort seedId, out string layoutName, out string detachedStr, out string hwIdentifier, out string shortCode, out string longCode, out Color hwColor)
        {
            detachedStr = null;
            shortCode = null;
            longCode = null;
            hwIdentifier = null;
            layoutName = null;
            hwColor = default;
            if (AdrNameSeedDataXml.Instance.NameSeedConfigs.TryGetValue(seedId, out AdrNameSeedConfig seedConf))
            {
                if (!(seedConf.HighwayParent is null))
                {
                    GetHighwayTypeData(out detachedStr, out shortCode, out longCode, seedConf.HighwayParent, seedConf.HighwayIdentifier);
                }
                layoutName = seedConf.HighwayParentName;
                hwIdentifier = seedConf.HighwayIdentifier;
                hwColor = seedConf.HighwayColor;
                return true;
            }
            return false;
        }
        public static string GetHighwayTypeDetachedString(string itemName) => AdrHighwayParentLibDataXml.Instance.Get(itemName)?.DettachedPrefix;

        private static void GetHighwayTypeData(out string detachedStr, out string shortCode, out string longCode, AdrHighwayParentXml parentConf, string identifier)
        {
            detachedStr = parentConf.DettachedPrefix;
            shortCode = parentConf.GetShortValue(identifier);
            longCode = parentConf.GetLongValue(identifier);
        }

        public static void ListAllHighwayTypes(string textFilter, Wrapper<string[]> result) => AdrHighwayParentLibDataXml.Instance.FilterBy(textFilter, result);

        public static string GetStreetFull(ushort segmentId)
        {
            string result = "";
            var usedQueue = new List<ushort>();
            NetManagerOverrides.GenerateSegmentNameInternal(segmentId, ref result, ref usedQueue, false);

            return result;
        }
        public static string GetStreetQualifier(ushort segmentId)
        {
            string result = "";
            var usedQueue = new List<ushort>();
            NetManagerOverrides.GenerateSegmentNameInternal(segmentId, ref result, ref usedQueue, true);
            return result.IsNullOrWhiteSpace() ? GetStreetFull(segmentId).Trim() : GetStreetFull(segmentId).Replace(result, "").Trim();
        }

        public static Color GetDistrictColor(ushort districtId)
        {
            Color color = AdrController.CurrentConfig.GetConfigForDistrict(districtId).DistrictColor;
            return color == default ? AdrController.CurrentConfig.GetConfigForDistrict(0).DistrictColor : color;
        }

        public static Vector2 GetStartPoint()
        {
            ushort buildingZM = AdrController.CurrentConfig.GlobalConfig.AddressingConfig.ZeroMarkBuilding;
            return buildingZM == 0
                ? Vector2.zero
                : BuildingManager.instance.m_buildings.m_buffer[buildingZM].m_flags == Building.Flags.None ? Vector2.zero : VectorUtils.XZ(BuildingManager.instance.m_buildings.m_buffer[buildingZM].m_position);
        }

        public static string GetPostalCode(Vector3 position) => AdrController.CurrentConfig.GlobalConfig.AddressingConfig.TokenizedPostalCodeFormat.TokenToPostalCode(position);

        public static bool IsAutonameAvailable(ref Building building)
        {
            if (building.Info.m_buildingAI is TransportStationAI tsai)
            {
                return AdrConfigXml.Instance.GlobalConfig.BuildingConfig.StationsNameGenerationConfig.IsRenameEnabled(tsai.m_transportInfo.m_transportType, tsai.m_transportInfo.m_vehicleType, tsai);
            }
            return false;
        }
    }
}