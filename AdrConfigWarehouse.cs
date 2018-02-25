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
            PREFIX_FILENAME = 0x0300 | TYPE_STRING | DISTRICT_CONFIG,

            ZIPCODE_GLOBAL_CONFIG = 0x000100 | GLOBAL_CONFIG,
            HIGHWAY_STATE_CONFIG = 0x000200 | GLOBAL_CONFIG,
            INTERSTATE_CONFIG = 0x000300 | GLOBAL_CONFIG,
            INTERNATIONAL_HIGHWAY_CONFIG = 0x000400 | GLOBAL_CONFIG,
            STREET_TYPE_NAMING_CONFIG = 0x000500 | GLOBAL_CONFIG,

            ZIPCODE_ENABLED = 0x01 | ZIPCODE_GLOBAL_CONFIG | TYPE_BOOL,
            ZIPCODE_SIZE = 0x02 | ZIPCODE_GLOBAL_CONFIG | TYPE_INT,
            ZIPCODE_PREFIX_SIZE = 0x03 | ZIPCODE_GLOBAL_CONFIG | TYPE_INT,
            ZIPCODE_SUFFIX_SIZE = 0x04 | ZIPCODE_GLOBAL_CONFIG | TYPE_INT,
            ZIPCODE_SEPARATOR = 0x05 | ZIPCODE_GLOBAL_CONFIG | TYPE_INT,

            HIGHWAY_ENABLED = 0x01 | HIGHWAY_STATE_CONFIG | TYPE_BOOL,
            HIGHWAY_PREFIX = 0x02 | HIGHWAY_STATE_CONFIG | TYPE_STRING,
            HIGHWAY_DIGITS = 0x03 | HIGHWAY_STATE_CONFIG | TYPE_INT,
            HIGHWAY_SEPARATOR = 0x04 | HIGHWAY_STATE_CONFIG | TYPE_INT,
            HIGHWAY_PREFIX_AS_SUFFIX = 0x05 | HIGHWAY_STATE_CONFIG | TYPE_BOOL,

            INTERSTATE_ENABLED = 0x01 | INTERSTATE_CONFIG | TYPE_BOOL,
            INTERSTATE_PREFIX = 0x02 | INTERSTATE_CONFIG | TYPE_STRING,
            INTERSTATE_DIGITS = 0x03 | INTERSTATE_CONFIG | TYPE_INT,
            INTERSTATE_SEPARATOR = 0x04 | INTERSTATE_CONFIG | TYPE_INT,
            INTERSTATE_PREFIX_AS_SUFFIX = 0x05 | INTERSTATE_CONFIG | TYPE_BOOL,
            INTERSTATE_ASSETS = 0x06 | INTERSTATE_CONFIG | TYPE_BOOL,

            INTERNATIONAL_HIGHWAY_ENABLED = 0x01 | INTERNATIONAL_HIGHWAY_CONFIG | TYPE_BOOL,
            INTERNATIONAL_HIGHWAY_PREFIX = 0x02 | INTERNATIONAL_HIGHWAY_CONFIG | TYPE_STRING,
            INTERNATIONAL_HIGHWAY_DIGITS = 0x03 | INTERNATIONAL_HIGHWAY_CONFIG | TYPE_INT,
            INTERNATIONAL_HIGHWAY_SEPARATOR = 0x04 | INTERNATIONAL_HIGHWAY_CONFIG | TYPE_INT,
            INTERNATIONAL_HIGHWAY_PREFIX_AS_SUFFIX = 0x05 | INTERNATIONAL_HIGHWAY_CONFIG | TYPE_BOOL,
            INTERNATIONAL_HIGHWAY_ASSETS = 0x06 | INTERNATIONAL_HIGHWAY_CONFIG | TYPE_BOOL,

        }

        public static ConfigIndex[] defaultTrueBoolProperties => new ConfigIndex[] {
            ConfigIndex.ZIPCODE_ENABLED
        };

        public override int getDefaultIntValueForProperty(ConfigIndex i)
        {
            return 0;
        }
    }
}

