using DubsBadHygiene;
using HarmonyLib;
using RimVore2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RV2_DBH
{
    [HarmonyPatch(typeof(JobGiver_UseToilet), "GetPriority")]
    internal static class Patch_JobGiver_UseToilet
    {
        [HarmonyPostfix]
        public static void IncreasePriorityIfDumpablePrey(ref float __result, Pawn pawn)
        {
            try
            {
                VoreTracker tracker = pawn.PawnData().VoreTracker;
                if(tracker == null)
                {
                    return;
                }
                bool validForDisposal = tracker.HasPreyReadyToRelease
                    && tracker.VoreTrackerRecords.Any(record => Common.ValidForToiletDisposal(record));
                if(validForDisposal)
                {
                    if(RV2Log.ShouldLog(false, "DubsBadHygiene")) 
                        RV2Log.Message($"Forced high priority for JobGiver_UseToilet because {pawn.LabelShort} has prey to release", "DubsBadHygiene");
                    __result = 50f;
                }
                return;
            }
            catch(Exception e)
            {
                Log.Error("Something went wrong when trying to increase priority for UseToilet: " + e);
            }
        }
    }
}
