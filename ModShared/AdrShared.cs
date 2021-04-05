using ColossalFramework;
using ColossalFramework.Math;
using Klyte.Addresses.Overrides;
using Klyte.Addresses.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Klyte.Addresses.ModShared
{
    public class AdrShared : MonoBehaviour
    {
        public static AdrShared Instance => AddressesMod.Controller?.SharedInstance;
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
        internal static void TriggerHighwaysChanged() => Instance?.EventHighwaysChanged?.Invoke();
        internal static void TriggerHighwaySeedChanged(ushort seed) => Instance?.EventHighwaySeedChanged?.Invoke(seed);

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
        public static string GetStreetFull(ushort idx)
        {
            string result = "";
            var usedQueue = new List<ushort>();
            NetManagerOverrides.GenerateSegmentNameInternal(idx, ref result, ref usedQueue, false);

            return result;
        }
        public static string GetStreetQualifier(ushort idx)
        {
            string result = "";
            var usedQueue = new List<ushort>();
            NetManagerOverrides.GenerateSegmentNameInternal(idx, ref result, ref usedQueue, true);
            return result.IsNullOrWhiteSpace() ? GetStreetFull(idx).Trim() : GetStreetFull(idx).Replace(result, "").Trim();
        }

        public static Color GetDistrictColor(ushort idx)
        {
            Color color = AdrController.CurrentConfig.GetConfigForDistrict(idx).DistrictColor;
            return color == default ? AdrController.CurrentConfig.GetConfigForDistrict(0).DistrictColor : color;
        }

        public static Vector2 GetStartPoint()
        {
            ushort buildingZM = AdrController.CurrentConfig.GlobalConfig.AddressingConfig.ZeroMarkBuilding;
            return buildingZM == 0
                ? Vector2.zero
                : BuildingManager.instance.m_buildings.m_buffer[buildingZM].m_flags == Building.Flags.None ? Vector2.zero : VectorUtils.XZ(BuildingManager.instance.m_buildings.m_buffer[buildingZM].m_position);
        }

        public static string GetPostalCode(Vector3 position) =>  AdrController.CurrentConfig.GlobalConfig.AddressingConfig.TokenizedPostalCodeFormat.TokenToPostalCode(position);
    }
}