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

    internal class AdrCitizenConfigTab : UICustomControl
    {
        public UIPanel MainContainer { get; private set; }

        private UIDropDown m_maleFiles;
        private UIDropDown m_femaleFiles;
        private UIDropDown m_lastNameFiles;

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

            CreateGroupFileSelect("K45_ADR_CITIZEN_MALE_FIRST_NAME_FILE", OnChangeSelectedCitizenFirstNameMasc, ReloadMaleNameFiles, out m_maleFiles);
            CreateGroupFileSelect("K45_ADR_CITIZEN_FEMALE_FIRST_NAME_FILE", OnChangeSelectedCitizenFirstNameFem, ReloadFemaleNameFiles, out m_femaleFiles);
            CreateGroupFileSelect("K45_ADR_CITIZEN_LAST_NAME_FILE", OnChangeSelectedCitizenLastName, ReloadLastNameFiles, out m_lastNameFiles);
        }


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
            DistrictManager.instance.NamesModified();
            isLoading = false;
        }
        private void ReloadMaleNameFiles() => ReloadFiles(
            AdrController.LoadLocalesCitizenFirstNameMale,
            AdrController.CurrentConfig.GlobalConfig.CitizenConfig.MaleNamesFile,
            AdrController.LoadedLocalesCitizenFirstNameMasc.Keys.ToList(),
            m_maleFiles,
            Locale.Get("K45_ADR_DEFAULT_FILE_NAME")
            );
        private void ReloadFemaleNameFiles() => ReloadFiles(
            AdrController.LoadLocalesCitizenFirstNameFemale,
            AdrController.CurrentConfig.GlobalConfig.CitizenConfig.FemaleNamesFile,
            AdrController.LoadedLocalesCitizenFirstNameFem.Keys.ToList(),
            m_femaleFiles,
            Locale.Get("K45_ADR_DEFAULT_FILE_NAME")
            );

        private void ReloadLastNameFiles() => ReloadFiles(
            AdrController.LoadLocalesCitizenLastName,
            AdrController.CurrentConfig.GlobalConfig.CitizenConfig.SurnamesFile,
            AdrController.LoadedLocalesCitizenLastName.Keys.ToList(),
            m_lastNameFiles,
            Locale.Get("K45_ADR_DEFAULT_FILE_NAME")
            );


        private void OnChangeSelectedCitizenFirstNameMasc(int idx)
        {
            if (isLoading)
            {
                return;
            }

            AdrController.CurrentConfig.GlobalConfig.CitizenConfig.MaleNamesFile = (idx > 0 ? m_maleFiles.selectedValue : null);
        }

        private void OnChangeSelectedCitizenFirstNameFem(int idx)
        {
            if (isLoading)
            {
                return;
            }
            AdrController.CurrentConfig.GlobalConfig.CitizenConfig.FemaleNamesFile = (idx > 0 ? m_femaleFiles.selectedValue : null);
        }

        private void OnChangeSelectedCitizenLastName(int idx)
        {
            if (isLoading)
            {
                return;
            }
            AdrController.CurrentConfig.GlobalConfig.CitizenConfig.SurnamesFile = (idx > 0 ? m_lastNameFiles.selectedValue : null);
        }
    }


}
