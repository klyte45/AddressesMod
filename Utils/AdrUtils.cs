using Klyte.Addresses.Xml;
using Klyte.Commons.Utils;
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
                if (!GetStreetAndNumber(sidewalk, midPosBuilding, out a, out b))
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
                e = AdrController.CurrentConfig.GlobalConfig.AddressingConfig.TokenizedPostalCodeFormat.TokenToPostalCode(sidewalk);
            }
            ParseToFormatableString(ref format, 5);

            addressLines = string.Format(format, a, b, c, d, e).Split("≠".ToCharArray());
        }

        internal static bool GetStreetAndNumber(Vector3 sidewalk, Vector3 midPosBuilding, out string streetName, out int number)
        {
            SegmentUtils.GetNearestSegment(sidewalk, out Vector3 targetPosition, out float targetLength, out ushort targetSegmentId);
            if (targetSegmentId == 0)
            {
                streetName = string.Empty;
                number = 0;
                return false;
            }
            var seed = NetManager.instance.m_segments.m_buffer[targetSegmentId].m_nameSeed;
            var invertStart = false;
            var offsetMeters = 0;
            if (AdrNameSeedDataXml.Instance.NameSeedConfigs.TryGetValue(seed, out AdrNameSeedConfig seedConf))
            {
                invertStart = seedConf.InvertMileageStart;
                offsetMeters = (int)seedConf.MileageOffset;
            }

            return SegmentUtils.GetAddressStreetAndNumber(targetPosition, targetSegmentId, targetLength, midPosBuilding, invertStart, offsetMeters, out number, out streetName);
        }

        private static void ParseToFormatableString(ref string input, byte count)
        {
            for (int i = 0; i < count; i++)
            {
                input = input.Replace(((char)('A' + i)).ToString(), $"{{{i}}}");
            }
        }
        #endregion
    }
}
