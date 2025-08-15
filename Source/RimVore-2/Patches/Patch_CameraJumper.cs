using HarmonyLib;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    [HarmonyPatch(typeof(CameraJumper))]
    public static class Patch_CameraJumper
    {
        [HarmonyPatch("GetAdjustedTarget")]
        [HarmonyPrefix]
        public static bool RedirectToPrey(GlobalTargetInfo target, ref GlobalTargetInfo __result)
        {
            if(!target.HasThing)
            {
                return true;
            }
            if(!(target.Thing is Pawn pawn))
            {
                return true;
            }
            if(!GlobalVoreTrackerUtility.IsActivePrey(pawn))
            {
                return true;
            }
            __result = pawn;
            return false;
        }
    }
}
