/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using HarmonyLib;
using Verse;

namespace RimVore2.Patches
{
    [HarmonyPatch(typeof(Need_Food), "NeedInterval")]
    public static class Patch_Need_Food
    {
        [HarmonyPostfix]
        private static void OffsetFoodFallDuringFeedingVore(ref Need_Food __instance, ref Pawn ___pawn)
        {
            try
            {
                if(___pawn.IsTrackingVore())
                {
                    if(GlobalVoreTrackerUtility.GetTrackerForPredator(___pawn).TrackedVores.Any(record => record.VorePath.def.feedsPredator))
                    {
                        RV2Log.Message("Counteracting food fall because vore feeds predator", true, false, "OngoingVore");
                        __instance.CurLevel += __instance.FoodFallPerTick * 150;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Warning("RimVore-2: Something went wrong when trying to intercept the the food fall need for pawn: " + e);
            }
        }
    }
}*/