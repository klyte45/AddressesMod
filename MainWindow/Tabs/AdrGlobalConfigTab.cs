using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using Klyte.Addresses.ModShared;
using Klyte.Addresses.Xml;
using Klyte.Commons.Extensors;
using Klyte.Commons.UI.SpriteNames;
using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Klyte.Addresses.Xml.AdrRicoNamesGenerationConfig;
using static Klyte.Commons.UI.DefaultEditorUILib;

namespace Klyte.Addresses.UI
{

    internal class AdrGlobalConfigTab : UICustomControl
    {
        public UIPanel MainContainer { get; private set; }

        private UITextField m_prefixPostalCodeCity;
        private UITextField m_postalCodeFormat;
        private UITextField m_addressLine1Format;
        private UITextField m_addressLine2Format;
        private UITextField m_addressLine3Format;

        private UIHelperExtension m_uiHelperGlobal;
        private UIDropDown m_districtPrefixGenFile;
        private UIDropDown m_districtNameGenFile;


        #region Awake
        public void Awake()
        {
            MainContainer = GetComponent<UIPanel>();

            MainContainer.autoLayout = true;
            MainContainer.autoLayoutDirection = LayoutDirection.Vertical;
            MainContainer.autoLayoutPadding = new RectOffset(0, 0, 2, 2);
            m_uiHelperGlobal = new UIHelperExtension(MainContainer);

            CreateGroupFileSelect("K45_ADR_DISTRICT_GEN_PREFIX_FILE", OnChangeSelectedDistrictPrefix, ReloadDistrictPrefixesFiles, out m_districtPrefixGenFile);
            CreateGroupFileSelect("K45_ADR_DISTRICT_GEN_NAME_FILE", OnChangeSelectedDistrictName, ReloadDistrictNamesFiles, out m_districtNameGenFile);
            ReloadDistrictPrefixesFiles();
            ReloadDistrictNamesFiles();

            AddIntField(Locale.Get("K45_ADR_DISTRICT_POSTAL_CODE"), out m_prefixPostalCodeCity, m_uiHelperGlobal, OnChangePostalCodePrefixCity, false);
            m_prefixPostalCodeCity.maxLength = 3;

            AddTextField(Locale.Get("K45_ADR_POSTAL_CODE_FORMAT_2"), out m_postalCodeFormat, m_uiHelperGlobal, OnChangePostalCodeFormat, AdrController.CurrentConfig.GlobalConfig.AddressingConfig.ZipcodeFormat);
            AddButtonInEditorRow(m_postalCodeFormat, CommonsSpriteNames.K45_QuestionMark, () => K45DialogControl.ShowModal(new K45DialogControl.BindProperties
            {
                showButton1 = true,
                textButton1 = Locale.Get("EXCEPTION_OK"),
                title = Locale.Get("K45_ADR_POSTAL_CODE_FORMAT_LEGEND_TITLE"),
                message = GetPostalCodeLegendText()
            }, (x) => true), null, true, 30);


            AddLabel(Locale.Get("K45_ADR_ADDRESS_LINES"), m_uiHelperGlobal, out _, out _);
            AddTextField(Locale.Get("K45_ADR_ADDRESS_LINE1"), out m_addressLine1Format, m_uiHelperGlobal, OnChangeAddressLine1, AdrController.CurrentConfig.GlobalConfig.AddressingConfig.AddressLine1);
            AddTextField(Locale.Get("K45_ADR_ADDRESS_LINE2"), out m_addressLine2Format, m_uiHelperGlobal, OnChangeAddressLine2, AdrController.CurrentConfig.GlobalConfig.AddressingConfig.AddressLine2);
            AddTextField(Locale.Get("K45_ADR_ADDRESS_LINE3"), out m_addressLine3Format, m_uiHelperGlobal, OnChangeAddressLine3, AdrController.CurrentConfig.GlobalConfig.AddressingConfig.AddressLine3);
            var commonPopupConfigHelpAddress = new K45DialogControl.BindProperties
            {
                showButton1 = true,
                textButton1 = Locale.Get("EXCEPTION_OK"),
                title = Locale.Get("K45_ADR_ADDRESS_LINES_FORMAT_LEGEND_TITLE"),
                message = GetAddressLegendText()
            };
            AddButtonInEditorRow(m_addressLine1Format, CommonsSpriteNames.K45_QuestionMark, () => K45DialogControl.ShowModal(commonPopupConfigHelpAddress, (x) => true), null, true, 30);
            AddButtonInEditorRow(m_addressLine2Format, CommonsSpriteNames.K45_QuestionMark, () => K45DialogControl.ShowModal(commonPopupConfigHelpAddress, (x) => true), null, true, 30);
            AddButtonInEditorRow(m_addressLine3Format, CommonsSpriteNames.K45_QuestionMark, () => K45DialogControl.ShowModal(commonPopupConfigHelpAddress, (x) => true), null, true, 30);

            m_uiHelperGlobal.AddSpace(15);

            var nameGenConfig = AdrController.CurrentConfig.GlobalConfig.BuildingConfig.StationsNameGenerationConfig;
            AdrStationNamesGenerationConfig getGenConfig() => AdrController.CurrentConfig.GlobalConfig.BuildingConfig.StationsNameGenerationConfig;
            AdrRicoNamesGenerationConfig getRicoGenConfig() => AdrController.CurrentConfig.GlobalConfig.BuildingConfig.RicoNamesGenerationConfig;

            KlyteMonoUtils.CreateUIElement(out UIPanel buildingTogglePanel, m_uiHelperGlobal.Self.transform);
            buildingTogglePanel.width = MainContainer.width;
            buildingTogglePanel.autoLayout = true;
            buildingTogglePanel.autoLayoutPadding = new RectOffset(2, 2, 2, 2);
            buildingTogglePanel.autoLayoutDirection = LayoutDirection.Horizontal;
            buildingTogglePanel.autoFitChildrenVertically = true;
            buildingTogglePanel.wrapLayout = true;

            var buildingTogglePanelHelper = new UIHelperExtension(buildingTogglePanel);

            void AddBuildingCheckbox(string icon, string locale, Action<bool> onChange, Func<bool> getCurrentVal) => AddIconCheckbox(icon, locale, out _, buildingTogglePanelHelper, (x) => { onChange(x); AdrShared.TriggerBuildingNameStrategyChanged(); }, new Vector2(45, 30), getCurrentVal());

            AddLabel(Locale.Get("K45_ADR_BUILDING_PASSENGERS"), buildingTogglePanelHelper, out UILabel lbl, out _);
            lbl.padding.top = 5;
            lbl.padding.bottom = 5;
            KlyteMonoUtils.LimitWidthAndBox(lbl, 220, out UIPanel panel);
            panel.maximumSize = new Vector2(230, 0);
            panel.minimumSize = new Vector2(230, 0);
            AddBuildingCheckbox("SubBarPublicTransportTrain", "K45_ADR_AUTONAME_TRAIN_STATIONS", x => getGenConfig().TrainsPassenger = x, () => getGenConfig().TrainsPassenger);
            AddBuildingCheckbox("SubBarPublicTransportMonorail", "K45_ADR_AUTONAME_MONORAIL_STATIONS", x => getGenConfig().Monorail = x, () => getGenConfig().Monorail);
            AddBuildingCheckbox("SubBarPublicTransportMetro", "K45_ADR_AUTONAME_METRO_STATIONS", x => getGenConfig().Metro = x, () => getGenConfig().Metro);
            AddBuildingCheckbox("SubBarPublicTransportCableCar", "K45_ADR_AUTONAME_CABLE_CAR_STATIONS", x => getGenConfig().CableCar = x, () => getGenConfig().CableCar);
            AddBuildingCheckbox("FeatureFerry", "K45_ADR_AUTONAME_FERRY_STATIONS", x => getGenConfig().Ferry = x, () => getGenConfig().Ferry);
            AddBuildingCheckbox("SubBarPublicTransportShip", "K45_ADR_AUTONAME_SHIP_STATIONS", x => getGenConfig().ShipPassenger = x, () => getGenConfig().ShipPassenger);
            AddBuildingCheckbox("FeatureBlimp", "K45_ADR_AUTONAME_BLIMP_STATIONS", x => getGenConfig().Blimp = x, () => getGenConfig().Blimp);
            AddBuildingCheckbox("SubBarPublicTransportPlane", "K45_ADR_AUTONAME_AIRPLANE_STATIONS", x => getGenConfig().AirplanePassenger = x, () => getGenConfig().AirplanePassenger);

            buildingTogglePanelHelper.AddSpace(5);
            AddLabel(Locale.Get("K45_ADR_BUILDING_CARGO"), buildingTogglePanelHelper, out lbl, out _);
            lbl.padding.top = 5;
            lbl.padding.bottom = 5;
            KlyteMonoUtils.LimitWidthAndBox(lbl, 220, out  panel);
            panel.maximumSize = new Vector2(230, 0);
            panel.minimumSize = new Vector2(230, 0);
            AddBuildingCheckbox("SubBarPublicTransportShip", "K45_ADR_ENABLE_CUSTOM_NAMING_CARGO_SHIP", x => getGenConfig().ShipCargo = x, () => getGenConfig().ShipCargo);
            AddBuildingCheckbox("SubBarPublicTransportTrain", "K45_ADR_ENABLE_CUSTOM_NAMING_CARGO_TRAIN", x => getGenConfig().TrainsCargo = x, () => getGenConfig().TrainsCargo);
            AddBuildingCheckbox("SubBarPublicTransportPlane", "K45_ADR_ENABLE_CUSTOM_NAMING_CARGO_AIRPLANE", x => getGenConfig().AirplaneCargo = x, () => getGenConfig().AirplaneCargo);

            buildingTogglePanelHelper.AddSpace(5);
            AddLabel(Locale.Get("K45_ADR_BUILDING_USE_ADDRESS"), buildingTogglePanelHelper, out lbl, out _);
            lbl.padding.top = 5;
            lbl.padding.bottom = 5;
            KlyteMonoUtils.LimitWidthAndBox(lbl, 220, out panel);
            panel.maximumSize = new Vector2(230, 0);
            panel.minimumSize = new Vector2(230, 0);
            AddBuildingCheckbox("SubBarDistrictSpecializationResidential", "K45_ADR_ENABLE_ADDRESS_NAMING_RES", x => getRicoGenConfig().Residence = x ? GenerationMethod.ADDRESS : GenerationMethod.NONE, () => getRicoGenConfig().Residence == GenerationMethod.ADDRESS);
            AddBuildingCheckbox("SubBarDistrictSpecializationIndustrial", "K45_ADR_ENABLE_ADDRESS_NAMING_IND", x => getRicoGenConfig().Industry = x ? GenerationMethod.ADDRESS : GenerationMethod.NONE, () => getRicoGenConfig().Industry == GenerationMethod.ADDRESS);
            AddBuildingCheckbox("SubBarDistrictSpecializationCommercial", "K45_ADR_ENABLE_ADDRESS_NAMING_COM", x => getRicoGenConfig().Commerce = x ? GenerationMethod.ADDRESS : GenerationMethod.NONE, () => getRicoGenConfig().Commerce == GenerationMethod.ADDRESS);
            AddBuildingCheckbox("SubBarDistrictSpecializationOffice", "K45_ADR_ENABLE_ADDRESS_NAMING_OFF", x => getRicoGenConfig().Office = x ? GenerationMethod.ADDRESS : GenerationMethod.NONE, () => getRicoGenConfig().Office == GenerationMethod.ADDRESS);
        }

        private string GetPostalCodeLegendText()
        {
            var result = new List<string>();
            var quantityLines = Locale.CountUnchecked("K45_ADR_POSTAL_CODE_FORMAT_LEGEND");
            for (int i = 0; Locale.Exists("K45_ADR_POSTAL_CODE_FORMAT_LEGEND", i); i++)
            {
                result.Add(Locale.Get("K45_ADR_POSTAL_CODE_FORMAT_LEGEND", i));
            }
            result.Add("");
            for (int i = 0; Locale.Exists("K45_ADR_POSTAL_CODE_FORMAT_OBSERVATION", i); i++)
            {
                result.Add($"<color yellow>{Locale.Get("K45_ADR_POSTAL_CODE_FORMAT_OBSERVATION", i)}</color>");
            }
            return string.Join("\n", result.ToArray());
        }

        private string GetAddressLegendText()
        {
            var result = new List<string>();
            for (int i = 0; Locale.Exists("K45_ADR_ADDRESS_FORMAT_LEGEND", i); i++)
            {
                result.Add(Locale.Get("K45_ADR_ADDRESS_FORMAT_LEGEND", i));
            }
            return string.Join("\n", result.ToArray());
        }
        #endregion
        private void CreateGroupFileSelect(string i18n, OnDropdownSelectionChanged onChanged, Action onReload, out UIDropDown dropDown)
        {
            AddDropdown(Locale.Get(i18n), out dropDown, m_uiHelperGlobal, new string[0], onChanged);
            AddButtonInEditorRow(dropDown, Commons.UI.SpriteNames.CommonsSpriteNames.K45_Reload, onReload, "K45_ADR_ROAD_NAME_FILES_RELOAD");
            onReload.Invoke();
        }


        private void OnChangePostalCodePrefixCity(int val) => AdrController.CurrentConfig.GlobalConfig.AddressingConfig.ZipcodeCityPrefix = val;

        private string SaveIfNotEmpty(UITextField input, string val)
        {
            string field;
            val = val?.Trim();
            if (val?.Length == 0)
            {
                field = null;
            }
            else
            {
                field = val;
            }
            input.text = field;
            return field;
        }

        private void OnChangePostalCodeFormat(string val) => AdrController.CurrentConfig.GlobalConfig.AddressingConfig.ZipcodeFormat = SaveIfNotEmpty(m_postalCodeFormat, val);
        private void OnChangeAddressLine1(string val) => AdrController.CurrentConfig.GlobalConfig.AddressingConfig.AddressLine1 = SaveIfNotEmpty(m_addressLine1Format, val);
        private void OnChangeAddressLine2(string val) => AdrController.CurrentConfig.GlobalConfig.AddressingConfig.AddressLine2 = SaveIfNotEmpty(m_addressLine2Format, val);
        private void OnChangeAddressLine3(string val) => AdrController.CurrentConfig.GlobalConfig.AddressingConfig.AddressLine3 = SaveIfNotEmpty(m_addressLine3Format, val);

        private void ReloadFiles(Action reloadAction, string selectedOption, List<string> referenceList, UIDropDown ddRef, string optionZeroText)
        {
            reloadAction();
            referenceList.Insert(0, optionZeroText);
            ddRef.items = referenceList.ToArray();
            if (referenceList.Contains(selectedOption))
            {
                ddRef.selectedValue = selectedOption;
            }
            else
            {
                ddRef.selectedIndex = 0;
            }
            DistrictManager.instance.NamesModified();
        }
        private void ReloadDistrictPrefixesFiles() => ReloadFiles(
            AdrController.LoadLocalesDistrictPrefix,
            AdrController.CurrentConfig.GlobalConfig.AddressingConfig.DistrictsConfig.QualifierFile,
            AdrController.LoadedLocalesDistrictPrefix.Keys.ToList(),
            m_districtPrefixGenFile,
            Locale.Get("K45_ADR_DEFAULT_FILE_NAME")
            );

        private void ReloadDistrictNamesFiles() => ReloadFiles(
            AdrController.LoadLocalesDistrictName,
            AdrController.CurrentConfig.GlobalConfig.AddressingConfig.DistrictsConfig.NamesFile,
            AdrController.LoadedLocalesDistrictName.Keys.ToList(),
            m_districtNameGenFile,
            Locale.Get("K45_ADR_DEFAULT_FILE_NAME")
            );


        private void OnChangeSelectedDistrictPrefix(int idx)
        {
            AdrController.CurrentConfig.GlobalConfig.AddressingConfig.DistrictsConfig.QualifierFile = (idx > 0 ? m_districtPrefixGenFile.selectedValue : null);
            DistrictManager.instance.NamesModified();
        }
        private void OnChangeSelectedDistrictName(int idx)
        {
            AdrController.CurrentConfig.GlobalConfig.AddressingConfig.DistrictsConfig.NamesFile = (idx > 0 ? m_districtNameGenFile.selectedValue : null);
            DistrictManager.instance.NamesModified();
        }

    }

}
