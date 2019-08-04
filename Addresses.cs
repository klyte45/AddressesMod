using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Addresses.TextureAtlas;
using Klyte.Addresses.UI;
using Klyte.Addresses.Utils;
using Klyte.Commons.Extensors;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using System.IO;
using System.Reflection;
using UnityEngine;

[assembly: AssemblyVersion("2.0.1.1")]

namespace Klyte.Addresses
{

    public class AddressesMod : BasicIUserMod<AddressesMod, AdrResourceLoader, AdrController, AdrCommonTextureAtlas, AdrConfigPanel, AdrCommonTextureAtlas.SpriteNames>
    {

        public AddressesMod() => Construct();


        public override string SimpleName => "Addresses & Names Mod";
        public override string Description => "Allow road name generation customization.";

        public override void DoErrorLog(string fmt, params object[] args) => LogUtils.DoErrorLog(fmt, args);

        public override void DoLog(string fmt, params object[] args) => LogUtils.DoLog(fmt, args);

        public override void LoadSettings()
        {
        }

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


            UIHelperExtension group7 = helper.AddGroupExtended(Locale.Get("K45_ADR_ADDITIONAL_FILES_SOURCE"));
            group7.AddLabel(Locale.Get("K45_ADR_GET_FILES_GITHUB"));
            group7.AddButton(Locale.Get("K45_ADR_GO_TO_GITHUB"), () => Application.OpenURL("https://github.com/klyte45/AddressesFiles"));
        }

        private static void AddFolderButton(string filePath, UIHelperExtension helper, string localeId)
        {
            FileInfo fileInfo = FileUtils.EnsureFolderCreation(filePath);
            helper.AddLabel(Locale.Get(localeId) + ":");
            UIButton namesFilesButton = ((UIButton) helper.AddButton("/", () => ColossalFramework.Utils.OpenInFileBrowser(fileInfo.FullName)));
            namesFilesButton.textColor = Color.yellow;
            KlyteMonoUtils.LimitWidth(namesFilesButton, 710);
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

        public static string RoadPath => FOLDER_NAME + Path.DirectorySeparatorChar + ROAD_SUBFOLDER_NAME;
        public static string RoadPrefixPath => FOLDER_NAME + Path.DirectorySeparatorChar + ROADPREFIX_SUBFOLDER_NAME;
        public static string NeigborsPath => FOLDER_NAME + Path.DirectorySeparatorChar + NEIGHBOR_SUBFOLDER_NAME;
        public static string DistrictPrefixPath => FOLDER_NAME + Path.DirectorySeparatorChar + DISTRICT_PREFIXES_SUBFOLDER_NAME;
        public static string DistrictNamePath => FOLDER_NAME + Path.DirectorySeparatorChar + DISTRICT_NAMES_SUBFOLDER_NAME;
        public static string CitizenFirstNameMascPath => FOLDER_NAME + Path.DirectorySeparatorChar + CITIZEN_FIRST_NAME_MASC_SUBFOLDER_NAME;
        public static string CitizenFirstNameFemPath => FOLDER_NAME + Path.DirectorySeparatorChar + CITIZEN_FIRST_NAME_FEM_SUBFOLDER_NAME;
        public static string CitizenLastNamePath => FOLDER_NAME + Path.DirectorySeparatorChar + CITIZEN_LAST_NAME_SUBFOLDER_NAME;


    }

}
