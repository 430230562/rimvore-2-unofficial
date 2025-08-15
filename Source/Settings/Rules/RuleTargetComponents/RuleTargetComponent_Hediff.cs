using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class RuleTargetComponent_Hediff : RuleTargetComponent
    {
        public override string ButtonTranslationKey => "RV2_Settings_Rule_RuleTargetComponent_Hediff";
        string hediffDefName;
        public RuleTargetComponent_Hediff() : base() { }
        public RuleTargetComponent_Hediff(RuleTargetRole targetRole, string hediffDefName) : base(targetRole)
        {
            this.hediffDefName = hediffDefName;
        }

        public override string Label => $"{ButtonTranslationKey.Translate()}: {labelGetter(TargetHediff)}";
        HediffDef TargetHediff => hediffDefName == null ? null : HediffDef.Named(hediffDefName);

        protected override bool AppliesToPawnInteral(Pawn pawn)
        {
            if(pawn.health?.hediffSet?.hediffs == null)
                return false;
            return pawn.health.hediffSet.hediffs.Any(hediff => hediff.def == TargetHediff);
        }

        public override string PawnExplanation(Pawn pawn)
        {
            if(pawn.health?.hediffSet?.hediffs == null)
                return "RV2_Settings_Rule_RuleExplanation_Hediff_NoHealth".Translate(pawn.LabelShortCap);
            return "RV2_Settings_Rule_RuleExplanation_Hediff".Translate(pawn.LabelShortCap, RuleCacheManager.ActiveHediffs);
        }
        public override object Clone()
        {
            return new RuleTargetComponent_Hediff()
            {
                TargetRole = TargetRole,
                inverted = inverted,
                hediffDefName = this.hediffDefName
            };
        }

        Func<HediffDef, string> labelGetter = (HediffDef def) => $"{def.LabelCap} ({def.defName})";
        Func<HediffDef, string> tooltipGetter = (HediffDef def) => def.description;
        public override void DrawInteractibleInternal(Listing_Standard list)
        {
            Action<HediffDef> selection = (HediffDef newDef) => hediffDefName = newDef.defName;
            list.CreateLabelledDropDown(RuleCacheManager.AllHediffDefs, TargetHediff, labelGetter, selection, null, "RV2_Settings_Rule_RuleTargetComponent_Hediff".Translate(), null, tooltipGetter);
        }

        public override RuleTargetStaleTrigger MakeStaleTrigger()
        {
            return new RuleTargetStaleTrigger_Timed_Rare(3);
        }

        public override bool IsValid()
        {
            return RuleCacheManager.AllHediffDefs.Any(def => def.defName == hediffDefName);
        }
        public override void SetFallback(out string message)
        {
            string originalDefName = hediffDefName;
            hediffDefName = RuleCacheManager.AllHediffDefs.First().defName;
            message = $"{this.GetType()} is no longer valid due to unloaded Def {originalDefName}. Resetting to {hediffDefName}";
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref hediffDefName, "hediffDefName");
        }
    }
}