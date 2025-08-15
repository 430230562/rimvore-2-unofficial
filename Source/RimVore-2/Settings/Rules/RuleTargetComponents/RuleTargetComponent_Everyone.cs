using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class RuleTargetComponent_Everyone : RuleTargetComponent
    {
        public override string ButtonTranslationKey => "RV2_Settings_Rule_RuleTargetComponent_Everyone";


        public RuleTargetComponent_Everyone() : base() { }
        public RuleTargetComponent_Everyone(RuleTargetRole targetRole) : base(targetRole) { }

        public override string Label => ButtonTranslationKey.Translate();

        protected override bool AppliesToPawnInteral(Pawn pawn)
        {
            return true;
        }
        public override string PawnExplanation(Pawn pawn)
        {
            return "RV2_Settings_Rule_RuleExplanation_Everyone".Translate();
        }
        public override object Clone()
        {
            return new RuleTargetComponent_Everyone()
            {
                TargetRole = TargetRole,
                inverted = inverted,
            };
        }

        public override RuleTargetStaleTrigger MakeStaleTrigger()
        {
            return new RuleTargetStaleTrigger_Never();
        }

        public override bool IsValid()
        {
            // everyone is always valid
            return true;
        }
        public override void SetFallback(out string message)
        {
            message = "This should never happen :)";
            // never called
            return;
        }
        public override void DrawInteractibleInternal(Listing_Standard list)
        {
            // nothing to draw
        }
    }
}
