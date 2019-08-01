using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using Klyte.Addresses.Extensors;
using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using System.Reflection;
using UnityEngine;

namespace Klyte.Addresses.Overrides
{
    internal class OutsideConnectionAIOverrides : MonoBehaviour, IRedirectable
    {
        #region Mod

#pragma warning disable IDE0051 // Remover membros privados não utilizados
        private static bool GenerateNameOverride(OutsideConnectionAI __instance, ushort buildingID, ref string __result, ref InstanceID caller)
        {
            float angle = Vector2.zero.GetAngleToPoint(VectorUtils.XZ(BuildingManager.instance.m_buildings.m_buffer[buildingID].m_position));
            LogUtils.DoLog($"[buildingID {buildingID}] angle => {angle}, pos => {BuildingManager.instance.m_buildings.m_buffer[buildingID].m_position} ");

            return GetNameBasedInAngle(__instance, out __result, ref caller, angle, out _);
        }
#pragma warning restore IDE0051 // Remover membros privados não utilizados
        public static string GetNameBasedInAngle(float angle, out bool canTrust)
        {
            InstanceID caller = new InstanceID();
            GetNameBasedInAngle(m_defaultAI, out string result, ref caller, angle, out canTrust);
            return result;
        }

        private static bool GetNameBasedInAngle(OutsideConnectionAI __instance, out string __result, ref InstanceID caller, float angle, out bool canTrust)
        {
            Randomizer? randomizer = AdrNeighborhoodExtension.GetRandomizerAt(angle);
            if (randomizer == null)
            {
                __result = OriginalGenerateName(__instance, caller);
                canTrust = false;
                return false;
            }
            string fileName = AdrController.CurrentConfig.GlobalConfig.NeighborhoodConfig.NamesFile;
            if (fileName == null || !AdrController.LoadedLocalesNeighborName.ContainsKey(fileName))
            {
                caller.Index = (uint) randomizer.GetValueOrDefault().seed;
                __result = OriginalGenerateName(__instance, caller);
                canTrust = true;
                return false;
            }
            string[] file = AdrController.LoadedLocalesNeighborName[fileName];
            __result = file[randomizer.GetValueOrDefault().Int32((uint) file.Length)];
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
            if (__instance.m_useCloseNames)
            {
                num += cityNameGroups.m_closeDistance.Length;
            }
            if (__instance.m_useMediumNames)
            {
                num += cityNameGroups.m_mediumDistance.Length;
            }
            if (__instance.m_useFarNames)
            {
                num += cityNameGroups.m_farDistance.Length;
            }
            if (num != 0)
            {
                Randomizer randomizer = new Randomizer(caller.Index);
                num = randomizer.Int32((uint) num);
                string text = null;
                if (text == null && __instance.m_useCloseNames)
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
                if (text == null && __instance.m_useMediumNames)
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
                if (text == null && __instance.m_useFarNames)
                {
                    if (num < cityNameGroups.m_farDistance.Length)
                    {
                        text = cityNameGroups.m_farDistance[num];
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
        public static readonly MethodInfo GenerateNameMethod = typeof(OutsideConnectionAI).GetMethod("GenerateName", RedirectorUtils.allFlags);
        private static readonly OutsideConnectionAI m_defaultAI = new OutsideConnectionAI
        {
            m_useCloseNames = true,
            m_useFarNames = true,
            m_useMediumNames = true
        };

        public Redirector RedirectorInstance { get; } = new Redirector();

        public void Awake()
        {
            LogUtils.DoLog("Loading OutsideConnectionAI Overrides");
            #region OutsideConnectionAIOverrides Hooks
            MethodInfo preRename = typeof(OutsideConnectionAIOverrides).GetMethod("GenerateNameOverride", RedirectorUtils.allFlags);

            RedirectorInstance.AddRedirect(GenerateNameMethod, preRename);
            #endregion
        }
        #endregion

    }
}
