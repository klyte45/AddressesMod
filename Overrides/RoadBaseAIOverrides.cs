using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using ColossalFramework.UI;
using ICities;
using Klyte.Addresses.Utils;
using Klyte.Extensions;
using Klyte.Harmony;
using Klyte.TransportLinesManager.Extensors;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Klyte.Addresses.Overrides
{
    class RoadBaseAIOverrides : Redirector<RoadBaseAIOverrides>
    {


        #region Mod

        private static bool GenerateStreetName(ref Randomizer r, ref string __result)
        {
            int range = AddressesMod.roadLocale.Length;
            if (range == 0)
            {
                return true;
            }
            int idx = r.Int32((uint)range);
            __result = AddressesMod.roadLocale[idx];
            return false;
        }
        #endregion

        #region Hooking

        public override void Awake()
        {
            AdrUtils.doLog("Loading RoadBaseAI Overrides");
            #region RoadBaseAI Hooks
            MethodInfo preRename = typeof(RoadBaseAIOverrides).GetMethod("GenerateStreetName", allFlags);

            AddRedirect(typeof(RoadBaseAI).GetMethod("GenerateStreetName", allFlags), preRename);
            #endregion
        }
        #endregion



    }
}
