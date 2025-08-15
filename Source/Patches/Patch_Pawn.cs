using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RimVore2
{
    [HarmonyPatch(typeof(Pawn), "Tick")]
    static class RV2_Patch_Pawn_Tick
    {
        [HarmonyPostfix]
        private static void RV2_PredatorTick(ref Pawn __instance)
        {
            try
            {
                //if(!Settings_General.PassNormalTicksDuringVore)
                if(!RV2Mod.Settings.debug.PassPredatorTicks)
                {
                    return;
                }
                if(__instance == null)
                {
                    return;
                }
                if(__instance.IsActivePredator())
                {
                    __instance.PawnData()?.VoreTracker?.Tick();
                }
            }
            catch(Exception e)
            {
                Log.Warning("RimVore-2: Something went wrong when trying to execute tick for predator, Error:\n" + e);
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), "TickRare")]
    static class RV2_Patch_Pawn_Tick_Rare
    {
        [HarmonyPostfix]
        private static void RV2_PredatorTickRare(ref Pawn __instance)
        {
            try
            {
                // mod "character editor" calls tick for null pawn
                if(__instance == null)
                {
                    return;
                }
                __instance.PawnData()?.VoreTracker?.TickRare();
                // tick temporary quirks
                if(RV2Mod.Settings.features.VoreQuirksEnabled)
                {
                    __instance.QuirkManager(false)?.Tick();
                }
            }
            catch(Exception e)
            {
                Log.Warning("RimVore-2: Something went wrong when trying to execute rare tick for predator, Error:\n" + e);
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), "Kill")]
    static class RV2_Patch_Pawn_Kill
    {
        [HarmonyPrefix]
        private static bool RV2_PredatorKillHook(ref Pawn __instance, Hediff exactCulprit)
        {
            Pawn pawn = __instance;
            try
            {
                pawn.PawnData()?.VoreTracker?.EjectAll(true);
                //VoreFeedUtility.UnReserve(pawn);
                /// if the game has decicded that a vore hediff is responsible for killing, skip the whole kill logic. Another hediff will be blamed, which means we prevent
                /// death messages like "cause of death: prey in penis"
                if(exactCulprit is Hediff_ContainingPrey)
                {
                    return false;
                }
                return true;
            }
            catch(Exception e)
            {
                Log.Warning("RimVore-2: Something went wrong during the RV-2 kill-hook, Error:\n" + e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), "Destroy")]
    static class RV2_Patch_Pawn_Destroy
    {
        [HarmonyPrefix]
        private static void RV2_PredatorDestroyHook(ref Pawn __instance)
        {
            Pawn pawn = __instance;
            try
            {
                pawn.PawnData()?.VoreTracker?.EjectAll(true);
                //VoreFeedUtility.UnReserve(pawn);
            }
            catch(Exception e)
            {
                Log.Warning("RimVore-2: Something went wrong when trying to make sure the killed pawn ejects all their prey if they are a predator, Error:\n" + e);
            }
        }
    }
}
