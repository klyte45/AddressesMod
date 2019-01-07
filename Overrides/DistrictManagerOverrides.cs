using ColossalFramework.Globalization;
using ColossalFramework.Math;
using Klyte.Addresses.Utils;
using Klyte.Commons.Extensors;
using System.Reflection;

namespace Klyte.Addresses.Overrides
{
    class DistrictManagerOverrides : Redirector<DistrictManagerOverrides>
    {
        #region Mod
        private static bool GenerateName(int district, DistrictManager __instance, ref string __result)
        {
            Randomizer randomizer = new Randomizer(__instance.m_districts.m_buffer[district].m_randomSeed);
            string format, arg;
            string filenamePrefix = AdrConfigWarehouse.getCurrentConfigString(AdrConfigWarehouse.ConfigIndex.DISTRICT_PREFIX_FILE);
            string filenameName = AdrConfigWarehouse.getCurrentConfigString(AdrConfigWarehouse.ConfigIndex.DISTRICT_NAMING_FILE);

            if (AdrController.loadedLocalesDistrictPrefix.ContainsKey(filenamePrefix))
            {
                var arrLen = AdrController.loadedLocalesDistrictPrefix[filenamePrefix].Length;
                format = AdrController.loadedLocalesDistrictPrefix[filenamePrefix][randomizer.Int32((uint)arrLen)];
            }
            else
            {
                format = Locale.Get("DISTRICT_PATTERN", randomizer.Int32(Locale.Count("DISTRICT_PATTERN")));
            }

            if (AdrController.loadedLocalesDistrictName.ContainsKey(filenameName))
            {
                var arrLen = AdrController.loadedLocalesDistrictName[filenameName].Length;
                arg = AdrController.loadedLocalesDistrictName[filenameName][randomizer.Int32((uint)arrLen)];
            }
            else
            {
                arg = Locale.Get("DISTRICT_NAME", randomizer.Int32(Locale.Count("DISTRICT_NAME")));
            }

            __result = StringUtils.SafeFormat(format, arg);
            return false;
        }

        #endregion

        #region Hooking
        public override void AwakeBody()
        {
            AdrUtils.doLog("Loading BuildingAI Overrides");
            #region RoadBaseAI Hooks
            MethodInfo preRename = typeof(DistrictManagerOverrides).GetMethod("GenerateName", allFlags);
            var GetNameMethod = typeof(DistrictManager).GetMethod("GenerateName", allFlags);
            AdrUtils.doLog($"Overriding GetName ({GetNameMethod} => {preRename})");
            AddRedirect(GetNameMethod, preRename);
            #endregion
        }

        public override void doLog(string text, params object[] param)
        {
            AdrUtils.doLog(text, param);
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
