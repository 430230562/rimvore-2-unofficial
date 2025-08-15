using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace RimVore2
{
    public class RollAction_Digest : RollAction
    {
        public DamageDef damageDef;

        public override bool TryAction(VoreTrackerRecord record, float rollStrength)
        {
            base.TryAction(record, rollStrength);

            AcidUtility.ApplyAcidByDigestionProgress(record, record.CurrentVoreStage.PercentageProgress, damageDef);
            return true;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if(damageDef == null)
            {
                yield return "Required field \"damageDef\" must be set";
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref damageDef, "damageDef");
        }
    }
}
