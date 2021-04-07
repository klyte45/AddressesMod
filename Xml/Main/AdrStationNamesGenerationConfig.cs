using System.Xml.Serialization;
using static TransportInfo;
using static VehicleInfo;

namespace Klyte.Addresses.Xml
{
    public class AdrStationNamesGenerationConfig
    {
        [XmlAttribute("trainPassenger")]
        public bool TrainsPassenger { get; set; }
        [XmlAttribute("bus")]
        public bool Bus { get; set; }
        [XmlAttribute("intercityBus")]
        public bool IntercityBus { get; set; }

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

        [XmlAttribute("trolleybus")]
        public bool Trolleybus { get; set; }

        [XmlAttribute("tram")]
        public bool Tram { get; set; }

        [XmlAttribute("helicopter")]
        public bool Helicopter { get; set; }

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
                    return false;
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
                    return false;
                case TransportInfo.TransportType.Train:
                    return buildingAI is CargoStationAI ? TrainsCargo : TrainsPassenger;
                case TransportInfo.TransportType.Tram:
                    return buildingAI is TransportStationAI && Tram;
                case TransportInfo.TransportType.Bus:
                    if (buildingAI is TransportStationAI tsai2)
                    {
                        var isUrban = (tsai2.m_info.m_class.m_level == ItemClass.Level.Level1);
                        if (isUrban && Bus)
                        {
                            return true;
                        }
                        var isIntermunicipal = (tsai2.m_info.m_class.m_level == ItemClass.Level.Level3);
                        if (isIntermunicipal && IntercityBus)
                        {
                            return true;
                        }

                    }
                    return false;
                case TransportInfo.TransportType.Trolleybus:
                    return buildingAI is TransportStationAI && Trolleybus;
                case TransportInfo.TransportType.Helicopter:
                    return buildingAI is TransportStationAI && Helicopter;
                default:
                    return false;
            }
        }

    }
}

