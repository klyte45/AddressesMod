using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Addresses.Extensors;
using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Klyte.Commons.UI.DefaultEditorUILib;

namespace Klyte.Addresses.UI
{

    internal class AdrNeighborConfigTab : UICustomControl
    {
        public static AdrNeighborConfigTab Instance { get; private set; }
        public UIPanel MainContainer { get; private set; }

        private UIHelperExtension m_uiHelperNeighbors;

        private AdrMapBordersChart m_borderChart;
        private UIPanel m_borderChartContainer;
        private List<AdrAzimuthEditorLineNeighbor> m_borderCities = new List<AdrAzimuthEditorLineNeighbor>();
        private UIDropDown m_neighborFileSelect;
        private UIScrollablePanel m_neighborsList;

        #region Awake
        public void Awake()
        {
            if (!(Instance is null))
            {
                Destroy(Instance);
            }
            Instance = this;

            MainContainer = GetComponent<UIPanel>();
            MainContainer.autoLayout = true;
            MainContainer.autoLayoutDirection = LayoutDirection.Vertical;
            MainContainer.clipChildren = true;

            m_uiHelperNeighbors = new UIHelperExtension(MainContainer);

            AddDropdown(Locale.Get("K45_ADR_REGION_CITIES_FILE"), out m_neighborFileSelect, m_uiHelperNeighbors, new string[0], OnChangeSelectedNeighborFile);
            AddButtonInEditorRow(m_neighborFileSelect, Commons.UI.SpriteNames.CommonsSpriteNames.K45_Reload, ReloadOptionsFilesNeighbor, "K45_ADR_ROAD_NAME_FILES_RELOAD");


            m_uiHelperNeighbors.AddSpace(10);

            KlyteMonoUtils.CreateUIElement(out UIPanel parentContainer, transform, "ParentContainer");
            parentContainer.width = MainContainer.width - MainContainer.padding.left - MainContainer.padding.right;
            parentContainer.height = MainContainer.height - 70f;
            parentContainer.autoLayoutDirection = LayoutDirection.Horizontal;
            parentContainer.autoLayout = true;

            KlyteMonoUtils.CreateUIElement(out m_borderChartContainer, parentContainer.transform, "NeighborhoodContainer");
            m_borderChartContainer.width = (parentContainer.width / 2f);
            m_borderChartContainer.height = parentContainer.height;
            m_borderChartContainer.autoLayout = false;
            m_borderChartContainer.useCenter = true;
            m_borderChartContainer.wrapLayout = false;
            m_borderChartContainer.tooltipLocaleID = "K45_ADR_CITY_NEIGHBORHOOD";
            m_borderChart = m_borderChartContainer.gameObject.AddComponent<AdrMapBordersChart>();

            //UILabel titleLabel = m_uiHelperNeighbors.AddLabel("");
            //titleLabel.autoSize = true;
            //titleLabel.textAlignment = UIHorizontalAlignment.Center;
            //titleLabel.minimumSize = new Vector2(DefaultWidth, 0);
            //KlyteMonoUtils.LimitWidth(titleLabel, DefaultWidth);
            //titleLabel.localeID = "K45_ADR_AZIMUTH_EDITOR_TITLE";



            KlyteMonoUtils.CreateUIElement(out UIPanel m_neighborEntryListPanel, parentContainer.transform, "NeighborhoodContainer");
            m_neighborEntryListPanel.width = (parentContainer.width / 2f);
            m_neighborEntryListPanel.height = parentContainer.height;
            m_neighborEntryListPanel.autoLayout = true;
            m_neighborEntryListPanel.autoLayoutDirection = LayoutDirection.Vertical;


            KlyteMonoUtils.CreateUIElement(out UIPanel titleItem, m_neighborEntryListPanel.transform, "NeighborhoodContainer");
            titleItem.gameObject.AddComponent<AdrAzimuthTitleLineNeighbor>();

            UIHelperExtension.AddSpace(m_neighborEntryListPanel, 5);
            KlyteMonoUtils.CreateUIElement(out UIPanel listContainer, m_neighborEntryListPanel.transform, "listContainer");
            listContainer.autoLayout = true;
            listContainer.autoLayoutDirection = LayoutDirection.Horizontal;
            listContainer.width = m_neighborEntryListPanel.width;
            listContainer.height = m_neighborEntryListPanel.height - 35;

            KlyteMonoUtils.CreateScrollPanel(listContainer, out m_neighborsList, out _, listContainer.width - 17.5f, listContainer.height);

            ReloadOptionsFilesNeighbor();
        }
        #endregion

        private bool m_isDirty = false;

        public void MarkDirty() => m_isDirty = true;

        public void Update()
        {
            if (m_isDirty)
            {
                RebuildList();
                m_isDirty = false;
            }
        }

        private void RebuildList()
        {
            AdrAzimuthEditorLineNeighbor[] currentLines = m_neighborsList?.GetComponentsInChildren<AdrAzimuthEditorLineNeighbor>();
            if (currentLines is null)
            {
                return;
            }

            int stopsCount = AdrNeighborhoodExtension.GetStopsCount();
            if (stopsCount == 0)
            {
                stopsCount = 1;
            }

            m_borderCities = new List<AdrAzimuthEditorLineNeighbor>();
            int count = 0;
            for (int i = 0; i < stopsCount; i++, count++)
            {
                if (i < currentLines.Length)
                {
                    currentLines[i].SetLegendInfo(GetColorForNumber(i), i + 1);
                    m_borderCities.Add(currentLines[i]);
                }
                else
                {
                    KlyteMonoUtils.CreateUIElement(out UIPanel newEntry, m_neighborsList.transform, "NeighborhoodEntry");
                    AdrAzimuthEditorLineNeighbor line = newEntry.gameObject.AddComponent<AdrAzimuthEditorLineNeighbor>();
                    line.SetLegendInfo(GetColorForNumber(i), i + 1);
                    line.OnValueChange += ValidateAngleStr;
                    line.OnFixedNameChange += SetFixedName;
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
            SaveAzimuthConfig(idx, (ushort)val);
            ReordenateFields();
        }
        private void SetFixedName(int idx, string val)
        {
            AdrNeighborhoodExtension.SetFixedName(idx, val);
            ReordenateFields();
        }

        private void ReordenateFields()
        {
            uint[] values = new uint[m_borderCities.Count];
            bool invalid = false;
            for (int i = 0; i < m_borderCities.Count; i++)
            {
                AdrAzimuthEditorLineNeighbor textField = m_borderCities[i];
                if (!uint.TryParse(textField.GetCurrentVal(), out uint res) || res < 0 || res > 360)
                {
                    textField.SetTextColor(Color.red);
                    values[i] = 1000;
                    invalid = true;
                }
                else
                {
                    textField.SetTextColor(Color.white);
                    values[i] = res;
                }
            }
            if (!invalid)
            {
                List<uint> sortedValues = new List<uint>(values);
                sortedValues.Sort();
                List<Tuple<int, int, Color, AdrAzimuthEditorLineNeighbor>> updateInfo = new List<Tuple<int, int, Color, AdrAzimuthEditorLineNeighbor>>();
                for (int i = 0; i < m_borderCities.Count; i++)
                {
                    int zOrder = sortedValues.IndexOf(values[i]);
                    updateInfo.Add(Tuple.New(zOrder, (int)values[i], GetColorForNumber(i), m_borderCities[i]));
                }
                updateInfo.Sort((x, y) => x.First - y.First);
                for (int i = 0; i < updateInfo.Count; i++)
                {
                    float start = updateInfo[i].Second;
                    float end = updateInfo[(i + 1) % updateInfo.Count].Second;
                    if (end < start)
                    {
                        end += 360;
                    }

                    float angle = (start + end) / 2f;
                    updateInfo[i].Fourth.SetData();
                }
                m_borderChart.SetValues(updateInfo.Select(x => Tuple.New(x.Second, x.Third)).ToList());
            }
            SegmentUtils.UpdateSegmentNamesView();
        }

        private void SaveAzimuthConfig(int idx, ushort value) => AdrNeighborhoodExtension.SetAzimuth(idx, value);

        private Color GetColorForNumber(int num) =>
            num < 0
            ? Color.gray
            : Color.Lerp(m_colorOrder[num % 10], Color.black, 0.2f + (num / 10 / 7.5f));

        private readonly List<Color> m_colorOrder = new List<Color>()
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

        private void OnChangeSelectedNeighborFile(int idx)
        {
            AdrController.CurrentConfig.GlobalConfig.NeighborhoodConfig.NamesFile = idx > 0 ? m_neighborFileSelect.items[idx] : null;

            RebuildList();
        }
        private void ReloadOptionsFilesNeighbor()
        {
            AdrController.LoadLocalesNeighborName();
            List<string> items = AdrController.LoadedLocalesNeighborName.Keys.ToList();
            items.Insert(0, Locale.Get("K45_ADR_DEFAULT_CITIES_REGIONAL_NAMES"));
            m_neighborFileSelect.items = items.ToArray();
            string filename = AdrController.CurrentConfig.GlobalConfig.NeighborhoodConfig.NamesFile;
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
