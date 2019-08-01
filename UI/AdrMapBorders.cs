using ColossalFramework.UI;
using Klyte.Commons.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Klyte.Addresses.UI
{
    public class AdrMapBordersChart : MonoBehaviour
    {
        private UIPanel m_container;
        private UISprite m_cityMap;
        private UIRadialChartExtended m_bordersInformation;

        public void SetValues(List<Tuple<int, Color>> locations360)
        {
            locations360.Sort((x, y) => x.First - y.First);
            if (locations360[0].First != 0)
            {
                locations360.Insert(0, Tuple.New(0, locations360.Last().Second));
            }
            if (locations360.Count != m_bordersInformation.sliceCount)
            {
                while (m_bordersInformation.sliceCount > 0)
                {
                    m_bordersInformation.RemoveSlice(0);
                }
                foreach (Tuple<int, Color> loc in locations360)
                {
                    m_bordersInformation.AddSlice(loc.Second, loc.Second);
                }
            }
            else
            {
                for (int i = 0; i < m_bordersInformation.sliceCount; i++)
                {
                    m_bordersInformation.GetSlice(i).innerColor = locations360[i].Second;
                    m_bordersInformation.GetSlice(i).outterColor = locations360[i].Second;
                }
            }

            List<int> targetValues = locations360.Select(x => x.First * 100 / 360).ToList();
            m_bordersInformation.SetValuesStarts(targetValues.ToArray());
        }

        public void Awake()
        {
            LogUtils.DoLog("AWAKE AdrMapBordersChart !");
            UIPanel panel = transform.gameObject.AddComponent<UIPanel>();
            panel.width = 370;
            panel.height = 70;
            panel.autoLayout = false;
            panel.useCenter = true;
            panel.wrapLayout = false;
            panel.tooltipLocaleID = "K45_ADR_CITY_NEIGHBORHOOD";

            KlyteMonoUtils.CreateUIElement(out m_container, transform, "NeighborhoodContainer");
            m_container.relativePosition = new Vector3((panel.width / 2f) - 35, 0);
            m_container.width = 70;
            m_container.height = 70;
            m_container.autoLayout = false;
            m_container.useCenter = true;
            m_container.wrapLayout = false;
            m_container.tooltipLocaleID = "K45_ADR_CITY_NEIGHBORHOOD";

            KlyteMonoUtils.CreateUIElement(out m_bordersInformation, m_container.transform, "Neighbors");
            m_bordersInformation.spriteName = "EmptySprite";
            m_bordersInformation.relativePosition = new Vector3(0, 0);
            m_bordersInformation.width = 70;
            m_bordersInformation.height = 70;

            KlyteMonoUtils.CreateUIElement(out m_cityMap, m_bordersInformation.transform, "City");
            m_cityMap.spriteName = "EmptySprite";
            m_cityMap.relativePosition = new Vector3(5, 5);
            m_cityMap.color = Color.gray;
            m_cityMap.width = 60;
            m_cityMap.height = 60;
            m_cityMap.tooltipLocaleID = "K45_ADR_CITY_AREA";
        }
    }

}

