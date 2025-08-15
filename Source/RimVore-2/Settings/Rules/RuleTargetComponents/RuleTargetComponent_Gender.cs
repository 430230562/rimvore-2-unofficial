using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class RuleTargetComponent_Gender : RuleTargetComponent
    {
        public override string ButtonTranslationKey => "RV2_Settings_Rule_RuleTargetComponent_Gender";
        Gender targetGender;
        public RuleTargetComponent_Gender() : base()
        {
            targetGender = Gender.Female;
        }
        public RuleTargetComponent_Gender(RuleTargetRole targetRole, Gender targetGender) : base(targetRole)
        {
            this.targetGender = targetGender;
        }

        public override string Label => $"{ButtonTranslationKey.Translate()}: {targetGender}";

        protected override bool AppliesToPawnInteral(Pawn pawn)
        {
            return pawn.gender == targetGender;
        }
        public override string PawnExplanation(Pawn pawn)
        {
            return "RV2_Settings_Rule_RuleExplanation_Gender".Translate(pawn.LabelShortCap, pawn.gender.GetLabel());
        }

        public override object Clone()
        {
            return new RuleTargetComponent_Gender()
            {
                TargetRole = TargetRole,
                inverted = inverted,
                targetGender = targetGender,
            };
        }

        Func<Gender, string> genderLabelGetter = (Gender gender) => gender.GetLabel().CapitalizeFirst();
        public override void DrawInteractibleInternal(Listing_Standard list)
        {
            Action<Gender> genderSelection = (Gender newGender) => targetGender = newGender;
            list.EnumLabeled("RV2_Settings_Rule_RuleTargetComponent_Gender".Translate(), targetGender, genderSelection, genderLabelGetter, null);
        }

        public override RuleTargetStaleTrigger MakeStaleTrigger()
        {
            return new RuleTargetStaleTrigger_Timed_Rare(20);
        }

        public override bool IsValid()
        {
            // it's an enum, this will always be valid
            return true;
        }

        public override void SetFallback(out string message)
        {
            message = "This should never happen :)";
            // never called
            return;
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref targetGender, "targetGender");
        }
    }
}
