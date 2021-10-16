using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using ColossalFramework.UI;
using Klyte.Addresses.Utils;
using Klyte.Commons.Extensions;
using Klyte.Commons.Utils;
using System.Collections;
using UnityEngine;

namespace Klyte.Addresses.UI
{
    public class StadiumListItem : UICustomControl
    {
        private UILabel addressLabel;
        private UITextField teamName;
        private UITextField stadiumName;
        private UIColorField teamColor;
        private UIButton gotoButton;
        private ushort currentBuildingId;

        public void Awake()
        {
            addressLabel = Find<UILabel>("BuildingAddress");
            stadiumName = Find<UITextField>("StadiumName");
            teamName = Find<UITextField>("TeamName");
            teamColor = Find<UIColorField>("TeamColor");
            gotoButton = Find<UIButton>("GoToButton");

            teamColor.eventSelectedColorChanged += SetNewColor;
            stadiumName.eventTextSubmitted += (x, y) => StartCoroutine(SetNewStadiumName(y));
            teamName.eventTextSubmitted += SetNewName;

            stadiumName.zOrder = 0;

            gotoButton.eventClick += delegate (UIComponent c, UIMouseEventParameter r)
            {
                if (currentBuildingId != 0)
                {
                    Vector3 position = BuildingManager.instance.m_buildings.m_buffer[currentBuildingId].m_position;
                    ToolsModifierControl.cameraController.SetTarget(new InstanceID() { Building = currentBuildingId }, position, true);
                }
            };
        }

        public void ResetData(ushort buildingId)
        {
            currentBuildingId = buildingId;
            UpdateFields();
        }

        private void UpdateFields()
        {
            ref Building building = ref BuildingManager.instance.m_buildings.m_buffer[currentBuildingId];
            Vector3 sidewalk = building.CalculateSidewalkPosition();
            AdrUtils.GetAddressLines(sidewalk, building.m_position, out string[] addressLines);
            addressLabel.text = string.Join("\n", addressLines);

            var eventIndex = building.m_eventIndex;
            ref EventData eventData = ref Singleton<EventManager>.instance.m_events.m_buffer[eventIndex];
            EventInfo eventInfo = eventData.Info;

            teamName.text = AdrController.CurrentConfig.GlobalConfig.FootballConfig.localTeamsNames.TryGetValue(currentBuildingId, out string teamNameStr)
                ? teamNameStr.IsNullOrWhiteSpace()
                    ? FallbackTeamName(currentBuildingId, eventInfo)
                    : teamNameStr
                : FallbackTeamName(currentBuildingId, eventInfo);

            Color color = eventInfo.m_eventAI.GetBuildingColor(eventIndex, ref eventData);
            teamColor.selectedColor = color;
            stadiumName.text = BuildingManager.instance.GetBuildingName(currentBuildingId, default);
        }

        private void SetNewColor(UIComponent a, Color x)
        {
            ref Building building = ref BuildingManager.instance.m_buildings.m_buffer[currentBuildingId];
            var eventIndex = building.m_eventIndex;
            ref EventData eventData = ref Singleton<EventManager>.instance.m_events.m_buffer[eventIndex];
            EventInfo eventInfo = eventData.Info;
            (eventInfo.m_eventAI as SportMatchAI).m_allowChangeColor = true;
            eventInfo.m_eventAI.SetColor(eventIndex, ref eventData, x);
        }

        private void SetNewName(UIComponent a, string name)
        {
            ref Building building = ref BuildingManager.instance.m_buildings.m_buffer[currentBuildingId];
            var eventIndex = building.m_eventIndex;
            ref EventData eventData = ref Singleton<EventManager>.instance.m_events.m_buffer[eventIndex];
            EventInfo eventInfo = eventData.Info;
            if (name.IsNullOrWhiteSpace() || name == FallbackTeamName(currentBuildingId, eventInfo))
            {
                AdrController.CurrentConfig.GlobalConfig.FootballConfig.localTeamsNames.Remove(currentBuildingId);
                teamName.text = FallbackTeamName(currentBuildingId, eventInfo);
            }
            else
            {
                AdrController.CurrentConfig.GlobalConfig.FootballConfig.localTeamsNames[currentBuildingId] = name;
            }
        }

        private IEnumerator SetNewStadiumName(string name)
        {
            yield return BuildingManager.instance.SetBuildingName(currentBuildingId, name);
            stadiumName.text = BuildingManager.instance.GetBuildingName(currentBuildingId, default);
        }

        private string FallbackTeamName(int m_building, EventInfo eventInfo)
        {
            var prefabName = PrefabCollection<EventInfo>.PrefabName((uint)eventInfo.m_prefabDataIndex);
            uint num = Locale.Count("SPORT_TEAM_NAME", prefabName);
            if (num != 0U)
            {
                Randomizer randomizer = new Randomizer(m_building);
                string format = Locale.Get("SPORT_TEAM_NAME", prefabName, randomizer.Int32(num));
                string text2 = SimulationManager.instance.m_metaData.m_CityName;
                if (text2 == null)
                {
                    text2 = "Cities: Skylines";
                }
                return StringUtils.SafeFormat(format, text2);
            }
            return prefabName;
        }

        public static void CreateStadiumLineTemplate()
        {
            var go = new GameObject();
            UIPanel panel = go.AddComponent<UIPanel>();
            panel.size = new Vector2(815, 30);
            panel.autoLayout = true;
            panel.wrapLayout = false;
            panel.autoLayoutDirection = LayoutDirection.Horizontal;
            panel.padding.top = 4;
            panel.padding.bottom = 4;


            var stadiumName = panel.AddUIComponent<UITextField>();
            KlyteMonoUtils.UiTextFieldDefaultsForm(stadiumName);
            stadiumName.width = (panel.width - 240) / 2;
            stadiumName.name = "StadiumName";
            stadiumName.forceZOrder = 0;
            stadiumName.zOrder = 0;

            var nameInput = panel.AddUIComponent<UITextField>();
            KlyteMonoUtils.UiTextFieldDefaultsForm(nameInput);
            nameInput.width = (panel.width - 240) / 2;
            nameInput.name = "TeamName";
            nameInput.forceZOrder = 1;
            nameInput.zOrder = 1;

            var teamColor = KlyteMonoUtils.CreateColorField(panel);
            teamColor.name = "TeamColor";
            teamColor.forceZOrder = 2;

            var labelAddress = UIHelperExtension.AddLabel(panel, "Address", 225, out _);
            labelAddress.name = "BuildingAddress";
            labelAddress.autoSize = false;
            labelAddress.height = 29f;
            labelAddress.width = 225;
            labelAddress.processMarkup = true;
            labelAddress.textScale = 0.55f;
            labelAddress.textAlignment = UIHorizontalAlignment.Right;
            labelAddress.forceZOrder = 3;

            KlyteMonoUtils.CreateUIElement(out UIButton gotoButton, panel.transform, "GoToButton", new Vector4(0, 0, 28, 28));
            KlyteMonoUtils.InitButton(gotoButton, true, "LineDetailButton");

            go.AddComponent<StadiumListItem>();

            UITemplateUtils.GetTemplateDict()["K45_ADR_StadiumLineTemplate"] = panel;
        }
    }



}
