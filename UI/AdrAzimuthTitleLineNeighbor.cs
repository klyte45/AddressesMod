using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using ColossalFramework.UI;
using System;
using UnityEngine;
using System.Linq;
using Klyte.Commons.Utils;
using System.Collections.Generic;
using Klyte.Addresses.Utils;
using Klyte.Commons.Extensors;
using ICities;
using Klyte.Addresses.Extensors;

namespace Klyte.Addresses.UI
{
    public class AdrAzimuthTitleLineNeighbor : MonoBehaviour
    {
        private UIPanel m_container;
        private UILabel m_cityId;
        private UILabel m_azimuthInput;
        private UILabel m_direction;
        private UILabel m_generatedName;


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

            CreateTitleLabel(out m_cityId, "CityId", "#", 30);
            CreateTitleLabel(out m_azimuthInput, "StartAzimuth", Locale.Get("ADR_AZIMUTH_GRADOS_TITLE"), 50);
            CreateTitleLabel(out m_direction, "Direction", Locale.Get("ADR_DIRECTION_TITLE"), 60);
            CreateTitleLabel(out m_generatedName, "GenName", Locale.Get("ADR_GEN_NAME_TITLE"), 150);

            KlyteUtils.createUIElement(out UIButton add, m_container.transform, "RegenName");
            add.textScale = 1f;
            add.width = 30;
            add.height = 30;
            add.tooltip = Locale.Get("ADR_ADD_REG_CITY");
            AdrUtils.initButton(add, true, "ButtonMenu");
            add.isVisible = true;
            add.text = "+";
            add.eventClick += (component, eventParam) =>
            {
                AdrNeighborhoodExtension.instance.SetAzimuth(99, 0);
            };
        }

        private void CreateTitleLabel(out UILabel label, string name, string text, uint width)
        {

            KlyteUtils.createUIElement(out UIPanel nameContainer, m_container.transform, "GenNameContainer");
            nameContainer.autoSize = false;
            nameContainer.width = width;
            nameContainer.height = 30;
            nameContainer.autoLayout = true;
            nameContainer.autoLayoutDirection = LayoutDirection.Horizontal;

            KlyteUtils.createUIElement(out label, nameContainer.transform, name);
            KlyteUtils.LimitWidth(label, width);
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

