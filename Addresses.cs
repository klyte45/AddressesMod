using ColossalFramework;
using ColossalFramework.DataBinding;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using Klyte.Addresses.i18n;
using Klyte.Addresses.TextureAtlas;
using Klyte.Addresses.UI;
using Klyte.Addresses.Utils;
using Klyte.Commons.Extensors;
using Klyte.Commons.Interfaces;
using Klyte.Commons.UI;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

[assembly: AssemblyVersion("1.4.9999.*")]

namespace Klyte.Addresses
{

    public class AddressesMod : BasicIUserMod<AddressesMod, AdrLocaleUtils, AdrResourceLoader, AdrController, AdrCommonTextureAtlas, AdrConfigPanel>
    {

        public AddressesMod()
        {
            Construct();
        }

        protected override ModTab? Tab => ModTab.Addresses;

        public override string SimpleName => "Addresses & Names Mod";
        public override string Description => "TLMR's Extension which allow road name generation customization. Requires Klyte Commons subscribed and active.";

        public override void doErrorLog(string fmt, params object[] args)
        {
            AdrUtils.doErrorLog(fmt, args);
        }

        public override void doLog(string fmt, params object[] args)
        {
            AdrUtils.doLog(fmt, args);
        }

        public override void LoadSettings()
        {
        }

        public override void TopSettingsUI(UIHelperExtension helper)
        {
            UIHelperExtension group8 = helper.AddGroupExtended(Locale.Get("ADR_GENERAL_INFO"));
            addFolderButton(roadPath, group8, "ADR_ROAD_NAME_FILES_PATH_TITLE");
            addFolderButton(roadPrefixPath, group8, "ADR_ROAD_PREFIX_NAME_FILES_PATH_TITLE");
            addFolderButton(neigborsPath, group8, "ADR_NEIGHBOR_CITIES_NAME_FILES_PATH_TITLE");
            addFolderButton(districtPrefixPath, group8, "ADR_DISTRICT_PREFIXES_FILES_PATH_TITLE");
            addFolderButton(districtNamePath, group8, "ADR_DISTRICT_NAME_FILES_PATH_TITLE");
            addFolderButton(citizenFirstNameMascPath, group8, "ADR_CITIZEN_FIRST_NAME_MASC_FILES_PATH_TITLE");
            addFolderButton(citizenFirstNameFemPath, group8, "ADR_CITIZEN_FIRST_NAME_FEM_FILES_PATH_TITLE");
            addFolderButton(citizenLastNamePath, group8, "ADR_CITIZEN_LAST_NAME_FILES_PATH_TITLE");


            UIHelperExtension group7 = helper.AddGroupExtended(Locale.Get("ADR_ADDITIONAL_FILES_SOURCE"));
            group7.AddLabel(Locale.Get("ADR_GET_FILES_GITHUB"));
            group7.AddButton(Locale.Get("ADR_GO_TO_GITHUB"), () => { Application.OpenURL("https://github.com/klyte45/AddressesFiles"); });
        }

        private static void addFolderButton(string filePath, UIHelperExtension helper, string localeId)
        {
            var fileInfo = AdrUtils.EnsureFolderCreation(filePath);
            helper.AddLabel(Locale.Get(localeId) + ":");
            var namesFilesButton = ((UIButton)helper.AddButton("/", () => { ColossalFramework.Utils.OpenInFileBrowser(fileInfo.FullName); }));
            namesFilesButton.textColor = Color.yellow;
            AdrUtils.LimitWidth(namesFilesButton, 710);
            namesFilesButton.text = fileInfo.FullName + Path.DirectorySeparatorChar;
        }


        public static readonly string FOLDER_NAME = AdrUtils.BASE_FOLDER_PATH + "AddressesNames";
        public const string ROAD_SUBFOLDER_NAME = "Roads";
        public const string ROADPREFIX_SUBFOLDER_NAME = "RoadsPrefix";
        public const string NEIGHBOR_SUBFOLDER_NAME = "RegionCities";
        public const string DISTRICT_PREFIXES_SUBFOLDER_NAME = "DistrictPrefix";
        public const string DISTRICT_NAMES_SUBFOLDER_NAME = "DistrictName";
        public const string CITIZEN_FIRST_NAME_MASC_SUBFOLDER_NAME = "CitizenFirstNameMale";
        public const string CITIZEN_FIRST_NAME_FEM_SUBFOLDER_NAME = "CitizenFirstNameFemale";
        public const string CITIZEN_LAST_NAME_SUBFOLDER_NAME = "CitizenLastName";

        public static string roadPath => FOLDER_NAME + Path.DirectorySeparatorChar + ROAD_SUBFOLDER_NAME;
        public static string roadPrefixPath => FOLDER_NAME + Path.DirectorySeparatorChar + ROADPREFIX_SUBFOLDER_NAME;
        public static string neigborsPath => FOLDER_NAME + Path.DirectorySeparatorChar + NEIGHBOR_SUBFOLDER_NAME;
        public static string districtPrefixPath => FOLDER_NAME + Path.DirectorySeparatorChar + DISTRICT_PREFIXES_SUBFOLDER_NAME;
        public static string districtNamePath => FOLDER_NAME + Path.DirectorySeparatorChar + DISTRICT_NAMES_SUBFOLDER_NAME;
        public static string citizenFirstNameMascPath => FOLDER_NAME + Path.DirectorySeparatorChar + CITIZEN_FIRST_NAME_MASC_SUBFOLDER_NAME;
        public static string citizenFirstNameFemPath => FOLDER_NAME + Path.DirectorySeparatorChar + CITIZEN_FIRST_NAME_FEM_SUBFOLDER_NAME;
        public static string citizenLastNamePath => FOLDER_NAME + Path.DirectorySeparatorChar + CITIZEN_LAST_NAME_SUBFOLDER_NAME;

    }

}
