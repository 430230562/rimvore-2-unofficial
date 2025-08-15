using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class RuleTargetComponent_Ability : RuleTargetComponent
    {
        public override string ButtonTranslationKey => "RV2_Settings_Rule_RuleTargetComponent_Ability";
        string abilityDefName;
        public RuleTargetComponent_Ability() : base() { }
        public RuleTargetComponent_Ability(RuleTargetRole targetRole, string abilityDefName) : base(targetRole)
        {
            this.abilityDefName = abilityDefName;
        }

        public override string Label => $"{ButtonTranslationKey.Translate()}: {labelGetter(TargetAbility)}";
        AbilityDef TargetAbility => abilityDefName == null ? null : DefDatabase<AbilityDef>.GetNamed(abilityDefName);

        protected override bool AppliesToPawnInteral(Pawn pawn)
        {
            if(pawn.abilities.abilities.NullOrEmpty())
                return false;
            return pawn.abilities.abilities.Any(ability => ability.def == TargetAbility);
        }
        public override string PawnExplanation(Pawn pawn)
        {
            if(pawn.abilities.abilities.NullOrEmpty())
                return "RV2_Settings_Rule_RuleExplanation_Ability_NoAbilities".Translate(pawn.LabelShortCap);
            return "RV2_Settings_Rule_RuleExplanation_Ability".Translate(pawn.LabelShortCap, RuleCacheManager.ActiveAbilities);
        }
        public override object Clone()
        {
            return new RuleTargetComponent_Ability()
            {
                TargetRole = TargetRole,
                inverted = inverted,
                abilityDefName = this.abilityDefName
            };
        }

        Func<AbilityDef, string> labelGetter = (AbilityDef precept) => precept.LabelCap;
        Func<AbilityDef, string> tooltipGetter = (AbilityDef precept) => precept.description;
        public override void DrawInteractibleInternal(Listing_Standard list)
        {
            Action<AbilityDef> selection = (AbilityDef newAbility) => abilityDefName = newAbility.defName;
            list.CreateLabelledDropDown(RuleCacheManager.AllAbilityDefs, TargetAbility, labelGetter, selection, null, ButtonTranslationKey.Translate(), null, tooltipGetter);
        }
        public override RuleTargetStaleTrigger MakeStaleTrigger()
        {
            return new RuleTargetStaleTrigger_Timed_Rare(20);
        }
        public override bool IsValid()
        {
            return RuleCacheManager.AllAbilityDefs.Any(def => def.defName == abilityDefName);
        }
        public override void SetFallback(out string message)
        {
            string originalDefName = abilityDefName;
            abilityDefName = RuleCacheManager.AllAbilityDefs.First().defName;
            message = $"{this.GetType()} is no longer valid due to unloaded Def {originalDefName}. Resetting to {abilityDefName}";
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref abilityDefName, "abilityDefName");
        }

    }
}