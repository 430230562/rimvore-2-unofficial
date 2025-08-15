using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace RimVore2
{
    public abstract class RollAction_Steal : RollAction
    {
        protected VoreRole takeFrom = VoreRole.Invalid;
        protected VoreRole giveTo = VoreRole.Invalid;

        protected Pawn TakerPawn => record.GetPawnByRole(giveTo);
        protected Pawn GiverPawn => record.GetPawnByRole(takeFrom);

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if(takeFrom == VoreRole.Invalid)
            {
                yield return "required field \"takeFrom\" must be set";
            }
            if(giveTo == VoreRole.Invalid)
            {
                yield return "required field \"giveTo\" must be set";
            }
            if(takeFrom != VoreRole.Invalid && takeFrom == giveTo) // no reason to present message if both giveTo and takeFrom are not set
            {
                yield return "can't use same role in fields \"takeFrom\" and \"giveTo\"";
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref takeFrom, "takeFrom");
            Scribe_Values.Look(ref giveTo, "giveTo");
        }
    }
}
