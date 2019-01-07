using ColossalFramework.Globalization;
using ColossalFramework.Math;
using Klyte.Addresses.Utils;
using Klyte.Commons.Extensors;
using System.Reflection;

namespace Klyte.Addresses.Overrides
{
    class CitizenAIOVerrides : Redirector<CitizenAIOVerrides>
    {
        #region Mod
        public static bool GenerateCitizenName(uint citizenID, byte family, ref string __result)
        {
            Randomizer randomizer = new Randomizer(citizenID);
            Randomizer randomizer2 = new Randomizer((int)family);
            var isMale = Citizen.GetGender(citizenID) == Citizen.Gender.Male;

            string name, surname;

            if (isMale)
            {
                string filenameFirst = AdrConfigWarehouse.getCurrentConfigString(AdrConfigWarehouse.ConfigIndex.MALE_FIRSTNAME_FILE);
                if (AdrController.loadedLocalesCitizenFirstNameMasc.ContainsKey(filenameFirst))
                {
                    var arrLen = AdrController.loadedLocalesCitizenFirstNameMasc[filenameFirst].Length;
                    name = AdrController.loadedLocalesCitizenFirstNameMasc[filenameFirst][randomizer.Int32((uint)arrLen)];

                }
                else
                {
                    name = Locale.Get("NAME_MALE_FIRST", randomizer.Int32(Locale.Count("NAME_MALE_FIRST")));
                }

            }
            else
            {

                string filenameFirst = AdrConfigWarehouse.getCurrentConfigString(AdrConfigWarehouse.ConfigIndex.FEMALE_FIRSTNAME_FILE);
                if (AdrController.loadedLocalesCitizenFirstNameFem.ContainsKey(filenameFirst))
                {
                    var arrLen = AdrController.loadedLocalesCitizenFirstNameFem[filenameFirst].Length;
                    name = AdrController.loadedLocalesCitizenFirstNameFem[filenameFirst][randomizer.Int32((uint)arrLen)];

                }
                else
                {
                    name = Locale.Get("NAME_FEMALE_FIRST", randomizer.Int32(Locale.Count("NAME_FEMALE_FIRST")));
                }

            }
            string filenameLast = AdrConfigWarehouse.getCurrentConfigString(AdrConfigWarehouse.ConfigIndex.LASTNAME_FILE);

            if (AdrController.loadedLocalesCitizenLastName.ContainsKey(filenameLast))
            {
                var arrLen = AdrController.loadedLocalesCitizenLastName[filenameLast].Length;
                surname = AdrController.loadedLocalesCitizenLastName[filenameLast][randomizer2.Int32((uint)arrLen)];
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
        public override void AwakeBody()
        {
            AdrUtils.doLog("Loading CitizenAI Overrides");
            #region CitizenAI Hooks
            MethodInfo preRename = typeof(CitizenAIOVerrides).GetMethod("GenerateCitizenName", allFlags);
            var GetNameMethod = typeof(CitizenAI).GetMethod("GenerateCitizenName", allFlags);
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
