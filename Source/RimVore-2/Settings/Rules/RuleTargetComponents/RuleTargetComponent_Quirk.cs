using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class RuleTargetComponent_Quirk : RuleTargetComponent
    {
        public override string ButtonTranslationKey => "RV2_Settings_Rule_RuleTargetComponent_Quirk";
        string quirkDefName;
        public RuleTargetComponent_Quirk() : base() { }
        public RuleTargetComponent_Quirk(RuleTargetRole targetRole, string quirkDefName) : base(targetRole)
        {
            this.quirkDefName = quirkDefName;
        }

        public override string Label => $"{ButtonTranslationKey.Translate()}: {labelGetter(TargetQuirk)}";
        QuirkDef TargetQuirk => quirkDefName == null ? null : DefDatabase<QuirkDef>.GetNamed(quirkDefName);

        protected override bool AppliesToPawnInteral(Pawn pawn)
        {
            QuirkManager quirks = pawn.QuirkManager(false);
            if(quirks == null)
                return false;
            return quirks.HasQuirk(TargetQuirk);
        }
        public override string PawnExplanation(Pawn pawn)
        {
            QuirkManager quirks = pawn.QuirkManager(false);
            if(quirks == null)
                return "RV2_Settings_Rule_RuleExplanation_Quirk_NoQuirks".Translate(pawn.LabelShortCap);
            return "RV2_Settings_Rule_RuleExplanation_Quirk".Translate(pawn.LabelShortCap, RuleCacheManager.ActiveQuirks);
        }
        public override object Clone()
        {
            return new RuleTargetComponent_Quirk()
            {
                TargetRole = TargetRole,
                inverted = inverted,
                quirkDefName = this.quirkDefName
            };
        }

        Func<QuirkDef, string> labelGetter = (QuirkDef def) => $"{def.label.CapitalizeFirst()} ({def.defName})";
        Func<QuirkDef, string> tooltipGetter = (QuirkDef def) => def.description;
        public override void DrawInteractibleInternal(Listing_Standard list)
        {
            Action<QuirkDef> selection = (QuirkDef newDef) => quirkDefName = newDef.defName;
            list.CreateLabelledDropDown(RuleCacheManager.AllQuirkDefs, TargetQuirk, labelGetter, selection, null, ButtonTranslationKey.Translate(), null, tooltipGetter);
        }

        public override RuleTargetStaleTrigger MakeStaleTrigger()
        {
            return new RuleTargetStaleTrigger_Timed_Rare(5);
        }

        public override bool IsValid()
        {
            return RuleCacheManager.AllQuirkDefs.Any(def => def.defName == quirkDefName);
        }
        public override void SetFallback(out string message)
        {
            string originalDefName = quirkDefName;
            quirkDefName = RuleCacheManager.AllQuirkDefs.First().defName;
            message = $"{this.GetType()} is no longer valid due to unloaded Def {originalDefName}. Resetting to {quirkDefName}";
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref quirkDefName, "quirkDefName");
        }
    }
}