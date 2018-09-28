using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Addresses.Utils;
using Klyte.Commons.Extensors;
using System;
using UnityEngine;

namespace Klyte.Addresses.UI
{

    internal class AdrGlobalConfigTab : UICustomControl
    {
        public UIComponent mainContainer { get; private set; }

        private UITextField m_prefixPostalCodeCity;
        private UITextField m_postalCodeFormat;
        private UITextField m_addressLine1Format;
        private UITextField m_addressLine2Format;
        private UITextField m_addressLine3Format;
        private UICheckBox m_enableStationAutoName;

        private UIHelperExtension m_uiHelperGlobal;

        #region Awake
        private void Awake()
        {
            mainContainer = GetComponent<UIComponent>();
            m_uiHelperGlobal = new UIHelperExtension(mainContainer);

            ((UIScrollablePanel)m_uiHelperGlobal.self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIScrollablePanel)m_uiHelperGlobal.self).wrapLayout = true;
            ((UIScrollablePanel)m_uiHelperGlobal.self).width = 370;

            m_prefixPostalCodeCity = m_uiHelperGlobal.AddTextField(Locale.Get("ADR_CITY_POSTAL_CODE"), null, AdrConfigWarehouse.getCurrentConfigInt(AdrConfigWarehouse.ConfigIndex.ZIPCODE_CITY_PREFIX).ToString(), onChangePostalCodePrefixCity);
            m_prefixPostalCodeCity.numericalOnly = true;
            m_prefixPostalCodeCity.maxLength = 3;

            m_postalCodeFormat = m_uiHelperGlobal.AddTextField(Locale.Get("ADR_POSTAL_CODE_FORMAT"), null, AdrConfigWarehouse.getCurrentConfigString(AdrConfigWarehouse.ConfigIndex.ZIPCODE_FORMAT), onChangePostalCodeFormat);
            m_prefixPostalCodeCity.maxLength = 3;

            string[] formatExplain = new string[12];
            for (int i = 0; i < formatExplain.Length; i++)
            {
                formatExplain[i] = Locale.Get("ADR_POSTAL_CODE_FORMAT_LEGEND", i);
            }

            AdrUtils.createUIElement(out UILabel formatExplanation, m_uiHelperGlobal.self.transform, "FormatText");
            formatExplanation.wordWrap = true;
            formatExplanation.textScale = 0.65f;
            formatExplanation.textColor = Color.yellow;
            formatExplanation.autoSize = false;
            formatExplanation.autoHeight = true;
            formatExplanation.width = 370;

            string[] obs = new string[2];
            for (int i = 0; i < obs.Length; i++)
            {
                obs[i] = Locale.Get("ADR_POSTAL_CODE_FORMAT_OBSERVATION", i);
            }

            AdrUtils.createUIElement(out UILabel obsExplanation, m_uiHelperGlobal.self.transform, "ObsText");
            obsExplanation.wordWrap = true;
            obsExplanation.textScale = 0.65f;
            obsExplanation.textColor = Color.gray;
            obsExplanation.autoSize = false;
            obsExplanation.autoHeight = true;
            obsExplanation.width = 370;


            formatExplanation.text = "∙ " + string.Join(Environment.NewLine + "∙ ", formatExplain);
            obsExplanation.text = string.Join(Environment.NewLine, obs);

            m_uiHelperGlobal.AddSpace(20);
            m_uiHelperGlobal.AddLabel(Locale.Get("ADR_ADDRESS_LINES"));
            m_addressLine1Format = m_uiHelperGlobal.AddTextField(Locale.Get("ADR_ADDRESS_LINE1"), null, AdrConfigWarehouse.getCurrentConfigString(AdrConfigWarehouse.ConfigIndex.ADDRESS_FORMAT_LINE1), onChangeAddressLine1);
            m_addressLine2Format = m_uiHelperGlobal.AddTextField(Locale.Get("ADR_ADDRESS_LINE2"), null, AdrConfigWarehouse.getCurrentConfigString(AdrConfigWarehouse.ConfigIndex.ADDRESS_FORMAT_LINE2), onChangeAddressLine2);
            m_addressLine3Format = m_uiHelperGlobal.AddTextField(Locale.Get("ADR_ADDRESS_LINE3"), null, AdrConfigWarehouse.getCurrentConfigString(AdrConfigWarehouse.ConfigIndex.ADDRESS_FORMAT_LINE3), onChangeAddressLine3);
            string[] formatExplainAddress = new string[6];
            for (int i = 0; i < formatExplainAddress.Length; i++)
            {
                formatExplainAddress[i] = Locale.Get("ADR_ADDRESS_FORMAT_LEGEND", i);
            }

            AdrUtils.createUIElement(out UILabel formatExplainAddressLabel, m_uiHelperGlobal.self.transform, "FormatText");
            formatExplainAddressLabel.wordWrap = true;
            formatExplainAddressLabel.textScale = 0.65f;
            formatExplainAddressLabel.textColor = Color.yellow;
            formatExplainAddressLabel.autoSize = false;
            formatExplainAddressLabel.autoHeight = true;
            formatExplainAddressLabel.width = 370;

            formatExplainAddressLabel.text = "∙ " + string.Join(Environment.NewLine + "∙ ", formatExplainAddress);


            m_uiHelperGlobal.AddSpace(20);
            m_uiHelperGlobal.AddLabel(Locale.Get("ADR_BUILDING_CONFIGURATIONS"));
            m_uiHelperGlobal.AddCheckboxLocale("ADR_AUTONAME_TRAIN_STATIONS", AdrConfigWarehouse.getCurrentConfigBool(AdrConfigWarehouse.ConfigIndex.ENABLE_CUSTOM_NAMING_STATIONS_TRAIN), (x) => AdrConfigWarehouse.setCurrentConfigBool(AdrConfigWarehouse.ConfigIndex.ENABLE_CUSTOM_NAMING_STATIONS_TRAIN, x));
            m_uiHelperGlobal.AddCheckboxLocale("ADR_AUTONAME_MONORAIL_STATIONS", AdrConfigWarehouse.getCurrentConfigBool(AdrConfigWarehouse.ConfigIndex.ENABLE_CUSTOM_NAMING_STATIONS_MONORAIL), (x) => AdrConfigWarehouse.setCurrentConfigBool(AdrConfigWarehouse.ConfigIndex.ENABLE_CUSTOM_NAMING_STATIONS_MONORAIL, x));
            m_uiHelperGlobal.AddCheckboxLocale("ADR_AUTONAME_METRO_STATIONS", AdrConfigWarehouse.getCurrentConfigBool(AdrConfigWarehouse.ConfigIndex.ENABLE_CUSTOM_NAMING_STATIONS_METRO), (x) => AdrConfigWarehouse.setCurrentConfigBool(AdrConfigWarehouse.ConfigIndex.ENABLE_CUSTOM_NAMING_STATIONS_METRO, x));
            m_uiHelperGlobal.AddCheckboxLocale("ADR_AUTONAME_CABLE_CAR_STATIONS", AdrConfigWarehouse.getCurrentConfigBool(AdrConfigWarehouse.ConfigIndex.ENABLE_CUSTOM_NAMING_STATIONS_CABLE_CAR), (x) => AdrConfigWarehouse.setCurrentConfigBool(AdrConfigWarehouse.ConfigIndex.ENABLE_CUSTOM_NAMING_STATIONS_CABLE_CAR, x));
            m_uiHelperGlobal.AddCheckboxLocale("ADR_AUTONAME_FERRY_STATIONS", AdrConfigWarehouse.getCurrentConfigBool(AdrConfigWarehouse.ConfigIndex.ENABLE_CUSTOM_NAMING_STATIONS_FERRY), (x) => AdrConfigWarehouse.setCurrentConfigBool(AdrConfigWarehouse.ConfigIndex.ENABLE_CUSTOM_NAMING_STATIONS_FERRY, x));
            m_uiHelperGlobal.AddCheckboxLocale("ADR_AUTONAME_SHIP_STATIONS", AdrConfigWarehouse.getCurrentConfigBool(AdrConfigWarehouse.ConfigIndex.ENABLE_CUSTOM_NAMING_STATIONS_SHIP), (x) => AdrConfigWarehouse.setCurrentConfigBool(AdrConfigWarehouse.ConfigIndex.ENABLE_CUSTOM_NAMING_STATIONS_SHIP, x));
            m_uiHelperGlobal.AddCheckboxLocale("ADR_AUTONAME_BLIMP_STATIONS", AdrConfigWarehouse.getCurrentConfigBool(AdrConfigWarehouse.ConfigIndex.ENABLE_CUSTOM_NAMING_STATIONS_BLIMP), (x) => AdrConfigWarehouse.setCurrentConfigBool(AdrConfigWarehouse.ConfigIndex.ENABLE_CUSTOM_NAMING_STATIONS_BLIMP, x));
            m_uiHelperGlobal.AddCheckboxLocale("ADR_AUTONAME_AIRPLANE_STATIONS", AdrConfigWarehouse.getCurrentConfigBool(AdrConfigWarehouse.ConfigIndex.ENABLE_CUSTOM_NAMING_STATIONS_AIRPLANE), (x) => AdrConfigWarehouse.setCurrentConfigBool(AdrConfigWarehouse.ConfigIndex.ENABLE_CUSTOM_NAMING_STATIONS_AIRPLANE, x));

            m_uiHelperGlobal.AddCheckboxLocale("ADR_ENABLE_CUSTOM_NAMING_CARGO_SHIP", AdrConfigWarehouse.getCurrentConfigBool(AdrConfigWarehouse.ConfigIndex.ENABLE_CUSTOM_NAMING_CARGO_SHIP), (x) => AdrConfigWarehouse.setCurrentConfigBool(AdrConfigWarehouse.ConfigIndex.ENABLE_CUSTOM_NAMING_CARGO_SHIP, x));
            m_uiHelperGlobal.AddCheckboxLocale("ADR_ENABLE_CUSTOM_NAMING_CARGO_TRAIN", AdrConfigWarehouse.getCurrentConfigBool(AdrConfigWarehouse.ConfigIndex.ENABLE_CUSTOM_NAMING_CARGO_TRAIN), (x) => AdrConfigWarehouse.setCurrentConfigBool(AdrConfigWarehouse.ConfigIndex.ENABLE_CUSTOM_NAMING_CARGO_TRAIN, x));
            m_uiHelperGlobal.AddCheckboxLocale("ADR_ENABLE_ADDRESS_NAMING_RES", AdrConfigWarehouse.getCurrentConfigBool(AdrConfigWarehouse.ConfigIndex.ENABLE_ADDRESS_NAMING_RES), (x) => AdrConfigWarehouse.setCurrentConfigBool(AdrConfigWarehouse.ConfigIndex.ENABLE_ADDRESS_NAMING_RES, x));
            m_uiHelperGlobal.AddCheckboxLocale("ADR_ENABLE_ADDRESS_NAMING_IND", AdrConfigWarehouse.getCurrentConfigBool(AdrConfigWarehouse.ConfigIndex.ENABLE_ADDRESS_NAMING_IND), (x) => AdrConfigWarehouse.setCurrentConfigBool(AdrConfigWarehouse.ConfigIndex.ENABLE_ADDRESS_NAMING_IND, x));
            m_uiHelperGlobal.AddCheckboxLocale("ADR_ENABLE_ADDRESS_NAMING_COM", AdrConfigWarehouse.getCurrentConfigBool(AdrConfigWarehouse.ConfigIndex.ENABLE_ADDRESS_NAMING_COM), (x) => AdrConfigWarehouse.setCurrentConfigBool(AdrConfigWarehouse.ConfigIndex.ENABLE_ADDRESS_NAMING_COM, x));
            m_uiHelperGlobal.AddCheckboxLocale("ADR_ENABLE_ADDRESS_NAMING_OFF", AdrConfigWarehouse.getCurrentConfigBool(AdrConfigWarehouse.ConfigIndex.ENABLE_ADDRESS_NAMING_OFF), (x) => AdrConfigWarehouse.setCurrentConfigBool(AdrConfigWarehouse.ConfigIndex.ENABLE_ADDRESS_NAMING_OFF, x));








        }
        #endregion


        private void onChangePostalCodePrefixCity(string val)
        {
            if (!int.TryParse(val, out int valInt)) return;
            AdrConfigWarehouse.setCurrentConfigInt(AdrConfigWarehouse.ConfigIndex.ZIPCODE_CITY_PREFIX, valInt);
        }

        private void onChangePostalCodeFormat(string val)
        {
            val = val?.Trim();
            if (val?.Length == 0)
            {
                AdrConfigWarehouse.setCurrentConfigString(AdrConfigWarehouse.ConfigIndex.ZIPCODE_FORMAT, null);
            }
            else
            {
                AdrConfigWarehouse.setCurrentConfigString(AdrConfigWarehouse.ConfigIndex.ZIPCODE_FORMAT, val);
            }
            m_postalCodeFormat.text = AdrConfigWarehouse.getCurrentConfigString(AdrConfigWarehouse.ConfigIndex.ZIPCODE_FORMAT);
        }

        private void onChangeAddressLine1(string val)
        {
            val = val?.Trim();
            if (val?.Length == 0)
            {
                AdrConfigWarehouse.setCurrentConfigString(AdrConfigWarehouse.ConfigIndex.ADDRESS_FORMAT_LINE1, null);
            }
            else
            {
                AdrConfigWarehouse.setCurrentConfigString(AdrConfigWarehouse.ConfigIndex.ADDRESS_FORMAT_LINE1, val);
            }
            m_addressLine1Format.text = AdrConfigWarehouse.getCurrentConfigString(AdrConfigWarehouse.ConfigIndex.ADDRESS_FORMAT_LINE1);
        }
        private void onChangeAddressLine2(string val)
        {
            val = val?.Trim();
            if (val?.Length == 0)
            {
                AdrConfigWarehouse.setCurrentConfigString(AdrConfigWarehouse.ConfigIndex.ADDRESS_FORMAT_LINE2, null);
            }
            else
            {
                AdrConfigWarehouse.setCurrentConfigString(AdrConfigWarehouse.ConfigIndex.ADDRESS_FORMAT_LINE2, val);
            }
            m_addressLine2Format.text = AdrConfigWarehouse.getCurrentConfigString(AdrConfigWarehouse.ConfigIndex.ADDRESS_FORMAT_LINE2);
        }
        private void onChangeAddressLine3(string val)
        {
            val = val?.Trim();
            if (val?.Length == 0)
            {
                AdrConfigWarehouse.setCurrentConfigString(AdrConfigWarehouse.ConfigIndex.ADDRESS_FORMAT_LINE3, null);
            }
            else
            {
                AdrConfigWarehouse.setCurrentConfigString(AdrConfigWarehouse.ConfigIndex.ADDRESS_FORMAT_LINE3, val);
            }
            m_addressLine3Format.text = AdrConfigWarehouse.getCurrentConfigString(AdrConfigWarehouse.ConfigIndex.ADDRESS_FORMAT_LINE3);
        }
    }

}
