using ColossalFramework.Globalization;
using ColossalFramework.Math;
using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using System.Reflection;
using UnityEngine;

namespace Klyte.Addresses.Overrides
{
    internal class CitizenAIOVerrides : MonoBehaviour, IRedirectable
    {
        public Redirector RedirectorInstance { get; } = new Redirector();
        #region Mod
        public static bool GenerateCitizenName(uint citizenID, byte family, ref string __result)
        {
            Randomizer randomizer = new Randomizer(citizenID);
            Randomizer randomizer2 = new Randomizer(family);
            bool isMale = Citizen.GetGender(citizenID) == Citizen.Gender.Male;

            string name, surname;

            if (isMale)
            {
                string filenameFirst = AdrController.CurrentConfig?.GlobalConfig?.CitizenConfig?.MaleNamesFile;
                if (AdrController.LoadedLocalesCitizenFirstNameMasc.ContainsKey(filenameFirst))
                {
                    int arrLen = AdrController.LoadedLocalesCitizenFirstNameMasc[filenameFirst].Length;
                    name = AdrController.LoadedLocalesCitizenFirstNameMasc[filenameFirst][randomizer.Int32((uint) arrLen)];

                }
                else
                {
                    name = Locale.Get("NAME_MALE_FIRST", randomizer.Int32(Locale.Count("NAME_MALE_FIRST")));
                }

            }
            else
            {

                string filenameFirst = AdrController.CurrentConfig?.GlobalConfig?.CitizenConfig?.FemaleNamesFile;
                if (AdrController.LoadedLocalesCitizenFirstNameFem.ContainsKey(filenameFirst))
                {
                    int arrLen = AdrController.LoadedLocalesCitizenFirstNameFem[filenameFirst].Length;
                    name = AdrController.LoadedLocalesCitizenFirstNameFem[filenameFirst][randomizer.Int32((uint) arrLen)];

                }
                else
                {
                    name = Locale.Get("NAME_FEMALE_FIRST", randomizer.Int32(Locale.Count("NAME_FEMALE_FIRST")));
                }

            }
            string filenameLast = AdrController.CurrentConfig?.GlobalConfig?.CitizenConfig?.SurnamesFile;

            if (AdrController.LoadedLocalesCitizenLastName.ContainsKey(filenameLast))
            {
                int arrLen = AdrController.LoadedLocalesCitizenLastName[filenameLast].Length;
                surname = AdrController.LoadedLocalesCitizenLastName[filenameLast][randomizer2.Int32((uint) arrLen)];
            }
            else
            {
                if (isMale)
                {
                    surname = Locale.Get("NAME_MALE_LAST", randomizer2.Int32(Locale.Count("NAME_MALE_LAST")));
                }
                else
                {
                    surname = Locale.Get("NAME_FEMALE_LAST", randomizer2.Int32(Locale.Count("NAME_FEMALE_LAST")));
                }
            }

            __result = StringUtils.SafeFormat(surname, name);
            return false;
        }

        #endregion

        #region Hooking
        public void Awake()
        {
            LogUtils.DoLog("Loading CitizenAI Overrides");
            #region CitizenAI Hooks
            MethodInfo preRename = typeof(CitizenAIOVerrides).GetMethod("GenerateCitizenName", RedirectorUtils.allFlags);
            MethodInfo GetNameMethod = typeof(CitizenAI).GetMethod("GenerateCitizenName", RedirectorUtils.allFlags);
            LogUtils.DoLog($"Overriding GetName ({GetNameMethod} => {preRename})");
            RedirectorInstance.AddRedirect(GetNameMethod, preRename);
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
