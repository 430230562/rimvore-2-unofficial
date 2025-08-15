using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace RimVore2
{
    public class ThinkNode_IsCurrentlyVored : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            return GlobalVoreTrackerUtility.GetVoreRecord(pawn) != null;
        }
    }
}
