using ColossalFramework;
using ColossalFramework.Math;
using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        private static List<ushort> calculatePathNet(ushort segmentID, bool startSegment, bool strict)
        {
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
            calculatePathNet(segmentID, segmentID, edgeNode, startSegment, ref result, strict);
            return result;
        }


        private static void calculatePathNet(ushort firstSegmentID, ushort segmentID, ushort nodeCurr, bool insertOnEnd, ref List<ushort> segmentOrder, bool strict)
        {
            ushort otherEdgeNode = 0;
            List<ushort> possibilities = new List<ushort>();
            int crossingDirectionsCounter = 0;
            for (int i = 0; i < 8; i++)
            {
                ushort segment = NetManager.instance.m_nodes.m_buffer[nodeCurr].GetSegment(i);

                if (segment != 0 && firstSegmentID != segment && segment != segmentID && !segmentOrder.Contains(segment) && IsSameName(segmentID, segment, false))
                {
                    crossingDirectionsCounter++;
                    if (strict && !IsSameName(segmentID, segment, strict))
                    {
                        continue;
                    }
                    possibilities.Add(segment);
                }
            }
            if (strict && crossingDirectionsCounter > 1)
            {
                return;
            }
            if (possibilities.Count > 0)
            {
                ushort segment = possibilities.Min();
                otherEdgeNode = NetManager.instance.m_segments.m_buffer[segment].GetOtherNode(nodeCurr);
                if (insertOnEnd || segmentOrder.Count == 0)
                {
                    segmentOrder.Add(segment);
                }
                else
                {
                    segmentOrder.Insert(0, segment);
                }
                calculatePathNet(firstSegmentID, segment, otherEdgeNode, insertOnEnd, ref segmentOrder, strict);
            }
        }
        public static List<ushort> getSegmentOrderRoad(ushort segmentID, bool strict, out bool startRef, out bool endRef)
        {
            var flags = NetManager.instance.m_segments.m_buffer[segmentID].m_flags;
            if (segmentID != 0 && flags != NetSegment.Flags.None)
            {
                List<ushort> path = calculatePathNet(segmentID, false, strict);
                path.Add(segmentID);

                ushort startNode0 = NetManager.instance.m_segments.m_buffer[path[0]].m_startNode;
                ushort endNode0 = NetManager.instance.m_segments.m_buffer[path[0]].m_endNode;
                ushort startNodeRef = NetManager.instance.m_segments.m_buffer[segmentID].m_startNode;
                ushort endNodeRef = NetManager.instance.m_segments.m_buffer[segmentID].m_endNode;

                bool circular = (path.Count > 2 && (startNode0 == endNodeRef || startNode0 == startNodeRef || endNode0 == endNodeRef || endNode0 == startNodeRef)) ||
                    (path.Count == 2 && (startNode0 == endNodeRef || startNode0 == startNodeRef) && (endNode0 == endNodeRef || endNode0 == startNodeRef));

                if (circular)
                {
                    AdrUtils.doLog("Circular!");
                    var refer = path.Min();
                    var referIdx = path.IndexOf(refer);
                    if (referIdx != 0)
                    {
                        path = path.GetRange(referIdx, path.Count - referIdx).Union(path.Take(referIdx)).ToList();
                    }
                }
                else
                {
                    path.AddRange(calculatePathNet(segmentID, true, strict));
                }
                AdrUtils.doLog($"[s={strict}]path = [{string.Join(",", path.Select(x => x.ToString()).ToArray())}]");
                GetEdgeNodes(ref path, out startRef, out endRef, strict);
                return path;
            }
            startRef = false;
            endRef = false;
            return null;
        }

        public static void GetClosestPositionAndDirectionAndPoint(NetSegment s, Vector3 point, out Vector3 pos, out Vector3 dir, out float length)
        {
            Bezier3 curve = default(Bezier3);
            curve.a = Singleton<NetManager>.instance.m_nodes.m_buffer[(int)s.m_startNode].m_position;
            curve.d = Singleton<NetManager>.instance.m_nodes.m_buffer[(int)s.m_endNode].m_position;
            bool smoothStart = (Singleton<NetManager>.instance.m_nodes.m_buffer[(int)s.m_startNode].m_flags & NetNode.Flags.Middle) != NetNode.Flags.None;
            bool smoothEnd = (Singleton<NetManager>.instance.m_nodes.m_buffer[(int)s.m_endNode].m_flags & NetNode.Flags.Middle) != NetNode.Flags.None;
            NetSegment.CalculateMiddlePoints(curve.a, s.m_startDirection, curve.d, s.m_endDirection, smoothStart, smoothEnd, out curve.b, out curve.c);
            float closestDistance = 1E+11f;
            float pointPerc = 0f;
            Vector3 targetA = curve.a;
            for (int i = 1; i <= 16; i++)
            {
                Vector3 vector = curve.Position((float)i / 16f);
                float dist = Segment3.DistanceSqr(targetA, vector, point, out float distSign);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    pointPerc = ((float)i - 1f + distSign) / 16f;
                }
                targetA = vector;
            }
            float precision = 0.03125f;
            for (int j = 0; j < 4; j++)
            {
                Vector3 a2 = curve.Position(Mathf.Max(0f, pointPerc - precision));
                Vector3 vector2 = curve.Position(pointPerc);
                Vector3 b = curve.Position(Mathf.Min(1f, pointPerc + precision));
                float num6 = Segment3.DistanceSqr(a2, vector2, point, out float num7);
                float num8 = Segment3.DistanceSqr(vector2, b, point, out float num9);
                if (num6 < num8)
                {
                    pointPerc = Mathf.Max(0f, pointPerc - precision * (1f - num7));
                }
                else
                {
                    pointPerc = Mathf.Min(1f, pointPerc + precision * num9);
                }
                precision *= 0.5f;
            }
            pos = curve.Position(pointPerc);
            dir = VectorUtils.NormalizeXZ(curve.Tangent(pointPerc));
            length = pointPerc;
        }

        public static bool IsSameName(ushort segment1, ushort segment2, bool strict = false)
        {
            NetManager nm = NetManager.instance;
            NetInfo info = nm.m_segments.m_buffer[(int)segment1].Info;
            NetInfo info2 = nm.m_segments.m_buffer[(int)segment2].Info;
            if (info.m_class.m_service != info2.m_class.m_service)
            {
                return false;
            }
            if (info.m_class.m_subService != info2.m_class.m_subService)
            {
                return false;
            }
            if (strict)
            {
                var seg1 = nm.m_segments.m_buffer[(int)segment1];
                var seg2 = nm.m_segments.m_buffer[(int)segment2];
                if ((seg1.m_endNode == seg2.m_endNode || seg1.m_startNode == seg2.m_startNode) && (nm.m_segments.m_buffer[(int)segment1].m_flags & NetSegment.Flags.Invert) == (nm.m_segments.m_buffer[(int)segment2].m_flags & NetSegment.Flags.Invert))
                {
                    return false;
                }
                if ((seg1.m_endNode == seg2.m_startNode || seg1.m_startNode == seg2.m_endNode) && (nm.m_segments.m_buffer[(int)segment1].m_flags & NetSegment.Flags.Invert) != (nm.m_segments.m_buffer[(int)segment2].m_flags & NetSegment.Flags.Invert))
                {
                    return false;
                }
                if (!((info.m_forwardVehicleLaneCount == info2.m_backwardVehicleLaneCount && info2.m_forwardVehicleLaneCount == info.m_backwardVehicleLaneCount)
                    || (info2.m_forwardVehicleLaneCount == info.m_forwardVehicleLaneCount && info2.m_backwardVehicleLaneCount == info.m_backwardVehicleLaneCount)))
                {
                    return false;
                }
                if (((RoadBaseAI)info.GetAI()).m_highwayRules != ((RoadBaseAI)info2.GetAI()).m_highwayRules)
                {
                    return false;
                }
            }
            bool customName1 = (nm.m_segments.m_buffer[(int)segment1].m_flags & NetSegment.Flags.CustomName) != NetSegment.Flags.None;
            bool customName2 = (nm.m_segments.m_buffer[(int)segment2].m_flags & NetSegment.Flags.CustomName) != NetSegment.Flags.None;
            if (customName1 != customName2)
            {
                return false;
            }
            if (customName1)
            {
                InstanceID id = default(InstanceID);
                id.NetSegment = segment1;
                string name = Singleton<InstanceManager>.instance.GetName(id);
                id.NetSegment = segment2;
                string name2 = Singleton<InstanceManager>.instance.GetName(id);
                return name == name2;
            }
            ushort nameSeed = nm.m_segments.m_buffer[(int)segment1].m_nameSeed;
            ushort nameSeed2 = nm.m_segments.m_buffer[(int)segment2].m_nameSeed;
            return nameSeed == nameSeed2;
        }

        public static void GetSegmentRoadEdges(ushort segmentId, bool strict, out ComparableRoad startRef, out ComparableRoad endRef)
        {
            List<ushort> accessSegments = getSegmentOrderRoad(segmentId, strict, out bool nodeStartS, out bool nodeStartE);
            AdrUtils.doLog($"[{segmentId}-> s: {strict}] segs = [{string.Join(",", accessSegments.Select(x => x.ToString()).ToArray())}]; start={nodeStartS}; end={nodeStartE}");
            startRef = new ComparableRoad(accessSegments[0], nodeStartS);
            endRef = new ComparableRoad(accessSegments[accessSegments.Count - 1], nodeStartE);
        }

        private static List<ushort> GetEdgeNodes(ref List<ushort> accessSegments, out bool nodeStartS, out bool nodeStartE, bool strict)
        {
            if (accessSegments.Count > 1)
            {
                var seg0 = NetManager.instance.m_segments.m_buffer[accessSegments[0]];
                var seg1 = NetManager.instance.m_segments.m_buffer[accessSegments[1]];
                var segN2 = NetManager.instance.m_segments.m_buffer[accessSegments[accessSegments.Count - 2]];
                var segN1 = NetManager.instance.m_segments.m_buffer[accessSegments[accessSegments.Count - 1]];
                nodeStartS = seg1.m_startNode == seg0.m_endNode || seg1.m_endNode == seg0.m_endNode;
                nodeStartE = segN2.m_startNode == segN1.m_endNode || segN2.m_endNode == segN1.m_endNode;
                if (strict)
                {
                    if (nodeStartS == ((NetManager.instance.m_segments.m_buffer[accessSegments[0]].m_flags & NetSegment.Flags.Invert) != 0))
                    {
                        accessSegments.Reverse();
                        var temp = nodeStartS;
                        nodeStartS = nodeStartE;
                        nodeStartE = temp;
                    }
                }
                else
                {
                    var nodeF = nodeStartS ? seg0.m_startNode : seg0.m_endNode;
                    var nodeL = nodeStartE ? segN1.m_startNode : segN1.m_endNode;
                    if (VectorUtils.XZ(NetManager.instance.m_nodes.m_buffer[nodeF].m_position).GetAngleToPoint(VectorUtils.XZ(NetManager.instance.m_nodes.m_buffer[nodeL].m_position)) > 180)
                    {
                        accessSegments.Reverse();
                        var temp = nodeStartS;
                        nodeStartS = nodeStartE;
                        nodeStartE = temp;
                    }
                }
            }
            else
            {
                nodeStartS = (NetManager.instance.m_segments.m_buffer[accessSegments[0]].m_flags & NetSegment.Flags.Invert) == 0;
                nodeStartE = (NetManager.instance.m_segments.m_buffer[accessSegments[0]].m_flags & NetSegment.Flags.Invert) != 0;
            }

            return accessSegments;
        }

        public struct ComparableRoad
        {
            public ComparableRoad(ushort segmentId, bool startNode)
            {
                var segment = NetManager.instance.m_segments.m_buffer[segmentId];
                if (startNode)
                {
                    nodeReference = segment.m_startNode;
                }
                else
                {
                    nodeReference = segment.m_endNode;
                }

                bool entering = startNode != ((NetManager.instance.m_segments.m_buffer[segmentId].m_flags & NetSegment.Flags.Invert) != 0);

                var node = NetManager.instance.m_nodes.m_buffer[nodeReference];
                isPassing = false;
                segmentReference = 0;
                for (int i = 0; i < 7; i++)
                {
                    var segment1 = node.GetSegment(i);
                    if (segment1 > 0 && segment1 != segmentId)
                    {
                        for (int j = i + 1; j < 8; j++)
                        {
                            var segment2 = node.GetSegment(j);
                            if (segment2 > 0 && segment2 != segmentId)
                            {
                                isPassing = IsSameName(segment1, segment2, true);
                                if (isPassing)
                                {
                                    segmentReference = segment1;
                                    break;
                                }
                            }
                        }
                        if (isPassing) break;
                    }
                }
                if (!isPassing)
                {
                    
                    for (int i = 0; i < 7; i++)
                    {
                        var segment1 = node.GetSegment(i);
                        if (segment1 > 0 && segment1 != segmentId)
                        {
                            var segment1Obj = NetManager.instance.m_segments.m_buffer[segment1];
                            bool isSegment1StartNode = segment1Obj.m_startNode == nodeReference;
                            bool isSegment1Entering = isSegment1StartNode != ((NetManager.instance.m_segments.m_buffer[segment1].m_flags & NetSegment.Flags.Invert) != 0);

                            if (!(segment1Obj.Info.GetAI() is RoadBaseAI seg1Ai)) continue;
                            bool isSegment1TwoWay = segment1Obj.Info.m_hasBackwardVehicleLanes && segment1Obj.Info.m_hasForwardVehicleLanes;

                            if (!isSegment1TwoWay && isSegment1Entering == entering)
                            {
                                AdrUtils.doLog($"IGNORED: {segment1} (Tw=>{isSegment1TwoWay},entering=>{entering},s1entering=>{isSegment1Entering})");
                                continue;
                            }

                            if (segmentReference == 0)
                            {
                                segmentReference = segment1;
                            }
                            else
                            {
                                NetSegment segmentRefObj = NetManager.instance.m_segments.m_buffer[segmentReference];
                                if (!((RoadBaseAI)segmentRefObj.Info.GetAI()).m_highwayRules && seg1Ai.m_highwayRules)
                                {
                                    segmentReference = segment1;
                                    continue;
                                }
                                if (((RoadBaseAI)segmentRefObj.Info.GetAI()).m_highwayRules && !seg1Ai.m_highwayRules)
                                {
                                    continue;
                                }
                                int laneCount1 = (segment1Obj.Info.m_forwardVehicleLaneCount + segment1Obj.Info.m_backwardVehicleLaneCount);
                                int laneCountRef = (segmentRefObj.Info.m_forwardVehicleLaneCount + segmentRefObj.Info.m_backwardVehicleLaneCount);
                                if (laneCount1 > laneCountRef)
                                {
                                    segmentReference = segment1;
                                    continue;
                                }
                                if (laneCount1 < laneCountRef)
                                {
                                    continue;
                                }
                                if (segment1Obj.Info.m_halfWidth > segmentRefObj.Info.m_halfWidth)
                                {
                                    segmentReference = segment1;
                                    continue;
                                }
                            }
                        }
                    }
                }
                if (segmentReference > 0)
                {
                    var segmentRefObj = NetManager.instance.m_segments.m_buffer[segmentReference];
                    NetInfo infoRef = segmentRefObj.Info;
                    RoadBaseAI aiRef = (RoadBaseAI)infoRef.GetAI();
                    isHighway = aiRef.m_highwayRules;
                    width = infoRef.m_halfWidth * 2;
                    lanes = infoRef.m_backwardVehicleLaneCount + infoRef.m_forwardVehicleLaneCount;
                }
                else
                {
                    isHighway = false;
                    width = 0;
                    lanes = 0;
                }
            }

            public readonly ushort nodeReference;
            public readonly ushort segmentReference;
            public readonly bool isHighway;
            public readonly bool isPassing;
            public readonly float width;
            public readonly int lanes;

            public int compareTo(ComparableRoad otherRoad)
            {
                int result = 0;
                if (otherRoad.isHighway != isHighway)
                {
                    result = isHighway ? 1 : -1;
                }
                if (otherRoad.isPassing != isPassing)
                {
                    result = isPassing ? 1 : -1;
                }
                if (otherRoad.width != width)
                {
                    result = otherRoad.width < width ? 1 : -1;
                }
                if (otherRoad.lanes != lanes)
                {
                    result = otherRoad.lanes < lanes ? 1 : -1;
                }
                AdrUtils.doLog($"cmp: {this} & {otherRoad} => {result}");
                return result;
            }

            public override string ToString()
            {
                return $"[n{nodeReference} s{segmentReference} h:{isHighway} p:{isPassing} w{width} l{lanes}]";
            }
        }

        #endregion

        private static ushort[] m_closestSegsFind = new ushort[16];
        #region Addresses

        public static void getAddressLines(Vector3 sidewalk, Vector3 midPosBuilding, out String[] addressLines)
        {
            addressLines = null;
            GetNearestSegment(sidewalk, out Vector3 targetPosition, out float targetLength, out ushort targetSegmentId);
            if (targetSegmentId == 0)
            {
                return;
            }

            int number = GetNumberAt(targetLength, targetSegmentId, out bool startAsEnd);

            string streetName = NetManager.instance.GetSegmentName(targetSegmentId)?.ToString();

            float angleTg = VectorUtils.XZ(targetPosition).GetAngleToPoint(VectorUtils.XZ(midPosBuilding));
            if (angleTg == 90 || angleTg == 270)
            {
                angleTg += Math.Sign(targetPosition.z - midPosBuilding.z);
            }

            NetSegment targSeg = NetManager.instance.m_segments.m_buffer[targetSegmentId];
            Vector3 startSeg = NetManager.instance.m_nodes.m_buffer[targSeg.m_startNode].m_position;
            Vector3 endSeg = NetManager.instance.m_nodes.m_buffer[targSeg.m_endNode].m_position;

            float angleSeg = Vector2.zero.GetAngleToPoint(VectorUtils.XZ(endSeg - startSeg));
            if (angleSeg == 180)
            {
                angleSeg += Math.Sign(targetPosition.z - midPosBuilding.z);
            }


            //AdrUtils.doLog($"angleTg = {angleTg};angleSeg = {angleSeg}");
            if ((angleTg + 90) % 360 < 180 ^ (angleSeg < 180 ^ startAsEnd))
            {
                number &= ~1;
            }
            else
            {
                number |= 1;
            }
            //AdrUtils.doLog($"number = {number} A");
            int districtId = GetDistrict(midPosBuilding);
            string districtName = "";
            if (districtId > 0)
            {
                districtName = DistrictManager.instance.GetDistrictName(districtId) + " - ";
            }

            addressLines = new String[] { streetName + "," + number, districtName + SimulationManager.instance.m_metaData.m_CityName, formatPostalCode(sidewalk, AdrConfigWarehouse.getCurrentConfigString(AdrConfigWarehouse.ConfigIndex.ZIPCODE_FORMAT)) };
        }

        private static int GetNumberAt(float targetLength, ushort targetSegmentId, out bool startAsEnd)
        {
            //AdrUtils.doLog($"targets = S:{targetSegmentId} P:{targetPosition} D:{targetDirection.magnitude} L:{targetLength}");

            List<ushort> roadSegments = AdrUtils.getSegmentOrderRoad(targetSegmentId, false, out bool startRef, out bool endRef);
            //AdrUtils.doLog("roadSegments = [{0}] ", string.Join(",", roadSegments.Select(x => x.ToString()).ToArray()));
            int targetSegmentIdIdx = roadSegments.IndexOf(targetSegmentId);
            //AdrUtils.doLog($"targSeg = {targetSegmentIdIdx} ({targetSegmentId})");
            var targSeg = NetManager.instance.m_segments.m_buffer[targetSegmentId];
            NetSegment preSeg = default(NetSegment);
            NetSegment posSeg = default(NetSegment);
            //AdrUtils.doLog("PreSeg");
            if (targetSegmentIdIdx > 0)
            {
                preSeg = NetManager.instance.m_segments.m_buffer[roadSegments[targetSegmentIdIdx - 1]];
            }
            //AdrUtils.doLog("PosSeg");
            if (targetSegmentIdIdx < roadSegments.Count - 1)
            {
                posSeg = NetManager.instance.m_segments.m_buffer[roadSegments[targetSegmentIdIdx + 1]];
            }
            //AdrUtils.doLog("startAsEnd");
            startAsEnd = (targetSegmentIdIdx > 0 && (preSeg.m_endNode == targSeg.m_endNode || preSeg.m_startNode == targSeg.m_endNode)) || (targetSegmentIdIdx < roadSegments.Count && (posSeg.m_endNode == targSeg.m_startNode || posSeg.m_startNode == targSeg.m_startNode));

            //AdrUtils.doLog($"startAsEnd = {startAsEnd}");

            if (startAsEnd)
            {
                targetLength = 1 - targetLength;
            }
            //AdrUtils.doLog("streetName"); 

            //AdrUtils.doLog("distanceFromStart");
            float distanceFromStart = 0;
            for (int i = 0; i < targetSegmentIdIdx; i++)
            {
                distanceFromStart += NetManager.instance.m_segments.m_buffer[roadSegments[i]].m_averageLength;
            }
            distanceFromStart += targetLength * targSeg.m_averageLength;
            return (int)Math.Round(distanceFromStart);

            //AdrUtils.doLog($"number = {number} B");
        }



        public static int GetNumberAt(ushort targetSegmentId, bool startNode)
        {
            List<ushort> roadSegments = AdrUtils.getSegmentOrderRoad(targetSegmentId, false, out bool startRef, out bool endRef);
            int targetSegmentIdIdx = roadSegments.IndexOf(targetSegmentId);
            var targSeg = NetManager.instance.m_segments.m_buffer[targetSegmentId];
            ushort targetNode = startNode ? targSeg.m_startNode : targSeg.m_endNode;
            NetSegment preSeg = default(NetSegment);
            if (targetSegmentIdIdx > 0)
            {
                preSeg = NetManager.instance.m_segments.m_buffer[roadSegments[targetSegmentIdIdx - 1]];
            }
            if (preSeg.m_endNode != targetNode && preSeg.m_startNode != targetNode)
            {
                targetSegmentIdIdx++;
            }
            float distanceFromStart = 0;
            for (int i = 0; i < targetSegmentIdIdx; i++)
            {
                distanceFromStart += NetManager.instance.m_segments.m_buffer[roadSegments[i]].m_averageLength;
            }
            return (int)Math.Round(distanceFromStart);
        }

        private static void GetNearestSegment(Vector3 sidewalk, out Vector3 targetPosition, out float targetLength, out ushort targetSegmentId)
        {
            NetManager.instance.GetClosestSegments(sidewalk, m_closestSegsFind, out int found);
            targetPosition = default(Vector3);
            Vector3 targetDirection = default(Vector3);
            targetLength = 0;
            targetSegmentId = 0;
            if (found == 0)
            {
                return;
            }
            else if (found > 1)
            {
                float minSqrDist = float.MaxValue;
                for (int i = 0; i < found; i++)
                {
                    ushort segId = m_closestSegsFind[i];
                    var seg = NetManager.instance.m_segments.m_buffer[segId];
                    if (!(seg.Info.GetAI() is RoadBaseAI)) continue;
                    GetClosestPositionAndDirectionAndPoint(seg, sidewalk, out Vector3 position, out Vector3 direction, out float length);
                    float sqrDist = Vector3.SqrMagnitude(sidewalk - position);
                    if (i == 0 || sqrDist < minSqrDist)
                    {
                        minSqrDist = sqrDist;
                        targetPosition = position;
                        targetDirection = direction;
                        targetLength = length;
                        targetSegmentId = segId;
                    }
                }
            }
            else
            {
                targetSegmentId = m_closestSegsFind[0];
                GetClosestPositionAndDirectionAndPoint(NetManager.instance.m_segments.m_buffer[targetSegmentId], sidewalk, out targetPosition, out targetDirection, out targetLength);
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
