using ColossalFramework.UI;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using UnityEngine;

namespace Klyte.Addresses.UI
{

    public class AdrConfigPanel : BasicKPanel<AddressesMod, AdrController, AdrConfigPanel>
    {

        public UIPanel ControlContainer { get; private set; }

        public override float PanelWidth => 875;

        public override float PanelHeight => 600;

        private UITabstrip m_stripMain;

        #region Awake
        protected override void AwakeActions()
        {


            KlyteMonoUtils.CreateUIElement(out m_stripMain, base.MainPanel.transform, "AdrTabstrip", new Vector4(5, 40, base.MainPanel.width - 10, 40));

            KlyteMonoUtils.CreateUIElement(out UITabContainer tabContainer, base.MainPanel.transform, "AdrTabContainer", new Vector4(0, 80, base.MainPanel.width, base.MainPanel.height - 80));
            m_stripMain.tabPages = tabContainer;
            m_stripMain.CreateTabLocalized<AdrDistrictConfigTab>("ToolbarIconDistrict", "K45_ADR_CONFIG_PER_DISTRICT_TAB", "AdrPerDistrict");
            m_stripMain.CreateTabLocalized<AdrNeighborConfigTab>("IconRightArrow", "K45_ADR_CONFIG_NEIGHBOR_TAB", "AdrNeighbor", false);
            m_stripMain.CreateTabLocalized<AdrCitizenConfigTab>("IconCitizen", "K45_ADR_CONFIG_CITIZEN_TAB", "AdrCitizen");
            m_stripMain.CreateTabLocalized<AdrGlobalConfigTab>("ToolbarIconZoomOutGlobe", "K45_ADR_CONFIG_GLOBAL_TAB", "AdrGlobal");
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
