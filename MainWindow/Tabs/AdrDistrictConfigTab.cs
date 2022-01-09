using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Addresses.ModShared;
using Klyte.Commons.Extensions;
using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using static Klyte.Commons.UI.DefaultEditorUILib;

namespace Klyte.Addresses.UI
{

    internal class AdrDistrictConfigTab : UICustomControl
    {
        public UIPanel MainContainer { get; private set; }

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
            isLoading = true;

            MainContainer = GetComponent<UIPanel>();
            MainContainer.autoLayout = true;
            MainContainer.autoLayoutDirection = LayoutDirection.Vertical;
            MainContainer.autoLayoutPadding = new RectOffset(0, 0, 2, 2);
            MainContainer.width = MainContainer.parent.width;

            m_uiHelperDistrict = new UIHelperExtension(MainContainer);

            AddEmptyDropdown(Locale.Get("K45_ADR_DISTRICT_TITLE"), out m_selectDistrict, m_uiHelperDistrict, null);
            AddButtonInEditorRow(m_selectDistrict, Commons.UI.SpriteNames.CommonsSpriteNames.K45_Reload, ReloadDistricts, "K45_ADR_RELOADFILES");
            m_selectDistrict.eventSelectedIndexChanged += (x, y) => OnDistrictSelect(y);
            m_uiHelperDistrict.AddSpace(30);

            AddEmptyDropdown(Locale.Get("K45_ADR_DISTRICT_NAME_FILE"), out m_roadNameFile, m_uiHelperDistrict, OnChangeSelectedRoadName);
            AddButtonInEditorRow(m_roadNameFile, Commons.UI.SpriteNames.CommonsSpriteNames.K45_Reload, ReloadOptionsRoad, "K45_ADR_ROAD_NAME_FILES_RELOAD");

            AddEmptyDropdown(Locale.Get("K45_ADR_STREETS_PREFIXES_NAME_FILE"), out m_prefixesFile, m_uiHelperDistrict, OnChangeSelectedRoadPrefix);
            AddButtonInEditorRow(m_prefixesFile, Commons.UI.SpriteNames.CommonsSpriteNames.K45_Reload, ReloadOptionsRoadPrefix, "K45_ADR_STREETS_PREFIXES_FILES_RELOAD");

            AddIntField(Locale.Get("K45_ADR_DISTRICT_POSTAL_CODE"), out m_prefixPostalCodeDistrict, m_uiHelperDistrict, OnChangePostalCodePrefixDistrict, false);
            AddButtonInEditorRow(m_prefixPostalCodeDistrict, Commons.UI.SpriteNames.CommonsSpriteNames.K45_Delete, ResetPostalCode, "K45_ADR_RESET_POSTAL_CODE", false, 30);
            m_prefixPostalCodeDistrict.maxLength = 3;

            AddColorField(m_uiHelperDistrict, Locale.Get("K45_ADR_DISTRICT_COLOR"), out m_colorDistrict, (y) => OnChangeDistrictColor(y));
            m_colorDistrict.width = 30;
            m_colorDistrict.height = 30;

            ReloadDistricts();
        }

        public void Start()
        {
            m_selectDistrict.selectedIndex = -1;
            m_selectDistrict.selectedIndex = 0;
            AdrFacade.Instance.EventDistrictChanged += () => dirty = true;
        }
        #endregion
        private bool dirty;

        private void Update()
        {
            if (dirty)
            {
                ReloadDistricts();
                dirty = false;
            }
        }

        private Xml.AdrDistrictConfig GetDistrictConfig()
        {
            ushort currentSelectedDistrict = GetSelectedConfigIndex();
            return AdrController.CurrentConfig.GetConfigForDistrict(currentSelectedDistrict);
        }

        private void ReloadFiles(Action reloadAction, string selectedOption, List<string> referenceList, UIDropDown ddRef, string optionZeroText)
        {
            isLoading = true;
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
            isLoading = false;
        }
        private void ReloadOptionsRoad() => ReloadFiles(
            AdrController.LoadLocalesRoadNames,
            GetDistrictConfig()?.RoadConfig?.NamesFile,
            AdrController.LoadedLocalesRoadName.Keys.ToList(),
            m_roadNameFile,
            Locale.Get(m_selectDistrict.selectedIndex == 0 ? "K45_ADR_DEFAULT_FILE_NAME" : "K45_ADR_DEFAULT_CITY_FILE_NAME")
            );

        private void ReloadOptionsRoadPrefix() => ReloadFiles(
            AdrController.LoadLocalesRoadPrefix,
            GetDistrictConfig()?.RoadConfig?.QualifierFile,
            AdrController.LoadedLocalesRoadPrefix.Keys.ToList(),
            m_prefixesFile,
            Locale.Get(m_selectDistrict.selectedIndex == 0 ? "K45_ADR_DEFAULT_FILE_NAME_PREFIX" : "K45_ADR_DEFAULT_CITY_FILE_NAME_PREFIX")
            );

        private bool isLoading;

        private void OnChangeSelectedRoadPrefix(int idx)
        {
            if (isLoading)
            {
                return;
            }

            GetDistrictConfig().RoadConfig.QualifierFile = idx > 0 ? m_prefixesFile.selectedValue : null;
            SegmentUtils.UpdateSegmentNamesView();
        }

        private void OnChangeSelectedRoadName(int idx)
        {
            if (isLoading)
            {
                return;
            }

            GetDistrictConfig().RoadConfig.NamesFile = idx > 0 ? m_roadNameFile.selectedValue : null;
            SegmentUtils.UpdateSegmentNamesView();
        }

        private void OnChangePostalCodePrefixDistrict(int val)
        {
            if (isLoading)
            {
                return;
            }
            m_prefixPostalCodeDistrict.textColor = Color.white;

            GetDistrictConfig().PostalCodePrefix = (ushort)val;
        }

        private void ResetPostalCode()
        {
            GetDistrictConfig().ResetDistrictCode();
            UpdateDistrictData();
        }

        private void OnChangeDistrictColor(Color c)
        {
            if (isLoading)
            {
                return;
            }

            GetDistrictConfig().DistrictColor = c;
        }

        private ushort GetSelectedConfigIndex()
        {
            if (m_cachedDistricts.ContainsKey(m_selectDistrict.selectedValue))
            {
                return (ushort)(m_cachedDistricts[m_selectDistrict.selectedValue] & 0xFF);
            }
            else
            {
                return 0;
            }
        }


        private void OnDistrictSelect(int x)
        {
            isLoading = true;
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
            isLoading = false;
            //load district info
            ReloadOptionsRoadPrefix();
            ReloadOptionsRoad();
            UpdateDistrictData();

        }

        private void UpdateDistrictData()
        {
            isLoading = true;
            var config = GetDistrictConfig();
            m_prefixPostalCodeDistrict.text = (config.PostalCodePrefix).ToString("D3");
            m_prefixPostalCodeDistrict.textColor = config.IsDefaultCode ? Color.yellow : Color.white;
            m_colorDistrict.selectedColor = config.DistrictColor;
            isLoading = false;
        }

        private void ReloadDistricts()
        {
            isLoading = true;
            m_cachedDistricts = DistrictUtils.GetValidDistricts();
            m_selectDistrict.items = m_cachedDistricts.Keys.OrderBy(x => x).ToArray();
            m_selectDistrict.selectedValue = m_lastSelectedItem;
            SegmentUtils.UpdateSegmentNamesView();
            isLoading = false;
        }
    }


}
