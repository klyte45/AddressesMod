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
            GetNameBasedInAngle(null, out string result, ref caller, angle, out canTrust);
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

        private static string OriginalGenerateName(OutsideConnectionAI __instance, InstanceID caller) => typeof(OutsideConnectionAI).GetMethod("GenerateName").Invoke(__instance, new object[] { 0u, caller })?.ToString();

        #endregion

        #region Hooking
        public static readonly MethodInfo GenerateNameMethod = typeof(OutsideConnectionAI).GetMethod("GenerateName", RedirectorUtils.allFlags);

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
