using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;
using RimWorld;

namespace RimVore2
{
    /// <remarks
    /// Copied over from the mod JecsTools - which is not officially supported any more
    /// </remarks>
    [HarmonyPatch(typeof(PawnGenerator), "GeneratePawn", new[] { typeof(PawnGenerationRequest) })]
    static class RV2_Patch_GeneratePawn
    {
        [HarmonyPostfix]
        private static void RV2_CallSpawnWithHediffGivers(ref Pawn __result)
        {
            try
            {
                IEnumerable<HediffGiver_StartWithHediff> startGivers = __result?.def?.race?.hediffGiverSets?
                    .SelectMany(set => set.hediffGivers)
                    .Where(hediffGiver => hediffGiver is HediffGiver_StartWithHediff)
                    .Cast<HediffGiver_StartWithHediff>();

                if(startGivers.EnumerableNullOrEmpty())
                {
                    return;
                }
                if(RV2Log.ShouldLog(true, "PawnGeneration"))
                    RV2Log.Message("HediffGivers for spawn with hediff found: " + string.Join(", ", startGivers.Select(g => g.hediff.defName)), false, "PawnGeneration");
                foreach(HediffGiver_StartWithHediff giver in startGivers)
                {
                    giver.TryApply(__result);
                }
            }
            catch(Exception e)
            {
                Log.Warning("RimVore-2: Something went wrong when trying to create a pawn with a starting hediff, Error:\n" + e);
            }
        }

        [HarmonyPostfix]
        public static void RV2_AddForcedHediffsFromBackstory(ref Pawn __result)
        {
            __result.TryGetRV2Backstory(out RV2_BackstoryDef adultBackstory, out RV2_BackstoryDef childBackstory);
            AddHediffsForBackstory(__result, adultBackstory);
            AddHediffsForBackstory(__result, childBackstory);
        }
        private static void AddHediffsForBackstory(Pawn pawn, RV2_BackstoryDef backstory)
        {
            if(backstory == null)
            {
                return;
            }
            foreach(HediffDef hediff in backstory.forcedHediffs)
            {
                pawn.health.AddHediff(hediff);
            }
        }

        [HarmonyPostfix]
        private static void RV2_EnforceGenitalsForRV2Backstory(ref Pawn __result)
        {
            Pawn pawn = __result;
            try
            {
                // if pawn has a RV2 back story, make sure they have the genitals needed to fit into the backstory
                if(BackstoryUtility.TryGetRV2Backstory(pawn, out RV2_BackstoryDef adultBackstory, out RV2_BackstoryDef childBackstory))
                {
                    if(adultBackstory != null)
                    {
                        adultBackstory.ApplyForcedGenitals(pawn);
                    }
                    if(childBackstory != null)
                    {
                        childBackstory.ApplyForcedGenitals(pawn);
                    }
                }
            }
            catch(Exception e)
            {
                Log.Warning("RimVore-2: Something went wrong when trying to force genitals in accordance to RV2_backstory, Error:\n" + e);
            }
        }
    }
}