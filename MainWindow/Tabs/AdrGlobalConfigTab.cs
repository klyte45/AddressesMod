using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Addresses.ModShared;
using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Klyte.Addresses.Xml.AdrRicoNamesGenerationConfig;

namespace Klyte.Addresses.UI
{

    internal class AdrGlobalConfigTab : UICustomControl
    {
        public UIComponent MainContainer { get; private set; }

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
            MainContainer = GetComponent<UIComponent>();
            m_uiHelperGlobal = new UIHelperExtension(MainContainer);

            ((UIScrollablePanel) m_uiHelperGlobal.Self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIScrollablePanel) m_uiHelperGlobal.Self).wrapLayout = true;
            ((UIScrollablePanel) m_uiHelperGlobal.Self).width = 370;

            m_districtPrefixGenFile = m_uiHelperGlobal.AddDropdownLocalized("K45_ADR_DISTRICT_GEN_PREFIX_FILE", new string[0], -1, OnChangeSelectedDistrictPrefix);
            m_districtPrefixGenFile.width = 370;
            m_uiHelperGlobal.AddSpace(1);
            KlyteMonoUtils.LimitWidth((UIButton) m_uiHelperGlobal.AddButton(Locale.Get("K45_ADR_DISTRICT_GEN_PREFIX_FILES_RELOAD"), ReloadDistrictPrefixesFiles), 380);
            m_uiHelperGlobal.AddSpace(20);

            m_districtNameGenFile = m_uiHelperGlobal.AddDropdownLocalized("K45_ADR_DISTRICT_GEN_NAME_FILE", new string[0], -1, OnChangeSelectedDistrictName);
            m_districtNameGenFile.width = 370;
            m_uiHelperGlobal.AddSpace(1);
            KlyteMonoUtils.LimitWidth((UIButton) m_uiHelperGlobal.AddButton(Locale.Get("K45_ADR_DISTRICT_GEN_NAME_FILES_RELOAD"), ReloadDistrictNamesFiles), 380);
            m_uiHelperGlobal.AddSpace(20);

            ReloadDistrictPrefixesFiles();
            ReloadDistrictNamesFiles();

            m_prefixPostalCodeCity = m_uiHelperGlobal.AddTextField(Locale.Get("K45_ADR_CITY_POSTAL_CODE"), null, AdrController.CurrentConfig.GlobalConfig.AddressingConfig.ZipcodeCityPrefix.ToString("D3"), OnChangePostalCodePrefixCity);
            m_prefixPostalCodeCity.numericalOnly = true;
            m_prefixPostalCodeCity.maxLength = 3;

            m_postalCodeFormat = m_uiHelperGlobal.AddTextField(Locale.Get("K45_ADR_POSTAL_CODE_FORMAT"), null, AdrController.CurrentConfig.GlobalConfig.AddressingConfig.ZipcodeFormat, OnChangePostalCodeFormat);
            m_prefixPostalCodeCity.maxLength = 3;

            string[] formatExplain = new string[12];
            for (int i = 0; i < formatExplain.Length; i++)
            {
                formatExplain[i] = Locale.Get("K45_ADR_POSTAL_CODE_FORMAT_LEGEND", i);
            }

            KlyteMonoUtils.CreateUIElement(out UILabel formatExplanation, m_uiHelperGlobal.Self.transform, "FormatText");
            formatExplanation.wordWrap = true;
            formatExplanation.textScale = 0.65f;
            formatExplanation.textColor = Color.yellow;
            formatExplanation.autoSize = false;
            formatExplanation.autoHeight = true;
            formatExplanation.width = 370;

            string[] obs = new string[2];
            for (int i = 0; i < obs.Length; i++)
            {
                obs[i] = Locale.Get("K45_ADR_POSTAL_CODE_FORMAT_OBSERVATION", i);
            }

            KlyteMonoUtils.CreateUIElement(out UILabel obsExplanation, m_uiHelperGlobal.Self.transform, "ObsText");
            obsExplanation.wordWrap = true;
            obsExplanation.textScale = 0.65f;
            obsExplanation.textColor = Color.gray;
            obsExplanation.autoSize = false;
            obsExplanation.autoHeight = true;
            obsExplanation.width = 370;


            formatExplanation.text = "∙ " + string.Join(Environment.NewLine + "∙ ", formatExplain);
            obsExplanation.text = string.Join(Environment.NewLine, obs);

            m_uiHelperGlobal.AddSpace(20);
            m_uiHelperGlobal.AddLabel(Locale.Get("K45_ADR_ADDRESS_LINES"));
            m_addressLine1Format = m_uiHelperGlobal.AddTextField(Locale.Get("K45_ADR_ADDRESS_LINE1"), null, AdrController.CurrentConfig.GlobalConfig.AddressingConfig.AddressLine1, OnChangeAddressLine1);
            m_addressLine2Format = m_uiHelperGlobal.AddTextField(Locale.Get("K45_ADR_ADDRESS_LINE2"), null, AdrController.CurrentConfig.GlobalConfig.AddressingConfig.AddressLine2, OnChangeAddressLine2);
            m_addressLine3Format = m_uiHelperGlobal.AddTextField(Locale.Get("K45_ADR_ADDRESS_LINE3"), null, AdrController.CurrentConfig.GlobalConfig.AddressingConfig.AddressLine3, OnChangeAddressLine3);
            string[] formatExplainAddress = new string[6];
            for (int i = 0; i < formatExplainAddress.Length; i++)
            {
                formatExplainAddress[i] = Locale.Get("K45_ADR_ADDRESS_FORMAT_LEGEND", i);
            }

            KlyteMonoUtils.CreateUIElement(out UILabel formatExplainAddressLabel, m_uiHelperGlobal.Self.transform, "FormatText");
            formatExplainAddressLabel.wordWrap = true;
            formatExplainAddressLabel.textScale = 0.65f;
            formatExplainAddressLabel.textColor = Color.yellow;
            formatExplainAddressLabel.autoSize = false;
            formatExplainAddressLabel.autoHeight = true;
            formatExplainAddressLabel.width = 370;

            formatExplainAddressLabel.text = "∙ " + string.Join(Environment.NewLine + "∙ ", formatExplainAddress);


            m_uiHelperGlobal.AddSpace(20);
            m_uiHelperGlobal.AddLabel(Locale.Get("K45_ADR_BUILDING_CONFIGURATIONS"));

            m_uiHelperGlobal.AddCheckboxLocale("K45_ADR_AUTONAME_TRAIN_STATIONS", AdrController.CurrentConfig.GlobalConfig.BuildingConfig.StationsNameGenerationConfig.TrainsPassenger,             (x) => { AdrController.CurrentConfig.GlobalConfig.BuildingConfig.StationsNameGenerationConfig.TrainsPassenger = x ;AdrEvents.TriggerBuildingNameStrategyChanged();});
            m_uiHelperGlobal.AddCheckboxLocale("K45_ADR_AUTONAME_MONORAIL_STATIONS", AdrController.CurrentConfig.GlobalConfig.BuildingConfig.StationsNameGenerationConfig.Monorail,                 (x) => {AdrController.CurrentConfig.GlobalConfig.BuildingConfig.StationsNameGenerationConfig.Monorail = x         ;AdrEvents.TriggerBuildingNameStrategyChanged();});
            m_uiHelperGlobal.AddCheckboxLocale("K45_ADR_AUTONAME_METRO_STATIONS", AdrController.CurrentConfig.GlobalConfig.BuildingConfig.StationsNameGenerationConfig.Metro,                       (x) => {AdrController.CurrentConfig.GlobalConfig.BuildingConfig.StationsNameGenerationConfig.Metro = x            ;AdrEvents.TriggerBuildingNameStrategyChanged();});
            m_uiHelperGlobal.AddCheckboxLocale("K45_ADR_AUTONAME_CABLE_CAR_STATIONS", AdrController.CurrentConfig.GlobalConfig.BuildingConfig.StationsNameGenerationConfig.CableCar,                (x) => {AdrController.CurrentConfig.GlobalConfig.BuildingConfig.StationsNameGenerationConfig.CableCar = x         ;AdrEvents.TriggerBuildingNameStrategyChanged();});
            m_uiHelperGlobal.AddCheckboxLocale("K45_ADR_AUTONAME_FERRY_STATIONS", AdrController.CurrentConfig.GlobalConfig.BuildingConfig.StationsNameGenerationConfig.Ferry,                       (x) => {AdrController.CurrentConfig.GlobalConfig.BuildingConfig.StationsNameGenerationConfig.Ferry = x            ;AdrEvents.TriggerBuildingNameStrategyChanged();});
            m_uiHelperGlobal.AddCheckboxLocale("K45_ADR_AUTONAME_SHIP_STATIONS", AdrController.CurrentConfig.GlobalConfig.BuildingConfig.StationsNameGenerationConfig.ShipPassenger,                (x) => {AdrController.CurrentConfig.GlobalConfig.BuildingConfig.StationsNameGenerationConfig.ShipPassenger = x    ;AdrEvents.TriggerBuildingNameStrategyChanged();});
            m_uiHelperGlobal.AddCheckboxLocale("K45_ADR_AUTONAME_BLIMP_STATIONS", AdrController.CurrentConfig.GlobalConfig.BuildingConfig.StationsNameGenerationConfig.Blimp,                       (x) => {AdrController.CurrentConfig.GlobalConfig.BuildingConfig.StationsNameGenerationConfig.Blimp = x            ;AdrEvents.TriggerBuildingNameStrategyChanged();});
            m_uiHelperGlobal.AddCheckboxLocale("K45_ADR_AUTONAME_AIRPLANE_STATIONS", AdrController.CurrentConfig.GlobalConfig.BuildingConfig.StationsNameGenerationConfig.AirplanePassenger,        (x) => {AdrController.CurrentConfig.GlobalConfig.BuildingConfig.StationsNameGenerationConfig.AirplanePassenger = x;AdrEvents.TriggerBuildingNameStrategyChanged();});
                                                                                                                                                                                                                                                                                                                                                                     
            m_uiHelperGlobal.AddCheckboxLocale("K45_ADR_ENABLE_CUSTOM_NAMING_CARGO_SHIP", AdrController.CurrentConfig.GlobalConfig.BuildingConfig.StationsNameGenerationConfig.ShipCargo,           (x) => {AdrController.CurrentConfig.GlobalConfig.BuildingConfig.StationsNameGenerationConfig.ShipCargo = x        ;AdrEvents.TriggerBuildingNameStrategyChanged();});
            m_uiHelperGlobal.AddCheckboxLocale("K45_ADR_ENABLE_CUSTOM_NAMING_CARGO_TRAIN", AdrController.CurrentConfig.GlobalConfig.BuildingConfig.StationsNameGenerationConfig.TrainsCargo,        (x) => {AdrController.CurrentConfig.GlobalConfig.BuildingConfig.StationsNameGenerationConfig.TrainsCargo = x      ;AdrEvents.TriggerBuildingNameStrategyChanged();});
            m_uiHelperGlobal.AddCheckboxLocale("K45_ADR_ENABLE_CUSTOM_NAMING_CARGO_AIRPLANE", AdrController.CurrentConfig.GlobalConfig.BuildingConfig.StationsNameGenerationConfig.AirplaneCargo,   (x) => {AdrController.CurrentConfig.GlobalConfig.BuildingConfig.StationsNameGenerationConfig.AirplaneCargo = x    ;AdrEvents.TriggerBuildingNameStrategyChanged(); });



            m_uiHelperGlobal.AddCheckboxLocale("K45_ADR_ENABLE_ADDRESS_NAMING_RES", AdrController.CurrentConfig.GlobalConfig.BuildingConfig.RicoNamesGenerationConfig.Residence == GenerationMethod.ADDRESS,    (x) =>{AdrController.CurrentConfig.GlobalConfig.BuildingConfig.RicoNamesGenerationConfig.Residence = x ? GenerationMethod.ADDRESS : GenerationMethod.NONE ;AdrEvents.TriggerBuildingNameStrategyChanged();}  );
            m_uiHelperGlobal.AddCheckboxLocale("K45_ADR_ENABLE_ADDRESS_NAMING_IND", AdrController.CurrentConfig.GlobalConfig.BuildingConfig.RicoNamesGenerationConfig.Industry == GenerationMethod.ADDRESS,     (x) =>{AdrController.CurrentConfig.GlobalConfig.BuildingConfig.RicoNamesGenerationConfig.Industry = x ? GenerationMethod.ADDRESS : GenerationMethod.NONE  ;AdrEvents.TriggerBuildingNameStrategyChanged();}  );
            m_uiHelperGlobal.AddCheckboxLocale("K45_ADR_ENABLE_ADDRESS_NAMING_COM", AdrController.CurrentConfig.GlobalConfig.BuildingConfig.RicoNamesGenerationConfig.Commerce == GenerationMethod.ADDRESS,     (x) =>{AdrController.CurrentConfig.GlobalConfig.BuildingConfig.RicoNamesGenerationConfig.Commerce = x ? GenerationMethod.ADDRESS : GenerationMethod.NONE  ;AdrEvents.TriggerBuildingNameStrategyChanged();}  );
            m_uiHelperGlobal.AddCheckboxLocale("K45_ADR_ENABLE_ADDRESS_NAMING_OFF", AdrController.CurrentConfig.GlobalConfig.BuildingConfig.RicoNamesGenerationConfig.Office == GenerationMethod.ADDRESS,       (x) =>{AdrController.CurrentConfig.GlobalConfig.BuildingConfig.RicoNamesGenerationConfig.Office = x ? GenerationMethod.ADDRESS : GenerationMethod.NONE; AdrEvents.TriggerBuildingNameStrategyChanged(); });
        }
        #endregion



        private void OnChangePostalCodePrefixCity(string val)
        {
            if (!int.TryParse(val, out int valInt))
            {
                return;
            }

            AdrController.CurrentConfig.GlobalConfig.AddressingConfig.ZipcodeCityPrefix = valInt;
        }

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
