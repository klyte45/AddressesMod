using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Addresses.Extensors;
using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Klyte.Addresses.UI
{

    internal class AdrNeighborConfigTab : UICustomControl
    {
        public UIComponent MainContainer { get; private set; }

        private UIHelperExtension m_uiHelperNeighbors;

        private AdrMapBordersChart m_borderChart;
        private List<AdrAzimuthEditorLineNeighbor> m_borderCities = new List<AdrAzimuthEditorLineNeighbor>();
        private UIDropDown m_neighborFileSelect;

        #region Awake
        public void Awake()
        {
            MainContainer = GetComponent<UIComponent>();
            m_uiHelperNeighbors = new UIHelperExtension(MainContainer);

            ((UIScrollablePanel) m_uiHelperNeighbors.Self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIScrollablePanel) m_uiHelperNeighbors.Self).wrapLayout = true;
            ((UIScrollablePanel) m_uiHelperNeighbors.Self).width = 370;

            m_neighborFileSelect = m_uiHelperNeighbors.AddDropdownLocalized("K45_ADR_REGION_CITIES_FILE", new string[0], -1, OnChangeSelectedNeighborFile);
            m_neighborFileSelect.width = 370;
            m_uiHelperNeighbors.AddSpace(1);
            KlyteMonoUtils.LimitWidth((UIButton) m_uiHelperNeighbors.AddButton(Locale.Get("K45_ADR_ROAD_NAME_FILES_RELOAD"), ReloadOptionsFilesNeighbor), 380);
            m_uiHelperNeighbors.AddSpace(10);

            UILabel titleLabel = m_uiHelperNeighbors.AddLabel("");
            titleLabel.autoSize = true;
            titleLabel.textAlignment = UIHorizontalAlignment.Center;
            titleLabel.minimumSize = new Vector2(370, 0);
            KlyteMonoUtils.LimitWidth(titleLabel, 370);
            titleLabel.localeID = "K45_ADR_AZIMUTH_EDITOR_TITLE";

            m_uiHelperNeighbors.AddSpace(5);
            KlyteMonoUtils.CreateElement(out m_borderChart, m_uiHelperNeighbors.Self.transform, "NeighborArea");
            m_uiHelperNeighbors.AddSpace(30);
            KlyteMonoUtils.CreateElement<AdrAzimuthTitleLineNeighbor>(m_uiHelperNeighbors.Self.transform);
            m_uiHelperNeighbors.AddSpace(5);

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
            AdrAzimuthEditorLineNeighbor[] currentLines = m_uiHelperNeighbors.Self.GetComponentsInChildren<AdrAzimuthEditorLineNeighbor>();
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
                    AdrAzimuthEditorLineNeighbor line = KlyteMonoUtils.CreateElement<AdrAzimuthEditorLineNeighbor>(m_uiHelperNeighbors.Self.transform);
                    line.SetLegendInfo(GetColorForNumber(i), i + 1);
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
            SaveAzimuthConfig(idx, (ushort) val);
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
                    updateInfo.Add(Tuple.New(zOrder, (int) values[i], GetColorForNumber(i), m_borderCities[i]));
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
                    updateInfo[i].Fourth.SetCardinalAngle(angle);
                }
                m_borderChart.SetValues(updateInfo.Select(x => Tuple.New(x.Second, x.Third)).ToList());
            }
            SegmentUtils.UpdateSegmentNamesView();
        }

        private void SaveAzimuthConfig(int idx, ushort value) => AdrNeighborhoodExtension.SetAzimuth(idx, value);

        private Color GetColorForNumber(int num)
        {
            if (num < 0)
            {
                return Color.gray;
            }

            if (num < 10)
            {
                return m_colorOrder[num % 10];
            }

            return Color.Lerp(m_colorOrder[num % 10], Color.black, num / 10 / 5f);
        }

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
