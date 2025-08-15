    using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimVore2
{
    /// <summary>
    /// Base game is very very odd with how it calculates verb usage likelihood.
    /// There are three verb categories: Best, Mid, Worst
    /// All verbs weight (really just DPS) is calculated, then all verbs are categorized: >0.95 of max weight means best category, verbs below 0.25 of max weight receive 0 weight and will never be used, the rest is "mid"
    /// There is a 75% chance to use a best verb and a 25% chance to use a mid verb. After the category is selected *all* verbs in that category have the same likelihood of being picked
    /// 
    /// So allowing all pawns to do grapple but not do it too often is practically impossible on a weighted approach. My preferred solution for now is to just force a hard weight, which base game will adapt to and calculate the "remaining" weights
    /// This is a relatively mediocre approach, but it's the most simple and understandable one - grapples just have a flat chance of being used on ANY pawn
    /// </summary>
    [HarmonyPatch(typeof(VerbEntry))]
    [HarmonyPatch(MethodType.Constructor)]
    [HarmonyPatch(new Type[] { typeof(Verb), typeof(Pawn), typeof(List<Verb>), typeof(float) })]
    public class Patch_VerbEntry_ctor
    {
        [HarmonyPostfix]
        private static void OverwriteCachedWeight(VerbEntry __instance, ref float ___cachedSelectionWeight, Pawn pawn, float highestSelWeight)
        {
            try
            {
                if(__instance.verb?.tool?.capacities?.Contains(RV2_Common.VoreGrappleToolCapacity) != true)
                    return;

                ___cachedSelectionWeight = CombatUtility.GetGrappleChance(pawn, true);
            }
            catch(Exception e)
            {
                Log.Warning("RimVore-2: Something went wrong trying to calculate grapple chance: " + e);
                return;
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_MeleeVerbs), "ChooseMeleeVerb")]
    public class Patch_VerbEntry_ChooseMeleeVerb
    {
        [HarmonyPostfix]
        private static void LogVerbSelection(Thing target, Pawn_MeleeVerbs __instance, Verb ___curMeleeVerb)
        {
            try
            {
                // extra check to spare the game from having to calculate the string if logging is disabled
                if(!RV2Mod.Settings.debug.VerboseLogging)
                    return;
                List<VerbEntry> updatedAvailableVerbsList = __instance.GetUpdatedAvailableVerbsList(false);
                if(RV2Log.ShouldLog(true, "VoreCombatGrapple"))
                    RV2Log.Message($"{__instance.Pawn.LabelShort} picked verb {___curMeleeVerb}. Selection with weights:\n{string.Join("\n", updatedAvailableVerbsList.Select(v => $" - {v.verb} - {v.GetSelectionWeight(target)} - usable? {v.verb.IsUsableOn(target)}"))}", false, "VoreCombatGrapple");
            }
            catch(Exception e)
            {
                Log.Warning("Something went wrong when trying to log out additional information for verb selection: " + e);
            }
        }
    }
}