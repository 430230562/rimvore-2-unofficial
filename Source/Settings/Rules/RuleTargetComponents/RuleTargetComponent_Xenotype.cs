using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class RuleTargetComponent_Xenotype : RuleTargetComponent
    {
        public override string ButtonTranslationKey => "RV2_Settings_Rule_RuleTargetComponent_Xenotype";
        string targetXenotypeName;
        public RuleTargetComponent_Xenotype() : base() { }
        public RuleTargetComponent_Xenotype(RuleTargetRole targetRole, string targetXenotypeName) : base(targetRole)
        {
            this.targetXenotypeName = targetXenotypeName;
        }

        public override bool RequiresBiotech => true;
        public override string Label => $"{ButtonTranslationKey.Translate()}: {TargetXenotypeDef.label}";
        XenotypeDef TargetXenotypeDef => targetXenotypeName == null ? null : DefDatabase<XenotypeDef>.GetNamed(targetXenotypeName);

        protected override bool AppliesToPawnInteral(Pawn pawn)
        {
            if(pawn.genes?.Xenotype == null)
            {
                return false;
            }
            return pawn.genes.Xenotype == TargetXenotypeDef;
        }
        public override string PawnExplanation(Pawn pawn)
        {
            if(pawn.genes?.Xenotype == null)
            {
                return "RV2_Settings_Rule_RuleExplanation_NoXenotype".Translate(pawn.LabelShortCap);
            }

            return "RV2_Settings_Rule_RuleExplanation_Xenotype".Translate(pawn.LabelShortCap, pawn.genes.Xenotype.LabelCap);
        }

        public override object Clone()
        {
            return new RuleTargetComponent_Xenotype()
            {
                TargetRole = TargetRole,
                inverted = inverted,
                targetXenotypeName = targetXenotypeName
            };
        }

        Func<XenotypeDef, string> xenotypeLabelGetter = (XenotypeDef xenotype) => xenotype.LabelCap;
        Func<XenotypeDef, string> xenotypeTooltipGetter = (XenotypeDef xenotype) => xenotype.description;
        public override void DrawInteractibleInternal(Listing_Standard list)
        {
            Action<XenotypeDef> xenotypeSelection = (XenotypeDef newXenotype) => targetXenotypeName = newXenotype.defName;
            list.CreateLabelledDropDown(RuleCacheManager.AllXenotypeDefs, TargetXenotypeDef, xenotypeLabelGetter, xenotypeSelection, null, ButtonTranslationKey.Translate(), null, xenotypeTooltipGetter);
        }

        public override RuleTargetStaleTrigger MakeStaleTrigger()
        {
            return new RuleTargetStaleTrigger_Timed_Rare(10);
        }

        public override bool IsValid()
        {
            return DefDatabase<XenotypeDef>.AllDefsListForReading.Any(def => def.defName == targetXenotypeName);
        }
        public override void SetFallback(out string message)
        {
            string originalDefName = targetXenotypeName;
            targetXenotypeName = RuleCacheManager.AllXenotypeDefs.First().defName;
            message = $"{this.GetType()} is no longer valid due to unloaded Def {originalDefName}. Resetting to {targetXenotypeName}";
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref targetXenotypeName, "targetXenotypeName");
        }
    }
}