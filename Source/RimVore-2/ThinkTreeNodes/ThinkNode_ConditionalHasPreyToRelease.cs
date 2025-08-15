using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace RimVore2
{
    class ThinkNode_ConditionalHasPreyToRelease : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            PawnData pawnData = pawn.PawnData();
            VoreTracker tracker = pawnData.VoreTracker;
            if(tracker == null)
            {
                return false;
            }
            return tracker.HasPreyReadyToRelease;
        }
    }
}
