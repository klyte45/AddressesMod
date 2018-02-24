using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.UI;
using Klyte.Addresses.LocaleStruct;
using Klyte.Addresses.Overrides;
using Klyte.Addresses.UI;
using Klyte.Addresses.Utils;
using Klyte.TransportLinesManager.Extensors;
using Klyte.TransportLinesManager.Utils;
using UnityEngine;

namespace Klyte.Addresses
{
    internal class AdrController : Singleton<AdrController>
    {
        internal static UITextureAtlas taAdr;
        private UIView uiView;
        private UIButton openAdrPanelButton;

        private static Dictionary<String, String[]> m_loadedLocalesRoadName;
        private static Dictionary<String, RoadPrefixFileIndexer> m_loadedLocalesRoadPrefix;

        public static Dictionary<String, String[]> loadedLocalesRoadName
        {
            get {
                if (m_loadedLocalesRoadName == null)
                {
                    m_loadedLocalesRoadName = new Dictionary<string, String[]>();
                    foreach (var filename in Directory.GetFiles(AddressesMod.roadPath, "*.txt").Select(x => x.Split(Path.DirectorySeparatorChar).Last()))
                    {
                        string fileContents = File.ReadAllText(AddressesMod.roadPath + Path.DirectorySeparatorChar + filename, Encoding.UTF8);
                        fileContents.Replace(Environment.NewLine, "\n");
                        m_loadedLocalesRoadName[filename] = fileContents.Split('\n').Where(x => !string.IsNullOrEmpty(x)).ToArray();
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

        public static void reloadLocalesRoad()
        {
            m_loadedLocalesRoadName = null;
        }

        public static void reloadLocalesRoadPrefix()
        {
            m_loadedLocalesRoadPrefix = null;
        }

        public void destroy()
        {
            Destroy(openAdrPanelButton);
        }

        public void Start()
        {
            uiView = FindObjectOfType<UIView>();
            if (!uiView)
                return;

            UITabstrip toolStrip = uiView.FindUIComponent<UITabstrip>("MainToolstrip");
            AdrUtils.createUIElement(out openAdrPanelButton, null);
            this.openAdrPanelButton.size = new Vector2(49f, 49f);
            this.openAdrPanelButton.name = "AddressesButton";
            this.openAdrPanelButton.tooltip = "Addresses (v" + AddressesMod.version + ")";
            this.openAdrPanelButton.relativePosition = new Vector3(0f, 5f);
            toolStrip.AddTab("AddressesButton", this.openAdrPanelButton.gameObject, null, null);
            openAdrPanelButton.atlas = taAdr;
            openAdrPanelButton.normalBgSprite = "AddressesIconSmall";
            openAdrPanelButton.focusedFgSprite = "ToolbarIconGroup6Focused";
            openAdrPanelButton.hoveredFgSprite = "ToolbarIconGroup6Hovered";
            this.openAdrPanelButton.eventButtonStateChanged += delegate (UIComponent c, UIButton.ButtonState s)
            {
                if (s == UIButton.ButtonState.Focused)
                {
                    internal_OpenAdrPanel();
                }
                else
                {
                    internal_CloseAdrPanel();
                }
            };
            AdrConfigPanel.Get();

            var typeTarg = typeof(Redirector<>);
            List<Type> instances = GetSubtypesRecursive(typeTarg);

            foreach (Type t in instances)
            {
                gameObject.AddComponent(t);
            }
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

        private void ToggleAdrPanel()
        {
            openAdrPanelButton.SimulateClick();
        }
        public void OpenAdrPanel()
        {
            AddressesMod.instance.showVersionInfoPopup();
            if (!AdrConfigPanel.Get().GetComponent<UIPanel>().isVisible)
            {
                openAdrPanelButton.SimulateClick();
            }
        }
        public void CloseAdrPanel()
        {
            if (AdrConfigPanel.Get().GetComponent<UIPanel>().isVisible)
            {
                openAdrPanelButton.SimulateClick();
            }
        }

        private void internal_CloseAdrPanel()
        {
            AdrConfigPanel.Get().GetComponent<UIPanel>().isVisible = false;
            openAdrPanelButton.Unfocus();
            openAdrPanelButton.state = UIButton.ButtonState.Normal;
        }

        private void internal_OpenAdrPanel()
        {
            AdrConfigPanel.Get().GetComponent<UIPanel>().isVisible = true;
        }

        private void initNearLinesOnWorldInfoPanel()
        {


            UIPanel parent2 = GameObject.Find("UIView").transform.GetComponentInChildren<ZonedBuildingWorldInfoPanel>().GetComponent<UIPanel>();

            if (parent2 != null)
            {
                parent2.eventVisibilityChanged += (component, value) =>
                {
                    UpdateAddressField(parent2);
                };
                parent2.eventPositionChanged += (component, value) =>
                {
                    UpdateAddressField(parent2);
                };
            }

            UIPanel parent = GameObject.Find("UIView").transform.GetComponentInChildren<CityServiceWorldInfoPanel>().GetComponent<UIPanel>();

            if (parent != null)
            {
                parent.eventVisibilityChanged += (component, value) =>
                {
                    UpdateAddressField(parent);
                };
                parent.eventPositionChanged += (component, value) =>
                {
                    UpdateAddressField(parent);
                };
            }

        }
        private ushort[] m_closestSegsFind = new ushort[16];
        private void UpdateAddressField(UIPanel parent)
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
                    NetManager.instance.GetClosestSegments(sidewalk, m_closestSegsFind, out int found);
                    Vector3 targetPosition = default(Vector3);
                    Vector3 targetDirection = default(Vector3);
                    float targetLength = 0;
                    ushort targetSegmentId = 0;
                    if (found == 0)
                    {
                        addressIcon.isVisible = false;
                        return;
                    }
                    else if (found > 1)
                    {
                        float minSqrDist = float.MaxValue;
                        for (int i = 0; i < found; i++)
                        {
                            ushort segId = m_closestSegsFind[i];
                            var seg = NetManager.instance.m_segments.m_buffer[segId];
                            if (!(seg.Info.GetAI() is RoadBaseAI)) continue;
                            AdrUtils.GetClosestPositionAndDirectionAndPoint(seg, sidewalk, out Vector3 position, out Vector3 direction, out float length);
                            float sqrDist = Vector3.SqrMagnitude(sidewalk - position);
                            if (i == 0 || sqrDist < minSqrDist)
                            {
                                minSqrDist = sqrDist;
                                targetPosition = position;
                                targetDirection = direction;
                                targetLength = length;
                                targetSegmentId = segId;
                            }
                        }
                    }
                    else
                    {
                        targetSegmentId = m_closestSegsFind[0];
                        AdrUtils.GetClosestPositionAndDirectionAndPoint(NetManager.instance.m_segments.m_buffer[targetSegmentId], sidewalk, out targetPosition, out targetDirection, out targetLength);
                    }
                    if (targetSegmentId == 0)
                    {
                        addressIcon.isVisible = false;
                        return;
                    }

                    //AdrUtils.doLog($"targets = S:{targetSegmentId} P:{targetPosition} D:{targetDirection.magnitude} L:{targetLength}");

                    List<ushort> roadSegments = AdrUtils.getNodeOrderRoad(targetSegmentId);
                    //AdrUtils.doLog("roadSegments = [{0}] ", string.Join(",", roadSegments.Select(x => x.ToString()).ToArray()));
                    int targetSegmentIdIdx = roadSegments.IndexOf(targetSegmentId);
                    //AdrUtils.doLog($"targSeg = {targetSegmentIdIdx} ({targetSegmentId})");
                    NetSegment targSeg = NetManager.instance.m_segments.m_buffer[targetSegmentId];
                    NetSegment preSeg = default(NetSegment);
                    NetSegment posSeg = default(NetSegment);
                    //AdrUtils.doLog("PreSeg");
                    if (targetSegmentIdIdx > 0)
                    {
                        preSeg = NetManager.instance.m_segments.m_buffer[roadSegments[targetSegmentIdIdx - 1]];
                    }
                    //AdrUtils.doLog("PosSeg");
                    if (targetSegmentIdIdx < roadSegments.Count - 1)
                    {
                        posSeg = NetManager.instance.m_segments.m_buffer[roadSegments[targetSegmentIdIdx + 1]];
                    }
                    //AdrUtils.doLog("startAsEnd");
                    bool startAsEnd = (targetSegmentIdIdx > 0 && (preSeg.m_endNode == targSeg.m_endNode || preSeg.m_startNode == targSeg.m_endNode)) || (targetSegmentIdIdx < roadSegments.Count && (posSeg.m_endNode == targSeg.m_startNode || posSeg.m_startNode == targSeg.m_startNode));

                    //AdrUtils.doLog($"startAsEnd = {startAsEnd}");

                    if (startAsEnd)
                    {
                        targetLength = 1 - targetLength;
                    }


                    //AdrUtils.doLog("streetName"); 
                    string streetName = NetManagerOverrides.GenerateSegmentNameMethod.Invoke(NetManager.instance, new object[] { targetSegmentId })?.ToString();

                    //AdrUtils.doLog("distanceFromStart");
                    float distanceFromStart = 0;
                    for (int i = 0; i < targetSegmentIdIdx; i++)
                    {
                        distanceFromStart += NetManager.instance.m_segments.m_buffer[roadSegments[i]].m_averageLength;
                    }
                    distanceFromStart += targetLength * targSeg.m_averageLength;
                    int number = (int)Math.Round(distanceFromStart);
                    //AdrUtils.doLog($"number = {number} B");

                    float angleTg = VectorUtils.XZ(targetPosition).GetAngleToPoint(VectorUtils.XZ(midPosBuilding));
                    if (angleTg == 90 || angleTg == 270)
                    {
                        angleTg += Math.Sign(targetPosition.z - midPosBuilding.z);
                    }
                    Vector3 startSeg = NetManager.instance.m_nodes.m_buffer[targSeg.m_startNode].m_position;
                    Vector3 endSeg = NetManager.instance.m_nodes.m_buffer[targSeg.m_endNode].m_position;

                    float angleSeg = Vector2.zero.GetAngleToPoint(VectorUtils.XZ(endSeg - startSeg));
                    if (angleSeg == 180)
                    {
                        angleSeg += Math.Sign(targetPosition.z - midPosBuilding.z);
                    }


                    //AdrUtils.doLog($"angleTg = {angleTg};angleSeg = {angleSeg}");
                    if ((angleTg + 90) % 360 < 180 ^ (angleSeg < 180 ^ startAsEnd))
                    {
                        number &= ~1;
                    }
                    else
                    {
                        number |= 1;
                    }
                    //AdrUtils.doLog($"number = {number} A");
                    int districtId = AdrUtils.GetBuildingDistrict(buildingId);
                    string districtName = "";
                    if (districtId > 0)
                    {
                        districtName = DistrictManager.instance.GetDistrictName(districtId) + " - ";
                    }

                    String[] addressLines = new String[] { streetName + "," + number, districtName + SimulationManager.instance.m_metaData.m_CityName };
                    addressLabel.prefix = addressLines[0];
                    addressLabel.suffix = addressLines[1];
                }
            }
        }
        private UIButton initBuildingEditOnWorldInfoPanel(UIPanel parent)
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