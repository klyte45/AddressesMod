using ColossalFramework.Globalization;
using ColossalFramework.Math;
using ColossalFramework.UI;
using ICities;
using Klyte.Addresses.Extensors;
using Klyte.Addresses.Overrides;
using Klyte.Commons.Utils;
using UnityEngine;

namespace Klyte.Addresses.UI
{
    internal class AdrAzimuthEditorLineNeighbor : MonoBehaviour
    {
        private UIPanel m_container;
        private UILabel m_cityId;
        private UITextField m_azimuthInput;
        private UILabel m_direction;
        private UILabel m_generatedName;
        private UIButton m_regenerateName;
        private UIButton m_die;
        public event OnButtonClicked OnDie;
        public event OnButtonClicked OnRegenerate;
        public event OnAzimuthChange OnValueChange;
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

        public void SetCardinalAngle(float angle)
        {
            m_direction.text = CardinalPoint.GetCardinalPoint16(angle);
            m_generatedName.text = OutsideConnectionAIOverrides.GetNameBasedInAngle(angle, out bool canTrust);
            if (!canTrust)
            {
                m_generatedName.text = "?????";
            }
        }

        public string GetCurrentVal() => m_azimuthInput.text;

        public void SetTextColor(Color c) => m_azimuthInput.color = c;

        public void Awake()
        {
            m_container = transform.gameObject.AddComponent<UIPanel>();
            m_container.width = transform.parent.gameObject.GetComponent<UIComponent>().width;
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
            m_azimuthInput.text = "0";
            m_azimuthInput.eventTextChanged += SendText;

            KlyteMonoUtils.CreateUIElement(out m_direction, m_container.transform, "Direction");
            m_direction.autoSize = false;
            m_direction.width = 60;
            m_direction.height = 30;
            m_direction.textScale = 1.125f;
            m_direction.textAlignment = UIHorizontalAlignment.Center;
            m_direction.padding = new RectOffset(3, 3, 5, 3);

            KlyteMonoUtils.CreateUIElement(out UIPanel nameContainer, m_container.transform, "GenNameContainer");
            nameContainer.autoSize = false;
            nameContainer.width = 150;
            nameContainer.height = 30;
            nameContainer.autoLayout = true;
            nameContainer.autoLayoutDirection = LayoutDirection.Horizontal;

            KlyteMonoUtils.CreateUIElement(out m_generatedName, nameContainer.transform, "GenName");
            m_generatedName.autoSize = true;
            m_generatedName.height = 30;
            m_generatedName.textScale = 1.125f;
            m_generatedName.padding = new RectOffset(3, 3, 5, 3);
            m_generatedName.text = "???";
            m_generatedName.textAlignment = UIHorizontalAlignment.Center;
            m_generatedName.minimumSize = new Vector2(150, 0);
            KlyteMonoUtils.LimitWidth(m_generatedName, 150);

            KlyteMonoUtils.CreateUIElement(out m_regenerateName, m_container.transform, "RegenName");
            m_regenerateName.textScale = 1f;
            m_regenerateName.width = 30;
            m_regenerateName.height = 30;
            m_regenerateName.tooltip = Locale.Get("K45_ADR_REGENERATE_NAME");
            KlyteMonoUtils.InitButton(m_regenerateName, true, "ButtonMenu");
            m_regenerateName.isVisible = true;
            m_regenerateName.text = "R";
            m_regenerateName.eventClick += (component, eventParam) =>
            {
                AdrNeighborhoodExtension.SetSeed(m_id, new Randomizer(new System.Random().Next()).UInt32(0xFEFFFFFF));
                OnRegenerate?.Invoke();
            };

            KlyteMonoUtils.CreateUIElement(out m_die, m_container.transform, "Delete");
            m_die.textScale = 1f;
            m_die.width = 30;
            m_die.height = 30;
            m_die.tooltip = Locale.Get("K45_ADR_DELETE_STOP_NEIGHBOR");
            KlyteMonoUtils.InitButton(m_die, true, "ButtonMenu");
            m_die.isVisible = true;
            m_die.text = "X";
            m_die.eventClick += (component, eventParam) =>
            {
                AdrNeighborhoodExtension.SafeCleanEntry(m_id);
                OnDie?.Invoke();
            };

        }

        private void SendText(UIComponent x, string y)
        {
            if (uint.TryParse(y, out uint angle))
            {
                OnValueChange?.Invoke(m_id, angle);
            }
        }
    }

    internal delegate void OnAzimuthChange(int idx, uint val);
}

