using ColossalFramework.Math;
using Klyte.Addresses.Overrides;
using Klyte.Addresses.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Klyte.Addresses.ModShared
{
    public static class AdrShared
    {
        public static event Action EventZeroMarkerBuildingChange;
        public static event Action EventRoadNamingChange;
        public static event Action EventDistrictColorChanged;
        public static event Action EventBuildingNameStrategyChanged;


        public static void TriggerZeroMarkerBuildingChange() => EventZeroMarkerBuildingChange?.Invoke();
        public static void TriggerRoadNamingChange() => EventRoadNamingChange?.Invoke();
        public static void TriggerDistrictColorChanged() => EventDistrictColorChanged?.Invoke();
        public static void TriggerBuildingNameStrategyChanged() => EventBuildingNameStrategyChanged?.Invoke();

        public static string GetStreetSuffix(ushort idx)
        {
            string result = "";
            var usedQueue = new List<ushort>();
            NetManagerOverrides.GenerateSegmentNameInternal(idx, ref result, ref usedQueue, true);

            return GetStreetFull(idx).Replace(GetStreetQualifier(idx), "");
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

            return result;
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