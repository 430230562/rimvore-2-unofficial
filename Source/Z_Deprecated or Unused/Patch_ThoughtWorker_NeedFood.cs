/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;
using RimWorld;

namespace RimVore2
{
    [HarmonyPatch(typeof(ThoughtWorker_NeedFood), "CurrentStateInternal")]
    static class RV2_Patch_ThoughtWorker_NeedFood
    {
        [HarmonyPostfix]
        static void RV2_BlockHungryIfVoreFeeds(Pawn p, ref ThoughtState __result)
        {
            ThoughtState backupResult = __result;
            try
            {
                // Log.Message("Pawn hunger injection " + p?.Label);
                if(p?.IsTrackingVore() == true)
                {
                    // Log.Message("tracking vore");
                    VoreTracker tracker = GlobalVoreTrackerUtility.GetTrackerForPredator(p);
                    
                    if(tracker.TrackedVores.Any(record => record.AnyFutureStageFeedsPredator))
                    {
                        // Log.Message("stage with feeding worker exists, setting NeedFood to Inactive");
                        __result = ThoughtState.Inactive;
                    }
                }
            }
            catch(Exception e)
            {
                Log.Error("There was an error whilst trying to intercept hungry thoughts if the pawn is actively digesting a pawn, error: " + e);
                __result = backupResult;
            }
        }
    }
}*/