using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimVore2
{
    [HarmonyPatch(typeof(JobDriver_Vomit), "MakeNewToils")]
    public class Patch_JobDriver_Vomit
    {
        [HarmonyPostfix]
        private static IEnumerable<Toil> EjectPreyViaVomit(IEnumerable<Toil> __result, JobDriver_Vomit __instance)
        {
            foreach(Toil toil in __result)
            {
                yield return toil;
            }
            Toil ejectToil = null;
            try
            {
                Pawn pawn = __instance.pawn;
                List<VoreTrackerRecord> records = pawn.PawnData()?.VoreTracker?.VoreTrackerRecords;
                if(records == null)
                {
                    yield break;
                }
                List<VoreTrackerRecord> vomitableRecords = records
                    .FindAll(record => record.CurrentBodyPart.def == VoreBodyPartDefOf.Stomach || record.CurrentBodyPart.def == VoreBodyPartDefOf.Jaw); // anything in stomach / mouth
                if(vomitableRecords.NullOrEmpty())
                {
                    yield break;
                }
                foreach(VoreTrackerRecord record in vomitableRecords)
                {
                    if(RV2Log.ShouldLog(false, "Jobs"))
                        RV2Log.Message($"Adding vomit ejection for prey {record.Prey}", "Jobs");
                    ejectToil = Toil_Vore.EjectToil(pawn, pawn, record.Prey, true);
                }
            }
            catch(Exception e)
            {
                RV2Log.Warning("RimVore-2: Something went wrong " + e, "Jobs");
                yield break;
            }
            if(ejectToil != null)
            {
                yield return ejectToil;
            }
        }
    }
}