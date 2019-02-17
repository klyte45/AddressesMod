using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using Klyte.Addresses.Utils;
using Klyte.Commons.Extensors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Klyte.Addresses.UI
{

    internal class AdrCitizenConfigTab : UICustomControl
    {
        public UIComponent mainContainer { get; private set; }

        private UIDropDown m_maleFiles;
        private UIDropDown m_femaleFiles;
        private UIDropDown m_lastNameFiles;

        private UIHelperExtension m_uiHelperDistrict;

        #region Awake
        private void Awake()
        {
            mainContainer = GetComponent<UIComponent>();

            m_uiHelperDistrict = new UIHelperExtension(mainContainer);

            ((UIScrollablePanel)m_uiHelperDistrict.self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIScrollablePanel)m_uiHelperDistrict.self).wrapLayout = true;
            ((UIScrollablePanel)m_uiHelperDistrict.self).width = 370;

            CreateGroupFileSelect("ADR_CITIZEN_MALE_FIRST_NAME_FILE", onChangeSelectedCitizenFirstNameMasc, reloadMaleNameFiles, out m_maleFiles);
            CreateGroupFileSelect("ADR_CITIZEN_FEMALE_FIRST_NAME_FILE", onChangeSelectedCitizenFirstNameFem, reloadFemaleNameFiles, out m_femaleFiles);
            CreateGroupFileSelect("ADR_CITIZEN_LAST_NAME_FILE", onChangeSelectedCitizenLastName, reloadLastNameFiles, out m_lastNameFiles);

        }

        private void CreateGroupFileSelect(string i18n, OnDropdownSelectionChanged onChanged, OnButtonClicked onReload, out UIDropDown dropDown)
        {
            dropDown = m_uiHelperDistrict.AddDropdownLocalized(i18n, new String[0], -1, onChanged);
            dropDown.width = 370;
            m_uiHelperDistrict.AddSpace(1);
            AdrUtils.LimitWidth((UIButton)m_uiHelperDistrict.AddButton(Locale.Get(i18n + "S_RELOAD"), onReload), 380);
            m_uiHelperDistrict.AddSpace(20);
            onReload.Invoke();
        }

        #endregion

        private void reloadMaleNameFiles()
        {
            AdrController.LoadLocalesCitizenFirstNameMale();
            List<string> items = AdrController.loadedLocalesCitizenFirstNameMasc.Keys.ToList();
            items.Insert(0, Locale.Get("ADR_DEFAULT_FILE_NAME"));
            m_maleFiles.items = items.ToArray();
            string filename = AdrConfigWarehouse.getCurrentConfigString(AdrConfigWarehouse.ConfigIndex.MALE_FIRSTNAME_FILE);
            if (items.Contains(filename))
            {
                m_maleFiles.selectedValue = filename;
            }
            else
            {
                m_maleFiles.selectedIndex = 0;
            }
            DistrictManager.instance.NamesModified();
        }
        private void reloadFemaleNameFiles()
        {
            AdrController.LoadLocalesCitizenFirstNameFemale();
            List<string> items = AdrController.loadedLocalesCitizenFirstNameFem.Keys.ToList();
            items.Insert(0, Locale.Get("ADR_DEFAULT_FILE_NAME"));
            m_femaleFiles.items = items.ToArray();
            string filename = AdrConfigWarehouse.getCurrentConfigString(AdrConfigWarehouse.ConfigIndex.FEMALE_FIRSTNAME_FILE);
            if (items.Contains(filename))
            {
                m_femaleFiles.selectedValue = filename;
            }
            else
            {
                m_femaleFiles.selectedIndex = 0;
            }
            DistrictManager.instance.NamesModified();
        }
        private void reloadLastNameFiles()
        {
            AdrController.LoadLocalesCitizenLastName();
            List<string> items = AdrController.loadedLocalesCitizenLastName.Keys.ToList();
            items.Insert(0, Locale.Get("ADR_DEFAULT_FILE_NAME"));
            m_lastNameFiles.items = items.ToArray();
            string filename = AdrConfigWarehouse.getCurrentConfigString(AdrConfigWarehouse.ConfigIndex.LASTNAME_FILE);
            if (items.Contains(filename))
            {
                m_lastNameFiles.selectedValue = filename;
            }
            else
            {
                m_lastNameFiles.selectedIndex = 0;
            }
            DistrictManager.instance.NamesModified();
        }

        private void onChangeSelectedCitizenFirstNameMasc(int idx)
        {
            AdrConfigWarehouse.setCurrentConfigString(AdrConfigWarehouse.ConfigIndex.MALE_FIRSTNAME_FILE, idx > 0 ? m_maleFiles.selectedValue : null);
        }
        private void onChangeSelectedCitizenFirstNameFem(int idx)
        {
            AdrConfigWarehouse.setCurrentConfigString(AdrConfigWarehouse.ConfigIndex.FEMALE_FIRSTNAME_FILE, idx > 0 ? m_femaleFiles.selectedValue : null);
        }
        private void onChangeSelectedCitizenLastName(int idx)
        {
            AdrConfigWarehouse.setCurrentConfigString(AdrConfigWarehouse.ConfigIndex.LASTNAME_FILE, idx > 0 ? m_lastNameFiles.selectedValue : null);
        }


    }


}
