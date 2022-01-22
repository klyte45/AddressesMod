using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using Klyte.Commons.Extensions;
using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Klyte.Commons.UI.DefaultEditorUILib;

namespace Klyte.Addresses.UI
{

    internal partial class AdrFootballConfigTab : UICustomControl
    {
        public UIPanel MainContainer { get; private set; }

        private UIDropDown m_opponentFiles;

        private UIHelperExtension m_uiHelperDistrict;

        private bool isLoading;
        #region Awake
        public void Awake()
        {
            isLoading = true;
            MainContainer = GetComponent<UIPanel>();

            MainContainer.autoLayout = true;
            MainContainer.autoLayoutDirection = LayoutDirection.Vertical;
            MainContainer.autoLayoutPadding = new RectOffset(0, 0, 2, 2);

            m_uiHelperDistrict = new UIHelperExtension(MainContainer);

            isLoading = false;


            CreateGroupFileSelect("K45_ADR_FOOTBALLOPPONENTS_NAME_FILE", OnChangeSelectedOpponentFile, ReloadOpponentFiles, out m_opponentFiles);

            m_uiHelperDistrict.AddLabel(Locale.Get("K45_ADR_CITYSTADIUMS"));
            var listTitle = MainContainer.AddUIComponent<UIPanel>();
            listTitle.width = MainContainer.width;
            listTitle.height = 20;
            listTitle.autoLayout = true;
            listTitle.autoLayoutDirection = LayoutDirection.Horizontal;

            var titleUiHelper = new UIHelperExtension(listTitle);

            CreateTitleLabel(titleUiHelper, Locale.Get("K45_ADR_STADIUMNAME"), (listTitle.width - 290) / 2);
            CreateTitleLabel(titleUiHelper, Locale.Get("K45_ADR_TEAMNAME"), (listTitle.width - 290) / 2);
            CreateTitleLabel(titleUiHelper, Locale.Get("K45_ADR_TEAMCLR_SHORT"), 50);
            CreateTitleLabel(titleUiHelper, Locale.Get("K45_ADR_STADIUMADDRESS"), 235);

            var listContainer = MainContainer.AddUIComponent<UIPanel>();
            listContainer.width = MainContainer.width;
            listContainer.height = MainContainer.height - 70;
            listContainer.autoLayout = true;
            listContainer.autoLayoutDirection = LayoutDirection.Horizontal;

            KlyteMonoUtils.CreateScrollPanel(listContainer, out m_scrollablePanel, out _, MainContainer.width - 10f, MainContainer.height - 100f);
            StadiumListItem.CreateStadiumLineTemplate();
            m_stadiumsTemplateList = new UITemplateList<UIPanel>(m_scrollablePanel, "K45_ADR_StadiumLineTemplate");
            OnReloadStadiums();

            MainContainer.eventVisibilityChanged += (x, y) =>
              {
                  if (y)
                  {
                      OnReloadStadiums();
                  }
              };

            BuildingManager.instance.EventBuildingCreated += (x) =>
            {
                if (MainContainer.isVisible)
                {
                    OnReloadStadiums();
                }
            };
            BuildingManager.instance.EventBuildingReleased += (x) =>
            {
                if (MainContainer.isVisible)
                {
                    OnReloadStadiums();
                }
            };
            BuildingManager.instance.EventBuildingRelocated += (x) =>
            {
                if (MainContainer.isVisible)
                {
                    OnReloadStadiums();
                }
            };
        }


        private void CreateTitleLabel(UIHelperExtension titleUiHelper, string content, float width) => titleUiHelper.AddLabel(content, width, true).textAlignment = UIHorizontalAlignment.Center;

        private void CreateGroupFileSelect(string i18n, OnDropdownSelectionChanged onChanged, Action onReload, out UIDropDown dropDown)
        {
            isLoading = true;
            AddEmptyDropdown(Locale.Get(i18n), out dropDown, m_uiHelperDistrict, onChanged);
            AddButtonInEditorRow(dropDown, Commons.UI.SpriteNames.CommonsSpriteNames.K45_Reload, onReload, "K45_ADR_ROAD_NAME_FILES_RELOAD");
            onReload.Invoke();
            isLoading = false;
        }

        #endregion

        private void ReloadFiles(Action reloadAction, string selectedOption, List<string> referenceList, UIDropDown ddRef, string optionZeroText)
        {
            isLoading = true;
            reloadAction();
            referenceList.Insert(0, optionZeroText);
            ddRef.items = referenceList.ToArray();
            if (referenceList.Contains(selectedOption))
            {
                ddRef.selectedValue = selectedOption;
            }
            else
            {
                ddRef.selectedIndex = 0;
            }
            isLoading = false;
        }
        private void ReloadOpponentFiles() => ReloadFiles(
            AdrController.LoadLocalesFootbalTeams,
            AdrController.CurrentConfig.GlobalConfig.FootballConfig.OpponentNamesFile,
            AdrController.LoadedLocalesFootballTeams.Keys.ToList(),
            m_opponentFiles,
            Locale.Get("K45_ADR_DEFAULT_FILE_NAME")
            );


        private void OnChangeSelectedOpponentFile(int idx)
        {
            if (isLoading)
            {
                return;
            }
            AdrController.CurrentConfig.GlobalConfig.FootballConfig.OpponentNamesFile = (idx > 0 ? m_opponentFiles.selectedValue : null);
        }

        private UIScrollablePanel m_scrollablePanel;
        private UITemplateList<UIPanel> m_stadiumsTemplateList;


        public void OnReloadStadiums()
        {
            var bm = BuildingManager.instance;
            var stadiums = bm.GetServiceBuildings(ItemClass.Service.Monument).ToArray().Where(x => (bm.m_buildings.m_buffer[x].Info.m_buildingAI as MonumentAI).m_supportEvents == EventManager.EventType.Football).ToList();

            UIPanel[] stadiumLines = m_stadiumsTemplateList.SetItemCount(stadiums.Count);
            for (int idx = 0; idx < stadiums.Count; idx++)
            {
                ushort buildingID = stadiums[idx];
                StadiumListItem itemControl = stadiumLines[idx].GetComponentInChildren<StadiumListItem>();
                itemControl.ResetData(buildingID);
            }
        }
    }


}
