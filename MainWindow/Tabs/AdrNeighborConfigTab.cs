using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensors;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Klyte.Addresses.Utils;
using Klyte.Commons.Utils;
using Klyte.Addresses.Extensors;

namespace Klyte.Addresses.UI
{

    internal class AdrNeighborConfigTab : UICustomControl
    {
        public UIComponent mainContainer { get; private set; }

        private UIHelperExtension m_uiHelperNeighbors;

        private AdrMapBordersChart m_borderChart;
        private List<AdrAzimuthEditorLineNeighbor> m_borderCities = new List<AdrAzimuthEditorLineNeighbor>();
        private UIDropDown m_neighborFileSelect;

        #region Awake
        private void Awake()
        {
            mainContainer = GetComponent<UIComponent>();
            m_uiHelperNeighbors = new UIHelperExtension(mainContainer);

            ((UIScrollablePanel)m_uiHelperNeighbors.self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIScrollablePanel)m_uiHelperNeighbors.self).wrapLayout = true;
            ((UIScrollablePanel)m_uiHelperNeighbors.self).width = 370;

            m_neighborFileSelect = m_uiHelperNeighbors.AddDropdownLocalized("ADR_REGION_CITIES_FILE", new String[0], -1, onChangeSelectedNeighborFile);
            m_neighborFileSelect.width = 370;
            m_uiHelperNeighbors.AddSpace(1);
            AdrUtils.LimitWidth((UIButton)m_uiHelperNeighbors.AddButton(Locale.Get("ADR_ROAD_NAME_FILES_RELOAD"), reloadOptionsFilesNeighbor), 380);           
            m_uiHelperNeighbors.AddSpace(10);

            UILabel titleLabel = m_uiHelperNeighbors.AddLabel("");
            titleLabel.autoSize = true;
            titleLabel.textAlignment = UIHorizontalAlignment.Center;
            titleLabel.minimumSize = new Vector2(370, 0);
            KlyteUtils.LimitWidth(titleLabel, 370);
            titleLabel.localeID = "ADR_AZIMUTH_EDITOR_TITLE";

            m_uiHelperNeighbors.AddSpace(5);
            AdrUtils.createElement(out m_borderChart, m_uiHelperNeighbors.self.transform, "NeighborArea");
            m_uiHelperNeighbors.AddSpace(30);
            AdrUtils.createElement<AdrAzimuthTitleLineNeighbor>(m_uiHelperNeighbors.self.transform);
            m_uiHelperNeighbors.AddSpace(5);

            reloadOptionsFilesNeighbor();
            AdrNeighborhoodExtension.instance.eventOnValueChanged += OnPropertyChange;
        }
        #endregion

        private void OnPropertyChange(int idx, NeighborOption? option, string value)
        {
            RebuildList();
        }

        private void RebuildList()
        {
            var currentLines = m_uiHelperNeighbors.self.GetComponentsInChildren<AdrAzimuthEditorLineNeighbor>();
            int stopsCount = AdrNeighborhoodExtension.instance.getStopsCount();
            if (stopsCount == 0) stopsCount = 1;
            m_borderCities = new List<AdrAzimuthEditorLineNeighbor>();
            int count = 0;
            for (int i = 0; i < stopsCount; i++, count++)
            {
                if (i < currentLines.Length)
                {
                    currentLines[i].SetLegendInfo(getColorForNumber(i), i + 1);
                    m_borderCities.Add(currentLines[i]);
                }
                else
                {
                    var line = AdrUtils.createElement<AdrAzimuthEditorLineNeighbor>(m_uiHelperNeighbors.self.transform);
                    line.SetLegendInfo(getColorForNumber(i), i + 1);
                    line.OnValueChange += ValidateAngleStr;
                    m_borderCities.Add(line);
                }
            }
            for (int i = count; i < currentLines.Length; i++)
            {
                GameObject.Destroy(currentLines[i].gameObject);
            }
            ReordenateFields();
        }

        private void ValidateAngleStr(int idx, uint val)
        {
            saveAzimuthConfig(idx, val);
            ReordenateFields();
        }

        private void ReordenateFields()
        {
            uint[] values = new uint[m_borderCities.Count];
            bool invalid = false;
            for (int i = 0; i < m_borderCities.Count; i++)
            {
                var textField = m_borderCities[i];
                if (!uint.TryParse(textField.getCurrentVal(), out uint res) || res < 0 || res > 360)
                {
                    textField.setTextColor(Color.red);
                    values[i] = 1000;
                    invalid = true;
                }
                else
                {
                    textField.setTextColor(Color.white);
                    values[i] = res;
                }
            }
            if (!invalid)
            {
                List<uint> sortedValues = new List<uint>(values);
                sortedValues.Sort();
                var updateInfo = new List<Tuple<int, int, Color, AdrAzimuthEditorLineNeighbor>>();
                for (int i = 0; i < m_borderCities.Count; i++)
                {
                    var zOrder = sortedValues.IndexOf(values[i]);
                    updateInfo.Add(Tuple.New(zOrder, (int)values[i], getColorForNumber(i), m_borderCities[i]));
                }
                updateInfo.Sort((x, y) => x.First - y.First);
                for (int i = 0; i < updateInfo.Count; i++)
                {
                    float start = updateInfo[i].Second;
                    float end = updateInfo[(i + 1) % updateInfo.Count].Second;
                    if (end < start) end += 360;
                    float angle = (start + end) / 2f;
                    updateInfo[i].Fourth.SetCardinalAngle(angle);
                }
                m_borderChart.SetValues(updateInfo.Select(x => Tuple.New(x.Second, x.Third)).ToList());
            }
            AdrUtils.UpdateSegmentNamesView();
        }

        private void saveAzimuthConfig(int idx, uint value)
        {
            AdrNeighborhoodExtension.instance.SetAzimuth(idx, value);
        }

        private Color getColorForNumber(int num)
        {
            if (num < 0) return Color.gray;
            if (num < 10) return colorOrder[num % 10];
            return Color.Lerp(colorOrder[num % 10], Color.black, (num / 10) / 5f);
        }

        private List<Color> colorOrder = new List<Color>()
        {
            Color.red,
            Color.Lerp(Color.red,Color.yellow,0.5f),
            Color.yellow,
            Color.green,
            Color.cyan,
            Color.Lerp(Color.blue,Color.cyan,0.5f),
            Color.blue,
            Color.Lerp(Color.blue,Color.magenta,0.5f),
            Color.magenta,
            Color.Lerp(Color.red,Color.magenta,0.5f),
        };

        private void onChangeSelectedNeighborFile(int idx)
        {
            if (idx > 0)
            {
                AdrConfigWarehouse.setCurrentConfigString(AdrConfigWarehouse.ConfigIndex.NEIGHBOR_CONFIG_NAME_FILE, m_neighborFileSelect.items[idx]);
            }
            else
            {
                AdrConfigWarehouse.setCurrentConfigString(AdrConfigWarehouse.ConfigIndex.NEIGHBOR_CONFIG_NAME_FILE, null);
            }
            RebuildList();
        }
        private void reloadOptionsFilesNeighbor()
        {
            AdrController.LoadLocalesNeighborName();
            List<string> items = AdrController.loadedLocalesNeighborName.Keys.ToList();
            items.Insert(0, Locale.Get("ADR_DEFAULT_CITIES_REGIONAL_NAMES"));
            m_neighborFileSelect.items = items.ToArray();
            string filename = AdrConfigWarehouse.getCurrentConfigString(AdrConfigWarehouse.ConfigIndex.NEIGHBOR_CONFIG_NAME_FILE);
            if (items.Contains(filename))
            {
                m_neighborFileSelect.selectedValue = filename;
            }
            else
            {
                m_neighborFileSelect.selectedIndex = 0;
            }
            RebuildList();
        }
    }


}
