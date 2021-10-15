using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Addresses.UI;
using Klyte.Commons.Extensions;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

[assembly: AssemblyVersion("3.1.0.*")]
 
namespace Klyte.Addresses
{

    public class AddressesMod : BasicIUserMod<AddressesMod, AdrController, AdrConfigPanel>
    {
        public override string IconName { get; } = "K45_ADRIcon";
        public override string SimpleName { get; } = "Addresses & Names Mod";
        public override string Description { get; } = "Allow road name generation customization.";

        public override void DoErrorLog(string fmt, params object[] args) => LogUtils.DoErrorLog(fmt, args);

        public override void DoLog(string fmt, params object[] args) => LogUtils.DoLog(fmt, args);


        public override void TopSettingsUI(UIHelperExtension helper)
        {

            UIHelperExtension group8 = helper.AddGroupExtended(Locale.Get("K45_ADR_GENERAL_INFO"));
            AddFolderButton(RoadPath, group8, "K45_ADR_ROAD_NAME_FILES_PATH_TITLE");
            AddFolderButton(RoadPrefixPath, group8, "K45_ADR_ROAD_PREFIX_NAME_FILES_PATH_TITLE");
            AddFolderButton(NeigborsPath, group8, "K45_ADR_NEIGHBOR_CITIES_NAME_FILES_PATH_TITLE");
            AddFolderButton(DistrictPrefixPath, group8, "K45_ADR_DISTRICT_PREFIXES_FILES_PATH_TITLE");
            AddFolderButton(DistrictNamePath, group8, "K45_ADR_DISTRICT_NAME_FILES_PATH_TITLE");
            AddFolderButton(CitizenFirstNameMascPath, group8, "K45_ADR_CITIZEN_FIRST_NAME_MASC_FILES_PATH_TITLE");
            AddFolderButton(CitizenFirstNameFemPath, group8, "K45_ADR_CITIZEN_FIRST_NAME_FEM_FILES_PATH_TITLE");
            AddFolderButton(CitizenLastNamePath, group8, "K45_ADR_CITIZEN_LAST_NAME_FILES_PATH_TITLE");
            AddFolderButton(HighwayConfigurationFolder, group8, "K45_ADR_HIGHWAY_CONFIGS_FILES_PATH_TITLE");
            AddFolderButton(FootballTeamDataFolder, group8, "K45_ADR_FOOTBALL_TEAMNAMES_FILES_PATH_TITLE");

            AdrController.ReloadAllFiles();

            UIHelperExtension group7 = helper.AddGroupExtended(Locale.Get("K45_ADR_ADDITIONAL_FILES_SOURCE"));
            group7.AddLabel(Locale.Get("K45_ADR_GET_FILES_GITHUB"));
            group7.AddButton(Locale.Get("K45_ADR_GO_TO_GITHUB"), () => Application.OpenURL("https://github.com/klyte45/AddressesFiles"));


            //group7.AddButton("TST", () => K45DialogControl.ShowModalPromptText(new K45DialogControl.BindProperties
            //{
            //    message = "TESTE"
            //}, (x, format) =>
            //{
            //    var or = format;
            //    format = format.Replace("\\]", "\0");
            //    if (Regex.IsMatch(format, @"(?<=\[)(?<!\\\[).+?(?<!\\\])(?=\])"))
            //    {
            //        format = Regex.Matches(format, @"(?<=\[)(?<!\\\[).+?(?<!\\\])(?=\])")[0].Groups[0].Value;
            //    }
            //    else
            //    {
            //        format = Regex.Replace(format ?? "", "(?!\\{)(\\w+|\\.)(?!\\})", "");
            //    }
            //    format = Regex.Replace(format, @"(?<!\\)(\[|\])", "");
            //    format = Regex.Replace(format, @"(\\)(\[|\])", "$2");
            //    format = Regex.Replace(format, @"\\\\", "\\");
            //    format = format.Replace("\0", "]");

            //    var formatFull = Regex.Replace(or, @"(?<!\\)(\[|\])", "");
            //    formatFull = Regex.Replace(formatFull, @"(\\)(\[|\])", "$2");
            //    formatFull = Regex.Replace(formatFull, @"\\\\", "\\");
            //    formatFull = formatFull.Replace("\0", "]");
            //    K45DialogControl.ShowModal(new K45DialogControl.BindProperties 
            //    {
            //        message = $"\"{or}\"\nPRE: {format}\nFULL: {formatFull}"
            //    }, (k) => true);
            //    return true;
            //}));
        }

        private static void AddFolderButton(string filePath, UIHelperExtension helper, string localeId)
        {
            FileInfo fileInfo = FileUtils.EnsureFolderCreation(filePath);
            helper.AddLabel(Locale.Get(localeId) + ":");
            var namesFilesButton = ((UIButton)helper.AddButton("/", () => ColossalFramework.Utils.OpenInFileBrowser(fileInfo.FullName)));
            namesFilesButton.textColor = Color.yellow;
            KlyteMonoUtils.LimitWidthAndBox(namesFilesButton, 710);
            namesFilesButton.text = fileInfo.FullName + Path.DirectorySeparatorChar;
        }



        public static readonly string FOLDER_NAME = FileUtils.BASE_FOLDER_PATH + "AddressesNames";
        public const string ROAD_SUBFOLDER_NAME = "Roads";
        public const string ROADPREFIX_SUBFOLDER_NAME = "RoadsPrefix";
        public const string NEIGHBOR_SUBFOLDER_NAME = "RegionCities";
        public const string DISTRICT_PREFIXES_SUBFOLDER_NAME = "DistrictPrefix";
        public const string DISTRICT_NAMES_SUBFOLDER_NAME = "DistrictName";
        public const string CITIZEN_FIRST_NAME_MASC_SUBFOLDER_NAME = "CitizenFirstNameMale";
        public const string CITIZEN_FIRST_NAME_FEM_SUBFOLDER_NAME = "CitizenFirstNameFemale";
        public const string CITIZEN_LAST_NAME_SUBFOLDER_NAME = "CitizenLastName";
        public const string HIGHWAY_PREFABS_SUBFOLDER_NAME = "HighwayConfigurations";
        public const string FOOTBALL_TEAM_DATA = "FootballTeams";

        public static string RoadPath { get; } = FOLDER_NAME + Path.DirectorySeparatorChar + ROAD_SUBFOLDER_NAME;
        public static string RoadPrefixPath { get; } = FOLDER_NAME + Path.DirectorySeparatorChar + ROADPREFIX_SUBFOLDER_NAME;
        public static string NeigborsPath { get; } = FOLDER_NAME + Path.DirectorySeparatorChar + NEIGHBOR_SUBFOLDER_NAME;
        public static string DistrictPrefixPath { get; } = FOLDER_NAME + Path.DirectorySeparatorChar + DISTRICT_PREFIXES_SUBFOLDER_NAME;
        public static string DistrictNamePath { get; } = FOLDER_NAME + Path.DirectorySeparatorChar + DISTRICT_NAMES_SUBFOLDER_NAME;
        public static string CitizenFirstNameMascPath { get; } = FOLDER_NAME + Path.DirectorySeparatorChar + CITIZEN_FIRST_NAME_MASC_SUBFOLDER_NAME;
        public static string CitizenFirstNameFemPath { get; } = FOLDER_NAME + Path.DirectorySeparatorChar + CITIZEN_FIRST_NAME_FEM_SUBFOLDER_NAME;
        public static string CitizenLastNamePath { get; } = FOLDER_NAME + Path.DirectorySeparatorChar + CITIZEN_LAST_NAME_SUBFOLDER_NAME;
        public static string HighwayConfigurationFolder { get; } = FOLDER_NAME + Path.DirectorySeparatorChar + HIGHWAY_PREFABS_SUBFOLDER_NAME;
        public static string FootballTeamDataFolder { get; } = FOLDER_NAME + Path.DirectorySeparatorChar + FOOTBALL_TEAM_DATA;


        public static string DefaultFileGlobalXml { get; } = "__DEFAULT.xml";
        public static string HighwayConfigurationDefaultGlobalFile { get; } = $"{HighwayConfigurationFolder}{Path.DirectorySeparatorChar}{DefaultFileGlobalXml}";

    }

}
