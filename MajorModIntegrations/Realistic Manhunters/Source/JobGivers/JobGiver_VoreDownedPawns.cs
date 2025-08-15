using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using RimVore2;
using RealisticManhunters;

namespace RV2_ReMa
{
    public class JobGiver_VoreDownedPawns : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            Job job = RimVore2.Patch_JobGiver_Manhunter.TryVoreDownedPawns(pawn);
            return job;
        }
    }
}
