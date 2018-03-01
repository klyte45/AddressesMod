using ColossalFramework;
using ColossalFramework.Globalization;
using Klyte.Commons.Extensors;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

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

            ZIPCODE_GLOBAL_CONFIG = 0x000100 | GLOBAL_CONFIG,
            
            ZIPCODE_FORMAT = 0x01 | ZIPCODE_GLOBAL_CONFIG | TYPE_INT,
            ZIPCODE_CITY_PREFIX = 0x02 | ZIPCODE_GLOBAL_CONFIG | TYPE_INT,            
        }

        public static ConfigIndex[] defaultTrueBoolProperties => new ConfigIndex[] {

        };

        public override string getDefaultStringValueForProperty(ConfigIndex i)
        {
            if (i == ConfigIndex.ZIPCODE_FORMAT)
            {
                return "GCEDF-AJ";
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

