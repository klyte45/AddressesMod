using ColossalFramework;
using Klyte.Addresses.Utils;
using Klyte.Commons.Extensors;
using System.Reflection;
using UnityEngine;

namespace Klyte.Addresses.Overrides
{
    class BuildingManagerOverrides : Redirector<BuildingManagerOverrides>
    {
        #region Mod

        private static bool GetNameStation(ref string __result, ushort buildingID, InstanceID caller)
        {
            AdrUtils.doLog($"START {buildingID}");
            if (BuildingManager.instance.m_buildings.m_buffer[buildingID].Info.GetAI() is TransportStationAI || BuildingManager.instance.m_buildings.m_buffer[buildingID].Info.GetAI() is CargoStationAI)
            {

                if (BuildingManager.instance.m_buildings.m_buffer[buildingID].Info.GetAI() is TransportStationAI transportStationAI)
                {
                    switch (transportStationAI.m_transportInfo.m_transportType)
                    {
                        case TransportInfo.TransportType.Airplane:
                            if (transportStationAI.m_transportInfo.m_vehicleType == VehicleInfo.VehicleType.Blimp)
                            {
                                if (!AdrConfigWarehouse.getCurrentConfigBool(AdrConfigWarehouse.ConfigIndex.ENABLE_CUSTOM_NAMING_STATIONS_BLIMP)) return true;
                            }
                            else if (transportStationAI.m_transportInfo.m_vehicleType == VehicleInfo.VehicleType.Plane)
                            {
                                if (!AdrConfigWarehouse.getCurrentConfigBool(AdrConfigWarehouse.ConfigIndex.ENABLE_CUSTOM_NAMING_STATIONS_AIRPLANE)) return true;
                            }
                            else
                            {
                                return true;
                            }
                            break;
                        case TransportInfo.TransportType.CableCar:
                            if (!AdrConfigWarehouse.getCurrentConfigBool(AdrConfigWarehouse.ConfigIndex.ENABLE_CUSTOM_NAMING_STATIONS_CABLE_CAR)) return true;
                            break;
                        case TransportInfo.TransportType.Metro:
                            if (!AdrConfigWarehouse.getCurrentConfigBool(AdrConfigWarehouse.ConfigIndex.ENABLE_CUSTOM_NAMING_STATIONS_METRO)) return true;
                            break;
                        case TransportInfo.TransportType.Monorail:
                            if (!AdrConfigWarehouse.getCurrentConfigBool(AdrConfigWarehouse.ConfigIndex.ENABLE_CUSTOM_NAMING_STATIONS_MONORAIL)) return true;
                            break;
                        case TransportInfo.TransportType.Ship:
                            if (transportStationAI.m_transportInfo.m_vehicleType == VehicleInfo.VehicleType.Ship)
                            {
                                if (!AdrConfigWarehouse.getCurrentConfigBool(AdrConfigWarehouse.ConfigIndex.ENABLE_CUSTOM_NAMING_STATIONS_SHIP)) return true;
                            }
                            else if (transportStationAI.m_transportInfo.m_vehicleType == VehicleInfo.VehicleType.Ferry)
                            {
                                if (!AdrConfigWarehouse.getCurrentConfigBool(AdrConfigWarehouse.ConfigIndex.ENABLE_CUSTOM_NAMING_STATIONS_FERRY)) return true;
                            }
                            else
                            {
                                return true;
                            }
                            break;
                        case TransportInfo.TransportType.Train:
                            if (!AdrConfigWarehouse.getCurrentConfigBool(AdrConfigWarehouse.ConfigIndex.ENABLE_CUSTOM_NAMING_STATIONS_TRAIN)) return true;
                            break;
                        default:
                            return true;
                    }
                }
                if (BuildingManager.instance.m_buildings.m_buffer[buildingID].Info.GetAI() is CargoStationAI cargoStationAI)
                {
                    switch (cargoStationAI.m_transportInfo.m_transportType)
                    {
                        case TransportInfo.TransportType.Ship:
                            if (!AdrConfigWarehouse.getCurrentConfigBool(AdrConfigWarehouse.ConfigIndex.ENABLE_CUSTOM_NAMING_CARGO_SHIP)) return true;
                            break;
                        case TransportInfo.TransportType.Train:
                            if (!AdrConfigWarehouse.getCurrentConfigBool(AdrConfigWarehouse.ConfigIndex.ENABLE_CUSTOM_NAMING_CARGO_TRAIN)) return true;
                            break;
                        default:
                            return true;
                    }
                }
                var sidewalk = BuildingManager.instance.m_buildings.m_buffer[buildingID].CalculateSidewalkPosition();
                AdrUtils.GetNearestSegment(sidewalk, out Vector3 targetPosition, out float targetLength, out ushort targetSegmentId);
                var crossingSegmentList = AdrUtils.GetCrossingPath(targetSegmentId);
                crossingSegmentList.Add(targetSegmentId);
                foreach (ushort segId in crossingSegmentList)
                {
                    if ((NetManager.instance.m_segments.m_buffer[(int)segId].m_flags & NetSegment.Flags.CustomName) != NetSegment.Flags.None)
                    {
                        InstanceID id = default(InstanceID);
                        id.NetSegment = segId;
                        __result = Singleton<InstanceManager>.instance.GetName(id);
                        if (__result != string.Empty)
                        {
                            Klyte.Commons.Overrides.BuildingManagerOverrides.CallBuildRenamedEvent(buildingID);
                            return false;
                        }
                    }

                    if (NetManagerOverrides.GetStreetNameForStation(segId, ref __result))
                    {
                        continue;
                    }
                    else
                    {
                        Klyte.Commons.Overrides.BuildingManagerOverrides.CallBuildRenamedEvent(buildingID);
                        return false;
                    }
                }
                foreach (ushort segId in crossingSegmentList)
                {
                    __result = NetManager.instance.GetSegmentName(segId) ?? string.Empty;

                    if (__result == string.Empty)
                    {
                        continue;
                    }
                    else
                    {
                        Klyte.Commons.Overrides.BuildingManagerOverrides.CallBuildRenamedEvent(buildingID);
                        return false;
                    }
                }
            }
            return true;


        }

        private static bool GetNameRico(ref string __result, ushort buildingID, InstanceID caller)
        {
            if (BuildingManager.instance.m_buildings.m_buffer[buildingID].Info.GetAI() is ResidentialBuildingAI && !AdrConfigWarehouse.getCurrentConfigBool(AdrConfigWarehouse.ConfigIndex.ENABLE_ADDRESS_NAMING_RES)) return true;
            if (BuildingManager.instance.m_buildings.m_buffer[buildingID].Info.GetAI() is IndustrialBuildingAI && !AdrConfigWarehouse.getCurrentConfigBool(AdrConfigWarehouse.ConfigIndex.ENABLE_ADDRESS_NAMING_IND)) return true;
            if (BuildingManager.instance.m_buildings.m_buffer[buildingID].Info.GetAI() is CommercialBuildingAI && !AdrConfigWarehouse.getCurrentConfigBool(AdrConfigWarehouse.ConfigIndex.ENABLE_ADDRESS_NAMING_COM)) return true;
            if (BuildingManager.instance.m_buildings.m_buffer[buildingID].Info.GetAI() is OfficeBuildingAI && !AdrConfigWarehouse.getCurrentConfigBool(AdrConfigWarehouse.ConfigIndex.ENABLE_ADDRESS_NAMING_OFF)) return true;
            var sidewalk = BuildingManager.instance.m_buildings.m_buffer[buildingID].CalculateSidewalkPosition();
            AdrUtils.getAddressLines(sidewalk, BuildingManager.instance.m_buildings.m_buffer[buildingID].m_position, out string[] addressLines);
            if (addressLines.Length == 0) return true;
            __result = addressLines[0];
            return false;
        }
        #endregion

        #region Hooking
        private static readonly MethodInfo GetNameMethod = typeof(BuildingAI).GetMethod("GenerateName", allFlags);
        private static readonly MethodInfo GetNameMethodRes = typeof(ResidentialBuildingAI).GetMethod("GenerateName", allFlags);
        private static readonly MethodInfo GetNameMethodInd = typeof(IndustrialBuildingAI).GetMethod("GenerateName", allFlags);
        private static readonly MethodInfo GetNameMethodCom = typeof(CommercialBuildingAI).GetMethod("GenerateName", allFlags);
        private static readonly MethodInfo GetNameMethodOff = typeof(OfficeBuildingAI).GetMethod("GenerateName", allFlags);
        //private static readonly MethodInfo GetNameMethodExt = typeof(IndustrialExtractorAI).GetMethod("GenerateName", allFlags);
        public override void AwakeBody()
        {
            AdrUtils.doLog("Loading BuildingAI Overrides");
            #region RoadBaseAI Hooks
            MethodInfo preRename = typeof(BuildingManagerOverrides).GetMethod("GetNameStation", allFlags);
            MethodInfo preRenameRico = typeof(BuildingManagerOverrides).GetMethod("GetNameRico", allFlags);
            AdrUtils.doLog($"Overriding GetName ({GetNameMethod} => {preRename})");
            AddRedirect(GetNameMethod, preRename);
            AddRedirect(GetNameMethodRes, preRenameRico);
            AddRedirect(GetNameMethodInd, preRenameRico);
            AddRedirect(GetNameMethodCom, preRenameRico);
            AddRedirect(GetNameMethodOff, preRenameRico);
            #endregion
        }

        public override void doLog(string text, params object[] param)
        {
            AdrUtils.doLog(text, param);
        }
        #endregion

    }
}
