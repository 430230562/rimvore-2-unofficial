using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace RimVore2
{
    public class RollAction_AddHediff : RollAction
    {
        public HediffDef hediff;
        protected BodyPartDef partDef;

        public override bool TryAction(VoreTrackerRecord record, float rollStrength)
        {
            base.TryAction(record, rollStrength);

            if(TargetPawn.health.hediffSet.HasHediff(hediff))
            {
                return false;
            }
            // no reason to null check, null body part is whole body and valid
            BodyPartRecord bodyPart = TargetPawn.GetBodyPartByDef(partDef);
            Hediff actualHediff = TargetPawn.health.AddHediff(hediff, bodyPart);
            if(RV2Log.ShouldLog(false, "OngoingVore"))
                RV2Log.Message($"Added hediff {actualHediff.def.defName} to {TargetPawn.Label}'s body part {bodyPart}", "OngoingVore");
            return true;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if(hediff == null)
            {
                yield return "required field \"hediff\" is not set";
            }
            if(target == VoreRole.Invalid)
            {
                yield return "required field \"target\" is not set";
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref hediff, "hediff");
            Scribe_Defs.Look(ref partDef, "partDef");
        }
    }
}
