using ColossalFramework;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                if (AddressesMod.instance != null)
                {
                    if (AddressesMod.debugMode)
                    {
                        Debug.LogWarningFormat("AdrV" + AddressesMod.version + " " + format, args);
                    }
                }
                else
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

        #region Districts Utils
        public static Dictionary<string, int> getValidDistricts()
        {
            Dictionary<string, int> districts = new Dictionary<string, int>
            {
                [$"<{Singleton<SimulationManager>.instance.m_metaData.m_CityName}>"] = 0
            };
            for (int i = 1; i <= 0x7F; i++)
            {
                if ((Singleton<DistrictManager>.instance.m_districts.m_buffer[i].m_flags & District.Flags.Created) != District.Flags.None)
                {
                    String districtName = Singleton<DistrictManager>.instance.GetDistrictName(i);
                    if (districts.ContainsKey(districtName))
                    {
                        districtName += $" (ID:{i})";
                    }
                    districts[districtName] = i;
                }
            }

            return districts;
        }
        #endregion

        #region Segment Utils
        public static void UpdateSegmentNamesView()
        {
            if (Singleton<NetManager>.exists)
            {
                var nm = Singleton<NetManager>.instance;

                HashSet<ushort> segments = new HashSet<ushort>();

                for (ushort i = 1; i < nm.m_segments.m_size; i++)
                {
                    if (nm.m_segments.m_buffer[i].m_flags != 0)
                    {
                        nm.UpdateSegmentRenderer(i, false);
                        segments.Add(i);
                    }
                }
                nm.m_updateNameVisibility = segments;
            }
        }

        private static List<ushort> calculatePathNet(ushort segmentID, bool startSegment)
        {
            Dictionary<ushort, int> nodeHistory = new Dictionary<ushort, int>();
            List<ushort> result = new List<ushort>();
            ushort edgeNode;
            if (startSegment)
            {
                edgeNode = NetManager.instance.m_segments.m_buffer[segmentID].m_startNode;
            }
            else
            {
                edgeNode = NetManager.instance.m_segments.m_buffer[segmentID].m_endNode;
            }
            calculatePathNet(segmentID, segmentID, edgeNode, startSegment, ref nodeHistory, ref result);
            return result;
        }


        private static void calculatePathNet(ushort firstSegmentID, ushort segmentID, ushort nodeCurr, bool insertOnEnd, ref Dictionary<ushort, int> nodeHistory, ref List<ushort> segmentOrder)
        {
            ushort otherEdgeNode = 0;
            while (nodeHistory[nodeCurr] < 8)
            {
                ushort segment = NetManager.instance.m_nodes.m_buffer[nodeCurr].GetSegment(nodeHistory[nodeCurr]++);
                if (segment != 0 && firstSegmentID != segment && segment != segmentID && NetManager.instance.IsSameName(segmentID, segment))
                {
                    otherEdgeNode = NetManager.instance.m_segments.m_buffer[segment].GetOtherNode(nodeCurr);
                    if (insertOnEnd || segmentOrder.Count == 0)
                    {
                        segmentOrder.Add(segment);
                    }
                    else
                    {
                        segmentOrder.Insert(0, segment);
                    }
                    calculatePathNet(firstSegmentID, segment, otherEdgeNode, insertOnEnd, ref nodeHistory, ref segmentOrder);
                    break;
                }
            }
        }


        public static List<ushort> getNodeOrderRoad(ushort segmentID)
        {
            var flags = NetManager.instance.m_segments.m_buffer[segmentID].m_flags;
            if (segmentID != 0 && flags != NetSegment.Flags.None)
            {
                List<ushort> path = calculatePathNet(segmentID, true);
                path.Add(segmentID);
                path.AddRange(calculatePathNet(segmentID, false));
                if (path.Last() < path.First())
                {
                    path.Reverse();
                }
                return path;
            }
            return null;
        }
        #endregion
    }
}
