using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace RimVore2
{
    public class ThinkNode_HasFulfilledVoreMentalStateTargetCount : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if(!(pawn.MentalState is MentalState_VoreTargeter mentalState))
            {
                return false;
            }
            return mentalState.HasVoredEnoughTargets;
        }
    }
}
