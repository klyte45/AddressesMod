using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Addresses.Extensors;
using Klyte.Commons.Utils;
using UnityEngine;

namespace Klyte.Addresses.UI
{
    public class AdrAzimuthTitleLineNeighbor : MonoBehaviour
    {
        private UIPanel m_container;


        public void Awake()
        {
            m_container = transform.gameObject.AddComponent<UIPanel>();
            m_container.width = transform.parent.gameObject.GetComponent<UIComponent>().width;
            m_container.height = 30;
            m_container.autoLayout = true;
            m_container.autoLayoutDirection = LayoutDirection.Horizontal;
            m_container.autoLayoutPadding = new RectOffset(2, 2, 2, 2);
            m_container.wrapLayout = false;
            m_container.name = "AzimuthEditorTitle";

            CreateTitleLabel(out UILabel m_cityId, "CityId", "#", 30);
            CreateTitleLabel(out UILabel m_azimuthInput, "StartAzimuth", Locale.Get("ADR_AZIMUTH_GRADOS_TITLE"), 50);
            CreateTitleLabel(out UILabel m_direction, "Direction", Locale.Get("ADR_DIRECTION_TITLE"), 60);
            CreateTitleLabel(out UILabel m_generatedName, "GenName", Locale.Get("ADR_GEN_NAME_TITLE"), 150);

            KlyteMonoUtils.CreateUIElement(out UIButton add, m_container.transform, "RegenName");
            add.textScale = 1f;
            add.width = 30;
            add.height = 30;
            add.tooltip = Locale.Get("ADR_ADD_REG_CITY");
            KlyteMonoUtils.InitButton(add, true, "ButtonMenu");
            add.isVisible = true;
            add.text = "+";
            add.eventClick += (component, eventParam) => AdrNeighborhoodExtension.SetAzimuth(99, 0);
        }

        private void CreateTitleLabel(out UILabel label, string name, string text, uint width)
        {

            KlyteMonoUtils.CreateUIElement(out UIPanel nameContainer, m_container.transform, "GenNameContainer");
            nameContainer.autoSize = false;
            nameContainer.width = width;
            nameContainer.height = 30;
            nameContainer.autoLayout = true;
            nameContainer.autoLayoutDirection = LayoutDirection.Horizontal;

            KlyteMonoUtils.CreateUIElement(out label, nameContainer.transform, name);
            KlyteMonoUtils.LimitWidth(label, width);
            label.autoSize = true;
            label.height = 30;
            label.padding = new RectOffset(3, 3, 4, 3);
            label.textAlignment = UIHorizontalAlignment.Center;
            label.text = text;
            label.verticalAlignment = UIVerticalAlignment.Middle;
            label.minimumSize = new Vector2(width, 0);
        }
    }

}

