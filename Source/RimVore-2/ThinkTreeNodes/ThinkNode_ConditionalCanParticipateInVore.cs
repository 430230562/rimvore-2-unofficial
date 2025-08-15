using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace RimVore2
{
    public class ThinkNode_ConditionalCanParticipateInVore : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if(RV2Mod.Settings.rules.AllowedInAutoVore(pawn))
            {
                return false;
            }
            // _ discards out result
            return pawn.CanParticipateInVore(out _);
        }
    }
}
