using Klyte.Commons;
using System;
using UnityEngine;

namespace Klyte.Addresses.Tools
{

    public class RoadSegmentTool : BasicNetTool<RoadSegmentTool>
    {
        public event Action<ushort> OnSelectSegment;
        protected static new Color m_hoverColor = new Color32(22, byte.MaxValue, 22, byte.MaxValue);
        public bool singleSelectMode = true;

        public ushort CurrentHoverSegment => m_hoverSegment;

        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo)
        {

            //if (m_hoverSegment != 0)
            //{
            //    Color toolColor = m_hoverColor;
            //    RenderOverlayUtils.RenderNetSegmentOverlay(cameraInfo, toolColor, m_hoverSegment);
            //    return;
            //}

        }

        protected override void OnLeftClick()
        {
            if (m_hoverSegment != 0)
            {
                OnSelectSegment?.Invoke(m_hoverSegment);
                if (singleSelectMode)
                {
                    ToolsModifierControl.SetTool<DefaultTool>();
                }
            }
        }

        protected override void OnDisable()
        {
            OnSelectSegment = null;
            base.OnDisable();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            singleSelectMode = true;
        }

    }

}
