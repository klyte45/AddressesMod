using Klyte.Commons.Interfaces;
using System.Linq;

namespace Klyte.Addresses
{
    internal class AdrConfigWarehouse : ConfigWarehouseBase<AdrConfigWarehouse.ConfigIndex, AdrConfigWarehouse>
    {
        private const string CONFIG_FILENAME = "KlyteAddresses";

        public override string ConfigFilename => CONFIG_FILENAME;
        public const string TRUE_VALUE = "1";
        public const string FALSE_VALUE = "0";

        public AdrConfigWarehouse() { }

        public override bool getDefaultBoolValueForProperty(ConfigIndex i)
        {
            return defaultTrueBoolProperties.Contains(i);
        }

        public enum ConfigIndex : uint
        {
            NIL = 0,
            CONFIG_GROUP = 0xF00000,
            TYPE_DATA = 0x0F0000,
            DESC_DATA = 0x00FF00,
            DISTRICT_SUBITEM_DATA = 0x0000FF,

            GLOBAL_CONFIG = 0x100000,
            DISTRICT_CONFIG = 0x200000,

            TYPE_STRING = 0x010000,
            TYPE_INT = 0x020000,
            TYPE_BOOL = 0x030000,
            TYPE_LIST = 0x040000,
            TYPE_DICTIONARY = 0x050000,

            ROAD_NAME_FILENAME = 0x000100 | TYPE_STRING | DISTRICT_CONFIG,
            ZIPCODE_PREFIX = 0x000200 | TYPE_INT | DISTRICT_CONFIG,
            PREFIX_FILENAME = 0x000300 | TYPE_STRING | DISTRICT_CONFIG,

            ADDRESSING_GLOBAL_CONFIG = 0x000100 | GLOBAL_CONFIG,
            NEIGHBOR_GLOBAL_CONFIG = 0x000200 | GLOBAL_CONFIG,
            BUILDING_GLOBAL_CONFIG = 0x000300 | GLOBAL_CONFIG,
            CITIZEN_GLOBAL_CONFIG = 0x000400 | GLOBAL_CONFIG,

            ZIPCODE_FORMAT = 0x01 | ADDRESSING_GLOBAL_CONFIG | TYPE_STRING,
            ZIPCODE_CITY_PREFIX = 0x02 | ADDRESSING_GLOBAL_CONFIG | TYPE_INT,
            ADDRESS_FORMAT_LINE1 = 0x03 | ADDRESSING_GLOBAL_CONFIG | TYPE_STRING,
            ADDRESS_FORMAT_LINE2 = 0x04 | ADDRESSING_GLOBAL_CONFIG | TYPE_STRING,
            ADDRESS_FORMAT_LINE3 = 0x05 | ADDRESSING_GLOBAL_CONFIG | TYPE_STRING,
            DISTRICT_PREFIX_FILE = 0x06 | ADDRESSING_GLOBAL_CONFIG | TYPE_STRING,
            DISTRICT_NAMING_FILE = 0x07 | ADDRESSING_GLOBAL_CONFIG | TYPE_STRING,


            NEIGHBOR_CONFIG_AZIMUTHS_STOPS = 0x01 | TYPE_STRING | NEIGHBOR_GLOBAL_CONFIG,
            NEIGHBOR_CONFIG_NAME_FILE = 0x02 | TYPE_STRING | NEIGHBOR_GLOBAL_CONFIG,

            ENABLE_CUSTOM_NAMING_STATIONS_TRAIN = 0x01 | TYPE_BOOL | BUILDING_GLOBAL_CONFIG,
            ENABLE_CUSTOM_NAMING_STATIONS_MONORAIL = 0x02 | TYPE_BOOL | BUILDING_GLOBAL_CONFIG,
            ENABLE_CUSTOM_NAMING_STATIONS_METRO = 0x03 | TYPE_BOOL | BUILDING_GLOBAL_CONFIG,
            ENABLE_CUSTOM_NAMING_STATIONS_CABLE_CAR = 0x04 | TYPE_BOOL | BUILDING_GLOBAL_CONFIG,
            ENABLE_CUSTOM_NAMING_STATIONS_FERRY = 0x05 | TYPE_BOOL | BUILDING_GLOBAL_CONFIG,
            ENABLE_CUSTOM_NAMING_STATIONS_SHIP = 0x06 | TYPE_BOOL | BUILDING_GLOBAL_CONFIG,
            ENABLE_CUSTOM_NAMING_STATIONS_BLIMP = 0x07 | TYPE_BOOL | BUILDING_GLOBAL_CONFIG,
            ENABLE_CUSTOM_NAMING_STATIONS_AIRPLANE = 0x08 | TYPE_BOOL | BUILDING_GLOBAL_CONFIG,
            ENABLE_CUSTOM_NAMING_CARGO_SHIP = 0x09 | TYPE_BOOL | BUILDING_GLOBAL_CONFIG,
            ENABLE_CUSTOM_NAMING_CARGO_TRAIN = 0x0A | TYPE_BOOL | BUILDING_GLOBAL_CONFIG,
            ENABLE_ADDRESS_NAMING_RES = 0x0B | TYPE_BOOL | BUILDING_GLOBAL_CONFIG,
            ENABLE_ADDRESS_NAMING_IND = 0x0c | TYPE_BOOL | BUILDING_GLOBAL_CONFIG,
            ENABLE_ADDRESS_NAMING_COM = 0x0d | TYPE_BOOL | BUILDING_GLOBAL_CONFIG,
            ENABLE_ADDRESS_NAMING_OFF = 0x0e | TYPE_BOOL | BUILDING_GLOBAL_CONFIG,

            MALE_FIRSTNAME_FILE = 0x000001 | TYPE_STRING | CITIZEN_GLOBAL_CONFIG,
            FEMALE_FIRSTNAME_FILE = 0x000002 | TYPE_STRING | CITIZEN_GLOBAL_CONFIG,
            LASTNAME_FILE = 0x000003 | TYPE_STRING | CITIZEN_GLOBAL_CONFIG,

        }

        public static ConfigIndex[] defaultTrueBoolProperties => new ConfigIndex[] {
             ConfigIndex.ENABLE_CUSTOM_NAMING_STATIONS_TRAIN,
             ConfigIndex.ENABLE_CUSTOM_NAMING_STATIONS_MONORAIL ,
             ConfigIndex.ENABLE_CUSTOM_NAMING_STATIONS_METRO    ,
             ConfigIndex.ENABLE_CUSTOM_NAMING_STATIONS_CABLE_CAR,
             ConfigIndex.ENABLE_CUSTOM_NAMING_STATIONS_FERRY,
             ConfigIndex.ENABLE_CUSTOM_NAMING_CARGO_SHIP           ,
             ConfigIndex.ENABLE_CUSTOM_NAMING_CARGO_TRAIN          ,
             ConfigIndex.ENABLE_ADDRESS_NAMING_RES                 ,
        };

        public override string getDefaultStringValueForProperty(ConfigIndex i)
        {
            if (i == ConfigIndex.ZIPCODE_FORMAT)
            {
                return "GCEDF-AJ";
            }
            if (i == ConfigIndex.ADDRESS_FORMAT_LINE1)
            {
                return "A, B";
            }
            if (i == ConfigIndex.ADDRESS_FORMAT_LINE2)
            {
                return "[D - ]C";
            }
            if (i == ConfigIndex.ADDRESS_FORMAT_LINE3)
            {
                return "E";
            }
            return base.getDefaultStringValueForProperty(i);
        }

        public override int getDefaultIntValueForProperty(ConfigIndex i)
        {
            if ((i & (ConfigIndex)~0xffu) == ConfigIndex.ZIPCODE_PREFIX)
            {
                return (int)i & 0xff;
            }
            return 0;
        }
    }
}

