using Klyte.Addresses.ModShared;
using Klyte.Addresses.UI;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Klyte.Addresses.Xml
{
    [XmlRoot("adrNeighborhoodConfig")]
    public class AdrNeighborhoodConfig
    {
        private string m_namesFile;

        [XmlArray("neighbors")]
        [XmlArrayItem("neighbor")]
        public List<AdrNeighborDetailConfig> Neighbors { get; set; } = new List<AdrNeighborDetailConfig>();

        public void AddToNeigborsListAt(int idx, AdrNeighborDetailConfig adrNeighbor)
        {
            Neighbors.Insert(Math.Min(idx, Neighbors.Count), adrNeighbor);
            AdrNeighborConfigTab.Instance?.MarkDirty();
        }
        public void RemoveNeighborAtIndex(int idx)
        {
            Neighbors.RemoveAt(idx);
            AdrNeighborConfigTab.Instance?.MarkDirty();
        }

        [XmlAttribute("namesFile")]
        public string NamesFile
        {
            get => m_namesFile; set
            {
                m_namesFile = value;
                AdrShared.TriggerBuildingNameStrategyChanged();
            }
        }
    }
}

