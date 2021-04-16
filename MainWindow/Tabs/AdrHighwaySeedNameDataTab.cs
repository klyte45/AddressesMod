using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using ColossalFramework.UI;
using Klyte.Addresses.ModShared;
using Klyte.Addresses.Tools;
using Klyte.Addresses.Xml;
using Klyte.Commons.Extensions;
using Klyte.Commons.Utils;
using System;
using System.Collections;
using UnityEngine;
using static Klyte.Commons.UI.DefaultEditorUILib;

namespace Klyte.Addresses.UI
{
    internal class AdrHighwaySeedNameDataTab : UICustomControl
    {
        private static AdrHighwaySeedNameDataTab m_instance;
        public static AdrHighwaySeedNameDataTab Instance
        {
            get
            {
                if (m_instance is null)
                {
                    m_instance = FindObjectOfType<AdrHighwaySeedNameDataTab>();
                }
                return m_instance;
            }
        }

        private AdrNameSeedConfig CurrentEditingInstance { get; set; }


        public UIPanel MainContainer { get; protected set; }

        private UIButton m_buttonTool;
        private UIPanel m_editingContainer;
        private UILabel m_labelSelectionDescription;
        private UIPanel m_topContainerBar;

        private UITextField m_hwType;
        private UITextField m_hwId;
        private UITextField m_hwName;
        private UICheckBox m_showDirectionName;
        private UITextField m_hwMileageOffset;
        private UICheckBox m_hwInvertMileage;
        private UIColorField m_hwColor;



        public ushort CurrentSeedId { get; private set; }
        public ushort CurrentSourceSegmentId { get; private set; }

        public void Awake()
        {

            MainContainer = GetComponent<UIPanel>();
            MainContainer.autoLayout = true;
            MainContainer.autoLayoutDirection = LayoutDirection.Vertical;
            MainContainer.autoLayoutPadding = new RectOffset(5, 5, 5, 5);

            var m_uiHelperHS = new UIHelperExtension(MainContainer);

            m_buttonTool = (UIButton)m_uiHelperHS.AddButton(Locale.Get("K45_ADR_PICK_A_SEGMENT"), EnablePickTool);
            KlyteMonoUtils.LimitWidth(m_buttonTool, (m_uiHelperHS.Self.width - 20), true);


            AddLabel("", m_uiHelperHS, out m_labelSelectionDescription, out m_topContainerBar, false);
            m_labelSelectionDescription.prefix = Locale.Get("K45_ADR_CURRENTSELECTION") + ": ";
            m_labelSelectionDescription.padding.top = 8;
            AddButtonInEditorRow(m_labelSelectionDescription, Commons.UI.SpriteNames.CommonsSpriteNames.K45_PaintBucket, EnterPaintBucketMode, "K45_ADR_PAINTSEED", true, 30);
            AddButtonInEditorRow(m_labelSelectionDescription, Commons.UI.SpriteNames.CommonsSpriteNames.K45_Reload, OnSegmentNewSeed, "K45_ADR_CHANGESEED_INTERCROSS", true, 30);
            KlyteMonoUtils.LimitWidthAndBox(m_labelSelectionDescription, (m_labelSelectionDescription.width), true);


            KlyteMonoUtils.CreateUIElement(out m_editingContainer, MainContainer.transform, "SecContainer", new Vector4(0, 0, MainContainer.width, MainContainer.height - 10 - m_topContainerBar.height));
            m_editingContainer.autoLayout = true;
            m_editingContainer.autoLayoutDirection = LayoutDirection.Vertical;
            m_editingContainer.autoLayoutPadding = new RectOffset(0, 0, 5, 5);
            var m_editingHelper = new UIHelperExtension(m_editingContainer);
            AddFilterableInput(Locale.Get("K45_ADR_HIGHWAYITEM_TYPE"), m_editingHelper, out m_hwType, out _, OnFilterHwTypes, OnSetHwType);
            AddTextField(Locale.Get("K45_ADR_HIGHWAYITEM_ID"), out m_hwId, m_editingHelper, OnSetHwId);
            AddTextField(Locale.Get("K45_ADR_HIGHWAYITEM_FORCEDNAME"), out m_hwName, m_editingHelper, OnSetHwForcedName);
            AddIntField(Locale.Get("K45_ADR_HIGHWAYITEM_MILEAGEOFFSET"), out m_hwMileageOffset, m_editingHelper, OnSetMileageOffset, false);
            AddCheckboxLocale("K45_ADR_HIGHWAYITEM_INVERTMILEAGE", out m_hwInvertMileage, m_editingHelper, OnSetMileageInvert);
            AddColorField(m_editingHelper, Locale.Get("K45_ADR_HIGHWAYITEM_COLOR"), out m_hwColor, OnSetHwColor);
            m_hwMileageOffset.width = 120;


            MainContainer.eventVisibilityChanged += (x, y) =>
            {
                if (y)
                {
                    eraseSeedBuffer = -1;
                    DoEraseSeed();
                }
                else
                {
                    if (ToolsModifierControl.toolController.CurrentTool is RoadSegmentTool)
                    {
                        ToolsModifierControl.SetTool<DefaultTool>();
                    }
                    OnSegmentSet(0);
                }
            };

            OnSegmentSet(0);
        }

        private string[] OnFilterHwTypes(string input) => AdrHighwayParentLibDataXml.Instance.FilterBy(input);

        private string OnSetHwType(string typedText, int idx, string[] options)
        {
            if (idx < 0 && Array.IndexOf(options, typedText) < 0)
            {
                return "";
            }
            var value = idx < 0 ? typedText : options[idx];
            CurrentEditingInstance.HighwayParentName = value;
            return value;
        }
        private void OnSetHwId(string s) => CurrentEditingInstance.HighwayIdentifier = s.IsNullOrWhiteSpace() ? "" : s.Trim();
        private void OnSetHwForcedName(string s) => CurrentEditingInstance.ForcedName = s.IsNullOrWhiteSpace() ? "" : s.Trim();
        private void OnSetMileageOffset(int s) => CurrentEditingInstance.MileageOffset = (uint)s;
        private void OnSetMileageInvert(bool s) => CurrentEditingInstance.InvertMileageStart = s;
        private void OnSetHwColor(Color s) => CurrentEditingInstance.HighwayColor = s;

        private void EnablePickTool()
        {
            eraseSeedBuffer = -1;
            DoEraseSeed();
            OnSegmentSet(0);
            ToolsModifierControl.toolController.CurrentTool = null;
            var tool = FindObjectOfType<RoadSegmentTool>();

            ToolsModifierControl.toolController.CurrentTool = tool;
            tool.OnSelectSegment += OnSegmentSet;

        }
        private void EnterPaintBucketMode()
        {
            eraseSeedBuffer = -1;
            DoEraseSeed();
            var tool = FindObjectOfType<RoadSegmentTool>();
            ToolsModifierControl.toolController.CurrentTool = tool;
            tool.OnSelectSegment += OnSegmentPaint;
            tool.singleSelectMode = false;
        }

        private void OnSegmentSet(ushort id)
        {
            CurrentSourceSegmentId = id;
            CurrentSeedId = id > 0 ? NetManager.instance.m_segments.m_buffer[id].m_nameSeed : default;
            ReloadSegment();
        }
        private void OnSegmentNewSeed()
        {
            var newSeedGen = new Randomizer(CurrentSeedId);
            var oldSeed = CurrentSeedId;
            while (AdrNameSeedDataXml.Instance.CurrentSavegameSeeds.ContainsKey(CurrentSeedId))
            {
                CurrentSeedId = (ushort)newSeedGen.UInt32(0xFFFF);
            }
            StartCoroutine(SetSeedInterCrossings(CurrentSourceSegmentId, oldSeed, CurrentSeedId));
        }

        private void OnSegmentPaint(ushort id)
        {
            if (CurrentSeedId > 0)
            {
                var oldSeedId = NetManager.instance.m_segments.m_buffer[id].m_nameSeed;
                StartCoroutine(SetSeedFull(id, oldSeedId, CurrentSeedId));
            }
        }

        private IEnumerator SetSeedFull(ushort segmentId, ushort oldSeed, ushort newSeed)
        {
            yield return RecursiveSetSeed(segmentId, oldSeed, newSeed, NetManager.instance.m_segments.m_buffer[segmentId].m_startNode, false);
            yield return RecursiveSetSeed(segmentId, oldSeed, newSeed, NetManager.instance.m_segments.m_buffer[segmentId].m_endNode, false, true);
            AdrNameSeedDataXml.Instance.EraseSeedCache();
            SegmentUtils.UpdateSegmentNamesView();
            AdrFacade.TriggerHighwaySeedChanged(oldSeed);
            AdrFacade.TriggerHighwaySeedChanged(newSeed);
        }

        private IEnumerator SetSeedInterCrossings(ushort segmentId, ushort oldSeed, ushort newSeed)
        {
            yield return RecursiveSetSeed(segmentId, oldSeed, newSeed, NetManager.instance.m_segments.m_buffer[segmentId].m_startNode, true);
            yield return RecursiveSetSeed(segmentId, oldSeed, newSeed, NetManager.instance.m_segments.m_buffer[segmentId].m_endNode, true, true);
            AdrNameSeedDataXml.Instance.EraseSeedCache();
            SegmentUtils.UpdateSegmentNamesView();
            AdrFacade.TriggerHighwaySeedChanged(oldSeed);
            AdrFacade.TriggerHighwaySeedChanged(newSeed);
        }
        private IEnumerator RecursiveSetSeed(ushort segmentId, ushort oldSeed, ushort newSeed, ushort srcNode, bool requireOnly2Segments, bool forceScan = false)
        {
            var instance = NetManager.instance;
            if (segmentId == 0 || ((instance.m_segments.m_buffer[segmentId].m_nameSeed == newSeed || instance.m_segments.m_buffer[segmentId].m_nameSeed != oldSeed) && !forceScan))
            {
                yield break;
            }
            yield return 0;
            if (!forceScan)
            {
                instance.m_segments.m_buffer[segmentId].m_nameSeed = newSeed;
            }
            var targetNode = instance.m_segments.m_buffer[segmentId].GetOtherNode(srcNode);
            var startNodeSegments = instance.m_nodes.m_buffer[targetNode].CountSegments();
            if (requireOnly2Segments && startNodeSegments != 2)
            {
                yield break;
            }

            for (int i = 0; i < startNodeSegments; i++)
            {
                var nextSegment = instance.m_nodes.m_buffer[targetNode].GetSegment(i);
                yield return RecursiveSetSeed(nextSegment, oldSeed, newSeed, targetNode, requireOnly2Segments);
                DoEraseSeed();
            }
        }

        private int eraseSeedBuffer = 0;
        private void DoEraseSeed()
        {
            eraseSeedBuffer++;
            if (eraseSeedBuffer % 5 == 0)
            {
                AdrNameSeedDataXml.Instance.EraseSeedCache();
                SegmentUtils.UpdateSegmentNamesView();
            }
        }


        private void ReloadSegment()
        {
            m_editingContainer.isVisible = CurrentSeedId != 0;
            m_topContainerBar.isVisible = CurrentSeedId != 0;
            var streetName = NetManager.instance.GetSegmentName(CurrentSourceSegmentId);
            m_labelSelectionDescription.text = $"{streetName} @ segId#{CurrentSourceSegmentId} - name seed #{CurrentSeedId}";
            if (CurrentSeedId > 0)
            {
                if (!AdrNameSeedDataXml.Instance.NameSeedConfigs.ContainsKey(CurrentSeedId))
                {
                    _ = AdrNameSeedDataXml.Instance.NameSeedConfigs + new AdrNameSeedConfig { Id = CurrentSeedId };
                }
                CurrentEditingInstance = AdrNameSeedDataXml.Instance.NameSeedConfigs[CurrentSeedId];
                m_hwType.text = CurrentEditingInstance.HighwayParentName ?? "";
                m_hwId.text = CurrentEditingInstance.HighwayIdentifier ?? "";
                m_hwName.text = CurrentEditingInstance.ForcedName ?? "";
                m_hwMileageOffset.text = CurrentEditingInstance.MileageOffset.ToString("0");
                m_hwInvertMileage.isChecked = CurrentEditingInstance.InvertMileageStart;
                m_hwColor.selectedColor = CurrentEditingInstance.HighwayColor;
            }
        }

    }

}
