using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Addresses.TextureAtlas;
using Klyte.Commons.Utils;
using UnityEngine;

namespace Klyte.Addresses.UI
{

    public class AdrConfigPanel : UICustomControl
    {

        public UIPanel ControlContainer { get; private set; }
        private UIPanel m_mainPanel;

        private UITabstrip m_stripMain;

        #region Awake
        public void Awake()
        {
            ControlContainer = GetComponent<UIPanel>();
            ControlContainer.area = new Vector4(0, 0, 0, 0);
            ControlContainer.isVisible = false;
            ControlContainer.name = "AdrPanel";

            KlyteMonoUtils.CreateUIElement(out m_mainPanel, ControlContainer.transform, "AdrListPanel", new Vector4(0, 0, 400, 600));
            m_mainPanel.backgroundSprite = "MenuPanel2";
            CreateTitleBar();

            KlyteMonoUtils.CreateUIElement(out m_stripMain, m_mainPanel.transform, "AdrTabstrip", new Vector4(5, 45, m_mainPanel.width - 5, 40));

            KlyteMonoUtils.CreateUIElement(out UITabContainer tabContainer, m_mainPanel.transform, "AdrTabContainer", new Vector4(5, 80, m_mainPanel.width - 5, m_mainPanel.height - 80));
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
            contentContainer.area = new Vector4(15, 0, m_mainPanel.width - 30, m_mainPanel.height - 70);
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

        private void CreateTitleBar()
        {
            KlyteMonoUtils.CreateUIElement(out UILabel titlebar, m_mainPanel.transform, "AdrListPanel", new Vector4(75, 10, m_mainPanel.width - 150, 20));
            titlebar.autoSize = false;
            titlebar.text = "Addresses v" + AddressesMod.Version;
            titlebar.textAlignment = UIHorizontalAlignment.Center;

            KlyteMonoUtils.CreateUIElement(out UIButton closeButton, m_mainPanel.transform, "CloseButton", new Vector4(m_mainPanel.width - 37, 5, 32, 32));
            KlyteMonoUtils.InitButton(closeButton, false, "buttonclose", true);
            closeButton.hoveredBgSprite = "buttonclosehover";
            closeButton.eventClick += (x, y) => AddressesMod.Instance.Controller.CloseAdrPanel();

            KlyteMonoUtils.CreateUIElement(out UISprite logo, m_mainPanel.transform, "AddressesIcon", new Vector4(22, 5f, 32, 32));
            logo.atlas = AdrCommonTextureAtlas.instance.Atlas;
            logo.spriteName = "AddressesIcon";
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
