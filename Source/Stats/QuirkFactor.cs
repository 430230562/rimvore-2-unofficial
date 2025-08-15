using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public abstract class QuirkFactor
    {
        public abstract bool CanGetFactor(Pawn pawn);
        public abstract float GetFactor(Pawn pawn, ModifierOperation operation);
        public abstract float ModifyValue(Pawn pawn, float value);
        public abstract string GetDescription(Pawn pawn);

        public abstract IEnumerable<string> ConfigErrors();
    }
}
