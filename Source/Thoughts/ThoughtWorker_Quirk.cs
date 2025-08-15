using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class ThoughtWorker_Quirk : ThoughtWorker
    {
        VoreQuirkThoughtDef quirkThoughtDef => (VoreQuirkThoughtDef)base.def;

        protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
        {
            if(pawn.QuirkManager(false) == null)
            {
                return false;
            }
            if(!AppliesToPawns(pawn, other))
            {
                return false;
            }
            return true;
        }

        private bool AppliesToPawns(Pawn thisPawn, Pawn otherPawn)
        {
            if(!quirkThoughtDef.thisPawnQuirks.NullOrEmpty())
            {
                if(!QuirkUtility.HasAllQuirks(thisPawn, quirkThoughtDef.thisPawnQuirks))
                {
                    return false;
                }
            }
            if(!quirkThoughtDef.otherPawnQuirks.NullOrEmpty())
            {
                if(!QuirkUtility.HasAllQuirks(otherPawn, quirkThoughtDef.otherPawnQuirks))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
