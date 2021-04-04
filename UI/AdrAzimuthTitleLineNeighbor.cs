using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Addresses.Extensions;
using Klyte.Commons.UI;
using Klyte.Commons.UI.SpriteNames;
using Klyte.Commons.Utils;
using UnityEngine;

namespace Klyte.Addresses.UI
{
    public class AdrAzimuthTitleLineNeighbor : UICustomControl
    {
        private UIPanel m_container;


        public void Awake()
        {
            m_container = GetComponent<UIPanel>();
            m_container.width = m_container.parent.width;
            m_container.height = 30;
            m_container.autoLayout = true;
            m_container.autoLayoutDirection = LayoutDirection.Horizontal;
            m_container.autoLayoutPadding = new RectOffset(2, 2, 2, 2);
            m_container.wrapLayout = false;
            m_container.name = "AzimuthEditorTitle";



            CreateTitleLabel(out UILabel m_cityId, "CityId", "#", 30);
            CreateTitleLabel(out UILabel m_azimuthInput, "StartAzimuth", Locale.Get("K45_ADR_AZIMUTH_GRADOS_TITLE"), 50);
            CreateTitleLabel(out UILabel m_direction, "Direction", Locale.Get("K45_ADR_DIRECTION_TITLE"), 60);
            CreateTitleLabel(out UILabel m_generatedName, "GenName", Locale.Get("K45_ADR_GEN_NAME_TITLE"), m_container.width - 247.5f);

            DefaultEditorUILib.AddButtonInEditorRow(m_generatedName, CommonsSpriteNames.K45_Plus, () => AdrNeighborhoodExtension.SetAzimuth(99, 0), "K45_ADR_ADD_REG_CITY", false, 30);
        }

        private void CreateTitleLabel(out UILabel label, string name, string text, float width)
        {

            KlyteMonoUtils.CreateUIElement(out UIPanel nameContainer, m_container.transform, "GenNameContainer");
            nameContainer.autoSize = false;
            nameContainer.width = width;
            nameContainer.height = 30;
            nameContainer.autoLayout = true;
            nameContainer.autoLayoutDirection = LayoutDirection.Horizontal;

            KlyteMonoUtils.CreateUIElement(out label, nameContainer.transform, name);
            KlyteMonoUtils.LimitWidthAndBox(label, width);
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

