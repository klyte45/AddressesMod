using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Addresses.Xml;
using Klyte.Commons.Extensions;
using Klyte.Commons.UI.SpriteNames;
using Klyte.Commons.Utils;
using System;
using System.Linq;
using UnityEngine;
using static Klyte.Commons.UI.DefaultEditorUILib;

namespace Klyte.Addresses.UI
{

    internal class AdrHighwayParentEditorTab : UICustomControl
    {
        public static AdrHighwayParentEditorTab Instance { get; private set; }
        public UIPanel MainContainer { get; protected set; }



        #region Panel areas
        private UIPanel m_editArea;
        #endregion

        #region Top bar controls
        private UITextField m_configList;
        private UIButton m_deleteButton;
        private UIButton m_importButton;
        private UIButton m_exportButton;
        private UILabel m_scopeInfo;
        #endregion
        #region Bottom bar panels
        private UITextField m_dettachedPrefix;
        private UITextField m_shortPrefix;
        private UITextField m_fullPrefix;
        private UICheckBox m_shortAddSpace;
        private UICheckBox m_fullAddSpace;
        private UICheckBox m_shortInvert;
        private UICheckBox m_fullInvert;
        private UILabel m_exampleShort;
        private UILabel m_exampleLong;
        #endregion

        private AdrHighwayParentXml m_editingInstance;

        internal ref AdrHighwayParentXml EditingInstance => ref m_editingInstance;

        public void Awake()
        {
            Instance = this;

            MainContainer = GetComponent<UIPanel>();
            MainContainer.autoLayout = true;
            MainContainer.autoLayoutDirection = LayoutDirection.Vertical;
            MainContainer.autoLayoutPadding = new RectOffset(5, 5, 5, 5);
            MainContainer.width = MainContainer.parent.width;
            MainContainer.clipChildren = true;

            var mainContainerHelper = new UIHelperExtension(MainContainer);


            AddFilterableInput("AAAA", mainContainerHelper, out m_configList, out _, OnFilterLayouts, OnConfigSelectionChange);
            m_configList.width += m_configList.parent.GetComponentInChildren<UILabel>().width;
            GameObject.Destroy(m_configList.parent.GetComponentInChildren<UILabel>());
            m_configList.tooltipLocaleID = "K45_ADR_TOOLTIP_LAYOUTSELECTOR";
            AddLabel("", mainContainerHelper, out m_scopeInfo, out _);
            m_scopeInfo.processMarkup = true;

            m_deleteButton = AddButtonInEditorRow(m_configList, CommonsSpriteNames.K45_Delete, OnDeleteConfig, "K45_ADR_DELETE_SELECTED_CONFIG", true, 30);
            m_importButton = AddButtonInEditorRow(m_configList, CommonsSpriteNames.K45_Import, OnImportIntoCity, "K45_ADR_IMPORTINTOCITY_CONFIG", false, 30);
            m_exportButton = AddButtonInEditorRow(m_configList, CommonsSpriteNames.K45_Export, OnExportAsGlobal, "K45_ADR_EXPORTASGLOBAL_CONFIG", true, 30);
            AddButtonInEditorRow(m_configList, CommonsSpriteNames.K45_New, ShowNewConfigModal, "K45_ADR_CREATE_NEW_CONFIG", true, 30);
            AddButtonInEditorRow(m_configList, CommonsSpriteNames.K45_Reload, OnRefresh, "K45_ADR_RELOADFILES", true, 30);

            m_importButton.color = Color.green;
            m_deleteButton.color = Color.red;
            m_exportButton.color = Color.cyan;

            KlyteMonoUtils.CreateUIElement(out m_editArea, MainContainer.transform, "editArea", new UnityEngine.Vector4(0, 0, MainContainer.width - MainContainer.padding.horizontal, MainContainer.height - 80 - MainContainer.padding.vertical - (MainContainer.autoLayoutPadding.vertical * 2) - 5));
            m_editArea.padding = new RectOffset(5, 5, 5, 5);
            m_editArea.autoLayout = true;
            m_editArea.autoLayoutPadding = new RectOffset(0, 0, 3, 3);
            m_editArea.autoLayoutDirection = LayoutDirection.Vertical;

            var editAreaHelper = new UIHelperExtension(m_editArea);

            AddTextField(Locale.Get("K45_ADR_HW_DETTACHEDPREFIX"), out m_dettachedPrefix, out UILabel dettachedLbl, editAreaHelper, OnEditDettachedPrefix);
            AddTextField(Locale.Get("K45_ADR_HW_SHORTPREFIX"), out m_shortPrefix, out UILabel shortLbl, editAreaHelper, OnEditShortPrefix);
            AddCheckboxLocale("K45_ADR_HW_SHORTINVERT", out m_shortInvert, editAreaHelper, OnSetInvertConcatenateShort);
            AddCheckboxLocale("K45_ADR_HW_SHORTADDSPACE", out m_shortAddSpace, editAreaHelper, OnSetSpaceShort);
            AddTextField(Locale.Get("K45_ADR_HW_FULLPREFIX"), out m_fullPrefix, out UILabel longLbl, editAreaHelper, OnEditLongPrefix);
            AddCheckboxLocale("K45_ADR_HW_FULLINVERT", out m_fullInvert, editAreaHelper, OnSetInvertConcatenateLong);
            AddCheckboxLocale("K45_ADR_HW_FULLADDSPACE", out m_fullAddSpace, editAreaHelper, OnSetSpaceLong);

            AddLabel(Locale.Get("K45_ADR_HW_EXAMPLETITLE"), editAreaHelper, out UILabel exampleTitle, out _, true);
            AddLabel("Example", editAreaHelper, out m_exampleShort, out _, false);
            AddLabel("Example", editAreaHelper, out m_exampleLong, out _, false);
            m_exampleShort.textAlignment = UIHorizontalAlignment.Center;
            m_exampleLong.textAlignment = UIHorizontalAlignment.Center;
            m_exampleShort.textScale = 3f;
            m_exampleLong.textScale = 3f;
            m_exampleShort.processMarkup = true;
            m_exampleLong.processMarkup = true;
            exampleTitle.processMarkup = true;
            shortLbl.processMarkup = true;
            longLbl.processMarkup = true;
            dettachedLbl.processMarkup = true;

            OnConfigSelectionChange("", -1, new string[0]);
        }

        private void OnEditShortPrefix(string x) { EditingInstance.ShortPrefix = x; ReloadExamples(); }

        private void OnEditDettachedPrefix(string x) { EditingInstance.DettachedPrefix = x; ReloadExamples(); }

        private void OnEditLongPrefix(string x) { EditingInstance.LongPrefix = x; ReloadExamples(); }

        private void OnSetInvertConcatenateShort(bool x) { EditingInstance.InvertShortConcatenationOrder = x; ReloadExamples(); }

        private void OnSetInvertConcatenateLong(bool x) { EditingInstance.InvertLongConcatenationOrder = x; ReloadExamples(); }

        private void OnSetSpaceShort(bool x) { EditingInstance.AddSpaceBetweenTermsShort = x; ReloadExamples(); }

        private void OnSetSpaceLong(bool x) { EditingInstance.AddSpaceBetweenTermsLong = x; ReloadExamples(); }
        private void ReloadExamples()
        {
            m_exampleShort.text = EditingInstance.GetShortValue("<color #888899>XXX</color>");
            m_exampleLong.text = EditingInstance.GetLongValue("<color #888899>XXX</color>");
        }

        public void OnRefresh()
        {
            AdrHighwayParentLibDataXml.Instance.ReloadGlobalConfigurations();
            m_configList.text = ExecuteItemChange(m_configList.text, AdrHighwayParentLibDataXml.Instance.Get(m_configList.text) != null) ?? "";
        }


        public void SetCurrentSelectionNewName(string newName)
        {
            AdrHighwayParentLibDataXml.Instance.Add(newName, ref EditingInstance);
            m_configList.text = newName;
        }

        private void OnImportIntoCity()
        {
            EditingInstance.m_configurationSource = ConfigurationSource.CITY;
            ExecuteItemChange(EditingInstance.SaveName, true);
        }
        private void OnExportAsGlobal() => EditingInstance.ExportAsGlobal();
        private void OnDeleteConfig()
        {
            K45DialogControl.ShowModal(
                   new K45DialogControl.BindProperties
                   {
                       title = Locale.Get("K45_ADR_EDITOR_CONFIGDELETE_TITLE"),
                       message = string.Format(Locale.Get("K45_ADR_EDITOR_CONFIGDELETE_MESSAGE"), m_configList.text),
                       showButton1 = true,
                       textButton1 = Locale.Get("YES"),
                       showButton2 = true,
                       textButton2 = Locale.Get("NO"),
                   }
                , (x) =>
                {
                    if (x == 1)
                    {
                        AdrHighwayParentLibDataXml.Instance.Remove(m_configList.text);
                        OnRefresh();
                    }
                    return true;
                }); ;
        }


        private void ShowNewConfigModal() => ShowNewConfigModal(null);
        private void ShowNewConfigModal(string lastError) => K45DialogControl.ShowModalPromptText(
                  new K45DialogControl.BindProperties
                  {
                      title = Locale.Get("K45_ADR_EDITOR_CONFIGNEW_TITLE"),
                      message = (lastError.IsNullOrWhiteSpace() ? "" : $"{ Locale.Get("K45_ADR_EDITOR_CONFIGNEW_ANERROROCURRED")} {lastError}\n\n") + Locale.Get("K45_ADR_EDITOR_CONFIGNEW_MESSAGE"),
                      showButton1 = true,
                      textButton1 = Locale.Get("EXCEPTION_OK"),
                      showButton2 = true,
                      textButton2 = Locale.Get("CANCEL")
                  }, (x, text) =>
                  {
                      if (x == 1)
                      {
                          string error = null;
                          if (text.IsNullOrWhiteSpace())
                          {
                              error = $"{ Locale.Get("K45_ADR_EDITOR_CONFIGNEW_INVALIDNAME")}";
                          }
                          else if (AdrHighwayParentLibDataXml.Instance.List().Contains(text))
                          {
                              error = $"{ Locale.Get("K45_ADR_EDITOR_CONFIGNEW_ALREADY_EXISTS")}";
                          }

                          if (error.IsNullOrWhiteSpace())
                          {
                              var newModel = new AdrHighwayParentXml
                              {
                                  m_configurationSource = ConfigurationSource.CITY
                              };
                              AdrHighwayParentLibDataXml.Instance.Add(text, ref newModel);
                              m_configList.text = ExecuteItemChange(text, true);
                          }
                          else
                          {
                              ShowNewConfigModal(error);
                          }
                      }
                      return true;
                  }
         );

        internal void ReplaceItem(string key, string data)
        {
            AdrHighwayParentXml newItem = XmlUtils.DefaultXmlDeserialize<AdrHighwayParentXml>(data);
            newItem.m_configurationSource = ConfigurationSource.CITY;
            AdrHighwayParentLibDataXml.Instance.Add(key, ref newItem);
        }

        private string[] OnFilterLayouts(string input) => AdrHighwayParentLibDataXml.Instance.FilterBy(input);

        private string OnConfigSelectionChange(string typed, int sel, string[] items)
        {
            if (sel == -1)
            {
                sel = Array.IndexOf(items, typed?.Trim());
            }
            bool isValidSelection = sel >= 0 && sel < items.Length;
            string targetValue = isValidSelection ? items[sel] : "";

            return ExecuteItemChange(targetValue, isValidSelection);
        }

        private string ExecuteItemChange(string targetValue, bool isValidSelection)
        {
            m_editArea.isVisible = isValidSelection;
            if (isValidSelection)
            {
                EditingInstance = AdrHighwayParentLibDataXml.Instance.Get(targetValue);

                switch (EditingInstance.m_configurationSource)
                {
                    case ConfigurationSource.GLOBAL:
                        m_deleteButton.Disable();
                        m_importButton.isVisible = true;
                        m_exportButton.isVisible = false;
                        m_scopeInfo.localeID = "K45_ADR_CURRENTSOURCE_GLOBAL";
                        m_editArea.Disable();
                        break;
                    case ConfigurationSource.CITY:
                        m_deleteButton.Enable();
                        m_editArea.Enable();
                        m_exportButton.Enable();
                        m_exportButton.isVisible = true;
                        m_importButton.isVisible = false;
                        m_scopeInfo.localeID = "K45_ADR_CURRENTSOURCE_CITY";
                        break;
                }

                m_shortPrefix.text = EditingInstance.ShortPrefix ?? "";
                m_fullPrefix.text = EditingInstance.LongPrefix ?? "";
                m_shortAddSpace.isChecked = EditingInstance.AddSpaceBetweenTermsShort;
                m_fullAddSpace.isChecked = EditingInstance.AddSpaceBetweenTermsLong;
                m_fullInvert.isChecked = EditingInstance.InvertLongConcatenationOrder;
                m_shortInvert.isChecked = EditingInstance.InvertShortConcatenationOrder;
                m_dettachedPrefix.text = EditingInstance.DettachedPrefix ?? "";
                ReloadExamples();
            }
            else
            {
                m_scopeInfo.localeID = "K45_ADR_EDITOR_SELECTAITEM";
                m_deleteButton.Disable();
                m_exportButton.Disable();
                m_exportButton.isVisible = true;
                m_importButton.isVisible = false;
            }
            return isValidSelection ? targetValue : null;
        }
    }

}
