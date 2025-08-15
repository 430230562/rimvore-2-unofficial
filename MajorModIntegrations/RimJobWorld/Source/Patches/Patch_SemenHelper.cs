using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using rjw;
using RimVore2;
using Verse;
using Verse.AI;

namespace RV2_RJW
{
    //[HarmonyPatch(typeof(SemenHelper), "cumOn")]
    public static class Patch_SemenHelper
    {
        private static HediffDef cumReservesHediffDef = HediffDef.Named("RV2_CumReserves");

        //[HarmonyPrefix]
        public static void AddCumReservesIfAvailable(ref float amount, Pawn giver)
        {
            try
            {
                Hediff cumReserves = giver?.health?.hediffSet?.hediffs?
                    .FirstOrDefault(hed => hed.def == cumReservesHediffDef);
                if(cumReserves == null)
                {
                    return;
                }
                if(RV2Log.ShouldLog(false, "RJW"))
                    RV2Log.Message($"Pawn {giver.LabelShort} has cum reserves at {cumReserves.Severity} severity, using those to increase the previous amount of {amount}", "RJW");
                amount += cumReserves.Severity;
                amount = amount.LimitClamp(0, 1);
                giver.health.RemoveHediff(cumReserves);
            }
            catch(Exception e)
            {
                Log.Warning("RimVore-2: Something went wrong when checking for cum reserves to apply during RJW cumOn method: " + e);
                return;
            }
        }
    }
}