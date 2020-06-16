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

        internal static void TriggerPostalCodeChanged() => Instance?.EventPostalCodeChanged?.Invoke();
        internal static void TriggerZeroMarkerBuildingChange() => Instance?.EventZeroMarkerBuildingChange?.Invoke();
        internal static void TriggerRoadNamingChange() => Instance?.EventRoadNamingChange?.Invoke();
        internal static void TriggerDistrictChanged() => Instance?.EventDistrictChanged?.Invoke();
        internal static void TriggerBuildingNameStrategyChanged() => Instance?.EventBuildingNameStrategyChanged?.Invoke();

        public static string GetStreetSuffix(ushort idx)
        {
            string result = "";
            var usedQueue = new List<ushort>();
            NetManagerOverrides.GenerateSegmentNameInternal(idx, ref result, ref usedQueue, true);

            return GetStreetFull(idx).Replace(GetStreetQualifier(idx), "").Trim();
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

            return GetStreetFull(idx).Replace(result, "").Trim();
        }

        public static Color GetDistrictColor(ushort idx)
        {
            Color color = AdrController.CurrentConfig.GetConfigForDistrict(idx).DistrictColor;
            if (color == default)
            {
                return AdrController.CurrentConfig.GetConfigForDistrict(0).DistrictColor;
            }
            return color;
        }

        public static Vector2 GetStartPoint()
        {
            ushort buildingZM = AdrController.CurrentConfig.GlobalConfig.AddressingConfig.ZeroMarkBuilding;
            if (buildingZM == 0)
            {
                return Vector2.zero;
            }
            else
            {
                return BuildingManager.instance.m_buildings.m_buffer[buildingZM].m_flags == Building.Flags.None ? Vector2.zero : VectorUtils.XZ(BuildingManager.instance.m_buildings.m_buffer[buildingZM].m_position);
            }
        }

        public static string GetPostalCode(Vector3 position) => AdrUtils.FormatPostalCode(position, AdrController.CurrentConfig.GlobalConfig.AddressingConfig.ZipcodeFormat);
    }
}