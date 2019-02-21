using ColossalFramework;
using ColossalFramework.UI;
using Klyte.Addresses.LocaleStruct;
using Klyte.Addresses.TextureAtlas;
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
                try
                {
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
                catch (Exception e)
                {
                    AdrUtils.doErrorLog($"Exception trying to update address:\n{e.GetType()}\n{e.StackTrace}");
                }
            }
        }

        private UIButton initBuildingEditOnWorldInfoPanel(UIComponent parent)
        {
            AdrUtils.createUIElement(out UIButton saida, parent.transform, "AddressesIcon", new Vector4(5, -40, 30, 30));
            saida.atlas = AdrCommonTextureAtlas.instance.atlas;
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

    delegate Dictionary<string, string[]> ReturnerDictionary();
}