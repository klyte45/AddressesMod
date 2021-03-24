using Klyte.Commons.Utils;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Klyte.Addresses.Utils
{
    internal class AdrUtils
    {

        #region Addresses

        public static void GetAddressLines(Vector3 sidewalk, Vector3 midPosBuilding, out string[] addressLines)
        {
            addressLines = null;
            string line1 = AdrController.CurrentConfig.GlobalConfig.AddressingConfig.AddressLine1;
            string line2 = AdrController.CurrentConfig.GlobalConfig.AddressingConfig.AddressLine2;
            string line3 = AdrController.CurrentConfig.GlobalConfig.AddressingConfig.AddressLine3;
            string format = line1 + "≠" + line2 + "≠" + line3;

            string a = "";//street
            int b = 0;//number
            string c = SimulationManager.instance.m_metaData.m_CityName;//city
            string d = "";//district
            string e = "";//zipcode

            if (format.ToCharArray().Intersect("AB".ToCharArray()).Count() > 0)
            {
                if (!SegmentUtils.GetAddressStreetAndNumber(sidewalk, midPosBuilding, out b, out a))
                {
                    return;
                }
            }

            if (format.ToCharArray().Intersect("D[]".ToCharArray()).Count() > 0)
            {
                int districtId = DistrictManager.instance.GetDistrict(midPosBuilding);
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
                e = FormatPostalCode(sidewalk, AdrController.CurrentConfig.GlobalConfig.AddressingConfig.ZipcodeFormat);
            }
            ParseToFormatableString(ref format, 5);

            addressLines = string.Format(format, a, b, c, d, e).Split("≠".ToCharArray());
        }

        private static void ParseToFormatableString(ref string input, byte count)
        {
            for (int i = 0; i < count; i++)
            {
                input = input.Replace(((char)('A' + i)).ToString(), $"{{{i}}}");
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
        public static string FormatPostalCode(Vector3 position, string format = "GCEDF-AJ")
        {
            format = format ?? "GCEDF-AJ";
            int district = DistrictManager.instance.GetDistrict(position) & 0xff;
            if (format.ToCharArray().Intersect("AB".ToCharArray()).Count() > 0)
            {
                int districtPrefix = AdrController.CurrentConfig.GetConfigForDistrict((ushort)district).ZipcodePrefix ?? district;
                format = format.Replace("A", (districtPrefix % 100).ToString("00"));
                format = format.Replace("B", (districtPrefix % 1000).ToString("000"));
            }
            if (format.ToCharArray().Intersect("CDEF".ToCharArray()).Count() > 0)
            {
                Vector2 tilePos = MapUtils.GetMapTile(position);
                format = format.Replace("C", Math.Floor(tilePos.x).ToString("0"));
                format = format.Replace("D", Math.Floor(tilePos.y).ToString("0"));
                format = format.Replace("E", Math.Floor(tilePos.x % 1 * 10).ToString("0"));
                format = format.Replace("F", Math.Floor(tilePos.y % 1 * 10).ToString("0"));
            }
            if (format.ToCharArray().Intersect("GHI".ToCharArray()).Count() > 0)
            {
                int cityPrefix = AdrController.CurrentConfig.GlobalConfig.AddressingConfig.ZipcodeCityPrefix;
                format = format.Replace("G", (cityPrefix % 10).ToString("0"));
                format = format.Replace("H", (cityPrefix % 100).ToString("00"));
                format = format.Replace("I", (cityPrefix % 1000).ToString("000"));
            }
            if (format.ToCharArray().Intersect("JKL".ToCharArray()).Count() > 0)
            {
                SegmentUtils.GetNearestSegment(position, out _, out _, out ushort targetSegmentId);
                format = format.Replace("J", (targetSegmentId % 10).ToString("0"));
                format = format.Replace("K", (targetSegmentId % 100).ToString("00"));
                format = format.Replace("L", (targetSegmentId % 1000).ToString("000"));
            }

            return format;
        }
        #endregion
    }
}
