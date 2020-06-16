using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using Klyte.Addresses.LocaleStruct;
using Klyte.Addresses.ModShared;
using Klyte.Addresses.Overrides;
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
    public class AdrController : BaseController<AddressesMod, AdrController>, ISerializableDataExtension
    {


        private static Dictionary<string, string[]> m_loadedLocalesRoadName;
        private static Dictionary<string, string[]> m_loadedLocalesNeighborName;
        private static Dictionary<string, string[]> m_loadedLocalesDistrictPrefix;
        private static Dictionary<string, string[]> m_loadedLocalesDistrictName;
        private static Dictionary<string, string[]> m_loadedLocalesCitizenFirstNameMasc;
        private static Dictionary<string, string[]> m_loadedLocalesCitizenFirstNameFem;
        private static Dictionary<string, string[]> m_loadedLocalesCitizenLastName;

        internal static Dictionary<string, string[]> LoadedLocalesRoadName => m_loadedLocalesRoadName;
        internal static Dictionary<string, RoadPrefixFileIndexer> LoadedLocalesRoadPrefix { get; private set; }
        internal static Dictionary<string, string[]> LoadedLocalesNeighborName => m_loadedLocalesNeighborName;
        internal static Dictionary<string, string[]> LoadedLocalesDistrictPrefix => m_loadedLocalesDistrictPrefix;
        internal static Dictionary<string, string[]> LoadedLocalesDistrictName => m_loadedLocalesDistrictName;
        internal static Dictionary<string, string[]> LoadedLocalesCitizenFirstNameMasc => m_loadedLocalesCitizenFirstNameMasc;
        internal static Dictionary<string, string[]> LoadedLocalesCitizenFirstNameFem => m_loadedLocalesCitizenFirstNameFem;
        internal static Dictionary<string, string[]> LoadedLocalesCitizenLastName => m_loadedLocalesCitizenLastName;

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

        public void OpenAdrPanel() => AddressesMod.Instance.OpenPanelAtModTab();
        public void CloseAdrPanel() => AddressesMod.Instance.ClosePanel();

        public AdrShared SharedInstance;

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

            InitNearLinesOnWorldInfoPanel();

            BuildingManagerOverrides.EventBuidlingReleased += RemoveZeroMarker;

            SharedInstance = gameObject.AddComponent<AdrShared>();
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
                parent2.eventVisibilityChanged += (component, value) => UpdateAddressField(parent2);
                parent2.eventPositionChanged += (component, value) => UpdateAddressField(parent2);
            }

        }

        private static Func<WorldInfoPanel, InstanceID> GetWipBuildingInstanceId { get; } = ReflectionUtils.GetGetFieldDelegate<WorldInfoPanel, InstanceID>(typeof(WorldInfoPanel).GetField("m_InstanceID", BindingFlags.NonPublic | BindingFlags.Instance));

        private void UpdateAddressField(UIComponent parent)
        {
            if (parent != null)
            {
                UIButton addressIcon = parent.Find<UIButton>("AddressesIcon");
                if (!addressIcon)
                {
                    addressIcon = InitBuildingEditOnWorldInfoPanel(parent);
                }
                UIButton zmButton = parent.Find<UIButton>("ZmButton");
                if (!zmButton)
                {
                    zmButton = InitZmButtonOnWorldInfoPanel(parent);
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

                        AdrUtils.GetAddressLines(sidewalk, midPosBuilding, out string[] addressLines);
                        if (addressLines == null)
                        {
                            addressIcon.isVisible = false;
                            return;
                        }
                        addressLabel.prefix = addressLines[0];
                        addressLabel.suffix = addressLines[1] + "\n" + addressLines[2];
                    }
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

        private UIButton InitBuildingEditOnWorldInfoPanel(UIComponent parent)
        {
            KlyteMonoUtils.CreateUIElement(out UIButton saida, parent.transform, "AddressesIcon", new Vector4(5, -40, 30, 30));
            saida.color = Color.white;
            saida.tooltipLocaleID = "K45_ADR_BUILDING_ADDRESS";
            KlyteMonoUtils.InitButtonSameSprite(saida, AddressesMod.Instance.IconName);

            KlyteMonoUtils.CreateUIElement(out UILabel addressContainer, saida.transform, "Address");
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
        private UIButton InitZmButtonOnWorldInfoPanel(UIComponent parent)
        {
            KlyteMonoUtils.CreateUIElement(out UIButton saida, parent.transform, "ZmBuilding", new Vector4(-40, -40, 30, 30));
            saida.color = Color.black;
            saida.focusedColor = Color.black;
            saida.hoveredColor = Color.cyan;
            saida.tooltipLocaleID = "K45_ADR_ZERO_MARKER_DETAIL";
            saida.eventDoubleClick += SetZeroMarkBuilding;
            KlyteMonoUtils.InitButtonSameSprite(saida, "ParkLevelStar");
            saida.canFocus = false;

            return saida;
        }


        internal static AdrConfigXml CurrentConfig { get; private set; } = new AdrConfigXml();

        #region Serialization
        protected const string ID = "K45_ADR_MAIN";
        public IManagers Managers => SerializableDataManager?.managers;

        public ISerializableData SerializableDataManager { get; private set; }

        public void OnCreated(ISerializableData serializableData) => SerializableDataManager = serializableData;
        public void OnLoadData()
        {
            if (ID == null || Singleton<ToolManager>.instance.m_properties.m_mode != ItemClass.Availability.Game)
            {
                return;
            }
            if (!SerializableDataManager.EnumerateData().Contains(ID))
            {
                return;
            }
            using var memoryStream = new MemoryStream(SerializableDataManager.LoadData(ID));
            byte[] storage = memoryStream.ToArray();
            Deserialize(System.Text.Encoding.UTF8.GetString(storage));
        }

        public void OnSaveData()
        {
            if (ID == null || Singleton<ToolManager>.instance.m_properties.m_mode != ItemClass.Availability.Game)
            {
                return;
            }

            string serialData = Serialize();
            LogUtils.DoLog($"serialData: {serialData ?? "<NULL>"}");
            if (serialData == null)
            {
                return;
            }

            byte[] data = System.Text.Encoding.UTF8.GetBytes(serialData);
            SerializableDataManager.SaveData(ID, data);
        }


        public void Deserialize(string data)
        {
            LogUtils.DoLog($"{GetType()} STR: \"{data}\"");
            if (data.IsNullOrWhiteSpace())
            {
                return;
            }
            try
            {
                CurrentConfig = XmlUtils.DefaultXmlDeserialize<AdrConfigXml>(data);
            }
            catch (Exception e)
            {
                LogUtils.DoErrorLog($"Error deserializing: {e.Message}\n{e.StackTrace}");
            }
        }

        public string Serialize() => XmlUtils.DefaultXmlSerialize(CurrentConfig, false);

        public void OnReleased() { }
        #endregion
    }

    internal delegate Dictionary<string, string[]> ReturnerDictionary();
}