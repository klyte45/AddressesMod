using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ColossalFramework;
using ColossalFramework.UI;
using Klyte.Addresses.UI;
using Klyte.Addresses.Utils;
using Klyte.TransportLinesManager.Extensors;
using Klyte.TransportLinesManager.Utils;
using UnityEngine;

namespace Klyte.Addresses
{
    internal class AdrController : Singleton<AdrController>
    {
        internal static UITextureAtlas taAdr;
        private UIView uiView;
        private UIButton openAdrPanelButton;
        private UIPanel buildingInfoParent;

        public void destroy()
        {
            Destroy(openAdrPanelButton);
        }

        public void Start()
        {
            uiView = FindObjectOfType<UIView>();
            if (!uiView)
                return;

            UITabstrip toolStrip = uiView.FindUIComponent<UITabstrip>("MainToolstrip");
            AdrUtils.createUIElement(out openAdrPanelButton, null);
            this.openAdrPanelButton.size = new Vector2(49f, 49f);
            this.openAdrPanelButton.name = "AddressesButton";
            this.openAdrPanelButton.tooltip = "Addresses (v" + AddressesMod.version + ")";
            this.openAdrPanelButton.relativePosition = new Vector3(0f, 5f);
            toolStrip.AddTab("AddressesButton", this.openAdrPanelButton.gameObject, null, null);
            openAdrPanelButton.atlas = taAdr;
            openAdrPanelButton.normalBgSprite = "AddressesIconSmall";
            openAdrPanelButton.focusedFgSprite = "ToolbarIconGroup6Focused";
            openAdrPanelButton.hoveredFgSprite = "ToolbarIconGroup6Hovered";
            this.openAdrPanelButton.eventButtonStateChanged += delegate (UIComponent c, UIButton.ButtonState s)
            {
                if (s == UIButton.ButtonState.Focused)
                {
                    internal_OpenAdrPanel();
                }
                else
                {
                    internal_CloseAdrPanel();
                }
            };
            AdrConfigPanel.Get();

            var typeTarg = typeof(Redirector<>);
            List<Type> instances = GetSubtypesRecursive(typeTarg);

            foreach (Type t in instances)
            {
                gameObject.AddComponent(t);
            }
        }

        private static List<Type> GetSubtypesRecursive(Type typeTarg)
        {
            var classes = from t in Assembly.GetAssembly(typeof(AdrController)).GetTypes()
                          let y = t.BaseType
                          where t.IsClass && y != null && y.IsGenericType == typeTarg.IsGenericType && (y.GetGenericTypeDefinition() == typeTarg || y.BaseType == typeTarg)
                          select t;
            List<Type> result = new List<Type>();
            foreach (Type t in classes)
            {
                if (t.IsAbstract)
                {
                    result.AddRange(GetSubtypesRecursive(t));
                }
                else
                {
                    result.Add(t);
                }
            }
            return result;
        }

        public void Awake()
        {
            initNearLinesOnWorldInfoPanel();
            AddressesMod.instance.showVersionInfoPopup();
        }

        private void ToggleAdrPanel()
        {
            openAdrPanelButton.SimulateClick();
        }
        public void OpenAdrPanel()
        {
            if (!AdrConfigPanel.Get().GetComponent<UIPanel>().isVisible)
            {
                openAdrPanelButton.SimulateClick();
            }
        }
        public void CloseAdrPanel()
        {
            if (AdrConfigPanel.Get().GetComponent<UIPanel>().isVisible)
            {
                openAdrPanelButton.SimulateClick();
            }
        }

        private void internal_CloseAdrPanel()
        {
            AdrConfigPanel.Get().GetComponent<UIPanel>().isVisible = false;
            openAdrPanelButton.Unfocus();
            openAdrPanelButton.state = UIButton.ButtonState.Normal;
        }

        private void internal_OpenAdrPanel()
        {
            AdrConfigPanel.Get().GetComponent<UIPanel>().isVisible = true;
        }

        private void initNearLinesOnWorldInfoPanel()
        {

            UIPanel parent = GameObject.Find("UIView").transform.GetComponentInChildren<CityServiceWorldInfoPanel>().gameObject.GetComponent<UIPanel>();

            if (parent == null)
                return;
            parent.eventVisibilityChanged += (component, value) =>
            {
                UpdateAddressField(parent);
            };
            parent.eventPositionChanged += (component, value) =>
            {
                UpdateAddressField(parent);
            };

            UIPanel parent2 = GameObject.Find("UIView").transform.GetComponentInChildren<ZonedBuildingWorldInfoPanel>().gameObject.GetComponent<UIPanel>();

            if (parent2 == null)
                return;

            parent2.eventVisibilityChanged += (component, value) =>
            {
                UpdateAddressField(parent);
            };
            parent2.eventPositionChanged += (component, value) =>
            {
                UpdateAddressField(parent);
            };

        }

        private void UpdateAddressField(UIPanel parent)
        {
            if (parent != null)
            {
                UIButton addressIcon = parent.Find<UIButton>("AddressesIcon");
                if (!addressIcon)
                {
                    addressIcon = initBuildingEditOnWorldInfoPanel(parent);
                }
                var prop = typeof(WorldInfoPanel).GetField("m_InstanceID", System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Instance);
                ushort buildingId = ((InstanceID)(prop.GetValue(parent.gameObject.GetComponent<WorldInfoPanel>()))).Building;
                addressIcon.isVisible = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingId].Info.m_placementMode == BuildingInfo.PlacementMode.Roadside;
                if (addressIcon.isVisible)
                {
                    UILabel addressLabel = addressIcon.Find<UILabel>("Address");
                    String[] addressLines = new String[] { "Linha1", "Linha2" };
                    addressLabel.prefix = addressLines[0];
                    addressLabel.suffix = addressLines[1];
                }
            }
        }
        private UIButton initBuildingEditOnWorldInfoPanel(UIPanel parent)
        {
            AdrUtils.createUIElement(out UIButton saida, parent.transform, "AddressesIcon", new Vector4(5, -40, 30, 30));
            saida.atlas = taAdr;
            saida.color = Color.white;
            saida.tooltipLocaleID = "ADR_BUILDING_ADDRESS";
            AdrUtils.initButtonSameSprite(saida, "AddressesIcon");

            AdrUtils.createUIElement(out UILabel prefixes, saida.transform, "Address", new Vector4(35, 1, 200, 60));
            prefixes.autoSize = true;
            prefixes.wordWrap = true;
            prefixes.textAlignment = UIHorizontalAlignment.Left;
            prefixes.useOutline = true;
            prefixes.text = Environment.NewLine;
            prefixes.textScale = 0.6f;

            return saida;
        }
    }
}