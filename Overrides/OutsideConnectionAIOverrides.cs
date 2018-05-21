using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using Klyte.Addresses.Utils;
using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using System;
using System.Reflection;
using UnityEngine;
using Klyte.Addresses.Extensors;

namespace Klyte.Addresses.Overrides
{
    class OutsideConnectionAIOverrides : Redirector<OutsideConnectionAIOverrides>
    {
        #region Mod

        private static bool GenerateNameOverride(OutsideConnectionAI __instance, ushort buildingID, ref string __result, ref InstanceID caller)
        {
            float angle = Vector2.zero.GetAngleToPoint(VectorUtils.XZ(BuildingManager.instance.m_buildings.m_buffer[buildingID].m_position));
            AdrUtils.doLog($"[buildingID {buildingID}] angle => {angle}, pos => {BuildingManager.instance.m_buildings.m_buffer[buildingID].m_position} ");
            return GetNameBasedInAngle(__instance, out __result, ref caller, angle, out bool canTrust);
        }
        public static string GetNameBasedInAngle(float angle, out bool canTrust)
        {
            InstanceID caller = new InstanceID();
            GetNameBasedInAngle(null, out string result, ref caller, angle, out canTrust);
            return result;
        }

        private static bool GetNameBasedInAngle(OutsideConnectionAI __instance, out string __result, ref InstanceID caller, float angle, out bool canTrust)
        {
            var randomizer = AdrNeighborhoodExtension.instance.GetRandomizerAt(angle);
            if (randomizer == null)
            {
                __result = OriginalGenerateName(__instance, caller);
                canTrust = false;
                return false;
            }
            var fileName = AdrConfigWarehouse.getCurrentConfigString(AdrConfigWarehouse.ConfigIndex.NEIGHBOR_CONFIG_NAME_FILE);
            if (fileName == null || !AdrController.loadedLocalesNeighborName.ContainsKey(fileName))
            {
                caller.Index = (uint)randomizer.GetValueOrDefault().seed;
                __result = OriginalGenerateName(__instance, caller);
                canTrust = true;
                return false;
            }
            String[] file = AdrController.loadedLocalesNeighborName[fileName];
            __result = file[randomizer.GetValueOrDefault().Int32((uint)file.Length)];
            canTrust = true;
            return false;
        }

        private static string OriginalGenerateName(OutsideConnectionAI __instance, InstanceID caller)
        {
            CityNameGroups.Environment cityNameGroups = Singleton<BuildingManager>.instance.m_cityNameGroups;
            if (cityNameGroups == null)
            {
                return null;
            }
            int num = 0;
            if (__instance?.m_useCloseNames ?? true)
            {
                num += cityNameGroups.m_closeDistance.Length;
            }
            if (__instance?.m_useMediumNames ?? true)
            {
                num += cityNameGroups.m_mediumDistance.Length;
            }
            if (__instance?.m_useFarNames ?? true)
            {
                num += cityNameGroups.m_farDistance.Length;
            }
            if (num != 0)
            {
                Randomizer randomizer = new Randomizer(caller.Index);
                num = randomizer.Int32((uint)num);
                string text = null;
                if (text == null && (__instance?.m_useCloseNames ?? true))
                {
                    if (num < cityNameGroups.m_closeDistance.Length)
                    {
                        text = cityNameGroups.m_closeDistance[num];
                    }
                    else
                    {
                        num -= cityNameGroups.m_closeDistance.Length;
                    }
                }
                if (text == null && (__instance?.m_useMediumNames ?? true))
                {
                    if (num < cityNameGroups.m_mediumDistance.Length)
                    {
                        text = cityNameGroups.m_mediumDistance[num];
                    }
                    else
                    {
                        num -= cityNameGroups.m_mediumDistance.Length;
                    }
                }
                if (text == null && (__instance?.m_useFarNames ?? true))
                {
                    if (num < cityNameGroups.m_farDistance.Length)
                    {
                        text = cityNameGroups.m_farDistance[num];
                    }
                    else
                    {
                        num -= cityNameGroups.m_farDistance.Length;
                    }
                }
                uint range = Locale.Count("CONNECTIONS_PATTERN", text);
                string format = Locale.Get("CONNECTIONS_PATTERN", text, randomizer.Int32(range));
                uint range2 = Locale.Count("CONNECTIONS_NAME", text);
                string arg = Locale.Get("CONNECTIONS_NAME", text, randomizer.Int32(range2));
                return StringUtils.SafeFormat(format, arg);
            }
            return null;
        }

        #endregion

        #region Hooking
        public static readonly MethodInfo GenerateNameMethod = typeof(OutsideConnectionAI).GetMethod("GenerateName", allFlags);
        public override void AwakeBody()
        {
            AdrUtils.doLog("Loading OutsideConnectionAI Overrides");
            #region RoadBaseAI Hooks
            MethodInfo preRename = typeof(OutsideConnectionAIOverrides).GetMethod("GenerateNameOverride", allFlags);

            AddRedirect(GenerateNameMethod, preRename);
            #endregion
        }
        #endregion

        public override void doLog(string text, params object[] param)
        {
            AdrUtils.doLog(text, param);
        }
    }
}
