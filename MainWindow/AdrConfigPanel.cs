using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using UnityEngine;

namespace Klyte.Addresses.UI
{

    public class AdrConfigPanel : BasicKPanel<AddressesMod, AdrController, AdrConfigPanel>
    {

        public UIPanel ControlContainer { get; private set; }

        public override float PanelWidth => 450;

        public override float PanelHeight => 600;

        private UITabstrip m_stripMain;

        #region Awake
        protected override void AwakeActions()
        {


            KlyteMonoUtils.CreateUIElement(out m_stripMain, base.MainPanel.transform, "AdrTabstrip", new Vector4(5, 45, base.MainPanel.width - 5, 40));

            KlyteMonoUtils.CreateUIElement(out UITabContainer tabContainer, base.MainPanel.transform, "AdrTabContainer", new Vector4(5, 80, base.MainPanel.width - 5, base.MainPanel.height - 80));
            m_stripMain.tabPages = tabContainer;

            CreateTab<AdrDistrictConfigTab>("ToolbarIconDistrict", "K45_ADR_CONFIG_PER_DISTRICT_TAB", "AdrPerDistrict");
            CreateTab<AdrNeighborConfigTab>("IconRightArrow", "K45_ADR_CONFIG_NEIGHBOR_TAB", "AdrNeighbor");
            CreateTab<AdrCitizenConfigTab>("IconCitizen", "K45_ADR_CONFIG_CITIZEN_TAB", "AdrCitizen");
            CreateTab<AdrGlobalConfigTab>("ToolbarIconZoomOutGlobe", "K45_ADR_CONFIG_GLOBAL_TAB", "AdrGlobal");
        }

        private void CreateTab<T>(string sprite, string localeKey, string objectName) where T : UICustomControl
        {
            UIButton tab = CreateTabTemplate();
            tab.normalFgSprite = sprite;
            tab.tooltip = Locale.Get(localeKey);

            KlyteMonoUtils.CreateUIElement(out UIPanel contentContainer, null);
            contentContainer.name = "Container";
            contentContainer.area = new Vector4(15, 0, MainPanel.width - 30, MainPanel.height - 70);
            m_stripMain.AddTab(objectName, tab.gameObject, contentContainer.gameObject);

            KlyteMonoUtils.CreateScrollPanel(contentContainer, out _, out _, contentContainer.width - 20, contentContainer.height - 5, new Vector3()).Self.gameObject.AddComponent<T>();
        }

        private static UIButton CreateTabTemplate()
        {
            KlyteMonoUtils.CreateUIElement(out UIButton tabTemplate, null, "AdrTabTemplate");
            KlyteMonoUtils.InitButton(tabTemplate, false, "GenericTab");
            tabTemplate.autoSize = false;
            tabTemplate.height = 30;
            tabTemplate.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            return tabTemplate;
        }
        
        #endregion


        public void SetActiveTab(int idx) => m_stripMain.selectedIndex = idx;

        public void Start()
        {
            SetActiveTab(-1);
            SetActiveTab(0);
        }
    }


}
