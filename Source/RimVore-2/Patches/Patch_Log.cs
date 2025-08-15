using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimVore2
{
    [HarmonyPatch(typeof(Log), "Clear")]
    public class Patch_Log
    {
        [HarmonyPostfix]
        private static void ClearMessageOnceCache()
        {
            try
            {
                RV2Log.UsedKeys.Clear();
            }
            catch(Exception e)
            {
                Log.Warning("RimVore-2: Something went wrong " + e);
                return;
            }
        }
    }
}