using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using Klyte.Extensions;
using Klyte.Harmony;
using Klyte.Commons.Extensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Klyte.Addresses.Utils;
using System.IO;
using Klyte.Commons.Overrides;
using Klyte.Commons.Utils;
using Klyte.Commons.UI;

namespace Klyte.Addresses.UI
{

    internal class AdrConfigPanel : UICustomControl
    {
        private const int NUM_SERVICES = 0;
        private static AdrConfigPanel instance;

        public UIPanel controlContainer { get; private set; }
        private UIPanel mainPanel;

        private UITabstrip m_StripMain;
        
        #region Awake
        private void Awake()
        {
            instance = this;
            controlContainer = GetComponent<UIPanel>();
            controlContainer.area = new Vector4(0, 0, 0, 0);
            controlContainer.isVisible = false;
            controlContainer.name = "AdrPanel";

            AdrUtils.createUIElement(out mainPanel, controlContainer.transform, "AdrListPanel", new Vector4(0, 0, 400, 600));
            mainPanel.backgroundSprite = "MenuPanel2";
            CreateTitleBar();

            AdrUtils.createUIElement(out m_StripMain, mainPanel.transform, "AdrTabstrip", new Vector4(5, 45, mainPanel.width - 5, 40));

            AdrUtils.createUIElement(out UITabContainer tabContainer, mainPanel.transform, "AdrTabContainer", new Vector4(5, 80, mainPanel.width - 5, mainPanel.height - 80));
            m_StripMain.tabPages = tabContainer;

            CreateTab<AdrDistrictConfigTab>("ToolbarIconDistrict", "ADR_CONFIG_PER_DISTRICT_TAB", "AdrPerDistrict");
            CreateTab<AdrNeighborConfigTab>("IconRightArrow", "ADR_CONFIG_NEIGHBOR_TAB", "AdrNeighbor");
            CreateTab<AdrGlobalConfigTab>("ToolbarIconZoomOutGlobe", "ADR_CONFIG_GLOBAL_TAB", "AdrGlobal");
        }        

        private void CreateTab<T>(string sprite, string localeKey, string objectName) where T : UICustomControl
        {
            UIButton tab = CreateTabTemplate();
            tab.normalFgSprite = sprite;
            tab.tooltip = Locale.Get(localeKey);

            AdrUtils.createUIElement(out UIPanel contentContainer, null);
            contentContainer.name = "Container";
            contentContainer.area = new Vector4(15, 0, mainPanel.width - 30, mainPanel.height - 70);
            m_StripMain.AddTab(objectName, tab.gameObject, contentContainer.gameObject);

            AdrUtils.CreateScrollPanel(contentContainer, out UIScrollablePanel scrollablePanel, out UIScrollbar scrollbar, contentContainer.width - 20, contentContainer.height - 5, new Vector3()).self.gameObject.AddComponent<T>();
        }

        private static UIButton CreateTabTemplate()
        {
            AdrUtils.createUIElement(out UIButton tabTemplate, null, "AdrTabTemplate");
            AdrUtils.initButton(tabTemplate, false, "GenericTab");
            tabTemplate.autoSize = false;
            tabTemplate.height = 30;
            tabTemplate.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            return tabTemplate;
        }

        private void CreateTitleBar()
        {
            AdrUtils.createUIElement(out UILabel titlebar, mainPanel.transform, "AdrListPanel", new Vector4(75, 10, mainPanel.width - 150, 20));
            titlebar.autoSize = false;
            titlebar.text = "Addresses v" + AddressesMod.version;
            titlebar.textAlignment = UIHorizontalAlignment.Center;
            AdrUtils.createDragHandle(titlebar, KlyteModsPanel.instance.mainPanel);

            AdrUtils.createUIElement(out UIButton closeButton, mainPanel.transform, "CloseButton", new Vector4(mainPanel.width - 37, 5, 32, 32));
            AdrUtils.initButton(closeButton, false, "buttonclose", true);
            closeButton.hoveredBgSprite = "buttonclosehover";
            closeButton.eventClick += (x, y) =>
            {
                AdrController.instance.CloseAdrPanel();
            };

            AdrUtils.createUIElement(out UISprite logo, mainPanel.transform, "AddressesIcon", new Vector4(22, 5f, 32, 32));
            logo.atlas = AdrController.taAdr;
            logo.spriteName = "AddressesIcon";
            AdrUtils.createDragHandle(logo, KlyteModsPanel.instance.mainPanel);
        }

        private static UIComponent CreateContentTemplate(float width, float height)
        {
            AdrUtils.createUIElement(out UIPanel contentContainer, null);
            contentContainer.name = "Container";
            contentContainer.area = new Vector4(0, 0, width, height);
            contentContainer.width = contentContainer.width - 20f;
            contentContainer.height = contentContainer.height;
            contentContainer.autoLayoutDirection = LayoutDirection.Vertical;
            contentContainer.autoLayoutStart = LayoutStart.TopLeft;
            contentContainer.autoLayoutPadding = new RectOffset(0, 0, 0, 0);
            contentContainer.autoLayout = true;
            contentContainer.clipChildren = true;
            contentContainer.relativePosition = new Vector3(5, 0);
            return contentContainer;
        }
        #endregion


        public void SetActiveTab(int idx)
        {
            m_StripMain.selectedIndex = idx;
        }

        private void Start()
        {
            SetActiveTab(-1);
            SetActiveTab(0);
        }
    }


}
