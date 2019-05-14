using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using ColossalFramework.UI;
using Klyte.Addresses.LocaleStruct;
using Klyte.Addresses.Overrides;
using Klyte.Addresses.TextureAtlas;
using Klyte.Addresses.UI;
using Klyte.Addresses.Utils;
using Klyte.Commons;
using Klyte.Commons.Extensors;
using Klyte.Commons.UI;
using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Klyte.Addresses
{
    public class AdrController : MonoBehaviour
    {

        private static Dictionary<String, String[]> m_loadedLocalesRoadName;
        private static Dictionary<String, RoadPrefixFileIndexer> m_loadedLocalesRoadPrefix;
        private static Dictionary<String, String[]> m_loadedLocalesNeighborName;
        private static Dictionary<String, String[]> m_loadedLocalesDistrictPrefix;
        private static Dictionary<String, String[]> m_loadedLocalesDistrictName;
        private static Dictionary<String, String[]> m_loadedLocalesCitizenFirstNameMasc;
        private static Dictionary<String, String[]> m_loadedLocalesCitizenFirstNameFem;
        private static Dictionary<String, String[]> m_loadedLocalesCitizenLastName;

        internal static Dictionary<String, String[]> loadedLocalesRoadName => m_loadedLocalesRoadName;
        internal static Dictionary<String, RoadPrefixFileIndexer> loadedLocalesRoadPrefix => m_loadedLocalesRoadPrefix;
        internal static Dictionary<String, String[]> loadedLocalesNeighborName => m_loadedLocalesNeighborName;
        internal static Dictionary<String, String[]> loadedLocalesDistrictPrefix => m_loadedLocalesDistrictPrefix;
        internal static Dictionary<String, String[]> loadedLocalesDistrictName => m_loadedLocalesDistrictName;
        internal static Dictionary<String, String[]> loadedLocalesCitizenFirstNameMasc => m_loadedLocalesCitizenFirstNameMasc;
        internal static Dictionary<String, String[]> loadedLocalesCitizenFirstNameFem => m_loadedLocalesCitizenFirstNameFem;
        internal static Dictionary<String, String[]> loadedLocalesCitizenLastName => m_loadedLocalesCitizenLastName;

        public static void LoadLocalesRoadPrefix()
        {
            m_loadedLocalesRoadPrefix = new Dictionary<string, RoadPrefixFileIndexer>();
            foreach (var filename in Directory.GetFiles(AddressesMod.roadPrefixPath, "*.txt").Select(x => x.Split(Path.DirectorySeparatorChar).Last()))
            {
                string fileContents = File.ReadAllText(AddressesMod.roadPrefixPath + Path.DirectorySeparatorChar + filename, Encoding.UTF8);
                fileContents.Replace(Environment.NewLine, "\n");
                m_loadedLocalesRoadPrefix[filename] = RoadPrefixFileIndexer.Parse(fileContents.Split('\n').Where(x => !string.IsNullOrEmpty(x) && !x.StartsWith("#")).ToArray());
                AdrUtils.doLog("LOADED PREFIX NAMES ({0}) QTT: {1}", filename, m_loadedLocalesRoadPrefix[filename].Length);
            }
        }

        public static void LoadLocalesRoadNames() => LoadSimpleNamesFiles(out m_loadedLocalesRoadName, AddressesMod.roadPath);
        public static void LoadLocalesNeighborName() => LoadSimpleNamesFiles(out m_loadedLocalesNeighborName, AddressesMod.neigborsPath);
        public static void LoadLocalesDistrictPrefix() => LoadSimpleNamesFiles(out m_loadedLocalesDistrictPrefix, AddressesMod.districtPrefixPath);
        public static void LoadLocalesDistrictName() => LoadSimpleNamesFiles(out m_loadedLocalesDistrictName, AddressesMod.districtNamePath);
        public static void LoadLocalesCitizenFirstNameMale() => LoadSimpleNamesFiles(out m_loadedLocalesCitizenFirstNameMasc, AddressesMod.citizenFirstNameMascPath);
        public static void LoadLocalesCitizenFirstNameFemale() => LoadSimpleNamesFiles(out m_loadedLocalesCitizenFirstNameFem, AddressesMod.citizenFirstNameFemPath);
        public static void LoadLocalesCitizenLastName() => LoadSimpleNamesFiles(out m_loadedLocalesCitizenLastName, AddressesMod.citizenLastNamePath);

        private static void LoadSimpleNamesFiles(out Dictionary<string, String[]> result, string path)
        {
            result = new Dictionary<string, String[]>();
            foreach (var filename in Directory.GetFiles(path, "*.txt").Select(x => x.Split(Path.DirectorySeparatorChar).Last()))
            {
                string fileContents = File.ReadAllText(path + Path.DirectorySeparatorChar + filename, Encoding.UTF8);
                result[filename] = fileContents.Split(Environment.NewLine.ToCharArray()).Select(x => x?.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray();
                AdrUtils.doLog($"LOADED Files at {path} ({filename}) QTT: {result[filename].Length}");
            }
        }

        public void OpenAdrPanel()
        {
            KlyteModsPanel.instance.OpenAt(ModTab.Addresses);
        }
        public void CloseAdrPanel()
        {
            KlyteCommonsMod.CloseKCPanel();
        }

        public void Awake()
        {
            LoadLocalesRoadNames();
            LoadLocalesRoadPrefix();
            LoadLocalesNeighborName();
            LoadLocalesDistrictPrefix();
            LoadLocalesDistrictName();
            LoadLocalesCitizenFirstNameMale();
            LoadLocalesCitizenFirstNameFemale();
            LoadLocalesCitizenLastName();

            initNearLinesOnWorldInfoPanel();

            AdrConfigWarehouse.eventOnPropertyChanged += (x, b, i, s) =>
            {
                if ((x & AdrConfigWarehouse.ConfigIndex.DISTRICT_COLOR) == AdrConfigWarehouse.ConfigIndex.DISTRICT_COLOR)
                {
                    Commons.Overrides.DistrictManagerOverrides.OnDistrictChanged();
                }
                if (x == AdrConfigWarehouse.ConfigIndex.CITY_ZERO_BUILDING)
                {
                    Shared.AdrEvents.TriggerZeroMarkerBuildingChange();
                }
            };

            Commons.Overrides.BuildingManagerOverrides.eventBuidlingReleased += RemoveZeroMarker;

            var dtbHookableType = Type.GetType("Klyte.DynamicTextBoards.Utils.DTBHookable, KlyteDynamicTextBoards");
            if (dtbHookableType != null)
            {
                dtbHookableType.GetField("GetStreetSuffix", Redirector.allFlags).SetValue(null, new Func<ushort, string>((ushort idx) =>
                {
                    var result = "";
                    var usedQueue = new List<ushort>();
                    NetManagerOverrides.GenerateSegmentNameInternal(idx, ref result, ref usedQueue, true);

                    return result;
                }));
                dtbHookableType.GetField("GetDistrictColor", Redirector.allFlags).SetValue(null, new Func<ushort, Color>((ushort idx) =>
                {
                    return AdrConfigWarehouse.GetDistrictColor(idx);
                }));
                dtbHookableType.GetField("GetStartPoint", Redirector.allFlags).SetValue(null, new Func<Vector2>(() =>
                {
                    var buildingZM = AdrConfigWarehouse.GetZeroMarkBuilding();
                    if (buildingZM == 0)
                    {
                        return Vector2.zero;
                    }
                    else
                    {
                        return BuildingManager.instance.m_buildings.m_buffer[buildingZM].m_flags == Building.Flags.None ? Vector2.zero : VectorUtils.XZ(BuildingManager.instance.m_buildings.m_buffer[buildingZM].m_position);
                    }
                }));
            }
        }

        private void RemoveZeroMarker(ushort building)
        {
            if (AdrConfigWarehouse.GetZeroMarkBuilding() == building)
            {
                AdrConfigWarehouse.setCurrentConfigInt(AdrConfigWarehouse.ConfigIndex.CITY_ZERO_BUILDING, null);
            }
        }

        private void initNearLinesOnWorldInfoPanel()
        {
            BuildingWorldInfoPanel[] panelList = GameObject.Find("UIView").GetComponentsInChildren<BuildingWorldInfoPanel>();
            AdrUtils.doLog("WIP LIST: [{0}]", string.Join(", ", panelList.Select(x => x.name).ToArray()));

            foreach (BuildingWorldInfoPanel wip in panelList)
            {
                AdrUtils.doLog("LOADING WIP HOOK FOR: {0}", wip.name);
                UIComponent parent2 = wip.GetComponent<UIComponent>();
                parent2.eventVisibilityChanged += (component, value) =>
                {
                    UpdateAddressField(parent2);
                };
                parent2.eventPositionChanged += (component, value) =>
                {
                    UpdateAddressField(parent2);
                };
            }

        }

        private static Func<WorldInfoPanel, InstanceID> GetWipBuildingInstanceId = ReflectionUtils.GetGetFieldDelegate<WorldInfoPanel, InstanceID>(typeof(WorldInfoPanel).GetField("m_InstanceID", BindingFlags.NonPublic | BindingFlags.Instance));

        private void UpdateAddressField(UIComponent parent)
        {
            if (parent != null)
            {
                UIButton addressIcon = parent.Find<UIButton>("AddressesIcon");
                if (!addressIcon)
                {
                    addressIcon = initBuildingEditOnWorldInfoPanel(parent);
                }
                UIButton zmButton = parent.Find<UIButton>("ZmButton");
                if (!zmButton)
                {
                    zmButton = initZmButtonOnWorldInfoPanel(parent);
                }
                try
                {
                    ushort buildingId = GetWipBuildingInstanceId(parent.gameObject.GetComponent<WorldInfoPanel>()).Building;
                    addressIcon.isVisible = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingId].Info.m_placementMode == BuildingInfo.PlacementMode.Roadside;
                    if (addressIcon.isVisible)
                    {
                        UILabel addressLabel = addressIcon.Find<UILabel>("Address");
                        Vector3 sidewalk = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingId].CalculateSidewalkPosition();
                        Vector3 midPosBuilding = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingId].m_position;

                        AdrUtils.getAddressLines(sidewalk, midPosBuilding, out String[] addressLines);
                        if (addressLines == null)
                        {
                            addressIcon.isVisible = false;
                            return;
                        }
                        addressLabel.prefix = addressLines[0];
                        addressLabel.suffix = addressLines[1] + "\n" + addressLines[2];
                    }
                    zmButton.color = buildingId == AdrConfigWarehouse.GetZeroMarkBuilding() ? Color.white : Color.black;
                    zmButton.focusedColor = zmButton.color;
                }
                catch (Exception e)
                {
                    AdrUtils.doErrorLog($"Exception trying to update address:\n{e.GetType()}\n{e.StackTrace}");
                }
            }
        }

        private void SetZeroMarkBuilding(UIComponent component, UIMouseEventParameter eventParam)
        {
            AdrConfigWarehouse.setCurrentConfigInt(AdrConfigWarehouse.ConfigIndex.CITY_ZERO_BUILDING, GetWipBuildingInstanceId(component.GetComponentInParent<WorldInfoPanel>()).Building);
            component.color = Color.white;
            ((UIButton)component).focusedColor = component.color;
        }

        private UIButton initBuildingEditOnWorldInfoPanel(UIComponent parent)
        {
            AdrUtils.createUIElement(out UIButton saida, parent.transform, "AddressesIcon", new Vector4(5, -40, 30, 30));
            saida.atlas = AdrCommonTextureAtlas.instance.atlas;
            saida.color = Color.white;
            saida.tooltipLocaleID = "ADR_BUILDING_ADDRESS";
            AdrUtils.initButtonSameSprite(saida, "AddressesIcon");

            AdrUtils.createUIElement(out UILabel addressContainer, saida.transform, "Address");
            addressContainer.autoSize = false;
            addressContainer.wordWrap = true;
            addressContainer.area = new Vector4(35, 1, parent.width - 40, 60);
            addressContainer.textAlignment = UIHorizontalAlignment.Left;
            addressContainer.useOutline = true;
            addressContainer.text = Environment.NewLine;
            addressContainer.textScale = 0.6f;
            addressContainer.isInteractive = false;

            return saida;
        }
        private UIButton initZmButtonOnWorldInfoPanel(UIComponent parent)
        {
            AdrUtils.createUIElement(out UIButton saida, parent.transform, "ZmBuilding", new Vector4(-40, -40, 30, 30));
            saida.color = Color.black;
            saida.focusedColor = Color.black;
            saida.hoveredColor = Color.cyan;
            saida.tooltipLocaleID = "ADR_ZERO_MARKER_DETAIL";
            saida.eventDoubleClick += SetZeroMarkBuilding;
            AdrUtils.initButtonSameSprite(saida, "ParkLevelStar");
            saida.canFocus = false;

            return saida;
        }
    }

    delegate Dictionary<string, string[]> ReturnerDictionary();
}