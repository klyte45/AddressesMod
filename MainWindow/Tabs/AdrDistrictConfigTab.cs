using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensors;
using System;
using System.Collections.Generic;
using System.Linq;
using Klyte.Addresses.Utils;
using Klyte.Commons.Overrides;

namespace Klyte.Addresses.UI
{

    internal class AdrDistrictConfigTab : UICustomControl
    {
        public UIComponent mainContainer { get; private set; }

        private UIDropDown m_selectDistrict;
        private Dictionary<string, int> m_cachedDistricts;
        private string m_lastSelectedItem;
        private UIDropDown m_districtNameFile;
        private UIDropDown m_prefixesFile;
        private UITextField m_prefixPostalCodeDistrict;

        private UIHelperExtension m_uiHelperDistrict;

        #region Awake
        private void Awake()
        {
            mainContainer = GetComponent<UIComponent>();

            m_uiHelperDistrict = new UIHelperExtension(mainContainer);

            ((UIScrollablePanel)m_uiHelperDistrict.self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIScrollablePanel)m_uiHelperDistrict.self).wrapLayout = true;
            ((UIScrollablePanel)m_uiHelperDistrict.self).width = 370;

            m_cachedDistricts = AdrUtils.getValidDistricts();
            m_selectDistrict = m_uiHelperDistrict.AddDropdownLocalized("ADR_DISTRICT_TITLE", m_cachedDistricts.Keys.OrderBy(x => x).ToArray(), 0, OnDistrictSelect);
            m_selectDistrict.width = 370;
            m_uiHelperDistrict.AddSpace(30);

            m_districtNameFile = m_uiHelperDistrict.AddDropdownLocalized("ADR_DISTRICT_NAME_FILE", new String[0], -1, onChangeSelectedRoadName);
            m_districtNameFile.width = 370;
            m_uiHelperDistrict.AddSpace(1);
            AdrUtils.LimitWidth((UIButton)m_uiHelperDistrict.AddButton(Locale.Get("ADR_ROAD_NAME_FILES_RELOAD"), reloadOptionsRoad), 380);
            m_uiHelperDistrict.AddSpace(20);

            m_prefixesFile = m_uiHelperDistrict.AddDropdownLocalized("ADR_STREETS_PREFIXES_NAME_FILE", new String[0], -1, onChangeSelectedRoadPrefix);
            m_prefixesFile.width = 370;
            m_uiHelperDistrict.AddSpace(1);
            AdrUtils.LimitWidth((UIButton)m_uiHelperDistrict.AddButton(Locale.Get("ADR_STREETS_PREFIXES_FILES_RELOAD"), reloadOptionsRoadPrefix), 380);
            m_uiHelperDistrict.AddSpace(40);

            m_prefixPostalCodeDistrict = m_uiHelperDistrict.AddTextField(Locale.Get("ADR_DISTRICT_POSTAL_CODE"), null, "", onChangePostalCodePrefixDistrict);
            m_prefixPostalCodeDistrict.numericalOnly = true;
            m_prefixPostalCodeDistrict.maxLength = 3;
            
            DistrictManagerOverrides.eventOnDistrictChanged += reloadDistricts;
            reloadDistricts();
        }

        public void Start()
        {
            m_selectDistrict.selectedIndex = -1;
            m_selectDistrict.selectedIndex = 0;
        }
        #endregion

        private void reloadOptionsRoad()
        {
            if (getSelectedConfigIndex() < 0) return;
            AdrController.LoadLocalesRoadNames();
            AdrConfigWarehouse.ConfigIndex currentSelectedDistrict = (AdrConfigWarehouse.ConfigIndex)(getSelectedConfigIndex());
            List<string> items = AdrController.loadedLocalesRoadName.Keys.ToList();
            items.Insert(0, Locale.Get(m_selectDistrict.selectedIndex == 0 ? "ADR_DEFAULT_FILE_NAME" : "ADR_DEFAULT_CITY_FILE_NAME"));
            m_districtNameFile.items = items.ToArray();
            string filename = AdrConfigWarehouse.getCurrentConfigString(AdrConfigWarehouse.ConfigIndex.ROAD_NAME_FILENAME | currentSelectedDistrict);
            if (items.Contains(filename))
            {
                m_districtNameFile.selectedValue = filename;
            }
            else
            {
                m_districtNameFile.selectedIndex = 0;
            }
        }

        private void reloadOptionsRoadPrefix()
        {
            if (getSelectedConfigIndex() < 0) return;
            AdrController.LoadLocalesRoadPrefix();
            List<string> items = AdrController.loadedLocalesRoadPrefix.Keys.ToList();
            AdrConfigWarehouse.ConfigIndex currentSelectedDistrict = (AdrConfigWarehouse.ConfigIndex)(getSelectedConfigIndex());
            items.Insert(0, Locale.Get(m_selectDistrict.selectedIndex == 0 ? "ADR_DEFAULT_FILE_NAME_PREFIX" : "ADR_DEFAULT_CITY_FILE_NAME_PREFIX"));
            m_prefixesFile.items = items.ToArray();
            string filename = AdrConfigWarehouse.getCurrentConfigString(AdrConfigWarehouse.ConfigIndex.PREFIX_FILENAME | currentSelectedDistrict);
            if (items.Contains(filename))
            {
                m_prefixesFile.selectedValue = filename;
            }
            else
            {
                m_prefixesFile.selectedIndex = 0;
            }
        }


        private void onChangeSelectedRoadPrefix(int idx)
        {
            setDistrictPropertyString(AdrConfigWarehouse.ConfigIndex.PREFIX_FILENAME, idx > 0 ? m_prefixesFile.selectedValue : null);
            AdrUtils.UpdateSegmentNamesView();
        }

        private void onChangeSelectedRoadName(int idx)
        {
            setDistrictPropertyString(AdrConfigWarehouse.ConfigIndex.ROAD_NAME_FILENAME, idx > 0 ? m_districtNameFile.selectedValue : null);
            AdrUtils.UpdateSegmentNamesView();
        }

        private void onChangePostalCodePrefixDistrict(string val)
        {
            setDistrictPropertyInt(AdrConfigWarehouse.ConfigIndex.ZIPCODE_PREFIX, val);
        }

        private void onChangePostalCodePrefixCity(string val)
        {
            if (!int.TryParse(val, out int valInt)) return;
            AdrConfigWarehouse.setCurrentConfigInt(AdrConfigWarehouse.ConfigIndex.ZIPCODE_CITY_PREFIX, valInt);
        }
        
        private void setDistrictPropertyString(AdrConfigWarehouse.ConfigIndex configIndex, string value)
        {
            if (!getSelectedDistrict(out AdrConfigWarehouse.ConfigIndex selectedDistrict)) return;
            AdrConfigWarehouse.setCurrentConfigString(configIndex | selectedDistrict, value);
        }

        private void setDistrictPropertyInt(AdrConfigWarehouse.ConfigIndex configIndex, string value)
        {
            if (!getSelectedDistrict(out AdrConfigWarehouse.ConfigIndex selectedDistrict) || !int.TryParse(value, out int valInt)) return;
            AdrConfigWarehouse.setCurrentConfigInt(configIndex | selectedDistrict, valInt);
        }

        private bool getSelectedDistrict(out AdrConfigWarehouse.ConfigIndex selectedDistrict)
        {
            selectedDistrict = 0;
            if (m_cachedDistricts.ContainsKey(m_selectDistrict.selectedValue))
            {
                selectedDistrict = (AdrConfigWarehouse.ConfigIndex)(m_cachedDistricts[m_selectDistrict.selectedValue] & 0xFF);
            }
            else if (m_selectDistrict.selectedIndex != 0)
            {
                return false;
            }

            return true;
        }

        private int getSelectedConfigIndex()
        {
            if (m_cachedDistricts.ContainsKey(m_selectDistrict.selectedValue))
            {
                return (m_cachedDistricts[m_selectDistrict.selectedValue] & 0xFF);
            }
            else if (m_selectDistrict.selectedIndex == 0)
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }


        private void OnDistrictSelect(int x)
        {
            String oldSel = m_lastSelectedItem;
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
            reloadOptionsRoadPrefix();
            reloadOptionsRoad();
            if (getSelectedDistrict(out AdrConfigWarehouse.ConfigIndex selectedDistrict))
            {
                m_prefixPostalCodeDistrict.text = AdrConfigWarehouse.getCurrentConfigInt(AdrConfigWarehouse.ConfigIndex.ZIPCODE_PREFIX | selectedDistrict).ToString();
            }
        }


        private void reloadDistricts()
        {
            m_cachedDistricts = AdrUtils.getValidDistricts();
            m_selectDistrict.items = m_cachedDistricts.Keys.OrderBy(x => x).ToArray();
            m_selectDistrict.selectedValue = m_lastSelectedItem;
            AdrUtils.UpdateSegmentNamesView();
        }

        public int getCurrentSelectedDistrictId()
        {
            if (m_lastSelectedItem == null || !m_cachedDistricts.ContainsKey(m_lastSelectedItem))
            {
                return -1;
            }
            return m_cachedDistricts[m_lastSelectedItem];
        }

    }


}
