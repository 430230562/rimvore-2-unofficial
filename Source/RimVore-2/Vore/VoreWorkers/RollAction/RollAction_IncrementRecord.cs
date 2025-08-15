using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace RimVore2
{
    public class RollAction_IncrementRecord : RollAction
    {

#pragma warning disable CS0649 // assigned from DEF
        private RecordDef recordDef;
#pragma warning restore CS0649 // assigned from DEF

        protected virtual RecordDef RecordDef => recordDef;

        public override bool TryAction(VoreTrackerRecord record, float rollStrength)
        {
            base.TryAction(record, rollStrength);
            if(TargetPawn.records == null)
            {
                return false;
            }
            TargetPawn.records.Increment(RecordDef);
            return true;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if(target == VoreRole.Invalid)
            {
                yield return "required field \"target\" not set";
            }
            if(recordDef == null && (this.GetType() == typeof(RollAction_IncrementRecord))) // only check the recordDef field if we are using the base class
            {
                yield return "required field \"recordDef\" not set";
            }
        }
    }
}
