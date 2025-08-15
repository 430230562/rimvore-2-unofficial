using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimVore2
{
    [HarmonyPatch(typeof(PawnUtility), "TrySpawnHatchedOrBornPawn")]
    public class Patch_PawnUtility_TrySpawnhatchedOrBornPawn
    {
        [HarmonyPostfix]
        private static void InterceptBirthOfVoredPawn(ref bool __result, ref Pawn pawn, ref Thing motherOrEgg)
        {
            try
            {
                if(!__result)
                {
                    return;
                }
                if(!(motherOrEgg is Pawn))
                {
                    return;
                }
                // GetValidAge will return -1 if no age check exists. In case an age check exists, we don't intercept newborns!
                if(RV2Mod.Settings.rules.GetValidAge(pawn, RuleTargetRole.All) > -1)
                {
                    return;
                }
                Pawn parent = (Pawn)motherOrEgg;
                VoreTrackerRecord record = parent.GetVoreRecord();
                if(record == null)
                {
                    return;
                }
                VoreTracker tracker = record.Predator.PawnData().VoreTracker;
                tracker.SplitOffNewVore(record, pawn, null, record.VorePathIndex);
                return;
            }
            catch(Exception e)
            {
                Log.Warning("RimVore-2: Something went wrong when attempting to intercept birth of a pawn: " + e);
                return;
            }
        }
    }

    [HarmonyPatch(typeof(PawnUtility), "ShouldCollideWithPawns")]
    public class Patch_PawnUtility_ShouldCollideWithPawns
    {
        [HarmonyPostfix]
        private static void InterceptGrappleCollisions(ref bool __result, ref Pawn p)
        {
            try
            {
                if(__result == false)  // nothing to do here
                    return;
                // grappling pawns can collide with each other (and everyone else by extension, but I can't change that easily)
                __result = !CombatUtility.IsInvolvedInGrapple(p);
            }
            catch(Exception e)
            {
                Log.Warning("RimVore-2: Something went wrong when allowing collisions for grappled pawns: " + e);
                return;
            }
        }
    }
}