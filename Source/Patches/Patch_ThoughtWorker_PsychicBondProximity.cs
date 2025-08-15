#if !v1_3
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2.Patches
{
    [HarmonyPatch(typeof(ThoughtWorker_PsychicBondProximity), nameof(ThoughtWorker_PsychicBondProximity.NearPsychicBondedPerson))]
    public static class Patch_ThoughtWorker_PsychicBondProximity
    {
        [HarmonyPostfix]
        public static void ConsiderVoredPawns(ref bool __result, Pawn pawn, Hediff_PsychicBond bondHediff)
        {
            if (__result)
            {
                return;
            }
            if(!(bondHediff.target is Pawn bondedPawn))
            {
                return;
            }
            VoreTracker tracker = pawn.PawnData()?.VoreTracker;
            if(tracker == null)
            {
                return;
            }
            IEnumerable<VoreTrackerRecord> records = tracker.AllRecordsIncludingNested();
            bool isBondedPawnVoredOrNestedVored = records.Any(record => record.Prey == bondedPawn);
            if (!isBondedPawnVoredOrNestedVored)
            {
                return;
            }
            __result = true;
        }
    }
}
#endif