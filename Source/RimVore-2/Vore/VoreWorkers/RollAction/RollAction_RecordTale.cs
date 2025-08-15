using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace RimVore2
{
    public class RollAction_RecordTale : RollAction
    {
#pragma warning disable CS0649 // assigned from DEF
        private TaleDef tale;
#pragma warning restore CS0649 // assigned from DEF
        protected virtual TaleDef Tale => tale;
        protected virtual Def AdditionalDef => null;

        public override bool TryAction(VoreTrackerRecord record, float rollStrength)
        {
            base.TryAction(record, rollStrength);

            if(Tale == null)
            {
                return false;
            }
            List<object> parameters = new List<object>();
            if(record.Predator != null)
                parameters.Add(record.Predator);
            if(record.Prey != null)
                parameters.Add(record.Prey);
            if(AdditionalDef != null)
                parameters.Add(AdditionalDef);
            TaleRecorder.RecordTale(Tale, parameters.ToArray());
            return true;
        }
    }
}
