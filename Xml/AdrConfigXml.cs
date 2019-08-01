using ColossalFramework;
using Klyte.Addresses.ModShared;
using Klyte.Addresses.Overrides;
using Klyte.Addresses.UI;
using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using static TransportInfo;
using static VehicleInfo;

namespace Klyte.Addresses.Xml
{
    [XmlRoot("adrConfig")]
    public class AdrConfigXml
    {
        [XmlElement("global")]
        public AdrGlobalConfig GlobalConfig { get; set; } = new AdrGlobalConfig();

        [XmlArray("districts")]
        [XmlArrayItem("district")]
        public List<AdrDistrictConfig> DistrictConfigs
        {
            get => m_districtConfigs;

            set => m_districtConfigs = value;
        }

        [XmlIgnore]
        private List<AdrDistrictConfig> m_districtConfigs = null;

        public AdrDistrictConfig GetConfigForDistrict(ushort districtId)
        {
            if (m_districtConfigs.Where(x => x.Id == districtId).Count() == 0)
            {
                m_districtConfigs.Add(new AdrDistrictConfig
                {
                    Id = districtId
                });
            }
            return m_districtConfigs.Where(x => x.Id == districtId).FirstOrDefault();
        }
    }

    [XmlRoot("adrGlobalConfig")]
    public class AdrGlobalConfig
    {
        [XmlElement("addressing")]
        public AdrAddressingConfig AddressingConfig { get; set; } = new AdrAddressingConfig();

        [XmlElement("neighbor")]
        public AdrNeighborhoodConfig NeighborhoodConfig { get; set; } = new AdrNeighborhoodConfig();

        [XmlElement("buildings")]
        public AdrBuildingConfig BuildingConfig { get; set; } = new AdrBuildingConfig();

        [XmlElement("citizen")]
        public AdrCitizenConfig CitizenConfig { get; set; } = new AdrCitizenConfig();
    }

    [XmlRoot("adrAddressingConfig")]
    public class AdrAddressingConfig
    {
        [XmlAttribute("zipcodeFormat")]
        public string ZipcodeFormat { get; set; } = "GCEDF-AJ";

        [XmlAttribute("zipcodeCityPrefix")]
        public int ZipcodeCityPrefix { get; set; }

        [XmlElement("addressLine1")]
        public string AddressLine1 { get; set; } = "A, B";

        [XmlElement("addressLine2")]
        public string AddressLine2 { get; set; } = "[D - ]C";

        [XmlElement("addressLine3")]
        public string AddressLine3 { get; set; } = "E";

        [XmlElement("districts")]
        public AdrGeneralQualifierConfig DistrictsConfig { get; set; } = new AdrGeneralQualifierConfig();

        [XmlAttribute("zeroMarkBuildingId")]
        public ushort ZeroMarkBuilding
        {
            get => m_savedZeroMarkBuilding; set {
                if (m_savedZeroMarkBuilding != value)
                {
                    AdrEvents.TriggerZeroMarkerBuildingChange();
                }
                m_savedZeroMarkBuilding = value;
            }
        }

        [XmlIgnore]
        private ushort m_savedZeroMarkBuilding;
    }


    [XmlRoot("adrNeighborhoodConfig")]
    public class AdrNeighborhoodConfig
    {
        [XmlArray("neighbors")]
        [XmlArrayItem("neighbor")]
        public List<AdrNeighborDetailConfig> Neighbors { get; set; }

        public void AddToNeigborsListAt(int idx, AdrNeighborDetailConfig adrNeighbor)
        {
            Neighbors.Insert(Math.Min(idx, Neighbors.Count), adrNeighbor);
            GameObject.FindObjectOfType<AdrNeighborConfigTab>()?.MarkDirty();
        }
        public void RemoveNeighborAtIndex(int idx)
        {
            Neighbors.RemoveAt(idx);
            GameObject.FindObjectOfType<AdrNeighborConfigTab>()?.MarkDirty();
        }

        [XmlAttribute("namesFile")]
        public string NamesFile { get; set; }
    }

    public class AdrNeighborDetailConfig
    {
        [XmlAttribute("nameSeed")]
        public uint Seed
        {
            get => m_seed;
            set {
                m_seed = value;
                GameObject.FindObjectOfType<AdrNeighborConfigTab>()?.MarkDirty();
            }
        }
        [XmlAttribute("azimuth")]
        public ushort Azimuth
        {
            get => m_azimuth;
            set {
                m_azimuth = value;
                GameObject.FindObjectOfType<AdrNeighborConfigTab>()?.MarkDirty();
            }
        }

        [XmlIgnore]
        private uint m_seed;
        [XmlIgnore]
        private ushort m_azimuth;
    }

    [XmlRoot("adrBuildingConfig")]
    public class AdrBuildingConfig
    {
        [XmlElement("stationsNameGeneration")]
        public AdrStationNamesGenerationConfig StationsNameGenerationConfig { get; set; } = new AdrStationNamesGenerationConfig();

        [XmlElement("ricoNameGeneration")]
        public AdrRicoNamesGenerationConfig RicoNamesGenerationConfig { get; set; } = new AdrRicoNamesGenerationConfig();
    }

    public class AdrStationNamesGenerationConfig
    {
        [XmlAttribute("trainPassenger")]
        public bool TrainsPassenger { get; set; }

        [XmlAttribute("trainCargo")]
        public bool TrainsCargo { get; set; }

        [XmlAttribute("monorail")]
        public bool Monorail { get; set; }

        [XmlAttribute("metro")]
        public bool Metro { get; set; }

        [XmlAttribute("cableCar")]
        public bool CableCar { get; set; }

        [XmlAttribute("ferry")]
        public bool Ferry { get; set; }

        [XmlAttribute("shipPassenger")]
        public bool ShipPassenger { get; set; }

        [XmlAttribute("shipCargo")]
        public bool ShipCargo { get; set; }

        [XmlAttribute("airplanePassenger")]
        public bool AirplanePassenger { get; set; }

        [XmlAttribute("airplaneCargo")]
        public bool AirplaneCargo { get; set; }

        [XmlAttribute("blimp")]
        public bool Blimp { get; set; }

        public bool IsRenameEnabled(TransportType transport, VehicleType vehicle, BuildingAI buildingAI)
        {
            switch (transport)
            {
                case TransportInfo.TransportType.Airplane:
                    switch (vehicle)
                    {
                        case VehicleType.Blimp:
                            return Blimp;
                        case VehicleType.Plane:
                            return buildingAI is CargoStationAI ? AirplaneCargo : AirplanePassenger;
                    }
                    break;
                case TransportInfo.TransportType.CableCar:
                    return CableCar;
                case TransportInfo.TransportType.Metro:
                    return Metro;
                case TransportInfo.TransportType.Monorail:
                    return Monorail;
                case TransportInfo.TransportType.Ship:
                    switch (vehicle)
                    {
                        case VehicleType.Ferry:
                            return Ferry;
                        case VehicleType.Ship:
                            return buildingAI is CargoStationAI ? ShipCargo : ShipPassenger;
                    }
                    break;
                case TransportInfo.TransportType.Train:
                    return buildingAI is CargoStationAI ? TrainsCargo : TrainsPassenger;

            }
            return false;
        }

    }

    public class AdrRicoNamesGenerationConfig
    {
        [XmlAttribute("industry")]
        public GenerationMethod Industry { get; set; }
        [XmlAttribute("commerce")]
        public GenerationMethod Commerce { get; set; }
        [XmlAttribute("office")]
        public GenerationMethod Office { get; set; }
        [XmlAttribute("residential")]
        public GenerationMethod Residence { get; set; }

        public enum GenerationMethod
        {
            NONE,
            ADDRESS
        }
    }

    [XmlRoot("adrCitizenConfig")]
    public class AdrCitizenConfig
    {
        [XmlAttribute("maleNamesFile")]
        public string MaleNamesFile { get; set; }

        [XmlAttribute("femleNamesFile")]
        public string FemaleNamesFile { get; set; }

        [XmlAttribute("surnamesFile")]
        public string SurnamesFile { get; set; }

    }

    [XmlRoot("adrDistrictConfig")]
    public class AdrDistrictConfig
    {
        [XmlAttribute("id")]
        public ushort Id { get; set; }

        [XmlElement("roads")]
        public AdrGeneralQualifierConfig RoadConfig { get; set; } = new AdrGeneralQualifierConfig();

        [XmlAttribute("zipcodePrefix")]
        public int? ZipcodePrefix { get; set; }

        [XmlIgnore]
        public Color DistrictColor { get => m_cachedColor; set => SetDistrictColor(value); }
        [XmlIgnore]
        private Color m_cachedColor;

        [XmlAttribute("color")]
        public string DistrictColorStr { get => m_cachedColor == default ? null : ColorExtensions.ToRGB(DistrictColor); set => SetDistrictColor(value.IsNullOrWhiteSpace() ? default : (Color) ColorExtensions.FromRGB(value)); }

        private void SetDistrictColor(Color c)
        {
            DistrictManagerOverrides.OnDistrictChanged();
            m_cachedColor = c;
        }
    }

    public class AdrGeneralQualifierConfig
    {
        [XmlAttribute("namesFile")]
        public string NamesFile { get; set; }

        [XmlAttribute("qualifierFile")]
        public string QualifierFile { get; set; }
    }
}

