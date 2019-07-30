using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Klyte.Addresses.UI
{

    internal class AdrCitizenConfigTab : UICustomControl
    {
        public UIComponent MainContainer { get; private set; }

        private UIDropDown m_maleFiles;
        private UIDropDown m_femaleFiles;
        private UIDropDown m_lastNameFiles;

        private UIHelperExtension m_uiHelperDistrict;

        #region Awake
        public void Awake()
        {
            MainContainer = GetComponent<UIComponent>();

            m_uiHelperDistrict = new UIHelperExtension(MainContainer);

            ((UIScrollablePanel) m_uiHelperDistrict.Self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIScrollablePanel) m_uiHelperDistrict.Self).wrapLayout = true;
            ((UIScrollablePanel) m_uiHelperDistrict.Self).width = 370;

            CreateGroupFileSelect("ADR_CITIZEN_MALE_FIRST_NAME_FILE", OnChangeSelectedCitizenFirstNameMasc, ReloadMaleNameFiles, out m_maleFiles);
            CreateGroupFileSelect("ADR_CITIZEN_FEMALE_FIRST_NAME_FILE", OnChangeSelectedCitizenFirstNameFem, ReloadFemaleNameFiles, out m_femaleFiles);
            CreateGroupFileSelect("ADR_CITIZEN_LAST_NAME_FILE", OnChangeSelectedCitizenLastName, ReloadLastNameFiles, out m_lastNameFiles);

        }


        private void CreateGroupFileSelect(string i18n, OnDropdownSelectionChanged onChanged, OnButtonClicked onReload, out UIDropDown dropDown)
        {
            dropDown = m_uiHelperDistrict.AddDropdownLocalized(i18n, new string[0], -1, onChanged);
            dropDown.width = 370;
            m_uiHelperDistrict.AddSpace(1);
            KlyteMonoUtils.LimitWidth((UIButton) m_uiHelperDistrict.AddButton(Locale.Get(i18n + "S_RELOAD"), onReload), 380);
            m_uiHelperDistrict.AddSpace(20);
            onReload.Invoke();
        }

        #endregion

        private void ReloadFiles(Action reloadAction, string selectedOption, List<string> referenceList, UIDropDown ddRef, string optionZeroText)
        {
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
        }
        private void ReloadMaleNameFiles() => ReloadFiles(
            AdrController.LoadLocalesCitizenFirstNameMale,
            AdrController.CurrentConfig.GlobalConfig.CitizenConfig.MaleNamesFile,
            AdrController.LoadedLocalesCitizenFirstNameMasc.Keys.ToList(),
            m_maleFiles,
            Locale.Get("ADR_DEFAULT_FILE_NAME")
            );
        private void ReloadFemaleNameFiles() => ReloadFiles(
            AdrController.LoadLocalesCitizenFirstNameFemale,
            AdrController.CurrentConfig.GlobalConfig.CitizenConfig.FemaleNamesFile,
            AdrController.LoadedLocalesCitizenFirstNameFem.Keys.ToList(),
            m_femaleFiles,
            Locale.Get("ADR_DEFAULT_FILE_NAME")
            );

        private void ReloadLastNameFiles() => ReloadFiles(
            AdrController.LoadLocalesCitizenLastName,
            AdrController.CurrentConfig.GlobalConfig.CitizenConfig.SurnamesFile,
            AdrController.LoadedLocalesCitizenLastName.Keys.ToList(),
            m_lastNameFiles,
            Locale.Get("ADR_DEFAULT_FILE_NAME")
            );


        private void OnChangeSelectedCitizenFirstNameMasc(int idx) => AdrController.CurrentConfig.GlobalConfig.CitizenConfig.MaleNamesFile = (idx > 0 ? m_maleFiles.selectedValue : null);
        private void OnChangeSelectedCitizenFirstNameFem(int idx) => AdrController.CurrentConfig.GlobalConfig.CitizenConfig.FemaleNamesFile = (idx > 0 ? m_femaleFiles.selectedValue : null);
        private void OnChangeSelectedCitizenLastName(int idx) => AdrController.CurrentConfig.GlobalConfig.CitizenConfig.SurnamesFile = (idx > 0 ? m_lastNameFiles.selectedValue : null);


    }


}
