using HarmonyLib;
using RimVore2;
using rjw;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RV2_RJW
{
    [HarmonyPatch(typeof(Verb_VoreGrapple), "IsUsableOn")]
    public static class Patch_Verb_VoreGrapple
    {
        [HarmonyPostfix]
        private static void DisallowDuringRapeBeating_IsUsableOn(ref bool __result, Verb_VoreGrapple __instance, Thing target)
        {
            if(__result == false)
                return;
            // rape beating uses the base pool for verbs, by default we should disable grapple moves for those
            if(!RV2_RJW_Settings.rjw.DisableVoreGrappleDuringSex)
                return;
            Pawn targetPawn = target as Pawn;
            Pawn caster = __instance.CasterPawn;
            if(targetPawn == null)
                return;
            if(JobUtility.HasSexJob(caster))
            {
                if(RV2Log.ShouldLog(true, "VoreCombatGrapple"))
                    RV2Log.Message($"{caster.LabelShort} - Grapple not usable, caster is currently having sex", false, "VoreCombatGrapple");
                __result = false;
                return;
            }
            if(JobUtility.HasSexJob(targetPawn))
            {
                if(RV2Log.ShouldLog(true, "VoreCombatGrapple"))
                    RV2Log.Message($"{target.LabelShort} - Grapple not usable, target is currently having sex", false, "VoreCombatGrapple");
                __result = false;
                return;
            }
        }
    }

    [HarmonyPatch(typeof(Verb_VoreGrapple), "Available")]
    public static class Patch_Verb_VoreGrapple_Available 
    { 
        [HarmonyPostfix]
        private static void DisallowDuringRapeBeating_Available(ref bool __result, Verb_VoreGrapple __instance)
        {
            if(__result == false)
                return;
            // rape beating uses the base pool for verbs, by default we should disable grapple moves for those
            if(!RV2_RJW_Settings.rjw.DisableVoreGrappleDuringSex)
                return;
            Pawn caster = __instance.CasterPawn;
            if(JobUtility.HasSexJob(caster))
            {
                if(RV2Log.ShouldLog(true, "VoreCombatGrapple"))
                    RV2Log.Message($"{caster.LabelShort} - Grapple not available, caster is currently having sex", false, "VoreCombatGrapple");
                __result = false;
                return;
            }
        }
    }
}
