using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class RuleTargetComponent_Diet : RuleTargetComponent
    {
        public override string ButtonTranslationKey => "RV2_Settings_Rule_RuleTargetComponent_Diet";
        FoodTypeFlags targetDiet;
        public RuleTargetComponent_Diet() : base()
        {
            targetDiet = FoodTypeFlags.VegetableOrFruit;
        }
        public RuleTargetComponent_Diet(RuleTargetRole targetRole, FoodTypeFlags targetDiet) : base(targetRole)
        {
            this.targetDiet = targetDiet;
        }

        public override string Label => $"{ButtonTranslationKey.Translate()}: {labelGetter(targetDiet)}";

        protected override bool AppliesToPawnInteral(Pawn pawn)
        {
            return pawn.RaceProps?.Eats(targetDiet) == true;
        }
        public override string PawnExplanation(Pawn pawn)
        {
            FoodTypeFlags flags = pawn.RaceProps.foodType;
            string flagExplanation = $"{flags} ({flags.ToHumanString()})";
            return "RV2_Settings_Rule_RuleExplanation_Diet".Translate(pawn.LabelShortCap, flagExplanation);
        }

        public override object Clone()
        {
            return new RuleTargetComponent_Diet()
            {
                TargetRole = TargetRole,
                inverted = inverted,
                targetDiet = targetDiet
            };
        }

        Func<FoodTypeFlags, string> labelGetter = (FoodTypeFlags flags) => UIUtility.EnumPresenter(flags.ToString());
        public override void DrawInteractibleInternal(Listing_Standard list)
        {
            Action<FoodTypeFlags> selection = (FoodTypeFlags newFlag) => targetDiet = newFlag;
            list.CreateLabelledDropDown(RuleCacheManager.ValidFoodTypeFlags, targetDiet, labelGetter, selection, null, ButtonTranslationKey.Translate());
        }

        public override RuleTargetStaleTrigger MakeStaleTrigger()
        {
            return new RuleTargetStaleTrigger_Never();
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
            Scribe_Values.Look(ref targetDiet, "targetDiet");
        }
    }
}
