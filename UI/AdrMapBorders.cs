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

namespace Klyte.Addresses.UI
{
    public class AdrMapBordersChart : MonoBehaviour
    {
        private UIPanel container;
        private UISprite cityMap;
        private UIRadialChartExtended bordersInformation;

        public void SetValues(List<Tuple<int, Color>> locations360)
        {
            locations360.Sort((x, y) => x.First - y.First);
            if (locations360[0].First != 0)
            {
                locations360.Insert(0, Tuple.New(0, locations360.Last().Second));
            }
            if (locations360.Count != bordersInformation.sliceCount)
            {
                while (bordersInformation.sliceCount > 0)
                {
                    bordersInformation.RemoveSlice(0);
                }
                foreach (var loc in locations360)
                {
                    bordersInformation.AddSlice(loc.Second, loc.Second);
                }
            }
            else
            {
                for (int i = 0; i < bordersInformation.sliceCount; i++)
                {
                    bordersInformation.GetSlice(i).innerColor = locations360[i].Second;
                    bordersInformation.GetSlice(i).outterColor = locations360[i].Second;
                }
            }

            List<int> targetValues = locations360.Select(x => x.First * 100 / 360).ToList();
            bordersInformation.SetValuesStarts(targetValues.ToArray());
        }

        public void Awake()
        {
            AdrUtils.doLog("AWAKE AdrMapBordersChart !");
            UIPanel panel = transform.gameObject.AddComponent<UIPanel>();
            panel.width = 370;
            panel.height = 70;
            panel.autoLayout = false;
            panel.useCenter = true;
            panel.wrapLayout = false;
            panel.tooltipLocaleID = "ADR_CITY_NEIGHBORHOOD";

            KlyteUtils.createUIElement(out container, transform, "NeighborhoodContainer");
            container.relativePosition = new Vector3(panel.width / 2f - 35, 0);
            container.width = 70;
            container.height = 70;
            container.autoLayout = false;
            container.useCenter = true;
            container.wrapLayout = false;
            container.tooltipLocaleID = "ADR_CITY_NEIGHBORHOOD";

            KlyteUtils.createUIElement(out bordersInformation, container.transform, "Neighbors");
            bordersInformation.spriteName = "EmptySprite";
            bordersInformation.relativePosition = new Vector3(0, 0);
            bordersInformation.width = 70;
            bordersInformation.height = 70;

            KlyteUtils.createUIElement(out cityMap, bordersInformation.transform, "City");
            cityMap.spriteName = "EmptySprite";
            cityMap.relativePosition = new Vector3(5, 5);
            cityMap.color = Color.gray;
            cityMap.width = 60;
            cityMap.height = 60;
            cityMap.tooltipLocaleID = "ADR_CITY_AREA";
        }
    }

}

