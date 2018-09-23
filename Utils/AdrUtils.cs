using Klyte.Commons.Utils;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Klyte.Addresses.Utils
{
    class AdrUtils : KlyteUtils
    {
        #region Logging
        public static void doLog(string format, params object[] args)
        {
            try
            {
                if (AddressesMod.debugMode)
                {
                    Console.WriteLine("AdrV" + AddressesMod.version + " " + format, args);
                }
            }
            catch
            {
                Debug.LogErrorFormat("AdrV" + AddressesMod.version + " Erro ao fazer log: {0} (args = {1})", format, args == null ? "[]" : string.Join(",", args.Select(x => x != null ? x.ToString() : "--NULL--").ToArray()));
            }
        }
        public static void doErrorLog(string format, params object[] args)
        {
            try
            {
                if (AddressesMod.instance != null)
                {
                    Debug.LogErrorFormat("AdrV" + AddressesMod.version + " " + format, args);
                }
                else
                {
                    Console.WriteLine("AdrV" + AddressesMod.version + " " + format, args);
                }

            }
            catch
            {
                Debug.LogErrorFormat("AdrV" + AddressesMod.version + " Erro ao logar ERRO!!!: {0} (args = [{1}])", format, args == null ? "" : string.Join(",", args.Select(x => x != null ? x.ToString() : "--NULL--").ToArray()));
            }
        }

        #endregion

        #region Addresses

        public static void getAddressLines(Vector3 sidewalk, Vector3 midPosBuilding, out String[] addressLines)
        {
            addressLines = null;
            string line1 = AdrConfigWarehouse.getCurrentConfigString(AdrConfigWarehouse.ConfigIndex.ADDRESS_FORMAT_LINE1);
            string line2 = AdrConfigWarehouse.getCurrentConfigString(AdrConfigWarehouse.ConfigIndex.ADDRESS_FORMAT_LINE2);
            string line3 = AdrConfigWarehouse.getCurrentConfigString(AdrConfigWarehouse.ConfigIndex.ADDRESS_FORMAT_LINE3);
            string format = line1 + "≠" + line2 + "≠" + line3;

            string a = "";//street
            int b = 0;//number
            string c = SimulationManager.instance.m_metaData.m_CityName;//city
            string d = "";//district
            string e = "";//zipcode

            if (format.ToCharArray().Intersect("AB".ToCharArray()).Count() > 0)
            {
                if (!GetAddressStreetAndNumber(sidewalk, midPosBuilding, out b, out a)) return;
            }

            if (format.ToCharArray().Intersect("D[]".ToCharArray()).Count() > 0)
            {
                int districtId = GetDistrict(midPosBuilding);
                if (districtId > 0)
                {
                    d = DistrictManager.instance.GetDistrictName(districtId);
                    format = Regex.Replace(format, @"\]|\[", "", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                }
                else
                {
                    format = Regex.Replace(format, @"\[[^]]*\]", "", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                }

            }

            if (format.Contains("E"))
            {
                e = formatPostalCode(sidewalk, AdrConfigWarehouse.getCurrentConfigString(AdrConfigWarehouse.ConfigIndex.ZIPCODE_FORMAT));
            }
            parseToFormatableString(ref format, 5);

            addressLines = String.Format(format, a, b, c, d, e).Split("≠".ToCharArray());
        }

        private static void parseToFormatableString(ref string input, byte count)
        {
            for (int i = 0; i < count; i++)
            {
                input = input.Replace(((char)((int)'A' + i)).ToString(), $"{{{i}}}");
            }
        }



        /* Format:
         * 
         * A = District code (2 digits) - 00 for none
         * B = District code (3 digits) - 000 for none
         * C = X map area (0-8)
         * D = Y map area (0-8)
         * E = Decimal X division in map area (0-9)
         * F = Decimal Y division in map area (0-9)
         * G = City fixed number (1 digit)
         * H = City fixed number (2 digits)
         * I = City fixed number (3 digits) 
         * J = Last digit of segmentId
         * K = Last 2 digits of segmentId
         * L = Last 3 digits of segmentId
         */
        public static string formatPostalCode(Vector3 position, string format = "GCEDF-AJ")
        {
            format = format ?? "GCEDF-AJ";
            int district = GetDistrict(position) & 0xff;
            if (format.ToCharArray().Intersect("AB".ToCharArray()).Count() > 0)
            {
                int districtPrefix = AdrConfigWarehouse.getCurrentConfigInt(AdrConfigWarehouse.ConfigIndex.ZIPCODE_PREFIX | (AdrConfigWarehouse.ConfigIndex)district);
                format = format.Replace("A", (districtPrefix % 100).ToString("00"));
                format = format.Replace("B", (districtPrefix % 1000).ToString("000"));
            }
            if (format.ToCharArray().Intersect("CDEF".ToCharArray()).Count() > 0)
            {
                Vector2 tilePos = getMapTile(position);
                format = format.Replace("C", Math.Floor(tilePos.x).ToString("0"));
                format = format.Replace("D", Math.Floor(tilePos.y).ToString("0"));
                format = format.Replace("E", Math.Floor((tilePos.x % 1) * 10).ToString("0"));
                format = format.Replace("F", Math.Floor((tilePos.y % 1) * 10).ToString("0"));
            }
            if (format.ToCharArray().Intersect("GHI".ToCharArray()).Count() > 0)
            {
                int cityPrefix = AdrConfigWarehouse.getCurrentConfigInt(AdrConfigWarehouse.ConfigIndex.ZIPCODE_CITY_PREFIX);
                format = format.Replace("G", (cityPrefix % 10).ToString("0"));
                format = format.Replace("H", (cityPrefix % 100).ToString("00"));
                format = format.Replace("I", (cityPrefix % 1000).ToString("000"));
            }
            if (format.ToCharArray().Intersect("JKL".ToCharArray()).Count() > 0)
            {
                GetNearestSegment(position, out Vector3 targetPosition, out float targetLength, out ushort targetSegmentId);
                format = format.Replace("J", (targetSegmentId % 10).ToString("0"));
                format = format.Replace("K", (targetSegmentId % 100).ToString("00"));
                format = format.Replace("L", (targetSegmentId % 1000).ToString("000"));
            }

            return format;
        }
        #endregion
    }
}
