using Klyte.Commons.Extensions;
using Klyte.Commons.Utils;
using System.Reflection;
using UnityEngine;

namespace Klyte.Addresses.Overrides
{
    internal class FootballOverrides : MonoBehaviour, IRedirectable
    {
        public Redirector RedirectorInstance { get; } = new Redirector();
        #region Mod
        public static bool GetFootballOpponentName(SportMatchAI __instance, ushort eventID, ref EventData data, ref string __result)
        {
            if (!(__instance is SportMatchAI) || AdrController.CurrentConfig?.GlobalConfig?.FootballConfig?.OpponentNamesFile is null)
            {
                return true;
            }
            var nameList = AdrController.LoadedLocalesFootballTeams[AdrController.CurrentConfig?.GlobalConfig?.FootballConfig?.OpponentNamesFile];
            __result = nameList[(int)data.m_customSeed % nameList.Length].Name;
            return false;
        }
        public static bool GetFootballOpponentColor(SportMatchAI __instance, ushort eventID, ref EventData data, ref Color __result)
        {
            if (!(__instance is SportMatchAI) || AdrController.CurrentConfig?.GlobalConfig?.FootballConfig?.OpponentNamesFile is null)
            {
                return true;
            }
            var nameList = AdrController.LoadedLocalesFootballTeams[AdrController.CurrentConfig?.GlobalConfig?.FootballConfig?.OpponentNamesFile];
            __result = nameList[(int)data.m_customSeed % nameList.Length].Color;
            return false;
        }
        public static bool GetLocalTeamName(SportMatchAI __instance, ushort eventID, ref EventData data, ref string __result) =>
            !(__instance is SportMatchAI) 
            || !AdrController.CurrentConfig.GlobalConfig.FootballConfig.localTeamsNames.TryGetValue(data.m_building, out __result);

        #endregion

        #region Hooking
        public void Awake()
        {
            LogUtils.DoLog("Loading SportMatchAI Overrides");
            #region SportMatchAI Hooks
            MethodInfo nameOver = typeof(FootballOverrides).GetMethod("GetFootballOpponentName", RedirectorUtils.allFlags);
            MethodInfo colorOver = typeof(FootballOverrides).GetMethod("GetFootballOpponentColor", RedirectorUtils.allFlags);
            MethodInfo nameOrig = typeof(SportMatchAI).GetMethod("GetOpponentName", RedirectorUtils.allFlags);
            MethodInfo colorOrig = typeof(SportMatchAI).GetMethod("GetOpponentColor", RedirectorUtils.allFlags);
            MethodInfo localNameOver = typeof(FootballOverrides).GetMethod("GetLocalTeamName", RedirectorUtils.allFlags);
            MethodInfo localNameOrig = typeof(SportMatchAI).GetMethod("GetTeamName", RedirectorUtils.allFlags);
            LogUtils.DoLog($"Overriding GetOpponentName ({nameOrig} => {nameOver})");
            RedirectorInstance.AddRedirect(nameOrig, nameOver);
            LogUtils.DoLog($"Overriding GetOpponentColor ({colorOrig} => {colorOver})");
            RedirectorInstance.AddRedirect(colorOrig, colorOver);
            LogUtils.DoLog($"Overriding GetTeamName ({localNameOrig} => {localNameOver})");
            RedirectorInstance.AddRedirect(localNameOrig, localNameOver);
            #endregion
        }

        #endregion



    }
}
