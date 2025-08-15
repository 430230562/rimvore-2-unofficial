using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimVore2
{
    [HarmonyPatch(typeof(Recipe_InstallImplant), "ApplyOnPawn")]
    public class Path_Recipe_InstallImplant_ApplyOnPawn
    {
        [HarmonyPostfix]
        private static void ThrowVoreImplantEventIfApplicable(Pawn pawn, Pawn billDoer, RecipeDef ___recipe)
        {
            try
            {
                if(___recipe.addsHediff?.HasVoreEnabler() == true)
                {
                    Find.HistoryEventsManager.RecordEvent(new HistoryEvent(IdeologyVoreEventDefOf.RV2_InstalledVoreEnabler, billDoer.Named(HistoryEventArgsNames.Doer), pawn.Named(HistoryEventArgsNames.Victim)), true);
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