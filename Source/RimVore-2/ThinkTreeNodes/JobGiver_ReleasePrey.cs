using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;


namespace RimVore2
{
    public class ThinkNode_JobGiver_ReleasePrey : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            PawnData pawnData = pawn.PawnData();
            VoreTrackerRecord record = pawnData.VoreTracker.VoreTrackerRecords
                .FindAll(r => r.HasReachedEnd)
                .RandomElement();
            bool isProduct = record.VorePath.def.voreProduct?.ValidContainers?.Count() > 0;
            IntVec3 targetPosition = PositionUtility.GetPreyReleasePosition(pawn, record.Prey, isProduct);
            if(RV2Log.ShouldLog(false, "Jobs"))
                RV2Log.Message($"Calculated target position {targetPosition}", "Jobs");
            Job releaseJob = JobMaker.MakeJob(VoreJobDefOf.RV2_ReleasePrey, targetPosition, record.Prey);
            return releaseJob;
        }
    }
}
