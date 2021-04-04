using ColossalFramework.Globalization;
using ColossalFramework.Math;
using Klyte.Addresses.ModShared;
using Klyte.Commons.Extensions;
using Klyte.Commons.Utils;
using System.Reflection;
using UnityEngine;

namespace Klyte.Addresses.Overrides
{
    internal class DistrictManagerOverrides : MonoBehaviour, IRedirectable
    {
        public Redirector RedirectorInstance { get; } = new Redirector();
        #region Mod
#pragma warning disable IDE0051 // Remover membros privados não utilizados
        private static bool GenerateName(int district, DistrictManager __instance, ref string __result)
        {
            if (AddressesMod.Controller == null)
            {
                return true;
            }

            Randomizer randomizer = new Randomizer(__instance.m_districts.m_buffer[district].m_randomSeed);
            string format, arg;
            string filenamePrefix = AdrController.CurrentConfig.GlobalConfig.AddressingConfig.DistrictsConfig.QualifierFile;
            string filenameName = AdrController.CurrentConfig.GlobalConfig.AddressingConfig.DistrictsConfig.NamesFile;

            if (AdrController.LoadedLocalesDistrictPrefix.ContainsKey(filenamePrefix ?? ""))
            {
                int arrLen = AdrController.LoadedLocalesDistrictPrefix[filenamePrefix].Length;
                format = AdrController.LoadedLocalesDistrictPrefix[filenamePrefix][randomizer.Int32((uint)arrLen)];
            }
            else
            {
                format = Locale.Get("DISTRICT_PATTERN", randomizer.Int32(Locale.Count("DISTRICT_PATTERN")));
            }

            if (AdrController.LoadedLocalesDistrictName.ContainsKey(filenameName ?? ""))
            {
                int arrLen = AdrController.LoadedLocalesDistrictName[filenameName].Length;
                arg = AdrController.LoadedLocalesDistrictName[filenameName][randomizer.Int32((uint)arrLen)];
            }
            else
            {
                arg = Locale.Get("DISTRICT_NAME", randomizer.Int32(Locale.Count("DISTRICT_NAME")));
            }

            __result = StringUtils.SafeFormat(format, arg);
            return false;
        }
#pragma warning restore IDE0051 // Remover membros privados não utilizados

        #endregion

        #region Hooking
        public void Awake()
        {
            LogUtils.DoLog("Loading District Overrides");
            #region District Hooks
            MethodInfo preRename = typeof(DistrictManagerOverrides).GetMethod("GenerateName", RedirectorUtils.allFlags);
            MethodInfo GetNameMethod = typeof(DistrictManager).GetMethod("GenerateName", RedirectorUtils.allFlags);
            LogUtils.DoLog($"Overriding GetName ({GetNameMethod} => {preRename})");
            RedirectorInstance.AddRedirect(GetNameMethod, preRename);


            MethodInfo posChange = typeof(AdrShared).GetMethod("TriggerDistrictChanged", RedirectorUtils.allFlags);

            RedirectorInstance.AddRedirect(typeof(DistrictManager).GetMethod("SetDistrictName", RedirectorUtils.allFlags), null, posChange);
            RedirectorInstance.AddRedirect(typeof(DistrictManager).GetMethod("AreaModified", RedirectorUtils.allFlags), null, posChange);
            #endregion
        }
        #endregion        

        /*
         * private string GenerateName(int district)
    {
        Randomizer randomizer = new Randomizer(this.m_districts.m_buffer[district].m_randomSeed);
        string format = Locale.Get("DISTRICT_PATTERN", randomizer.Int32(Locale.Count("DISTRICT_PATTERN")));
        string arg = Locale.Get("DISTRICT_NAME", randomizer.Int32(Locale.Count("DISTRICT_NAME")));
        return StringUtils.SafeFormat(format, arg);
    }
         */

    }
}
