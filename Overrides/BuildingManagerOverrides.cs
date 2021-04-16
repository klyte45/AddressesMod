using ColossalFramework;
using Klyte.Addresses.Utils;
using Klyte.Commons.Extensions;
using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Klyte.Addresses.Overrides
{
    internal class BuildingManagerOverrides : MonoBehaviour, IRedirectable
    {
        #region Mod

#pragma warning disable IDE0051 // Remover membros privados não utilizados
        private static bool GetNameStation(ref string __result, ushort buildingID)
        {
            LogUtils.DoLog($"START {buildingID}");
            if (BuildingManager.instance.m_buildings.m_buffer[buildingID].Info.GetAI() is TransportStationAI || BuildingManager.instance.m_buildings.m_buffer[buildingID].Info.GetAI() is CargoStationAI)
            {

                if (BuildingManager.instance.m_buildings.m_buffer[buildingID].Info.GetAI() is TransportStationAI transportStationAI)
                {
                    if (!AdrController.CurrentConfig.GlobalConfig.BuildingConfig.StationsNameGenerationConfig.IsRenameEnabled(transportStationAI.m_transportInfo.m_transportType, transportStationAI.m_transportInfo.m_vehicleType, transportStationAI))
                    {
                        return true;
                    }
                }
                if (BuildingManager.instance.m_buildings.m_buffer[buildingID].Info.GetAI() is CargoStationAI cargoStationAI)
                {
                    if (!AdrController.CurrentConfig.GlobalConfig.BuildingConfig.StationsNameGenerationConfig.IsRenameEnabled(cargoStationAI.m_transportInfo.m_transportType, cargoStationAI.m_transportInfo.m_vehicleType, cargoStationAI))
                    {
                        return true;
                    }
                }
                Vector3 sidewalk = BuildingManager.instance.m_buildings.m_buffer[buildingID].CalculateSidewalkPosition();
                SegmentUtils.GetNearestSegment(sidewalk, out _, out _, out ushort targetSegmentId);
                List<ushort> crossingSegmentList = SegmentUtils.GetCrossingPath(targetSegmentId);
                crossingSegmentList.Add(targetSegmentId);
                foreach (ushort segId in crossingSegmentList)
                {
                    if ((NetManager.instance.m_segments.m_buffer[segId].m_flags & NetSegment.Flags.CustomName) != NetSegment.Flags.None)
                    {
                        InstanceID id = default;
                        id.NetSegment = segId;
                        __result = Singleton<InstanceManager>.instance.GetName(id);
                        if (__result != string.Empty)
                        {
                            InstanceManagerOverrides.CallBuildRenamedEvent(buildingID);
                            return false;
                        }
                    }

                    if (NetManagerOverrides.GetStreetNameForStation(segId, ref __result))
                    {
                        continue;
                    }
                    else
                    {
                        InstanceManagerOverrides.CallBuildRenamedEvent(buildingID);
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
                        InstanceManagerOverrides.CallBuildRenamedEvent(buildingID);
                        return false;
                    }
                }
            }
            return true;


        }

        private static bool GetNameRico(ref string __result, ushort buildingID)
        {
            if (BuildingManager.instance.m_buildings.m_buffer[buildingID].Info.m_buildingAI is ResidentialBuildingAI)
            {
                if (AdrController.CurrentConfig.GlobalConfig.BuildingConfig.RicoNamesGenerationConfig.Residence == Xml.AdrRicoNamesGenerationConfig.GenerationMethod.NONE)
                {
                    return true;
                }
            }
            else if (BuildingManager.instance.m_buildings.m_buffer[buildingID].Info.m_buildingAI is IndustrialBuildingAI)
            {
                if (AdrController.CurrentConfig.GlobalConfig.BuildingConfig.RicoNamesGenerationConfig.Industry == Xml.AdrRicoNamesGenerationConfig.GenerationMethod.NONE)
                {
                    return true;
                }
            }
            else if (BuildingManager.instance.m_buildings.m_buffer[buildingID].Info.m_buildingAI is CommercialBuildingAI)
            {
                if (AdrController.CurrentConfig.GlobalConfig.BuildingConfig.RicoNamesGenerationConfig.Commerce == Xml.AdrRicoNamesGenerationConfig.GenerationMethod.NONE)
                {
                    return true;
                }
            }
            else if (BuildingManager.instance.m_buildings.m_buffer[buildingID].Info.m_buildingAI is OfficeBuildingAI)
            {
                if (AdrController.CurrentConfig.GlobalConfig.BuildingConfig.RicoNamesGenerationConfig.Office == Xml.AdrRicoNamesGenerationConfig.GenerationMethod.NONE)
                {
                    return true;
                }
            }
            else
            {
                return true;
            }

            Vector3 sidewalk = BuildingManager.instance.m_buildings.m_buffer[buildingID].CalculateSidewalkPosition();
            AdrUtils.GetAddressLines(sidewalk, BuildingManager.instance.m_buildings.m_buffer[buildingID].m_position, out string[] addressLines);
            if (addressLines.Length == 0)
            {
                return true;
            }

            __result = addressLines[0];
            return false;
        }

        private static void OnBuildingCreated(ref ushort building)
        {
            ushort building_ = building;

            new AsyncAction(() => EventBuildingCreated?.Invoke(building_)).Execute();
        }
        private static void OnBuildingReleased(ref ushort building)
        {
            ushort building_ = building;
            new AsyncAction(() => EventBuidlingReleased?.Invoke(building_)).Execute();
        }

#pragma warning restore IDE0051 // Remover membros privados não utilizados
        #endregion

        #region Hooking
        private static readonly MethodInfo m_getNameMethod = typeof(BuildingAI).GetMethod("GenerateName", RedirectorUtils.allFlags);
        private static readonly MethodInfo m_getNameMethodRes = typeof(ResidentialBuildingAI).GetMethod("GenerateName", RedirectorUtils.allFlags);
        private static readonly MethodInfo m_getNameMethodInd = typeof(IndustrialBuildingAI).GetMethod("GenerateName", RedirectorUtils.allFlags);
        private static readonly MethodInfo m_getNameMethodCom = typeof(CommercialBuildingAI).GetMethod("GenerateName", RedirectorUtils.allFlags);
        private static readonly MethodInfo m_getNameMethodOff = typeof(OfficeBuildingAI).GetMethod("GenerateName", RedirectorUtils.allFlags);

        public Redirector RedirectorInstance { get; } = new Redirector();


        #endregion

        #region Events
        public static event Action<ushort> EventBuildingCreated;
        public static event Action<ushort> EventBuidlingReleased;

        #endregion

        #region Hooking

        public void Awake()
        {
            LogUtils.DoLog("Loading Building Manager Overrides");
            #region BuildingManager Hooks
            MethodInfo OnBuildingCreated = GetType().GetMethod("OnBuildingCreated", RedirectorUtils.allFlags);
            MethodInfo OnBuildingReleased = GetType().GetMethod("OnBuildingReleased", RedirectorUtils.allFlags);

            RedirectorInstance.AddRedirect(typeof(BuildingManager).GetMethod("CreateBuilding", RedirectorUtils.allFlags), null, OnBuildingCreated);
            RedirectorInstance.AddRedirect(typeof(BuildingManager).GetMethod("ReleaseBuilding", RedirectorUtils.allFlags), null, OnBuildingReleased);
            #endregion

            #region RoadBaseAI Hooks
            MethodInfo preRename = typeof(BuildingManagerOverrides).GetMethod("GetNameStation", RedirectorUtils.allFlags);
            MethodInfo preRenameRico = typeof(BuildingManagerOverrides).GetMethod("GetNameRico", RedirectorUtils.allFlags);
            LogUtils.DoLog($"Overriding GetName ({m_getNameMethod} => {preRename})");
            RedirectorInstance.AddRedirect(m_getNameMethod, preRename);
            RedirectorInstance.AddRedirect(m_getNameMethodRes, preRenameRico);
            RedirectorInstance.AddRedirect(m_getNameMethodInd, preRenameRico);
            RedirectorInstance.AddRedirect(m_getNameMethodCom, preRenameRico);
            RedirectorInstance.AddRedirect(m_getNameMethodOff, preRenameRico);
            #endregion

        }
        #endregion

    }
}
