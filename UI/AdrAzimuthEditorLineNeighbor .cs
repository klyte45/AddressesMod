using ColossalFramework.Math;
using ColossalFramework.UI;
using ICities;
using Klyte.Addresses.Extensions;
using Klyte.Addresses.Overrides;
using Klyte.Commons.UI;
using Klyte.Commons.UI.SpriteNames;
using Klyte.Commons.Utils;
using System;
using UnityEngine;

namespace Klyte.Addresses.UI
{
    internal class AdrAzimuthEditorLineNeighbor : MonoBehaviour
    {
        private UIPanel m_container;
        private UILabel m_cityId;
        private UITextField m_azimuthInput;
        private UILabel m_direction;
        private UITextField m_generatedName;
        private UIButton m_die;
        public event OnButtonClicked OnDie;
        public event OnButtonClicked OnRegenerate;
        public event OnAzimuthChange OnValueChange;
        public event Action<int, string> OnFixedNameChange;
        private int m_id;

        public void SetLegendInfo(Color c, int id)
        {
            m_id = id - 1;
            m_cityId.text = (id % 100).ToString();
            m_cityId.color = c;
            m_azimuthInput.eventTextChanged -= SendText;
            m_azimuthInput.text = AdrNeighborhoodExtension.GetAzimuth(m_id).ToString();
            m_azimuthInput.eventTextChanged += SendText;
            m_die.isVisible = m_id > 0;
        }

        public void SetData()
        {
            var angle = AdrNeighborhoodExtension.GetAzimuth(m_id);
            var cityNameFixed = AdrNeighborhoodExtension.GetFixedName(m_id);

            m_direction.text = CardinalPoint.GetCardinalPoint16LocalizedShort(angle);
            bool canTrust = true;
            var name = cityNameFixed ?? OutsideConnectionAIOverrides.GetNameBasedInAngle(angle, out canTrust);
            m_generatedName.text = name;
            m_generatedName.tooltip = name;
            if (cityNameFixed == null && !canTrust)
            {
                m_generatedName.text = "?????";
            }
        }

        public string GetCurrentVal() => m_azimuthInput.text;

        public void SetTextColor(Color c) => m_azimuthInput.color = c;

        public void Awake()
        {
            m_container = GetComponent<UIPanel>();
            m_container.width = m_container.parent.width;
            m_container.height = 30;
            m_container.autoLayout = true;
            m_container.autoLayoutDirection = LayoutDirection.Horizontal;
            m_container.autoLayoutPadding = new RectOffset(2, 2, 2, 2);
            m_container.wrapLayout = false;
            m_container.name = "AzimuthInputLine";

            KlyteMonoUtils.CreateUIElement(out m_cityId, m_container.transform, "CityId");
            m_cityId.autoSize = false;
            m_cityId.relativePosition = new Vector3(0, 0);
            m_cityId.backgroundSprite = "EmptySprite";
            m_cityId.width = 30;
            m_cityId.height = 30;
            m_cityId.textScale = 1.3f;
            m_cityId.padding = new RectOffset(3, 3, 4, 3);
            m_cityId.useOutline = true;
            m_cityId.textAlignment = UIHorizontalAlignment.Center;

            KlyteMonoUtils.CreateUIElement(out m_azimuthInput, m_container.transform, "StartAzimuth");
            KlyteMonoUtils.UiTextFieldDefaults(m_azimuthInput);
            m_azimuthInput.normalBgSprite = "OptionsDropboxListbox";
            m_azimuthInput.width = 50;
            m_azimuthInput.height = 28;
            m_azimuthInput.textScale = 1.6f;
            m_azimuthInput.maxLength = 3;
            m_azimuthInput.numericalOnly = true;
            m_azimuthInput.allowNegative = true;
            m_azimuthInput.text = "0";
            m_azimuthInput.eventTextChanged += SendText;
            m_azimuthInput.eventMouseWheel += DefaultEditorUILib.RollInteger;

            KlyteMonoUtils.CreateUIElement(out m_direction, m_container.transform, "Direction");
            m_direction.autoSize = false;
            m_direction.width = 60;
            m_direction.height = 30;
            m_direction.textScale = 1.125f;
            m_direction.textAlignment = UIHorizontalAlignment.Center;
            m_direction.padding = new RectOffset(3, 3, 5, 3);

            KlyteMonoUtils.CreateUIElement(out m_generatedName, m_container.transform, "GenName");
            KlyteMonoUtils.UiTextFieldDefaults(m_generatedName);
            m_generatedName.autoSize = true;
            m_generatedName.height = 30;
            m_generatedName.textScale = 1.125f;
            m_generatedName.padding = new RectOffset(3, 3, 5, 3);
            m_generatedName.text = "???";
            m_generatedName.horizontalAlignment = UIHorizontalAlignment.Center;
            m_generatedName.width = m_container.width - 235;
            m_generatedName.eventTextSubmitted += SetFixedName;
            m_generatedName.submitOnFocusLost = true;


            m_die = DefaultEditorUILib.AddButtonInEditorRow(m_generatedName, CommonsSpriteNames.K45_X, () =>
            {
                AdrNeighborhoodExtension.SafeCleanEntry(m_id);
                OnDie?.Invoke();
            }, "K45_ADR_DELETE_STOP_NEIGHBOR", false, 30);

            DefaultEditorUILib.AddButtonInEditorRow(m_generatedName, CommonsSpriteNames.K45_Reload, () =>
            {
                AdrNeighborhoodExtension.SetSeed(m_id, new Randomizer(new System.Random().Next()).UInt32(0xFEFFFFFF));
                OnRegenerate?.Invoke();
            }, "K45_ADR_REGENERATE_NAME", false, 30);

        }

        private void SendText(UIComponent x, string y)
        {
            if (int.TryParse(y, out int angle))
            {
                OnValueChange?.Invoke(m_id, (uint)(((angle + 360) % 360) + 360) % 360);
            }
        }

        private void SetFixedName(UIComponent x, string y) => OnFixedNameChange?.Invoke(m_id, y);
    }

    internal delegate void OnAzimuthChange(int idx, uint val);
}

