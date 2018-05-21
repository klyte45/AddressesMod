using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using Klyte.Commons.Extensors;
using Klyte.Harmony;
using Klyte.Commons.Extensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Klyte.Addresses.Utils;
using System.IO;
using Klyte.Commons.Overrides;
using Klyte.Commons.Utils;

namespace Klyte.Addresses.UI
{

    internal class AdrGlobalConfigTab : UICustomControl
    {
        public UIComponent mainContainer { get; private set; }

        private UITextField m_prefixPostalCodeCity;
        private UITextField m_postalCodeFormat;

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
    }

}
