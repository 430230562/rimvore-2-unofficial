using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace RimVore2
{
    public class RollAction_IncreaseNeed : RollAction
    {
        public NeedDef need;
        public override bool TryAction(VoreTrackerRecord record, float rollStrength)
        {
            base.TryAction(record, rollStrength);
            if(invert)
            {
                rollStrength *= -1;
            }
            return RV2PawnUtility.TryIncreaseNeed(TargetPawn, need, rollStrength);
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if(target == VoreRole.Invalid)
            {
                yield return "required field \"target\" is not set";
            }
            if(need == null)
            {
                yield return "required field \"need\" is not set";
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref need, "need");
        }
    }
}
