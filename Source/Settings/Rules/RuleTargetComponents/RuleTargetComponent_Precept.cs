using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class RuleTargetComponent_Precept : RuleTargetComponent
    {
        public override bool RequiresIdeology => true;
        public override string ButtonTranslationKey => "RV2_Settings_Rule_RuleTargetComponent_Precept";
        string preceptDefName;
        public RuleTargetComponent_Precept() : base() { }
        public RuleTargetComponent_Precept(RuleTargetRole targetRole, string preceptDefName) : base(targetRole)
        {
            this.preceptDefName = preceptDefName;
        }

        public override string Label => $"{ButtonTranslationKey.Translate()}: {labelGetter(TargetPrecept)}";
        PreceptDef TargetPrecept => preceptDefName == null ? null : DefDatabase<PreceptDef>.GetNamed(preceptDefName);

        protected override bool AppliesToPawnInteral(Pawn pawn)
        {
            if(pawn.Ideo == null)
                return false;
            return pawn.Ideo.PreceptsListForReading.Any(precept => precept.def == TargetPrecept);
        }
        public override string PawnExplanation(Pawn pawn)
        {
            if(pawn.Ideo == null)
                return "RV2_Settings_Rule_RuleExplanation_Precept_NoIdeo".Translate(pawn.LabelShortCap);
            return "RV2_Settings_Rule_RuleExplanation_Precept".Translate(pawn.LabelShortCap, pawn.Ideo.name, RuleCacheManager.ActivePrecepts);
        }
        public override object Clone()
        {
            return new RuleTargetComponent_Precept()
            {
                TargetRole = TargetRole,
                inverted = inverted,
                preceptDefName = this.preceptDefName
            };
        }

        Func<PreceptDef, string> labelGetter = (PreceptDef precept) => precept.defName;
        Func<PreceptDef, string> tooltipGetter = (PreceptDef precept) => precept.description;
        public override void DrawInteractibleInternal(Listing_Standard list)
        {
            Action<PreceptDef> selection = (PreceptDef newPrecept) => preceptDefName = newPrecept.defName;
            list.CreateLabelledDropDown(RuleCacheManager.AllPreceptDefs, TargetPrecept, labelGetter, selection, null, "RV2_Settings_Rule_RuleTargetComponent_Precept".Translate(), null, tooltipGetter);
        }

        public override RuleTargetStaleTrigger MakeStaleTrigger()
        {
            return new RuleTargetStaleTrigger_Timed_Rare(10);
        }

        public override bool IsValid()
        {
            return RuleCacheManager.AllPreceptDefs.Any(def => def.defName == preceptDefName);
        }
        public override void SetFallback(out string message)
        {
            string originalDefName = preceptDefName;
            preceptDefName = RuleCacheManager.AllPreceptDefs.First().defName;
            message = $"{this.GetType()} is no longer valid due to unloaded Def {originalDefName}. Resetting to {preceptDefName}";
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref preceptDefName, "preceptDefName");
        }
    }
}