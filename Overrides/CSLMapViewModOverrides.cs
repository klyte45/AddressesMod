using Harmony;
using Klyte.Addresses.ModShared;
using Klyte.Commons.Extensions;
using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Klyte.Addresses.Overrides
{
    internal class CSLMapViewModOverrides : Redirector, IRedirectable
    {
        public void Awake()
        {
            LogUtils.DoLog("Loading CSLMapViewModOverrides Overrides");

            try
            {
                MethodInfo transpileMethod = typeof(CSLMapViewModOverrides).GetMethod("ExportSegmentsTranspile", RedirectorUtils.allFlags);
                MethodInfo transpileMethodBuilding = typeof(CSLMapViewModOverrides).GetMethod("ExportBuildingTranspile", RedirectorUtils.allFlags);
                MethodInfo ExportSegments = Type.GetType("CSLMod.CSLMapView.Modding.Exporter.MapInfoExporter, CSLMapViewMod")?.GetMethod("ExportSegments", RedirectorUtils.allFlags);
                MethodInfo ExportBuilding = Type.GetType("CSLMod.CSLMapView.Modding.Exporter.MapInfoExporter, CSLMapViewMod")?.GetMethod("ExportBuilding", RedirectorUtils.allFlags);
                if (ExportSegments != null && ExportBuilding != null)
                {
                    LogUtils.DoLog($"Overriding GetName ({ExportSegments} => {transpileMethod})");
                    AddRedirect(ExportSegments, null, null, transpileMethod);
                    LogUtils.DoLog($"Overriding GetName ({ExportBuilding} => {transpileMethodBuilding})");
                    AddRedirect(ExportBuilding, null, null, transpileMethodBuilding);
                }
            }
            catch (Exception e)
            {
                LogUtils.DoErrorLog($"Error transpiling CSLMapView: {e}");
            }

        }

        public static IEnumerable<CodeInstruction> ExportBuildingTranspile(ILGenerator il, IEnumerable<CodeInstruction> instr)
        {
            var instrList = new List<CodeInstruction>(instr);
            var trLbl = il.DefineLabel();
            for (int i = 3; i < instrList.Count - 2; i++)
            {
                if (instrList[i].opcode == OpCodes.Brfalse
                    && instrList[i - 2].opcode == OpCodes.Ldc_I4_8)
                {
                    instrList[i + 1].labels.Add(trLbl);
                    var codeList = new List<CodeInstruction>
                            {
                                 new CodeInstruction(OpCodes.Brtrue_S, trLbl),
                                 new CodeInstruction(OpCodes.Ldloca_S,3)  ,
                                 new CodeInstruction(OpCodes.Call, typeof(AdrFacade).GetMethod("IsAutonameAvailable", RedirectorUtils.allFlags) ),
                            };

                    instrList.InsertRange(i, codeList);


                    break;
                }

            }
            LogUtils.PrintMethodIL(instrList);

            return instrList;
        }
        public static IEnumerable<CodeInstruction> ExportSegmentsTranspile(ILGenerator il, IEnumerable<CodeInstruction> instr)
        {
            var instrList = new List<CodeInstruction>(instr);
            var outLbl = il.DefineLabel();
            Label ifLbl;
            for (int i = 3; i < instrList.Count - 2; i++)
            {
                if (instrList[i].opcode == OpCodes.Brfalse
                    && instrList[i - 2].opcode == OpCodes.Ldc_I4
                    && (int)instrList[i - 2].operand == 0x800_0000)
                {
                    ifLbl = (Label)instrList[i].operand;
                    for (int j = i; j < instrList.Count - 2; j++)
                    {
                        if (instrList[j].labels.Contains(ifLbl))
                        {
                            instrList[j].labels.Remove(ifLbl);
                            instrList[j].labels.Add(outLbl);
                            var instr2 = new CodeInstruction(OpCodes.Ldloc, 6);
                            instr2.labels.Add(ifLbl);
                            var codeList = new List<CodeInstruction>
                            {
                                 new CodeInstruction(OpCodes.Br_S, outLbl),
                                 instr2,
                                 new CodeInstruction(OpCodes.Ldloc_0)  ,
                                 new CodeInstruction(OpCodes.Call, typeof(AdrFacade).GetMethod("GetStreetFull", RedirectorUtils.allFlags) ),
                                 instrList[j-1]
                            };

                            instrList.InsertRange(j, codeList);
                            break;
                        }
                    }
                    break;
                }

            }
            LogUtils.PrintMethodIL(instrList);

            return instrList;
        }


    }
}
