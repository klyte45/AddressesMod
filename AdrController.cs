using ColossalFramework;
using ColossalFramework.UI;
using Klyte.Addresses.LocaleStruct;
using Klyte.Addresses.UI;
using Klyte.Addresses.Utils;
using Klyte.Commons;
using Klyte.Commons.Extensors;
using Klyte.Commons.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Klyte.Addresses
{
    internal class AdrController : Singleton<AdrController>
    {
        internal static UITextureAtlas taAdr;

        private static Dictionary<String, String[]> m_loadedLocalesRoadName;
        private static Dictionary<String, RoadPrefixFileIndexer> m_loadedLocalesRoadPrefix;
        private static Dictionary<String, String[]> m_loadedLocalesNeighborName;

        public static Dictionary<String, String[]> loadedLocalesRoadName
        {
            get {
                if (m_loadedLocalesRoadName == null)
                {
                    m_loadedLocalesRoadName = new Dictionary<string, String[]>();
                    foreach (var filename in Directory.GetFiles(AddressesMod.roadPath, "*.txt").Select(x => x.Split(Path.DirectorySeparatorChar).Last()))
                    {
                        string fileContents = File.ReadAllText(AddressesMod.roadPath + Path.DirectorySeparatorChar + filename, Encoding.UTF8);
                        m_loadedLocalesRoadName[filename] = fileContents.Split(Environment.NewLine.ToCharArray()).Select(x => x?.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray();
                        AdrUtils.doLog("LOADED NAMES ({0}) QTT: {1}", filename, m_loadedLocalesRoadName[filename].Length);
                    }
                }

                return m_loadedLocalesRoadName;
            }
        }

        public static Dictionary<String, RoadPrefixFileIndexer> loadedLocalesRoadPrefix
        {
            get {
                if (m_loadedLocalesRoadPrefix == null)
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

                return m_loadedLocalesRoadPrefix;
            }
        }

        public static Dictionary<String, String[]> loadedLocalesNeighborName
        {
            get {
                if (m_loadedLocalesNeighborName == null)
                {
                    m_loadedLocalesNeighborName = new Dictionary<string, String[]>();
                    foreach (var filename in Directory.GetFiles(AddressesMod.neigborsPath, "*.txt").Select(x => x.Split(Path.DirectorySeparatorChar).Last()))
                    {
                        string fileContents = File.ReadAllText(AddressesMod.neigborsPath + Path.DirectorySeparatorChar + filename, Encoding.UTF8);
                        m_loadedLocalesNeighborName[filename] = fileContents.Split(Environment.NewLine.ToCharArray()).Select(x => x?.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray();
                        AdrUtils.doLog("LOADED Neighbors ({0}) QTT: {1}", filename, m_loadedLocalesNeighborName[filename].Length);
                    }
                }

                return m_loadedLocalesNeighborName;
            }
        }

        public static void reloadLocalesRoad()
        {
            m_loadedLocalesRoadName = null;
        }

        public static void reloadLocalesRoadPrefix()
        {
            m_loadedLocalesRoadPrefix = null;
        }

        public static void reloadLocalesNeighbors()
        {
            m_loadedLocalesNeighborName = null;
        }


        public void Start()
        {
            KlyteModsPanel.instance.AddTab(ModTab.Addresses, typeof(AdrConfigPanel), taAdr, "AddressesIcon", "Addresses (v" + AddressesMod.version + ")").eventVisibilityChanged += (x, y) => { if (y) AddressesMod.instance.showVersionInfoPopup(); };

            var typeTarg = typeof(Redirector<>);
            List<Type> instances = GetSubtypesRecursive(typeTarg);

            foreach (Type t in instances)
            {
                gameObject.AddComponent(t);
            }
        }

        public void OpenAdrPanel()
        {
            KlyteModsPanel.instance.OpenAt(ModTab.Addresses);
        }
        public void CloseAdrPanel()
        {
            KCController.instance.CloseKCPanel();
        }


        private static List<Type> GetSubtypesRecursive(Type typeTarg)
        {
            var classes = from t in Assembly.GetAssembly(typeof(AdrController)).GetTypes()
                          let y = t.BaseType
                          where t.IsClass && y != null && y.IsGenericType == typeTarg.IsGenericType && (y.GetGenericTypeDefinition() == typeTarg || y.BaseType == typeTarg)
                          select t;
            List<Type> result = new List<Type>();
            foreach (Type t in classes)
            {
                if (t.IsAbstract)
                {
                    result.AddRange(GetSubtypesRecursive(t));
                }
                else
                {
                    result.Add(t);
                }
            }
            return result;
        }

        public void Awake()
        {
            initNearLinesOnWorldInfoPanel();
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
        private void UpdateAddressField(UIComponent parent)
        {
            if (parent != null)
            {
                UIButton addressIcon = parent.Find<UIButton>("AddressesIcon");
                if (!addressIcon)
                {
                    addressIcon = initBuildingEditOnWorldInfoPanel(parent);
                }
                var prop = typeof(WorldInfoPanel).GetField("m_InstanceID", System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Instance);
                ushort buildingId = ((InstanceID)(prop.GetValue(parent.gameObject.GetComponent<WorldInfoPanel>()))).Building;
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
            }
        }

        private UIButton initBuildingEditOnWorldInfoPanel(UIComponent parent)
        {
            AdrUtils.createUIElement(out UIButton saida, parent.transform, "AddressesIcon", new Vector4(5, -40, 30, 30));
            saida.atlas = taAdr;
            saida.color = Color.white;
            saida.tooltipLocaleID = "ADR_BUILDING_ADDRESS";
            AdrUtils.initButtonSameSprite(saida, "AddressesIcon");

            AdrUtils.createUIElement(out UILabel prefixes, saida.transform, "Address");
            prefixes.autoSize = false;
            prefixes.wordWrap = true;
            prefixes.area = new Vector4(35, 1, parent.width - 40, 60);
            prefixes.textAlignment = UIHorizontalAlignment.Left;
            prefixes.useOutline = true;
            prefixes.text = Environment.NewLine;
            prefixes.textScale = 0.6f;

            return saida;
        }
    }
}