using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace RimVore2
{
    [HarmonyPatch(typeof(WorldPawnGC), "GetCriticalPawnReason")]
    public class Patch_WorldPawnGC_GetCriticalPawnReason
    {
        [HarmonyPostfix]
        private static void FlagVoringPawnsAsCritical(Pawn pawn, ref string __result)
        {
            try
            {
                // nothing to do if pawn is already considered critical
                if(__result != null)
                {
                    return;
                }
                // if pawn is currently vored, consider their existance critical, preventing GC actions
                if(pawn.IsActivePrey())
                {
                    RV2Log.Warning($"Prevented pawn {pawn.LabelShort} from being removed in GC while being an active prey");
                    __result = "CurrentlyVored";
                }
                // if pawn is currently voring, consider their existance critical, preventing GC actions
                if(pawn.IsActivePredator())
                {
                    RV2Log.Warning($"Prevented pawn {pawn.LabelShort} from being removed in GC while being an active predator");
                    __result = "CurrentlyVoring";
                }
            }
            catch(Exception e)
            {
                Log.Warning("RimVore-2: Something went wrong " + e);
                return;
            }
        }
    }
}