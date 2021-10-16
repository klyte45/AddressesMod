using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Addresses.LocaleStruct;
using Klyte.Addresses.ModShared;
using Klyte.Addresses.Overrides;
using Klyte.Addresses.Tools;
using Klyte.Addresses.UI;
using Klyte.Addresses.Utils;
using Klyte.Addresses.Xml;
using Klyte.Commons.Interfaces;
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
    public class AdrController : BaseController<AddressesMod, AdrController>
    {


        private static Dictionary<string, string[]> m_loadedLocalesRoadName;
        private static Dictionary<string, string[]> m_loadedLocalesNeighborName;
        private static Dictionary<string, string[]> m_loadedLocalesDistrictPrefix;
        private static Dictionary<string, string[]> m_loadedLocalesDistrictName;
        private static Dictionary<string, string[]> m_loadedLocalesCitizenFirstNameMasc;
        private static Dictionary<string, string[]> m_loadedLocalesCitizenFirstNameFem;
        private static Dictionary<string, string[]> m_loadedLocalesCitizenLastName;
        private static Dictionary<string, FootballTeamData[]> m_loadedLocalesFootballTeams;

        internal static Dictionary<string, string[]> LoadedLocalesRoadName => m_loadedLocalesRoadName;
        internal static Dictionary<string, RoadPrefixFileIndexer> LoadedLocalesRoadPrefix { get; private set; }
        internal static Dictionary<string, string[]> LoadedLocalesNeighborName => m_loadedLocalesNeighborName;
        internal static Dictionary<string, string[]> LoadedLocalesDistrictPrefix => m_loadedLocalesDistrictPrefix;
        internal static Dictionary<string, string[]> LoadedLocalesDistrictName => m_loadedLocalesDistrictName;
        internal static Dictionary<string, string[]> LoadedLocalesCitizenFirstNameMasc => m_loadedLocalesCitizenFirstNameMasc;
        internal static Dictionary<string, string[]> LoadedLocalesCitizenFirstNameFem => m_loadedLocalesCitizenFirstNameFem;
        internal static Dictionary<string, string[]> LoadedLocalesCitizenLastName => m_loadedLocalesCitizenLastName;
        internal static Dictionary<string, FootballTeamData[]> LoadedLocalesFootballTeams => m_loadedLocalesFootballTeams;

        public static void LoadLocalesRoadPrefix()
        {
            LoadedLocalesRoadPrefix = new Dictionary<string, RoadPrefixFileIndexer>();
            foreach (string filename in Directory.GetFiles(AddressesMod.RoadPrefixPath, "*.txt").Select(x => x.Split(Path.DirectorySeparatorChar).Last()))
            {
                string fileContents = File.ReadAllText(AddressesMod.RoadPrefixPath + Path.DirectorySeparatorChar + filename, Encoding.UTF8);
                fileContents.Replace(Environment.NewLine, "\n");
                LoadedLocalesRoadPrefix[filename] = RoadPrefixFileIndexer.Parse(fileContents.Split('\n').Where(x => !string.IsNullOrEmpty(x) && !x.StartsWith("#")).ToArray());
                LogUtils.DoLog("LOADED PREFIX NAMES ({0}) QTT: {1}", filename, LoadedLocalesRoadPrefix[filename].Length);
            }
        }

        public static void LoadLocalesRoadNames() => LoadSimpleNamesFiles(out m_loadedLocalesRoadName, AddressesMod.RoadPath);
        public static void LoadLocalesNeighborName() => LoadSimpleNamesFiles(out m_loadedLocalesNeighborName, AddressesMod.NeigborsPath);
        public static void LoadLocalesDistrictPrefix() => LoadSimpleNamesFiles(out m_loadedLocalesDistrictPrefix, AddressesMod.DistrictPrefixPath);
        public static void LoadLocalesDistrictName() => LoadSimpleNamesFiles(out m_loadedLocalesDistrictName, AddressesMod.DistrictNamePath);
        public static void LoadLocalesCitizenFirstNameMale() => LoadSimpleNamesFiles(out m_loadedLocalesCitizenFirstNameMasc, AddressesMod.CitizenFirstNameMascPath);
        public static void LoadLocalesCitizenFirstNameFemale() => LoadSimpleNamesFiles(out m_loadedLocalesCitizenFirstNameFem, AddressesMod.CitizenFirstNameFemPath);
        public static void LoadLocalesCitizenLastName() => LoadSimpleNamesFiles(out m_loadedLocalesCitizenLastName, AddressesMod.CitizenLastNamePath);
        public static void LoadLocalesFootbalTeams() => LoadFootballNamesFiles(out m_loadedLocalesFootballTeams, AddressesMod.FootballTeamDataFolder);

        private static void LoadSimpleNamesFiles(out Dictionary<string, string[]> result, string path)
        {
            result = new Dictionary<string, string[]>();
            foreach (string filename in Directory.GetFiles(path, "*.txt").Select(x => x.Split(Path.DirectorySeparatorChar).Last()))
            {
                string fileContents = File.ReadAllText(path + Path.DirectorySeparatorChar + filename, Encoding.UTF8);
                result[filename] = fileContents.Split(Environment.NewLine.ToCharArray()).Select(x => x?.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray();
                LogUtils.DoLog($"LOADED Files at {path} ({filename}) QTT: {result[filename].Length}");
            }
        }
        private static void LoadFootballNamesFiles(out Dictionary<string, FootballTeamData[]> result, string path)
        {
            var errorFiles = new List<string>();
            result = new Dictionary<string, FootballTeamData[]>();
            foreach (string filename in Directory.GetFiles(path, "*.txt").Select(x => x.Split(Path.DirectorySeparatorChar).Last()))
            {
                string fileContents = File.ReadAllText(path + Path.DirectorySeparatorChar + filename, Encoding.UTF8);
                result[filename] = fileContents.Split(Environment.NewLine.ToCharArray()).Select(x => x?.Trim()).Where(x => !string.IsNullOrEmpty(x) && !x.StartsWith("#")).Select(x => new FootballTeamData(x)).ToArray();
                var invalidData = result[filename].Where(x => x.Color == default);
                if (invalidData.Count() > 0)
                {
                    LogUtils.DoErrorLog($"Invalid lines at {filename}:\n{string.Join("\n", invalidData.Select(x => x.Name).ToArray())}\n\n===> All lines must have the pattern below:\nName of team=RRGGBB\nRRGGBB is the color of the team, hexadecimal format");
                    errorFiles.Add(filename);
                }
                result[filename] = result[filename].Where(x => x.Color != default).ToArray();
                LogUtils.DoLog($"LOADED Football Files at {path} ({filename}) QTT: {result[filename].Length}");
            }
            if (errorFiles.Count > 0)
            {
                K45DialogControl.ShowModal(new K45DialogControl.BindProperties
                {
                    textButton1 = "OK",
                    showButton1 = true,
                    message = Locale.Get("K45_ADR_FOOTBALLFILES_PROBLEM_HEAD") + $"\n{string.Join("\n", errorFiles.Take(10).ToArray())}{(errorFiles.Count > 10 ? $"\n... others {errorFiles.Count - 10} files" : "")}"
                }, (x) => true);
            }
        }

        public static void ReloadAllFiles()
        {
            LoadLocalesRoadNames();
            LoadLocalesRoadPrefix();
            LoadLocalesNeighborName();
            LoadLocalesDistrictPrefix();
            LoadLocalesDistrictName();
            LoadLocalesCitizenFirstNameMale();
            LoadLocalesCitizenFirstNameFemale();
            LoadLocalesCitizenLastName();
            LoadLocalesFootbalTeams();
        }

        public void OpenAdrPanel() => AddressesMod.Instance.OpenPanelAtModTab();
        public void CloseAdrPanel() => AddressesMod.Instance.ClosePanel();

        public AdrFacade FacadeInstance { get; private set; }

        public void Awake()
        {
            if (FindObjectOfType<RoadSegmentTool>() is null)
            {
                ToolsModifierControl.toolController.gameObject.AddComponent<RoadSegmentTool>();
            }

            InitNearLinesOnWorldInfoPanel();

            BuildingManagerOverrides.EventBuidlingReleased += RemoveZeroMarker;

            FacadeInstance = gameObject.AddComponent<AdrFacade>();

            FacadeInstance.EventHighwaysChanged += AdrNameSeedDataXml.Instance.EraseParentCaches;
        }

        private void RemoveZeroMarker(ushort building)
        {
            if (CurrentConfig.GlobalConfig.AddressingConfig.ZeroMarkBuilding == building)
            {
                CurrentConfig.GlobalConfig.AddressingConfig.ZeroMarkBuilding = default;
            }
        }

        private void InitNearLinesOnWorldInfoPanel()
        {
            BuildingWorldInfoPanel[] panelList = GameObject.Find("UIView").GetComponentsInChildren<BuildingWorldInfoPanel>();
            LogUtils.DoLog("WIP LIST: [{0}]", string.Join(", ", panelList.Select(x => x.name).ToArray()));

            foreach (BuildingWorldInfoPanel wip in panelList)
            {
                LogUtils.DoLog("LOADING WIP HOOK FOR: {0}", wip.name);
                UIComponent parent2 = wip.GetComponent<UIComponent>();
                InitBuildingEditOnWorldInfoPanel(parent2);
                parent2.eventVisibilityChanged += (component, value) => UpdateAddressField(parent2);
                parent2.eventPositionChanged += (component, value) => UpdateAddressField(parent2);
            }

        }

        private static Func<WorldInfoPanel, InstanceID> GetWipBuildingInstanceId { get; } = ReflectionUtils.GetGetFieldDelegate<WorldInfoPanel, InstanceID>(typeof(WorldInfoPanel).GetField("m_InstanceID", BindingFlags.NonPublic | BindingFlags.Instance));

        private void UpdateAddressField(UIComponent parent)
        {
            if (parent != null)
            {
                var addressBar = parent.Find<UIPanel>("AddressesBar");
                if (!addressBar)
                {
                    LogUtils.DoErrorLog($"ADDRESSES BAR NOT FOUND @ {parent.name}!");
                    return;
                }
                try
                {
                    var addressLabel = addressBar.Find<UILabel>("Address");
                    ushort buildingId = GetWipBuildingInstanceId(parent.gameObject.GetComponent<WorldInfoPanel>()).Building;

                    Vector3 sidewalk = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingId].CalculateSidewalkPosition();
                    Vector3 midPosBuilding = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingId].m_position;

                    AdrUtils.GetAddressLines(sidewalk, midPosBuilding, out string[] addressLines);
                    if (addressLines == null)
                    {
                        addressLabel.prefix = "";
                        addressLabel.suffix = Locale.Get("K45_ADR_NOADDRESSAVAILABLE");
                    }
                    else
                    {
                        addressLabel.prefix = addressLines[0];
                        addressLabel.suffix = addressLines[1] + "\n" + addressLines[2];
                    }

                    var zmButton = addressBar.Find<UIButton>("ZmBuilding");
                    zmButton.color = buildingId == CurrentConfig.GlobalConfig.AddressingConfig.ZeroMarkBuilding ? Color.white : Color.black;
                    zmButton.focusedColor = zmButton.color;
                }
                catch (Exception e)
                {
                    LogUtils.DoErrorLog($"Exception trying to update address:\n{e.GetType()}\n{e.StackTrace}");
                }
            }
        }

        private void SetZeroMarkBuilding(UIComponent component, UIMouseEventParameter eventParam)
        {
            CurrentConfig.GlobalConfig.AddressingConfig.ZeroMarkBuilding = GetWipBuildingInstanceId(component.GetComponentInParent<WorldInfoPanel>()).Building;
            component.color = Color.white;
            ((UIButton)component).focusedColor = component.color;
        }

        private void InitBuildingEditOnWorldInfoPanel(UIComponent parent)
        {
            var container = parent.Find<UIPanel>("MainSectionPanel");
            var isInherited = !(container is null);
            if (!isInherited)
            {
                container = parent.GetComponent<UIPanel>();
            }
            KlyteMonoUtils.CreateUIElement(out UIPanel addressesComponent, container.transform, "AddressesBar");
            addressesComponent.forceZOrder = 2;
            addressesComponent.autoLayout = true;
            addressesComponent.autoLayoutDirection = LayoutDirection.Horizontal;
            addressesComponent.height = 40;
            addressesComponent.width = container.width;
            if (isInherited)
            {
                addressesComponent.width = container.width;
            }
            else
            {
                addressesComponent.width = 300;
                addressesComponent.padding.top = 5;
                addressesComponent.relativePosition = new Vector3(container.width + 10, 0);
                addressesComponent.backgroundSprite = "GenericPanelDark";
            }
            KlyteMonoUtils.CreateUIElement(out UIButton saida, addressesComponent.transform, "AddressesIcon", new Vector4(5, -40, 30, 30));
            saida.color = Color.white;
            saida.tooltipLocaleID = "K45_ADR_BUILDING_ADDRESS";
            KlyteMonoUtils.InitButtonSameSprite(saida, AddressesMod.Instance.IconName);

            KlyteMonoUtils.CreateUIElement(out UILabel addressLabel, addressesComponent.transform, "Address");
            addressLabel.autoSize = false;
            addressLabel.wordWrap = true;
            addressLabel.area = new Vector4(35, 1, addressesComponent.width - 70, 60);
            addressLabel.textAlignment = UIHorizontalAlignment.Left;
            addressLabel.useOutline = true;
            addressLabel.text = Environment.NewLine;
            addressLabel.textScale = 0.6f;
            addressLabel.isInteractive = false;

            KlyteMonoUtils.CreateUIElement(out UIButton zeroMarker, addressesComponent.transform, "ZmBuilding", new Vector4(0, 0, 30, 30));
            zeroMarker.color = Color.gray;
            zeroMarker.focusedColor = Color.gray;
            zeroMarker.hoveredColor = Color.cyan;
            zeroMarker.tooltipLocaleID = "K45_ADR_ZERO_MARKER_DETAIL";
            zeroMarker.eventDoubleClick += SetZeroMarkBuilding;
            KlyteMonoUtils.InitButtonSameSprite(zeroMarker, "ParkLevelStar");
            zeroMarker.canFocus = false;

            if (parent.GetComponent<FootballPanel>())
            {
                addressesComponent.width += 30;
                KlyteMonoUtils.CreateUIElement(out UIButton stadiumEdit, addressesComponent.transform, "ZmBuilding", new Vector4(0, 0, 30, 30));
                stadiumEdit.tooltipLocaleID = "K45_ADR_GOTO_STADIUMSSETTINGS";
                KlyteMonoUtils.InitButton(stadiumEdit, false, "ButtonMenu");
                KlyteMonoUtils.InitButtonFg(stadiumEdit, false, "SubBarMonumentFootball");
                stadiumEdit.canFocus = false;
                stadiumEdit.eventClicked += (x, y) =>
                {
                    OpenAdrPanel();
                    AdrConfigPanel.Instance.SetActiveTab("AdrFootball");
                    parent.GetComponent<FootballPanel>().CloseEverything();
                };
            }
        }

        internal static AdrConfigXml CurrentConfig => AdrConfigXml.Instance;

    }

    internal delegate Dictionary<string, string[]> ReturnerDictionary();
}