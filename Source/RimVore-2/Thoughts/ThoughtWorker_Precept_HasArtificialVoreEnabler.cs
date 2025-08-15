using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class ThoughtWorker_Precept_HasArtificialVoreEnabler : ThoughtWorker_Precept
    {
        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            return p.health?.hediffSet?.hediffs?.Any(hed => hed.def.HasVoreEnabler()) == true;
        }
    }
    public class ThoughtWorker_Precept_HasArtificialVoreEnabler_Social : ThoughtWorker_Precept_Social
    {
        protected override ThoughtState ShouldHaveThought(Pawn p, Pawn otherPawn)
        {
            return otherPawn.health?.hediffSet?.hediffs?.Any(hed => hed.def.HasVoreEnabler()) == true;
        }
    }
}