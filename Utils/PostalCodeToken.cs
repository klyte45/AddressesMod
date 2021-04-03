using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Klyte.Addresses.Utils
{
    public enum PostalCodeToken
    {
        NONE,
        NAMESEED_D5,
        NAMESEED_D4,
        NAMESEED_D3,
        NAMESEED_D2,
        NAMESEED_D1,
        SEGMENT_D5,
        SEGMENT_D4,
        SEGMENT_D3,
        SEGMENT_D2,
        SEGMENT_D1,
        CITY_PRE_D3,
        CITY_PRE_D2,
        CITY_PRE_D1,
        DISTRICT_PRE_D3,
        DISTRICT_PRE_D2,
        DISTRICT_PRE_D1,
        CX_I,
        CX_D,
        CY_I,
        CY_D,
    }

    public class PostalCodeTokenContainer
    {
        public PostalCodeToken token = PostalCodeToken.NONE;
        public string fixedText;
    }



    public static class PostalCodeTokensExtensions
    {
        public static string AsString(this PostalCodeToken token)
        {
            switch (token)
            {
                default:
                case PostalCodeToken.NONE: return "";
                case PostalCodeToken.NAMESEED_D5: return "A";
                case PostalCodeToken.NAMESEED_D4: return "B";
                case PostalCodeToken.NAMESEED_D3: return "C";
                case PostalCodeToken.NAMESEED_D2: return "D";
                case PostalCodeToken.NAMESEED_D1: return "E";
                case PostalCodeToken.SEGMENT_D5: return "F";
                case PostalCodeToken.SEGMENT_D4: return "G";
                case PostalCodeToken.SEGMENT_D3: return "H";
                case PostalCodeToken.SEGMENT_D2: return "I";
                case PostalCodeToken.SEGMENT_D1: return "J";
                case PostalCodeToken.CITY_PRE_D3: return "K";
                case PostalCodeToken.CITY_PRE_D2: return "L";
                case PostalCodeToken.CITY_PRE_D1: return "M";
                case PostalCodeToken.DISTRICT_PRE_D3: return "N";
                case PostalCodeToken.DISTRICT_PRE_D2: return "O";
                case PostalCodeToken.DISTRICT_PRE_D1: return "P";
                case PostalCodeToken.CX_I: return "X";
                case PostalCodeToken.CX_D: return "x";
                case PostalCodeToken.CY_I: return "Y";
                case PostalCodeToken.CY_D: return "y";
            }
        }

        public static PostalCodeTokenContainer[] TokenizeAsPostalCode(this string format)
        {
            bool intoQuote = false;
            bool escapeNext = false;
            string currentQuoteData = null;
            List<PostalCodeTokenContainer> result = new List<PostalCodeTokenContainer>();
            foreach (var letter in format)
            {
                if (escapeNext)
                {
                    escapeNext = false;
                    if (result[result.Count - 1].token == PostalCodeToken.NONE)
                    {
                        result[result.Count - 1].fixedText += letter;
                    }
                    else
                    {
                        result.Add(new PostalCodeTokenContainer
                        {
                            fixedText = letter.ToString()
                        });
                    }
                    continue;
                }
                if (intoQuote)
                {
                    if (letter == '\\')
                    {
                        escapeNext = true;
                        continue;
                    }
                    if (letter == '"')
                    {
                        intoQuote = false;
                        result.Add(new PostalCodeTokenContainer
                        {
                            fixedText = currentQuoteData,
                            token = PostalCodeToken.NONE
                        });
                        currentQuoteData = null;
                        continue;
                    }
                    currentQuoteData += letter;
                    continue;
                }
                switch (letter)
                {
                    case '"':
                        currentQuoteData = "";
                        intoQuote = true;
                        continue;
                    case '\\':
                        escapeNext = true;
                        continue;
                    case 'A': result.Add(new PostalCodeTokenContainer { token = PostalCodeToken.NAMESEED_D5 }); break;
                    case 'B': result.Add(new PostalCodeTokenContainer { token = PostalCodeToken.NAMESEED_D4 }); break;
                    case 'C': result.Add(new PostalCodeTokenContainer { token = PostalCodeToken.NAMESEED_D3 }); break;
                    case 'D': result.Add(new PostalCodeTokenContainer { token = PostalCodeToken.NAMESEED_D2 }); break;
                    case 'E': result.Add(new PostalCodeTokenContainer { token = PostalCodeToken.NAMESEED_D1 }); break;
                    case 'F': result.Add(new PostalCodeTokenContainer { token = PostalCodeToken.SEGMENT_D5 }); break;
                    case 'G': result.Add(new PostalCodeTokenContainer { token = PostalCodeToken.SEGMENT_D4 }); break;
                    case 'H': result.Add(new PostalCodeTokenContainer { token = PostalCodeToken.SEGMENT_D3 }); break;
                    case 'I': result.Add(new PostalCodeTokenContainer { token = PostalCodeToken.SEGMENT_D2 }); break;
                    case 'J': result.Add(new PostalCodeTokenContainer { token = PostalCodeToken.SEGMENT_D1 }); break;
                    case 'K': result.Add(new PostalCodeTokenContainer { token = PostalCodeToken.CITY_PRE_D3 }); break;
                    case 'L': result.Add(new PostalCodeTokenContainer { token = PostalCodeToken.CITY_PRE_D2 }); break;
                    case 'M': result.Add(new PostalCodeTokenContainer { token = PostalCodeToken.CITY_PRE_D1 }); break;
                    case 'N': result.Add(new PostalCodeTokenContainer { token = PostalCodeToken.DISTRICT_PRE_D3 }); break;
                    case 'O': result.Add(new PostalCodeTokenContainer { token = PostalCodeToken.DISTRICT_PRE_D2 }); break;
                    case 'P': result.Add(new PostalCodeTokenContainer { token = PostalCodeToken.DISTRICT_PRE_D1 }); break;
                    case 'X': result.Add(new PostalCodeTokenContainer { token = PostalCodeToken.CX_I }); break;
                    case 'x': result.Add(new PostalCodeTokenContainer { token = PostalCodeToken.CX_D }); break;
                    case 'Y': result.Add(new PostalCodeTokenContainer { token = PostalCodeToken.CY_I }); break;
                    case 'y': result.Add(new PostalCodeTokenContainer { token = PostalCodeToken.CY_D }); break;
                    default:
                        if (result[result.Count - 1].token == PostalCodeToken.NONE)
                        {
                            result[result.Count - 1].fixedText += letter;
                        }
                        else
                        {
                            result.Add(new PostalCodeTokenContainer
                            {
                                fixedText = letter.ToString()
                            });
                        }
                        break;
                }
            }
            return result.ToArray();
        }
        public static string AsPostalCodeString(this PostalCodeTokenContainer[] tokenList)
        {
            var result = "";
            foreach (var token in tokenList)
            {
                if (token.token == PostalCodeToken.NONE)
                {
                    if ("ABCDEFGHIJKLMNOPXxYy\"\\".ToCharArray().Intersect(token.fixedText.ToCharArray()).Any())
                    {
                        result += token.fixedText.Length == 1 ? $"\\{token.fixedText}" : $"\"{token.fixedText.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"";
                    }
                    else
                    {
                        result += token.fixedText;
                    }

                }
                else
                {
                    result += token.token.AsString();
                }
            }
            return result;
        }
        public static string TokenToPostalCode(this PostalCodeTokenContainer[] tokenList, Vector3 position)
        {
            string result = "";

            string cachedSeedName = null;
            string cachedSegmentId = null;
            string cachedCityPre = null;
            string cachedDistrictPre = null;
            string cachedCx = null;
            string cachedCy = null;

            string seedName()
            {
                if (cachedSeedName == null)
                {
                    SegmentUtils.GetNearestSegment(position, out _, out _, out ushort targetSegmentId);
                    cachedSeedName = NetManager.instance.m_segments.m_buffer[targetSegmentId].m_nameSeed.ToString("00000");
                }
                return cachedSeedName;
            }
            string segmentId()
            {
                if (cachedSegmentId == null)
                {
                    SegmentUtils.GetNearestSegment(position, out _, out _, out ushort targetSegmentId);
                    cachedSegmentId = targetSegmentId.ToString("00000");
                }
                return cachedSegmentId;
            }
            string cityPre() => cachedCityPre ?? (cachedCityPre = AdrController.CurrentConfig.GlobalConfig.AddressingConfig.ZipcodeCityPrefix.ToString("000"));
            string distPre()
            {
                if (cachedDistrictPre == null)
                {
                    int district = DistrictManager.instance.GetDistrict(position) & 0xff;
                    cachedDistrictPre = (AdrController.CurrentConfig.GetConfigForDistrict((ushort)district).PostalCodePrefix).ToString("000");
                }
                return cachedDistrictPre;
            }

            string cx()
            {
                if (cachedCx == null)
                {
                    Vector2 tilePos = MapUtils.GetMapTile(position);
                    cachedCx = Math.Floor(tilePos.x * 10).ToString("00");
                    cachedCy = Math.Floor(tilePos.y * 10).ToString("00");
                }
                return cachedCx;
            }
            string cy()
            {
                if (cachedCy == null)
                {
                    Vector2 tilePos = MapUtils.GetMapTile(position);
                    cachedCx = Math.Floor(tilePos.x * 10).ToString("00");
                    cachedCy = Math.Floor(tilePos.y * 10).ToString("00");
                }
                return cachedCy;
            }

            foreach (var token in tokenList)
            {
                switch (token.token)
                {
                    case PostalCodeToken.NONE: result += token.fixedText; break;
                    case PostalCodeToken.NAMESEED_D5: result += seedName()[0]; break;
                    case PostalCodeToken.NAMESEED_D4: result += seedName()[1]; break;
                    case PostalCodeToken.NAMESEED_D3: result += seedName()[2]; break;
                    case PostalCodeToken.NAMESEED_D2: result += seedName()[3]; break;
                    case PostalCodeToken.NAMESEED_D1: result += seedName()[4]; break;
                    case PostalCodeToken.SEGMENT_D5: result += segmentId()[0]; break;
                    case PostalCodeToken.SEGMENT_D4: result += segmentId()[1]; break;
                    case PostalCodeToken.SEGMENT_D3: result += segmentId()[2]; break;
                    case PostalCodeToken.SEGMENT_D2: result += segmentId()[3]; break;
                    case PostalCodeToken.SEGMENT_D1: result += segmentId()[4]; break;
                    case PostalCodeToken.CITY_PRE_D3: result += cityPre()[0]; break;
                    case PostalCodeToken.CITY_PRE_D2: result += cityPre()[1]; break;
                    case PostalCodeToken.CITY_PRE_D1: result += cityPre()[2]; break;
                    case PostalCodeToken.DISTRICT_PRE_D3: result += distPre()[0]; break;
                    case PostalCodeToken.DISTRICT_PRE_D2: result += distPre()[1]; break;
                    case PostalCodeToken.DISTRICT_PRE_D1: result += distPre()[2]; break;
                    case PostalCodeToken.CX_I: result += cx()[0]; break;
                    case PostalCodeToken.CX_D: result += cx()[1]; break;
                    case PostalCodeToken.CY_I: result += cy()[0]; break;
                    case PostalCodeToken.CY_D: result += cy()[1]; break;
                }
            }

            return result;
        }
    }
}
