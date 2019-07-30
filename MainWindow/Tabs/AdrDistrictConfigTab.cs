using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Addresses.Overrides;
using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Klyte.Addresses.UI
{

    internal class AdrDistrictConfigTab : UICustomControl
    {
        public UIComponent MainContainer { get; private set; }

        private UIDropDown m_selectDistrict;
        private Dictionary<string, int> m_cachedDistricts;
        private string m_lastSelectedItem;
        private UIDropDown m_roadNameFile;
        private UIDropDown m_prefixesFile;
        private UITextField m_prefixPostalCodeDistrict;
        private UIColorField m_colorDistrict;

        private UIHelperExtension m_uiHelperDistrict;

        #region Awake
        public void Awake()
        {
            MainContainer = GetComponent<UIComponent>();

            m_uiHelperDistrict = new UIHelperExtension(MainContainer);

            ((UIScrollablePanel) m_uiHelperDistrict.Self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIScrollablePanel) m_uiHelperDistrict.Self).wrapLayout = true;
            ((UIScrollablePanel) m_uiHelperDistrict.Self).width = 370;

            m_cachedDistricts = DistrictUtils.GetValidDistricts();
            m_selectDistrict = m_uiHelperDistrict.AddDropdownLocalized("ADR_DISTRICT_TITLE", m_cachedDistricts.Keys.OrderBy(x => x).ToArray(), 0, OnDistrictSelect);
            m_selectDistrict.width = 370;
            m_uiHelperDistrict.AddSpace(30);

            m_roadNameFile = m_uiHelperDistrict.AddDropdownLocalized("ADR_DISTRICT_NAME_FILE", new string[0], -1, OnChangeSelectedRoadName);
            m_roadNameFile.width = 370;
            m_uiHelperDistrict.AddSpace(1);
            KlyteMonoUtils.LimitWidth((UIButton) m_uiHelperDistrict.AddButton(Locale.Get("ADR_ROAD_NAME_FILES_RELOAD"), ReloadOptionsRoad), 380);
            m_uiHelperDistrict.AddSpace(20);

            m_prefixesFile = m_uiHelperDistrict.AddDropdownLocalized("ADR_STREETS_PREFIXES_NAME_FILE", new string[0], -1, OnChangeSelectedRoadPrefix);
            m_prefixesFile.width = 370;
            m_uiHelperDistrict.AddSpace(1);
            KlyteMonoUtils.LimitWidth((UIButton) m_uiHelperDistrict.AddButton(Locale.Get("ADR_STREETS_PREFIXES_FILES_RELOAD"), ReloadOptionsRoadPrefix), 380);
            m_uiHelperDistrict.AddSpace(40);

            m_prefixPostalCodeDistrict = m_uiHelperDistrict.AddTextField(Locale.Get("ADR_DISTRICT_POSTAL_CODE"), null, "", OnChangePostalCodePrefixDistrict);
            m_prefixPostalCodeDistrict.numericalOnly = true;
            m_prefixPostalCodeDistrict.maxLength = 3;

            m_colorDistrict = m_uiHelperDistrict.AddColorPicker(Locale.Get("ADR_DISTRICT_COLOR"), Color.white, OnChangeDistrictColor, out UILabel title);
            m_colorDistrict.width = 20;
            m_colorDistrict.height = 20;
            KlyteMonoUtils.LimitWidth(title, 350);

            DistrictManagerOverrides.EventOnDistrictChanged += ReloadDistricts;
            ReloadDistricts();
        }

        public void Start()
        {
            m_selectDistrict.selectedIndex = -1;
            m_selectDistrict.selectedIndex = 0;
        }
        #endregion


        private Xml.AdrDistrictConfig GetDistrictConfig()
        {
            ushort currentSelectedDistrict = GetSelectedConfigIndex();
            return AdrController.CurrentConfig.GetConfigForDistrict(currentSelectedDistrict);
        }

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
        }
        private void ReloadOptionsRoad() => ReloadFiles(
            AdrController.LoadLocalesRoadNames,
            GetDistrictConfig()?.RoadConfig?.NamesFile,
            AdrController.LoadedLocalesRoadName.Keys.ToList(),
            m_roadNameFile,
            Locale.Get(m_selectDistrict.selectedIndex == 0 ? "ADR_DEFAULT_FILE_NAME" : "ADR_DEFAULT_CITY_FILE_NAME")
            );

        private void ReloadOptionsRoadPrefix() => ReloadFiles(
            AdrController.LoadLocalesRoadPrefix,
            GetDistrictConfig()?.RoadConfig?.QualifierFile,
            AdrController.LoadedLocalesRoadPrefix.Keys.ToList(),
            m_prefixesFile,
            Locale.Get(m_selectDistrict.selectedIndex == 0 ? "ADR_DEFAULT_FILE_NAME_PREFIX" : "ADR_DEFAULT_CITY_FILE_NAME_PREFIX")
            );



        private void OnChangeSelectedRoadPrefix(int idx)
        {
            GetDistrictConfig().RoadConfig.QualifierFile = idx > 0 ? m_prefixesFile.selectedValue : null;
            SegmentUtils.UpdateSegmentNamesView();
        }

        private void OnChangeSelectedRoadName(int idx)
        {
            GetDistrictConfig().RoadConfig.NamesFile = idx > 0 ? m_roadNameFile.selectedValue : null;
            SegmentUtils.UpdateSegmentNamesView();
        }

        private void OnChangePostalCodePrefixDistrict(string val)
        {
            if (int.TryParse(val, out int intval))
            {
                GetDistrictConfig().ZipcodePrefix = intval;
            }
        }
        private void OnChangeDistrictColor(Color c) => GetDistrictConfig().DistrictColor = c;

        private ushort GetSelectedConfigIndex()
        {
            if (m_cachedDistricts.ContainsKey(m_selectDistrict.selectedValue))
            {
                return (ushort) (m_cachedDistricts[m_selectDistrict.selectedValue] & 0xFF);
            }
            else
            {
                return 0;
            }
        }


        private void OnDistrictSelect(int x)
        {
            try
            {
                m_lastSelectedItem = m_selectDistrict.items[x];
            }
            catch
            {
                if (m_selectDistrict.items.Length > 0)
                {
                    m_lastSelectedItem = m_selectDistrict.items[0];
                }
                else
                {
                    m_lastSelectedItem = null;
                    m_selectDistrict.selectedIndex = -1;
                }
            }
            //load district info
            ReloadOptionsRoadPrefix();
            ReloadOptionsRoad();
            m_prefixPostalCodeDistrict.text = GetDistrictConfig().ZipcodePrefix?.ToString("%03d") ?? "";
            m_colorDistrict.selectedColor = GetDistrictConfig().DistrictColor;

        }


        private void ReloadDistricts()
        {
            m_cachedDistricts = DistrictUtils.GetValidDistricts();
            m_selectDistrict.items = m_cachedDistricts.Keys.OrderBy(x => x).ToArray();
            m_selectDistrict.selectedValue = m_lastSelectedItem;
            SegmentUtils.UpdateSegmentNamesView();
        }

    }


}
