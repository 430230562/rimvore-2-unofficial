using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class RuleTargetComponent_Apparel : RuleTargetComponent
    {
        public override string ButtonTranslationKey => "RV2_Settings_Rule_RuleTargetComponent_Apparel";
        string apparelDefName;
        public RuleTargetComponent_Apparel() : base() { }
        public RuleTargetComponent_Apparel(RuleTargetRole targetRole, string apparelDefName) : base(targetRole)
        {
            this.apparelDefName = apparelDefName;
        }

        public override string Label => $"{ButtonTranslationKey.Translate()}: {labelGetter(TargetApparel)}";
        ThingDef TargetApparel => apparelDefName == null ? null : DefDatabase<ThingDef>.GetNamed(apparelDefName);

        protected override bool AppliesToPawnInteral(Pawn pawn)
        {
            List<Apparel> apparel = pawn.apparel.WornApparel;
            if(apparel.NullOrEmpty())
                return false;
            return apparel.Any(app => app.def == TargetApparel);
        }
        public override string PawnExplanation(Pawn pawn)
        {
            List<Apparel> apparel = pawn.apparel.WornApparel;
            if(apparel.NullOrEmpty())
                return "RV2_Settings_Rule_RuleExplanation_Apparel_NoApparel".Translate(pawn.LabelShortCap);
            return "RV2_Settings_Rule_RuleExplanation_Apparel".Translate(pawn.LabelShortCap, RuleCacheManager.WornApparel);
        }
        public override object Clone()
        {
            return new RuleTargetComponent_Apparel()
            {
                TargetRole = TargetRole,
                inverted = inverted,
                apparelDefName = this.apparelDefName
            };
        }

        Func<ThingDef, string> labelGetter = (ThingDef def) => def.LabelCap;
        Func<ThingDef, string> tooltipGetter = (ThingDef def) => def.description;
        public override void DrawInteractibleInternal(Listing_Standard list)
        {
            Action<ThingDef> selection = (ThingDef newDef) => apparelDefName = newDef.defName;
            list.CreateLabelledDropDown(RuleCacheManager.AllApparelThingDefs, TargetApparel, labelGetter, selection, null, ButtonTranslationKey.Translate(), null, tooltipGetter);
        }

        public override RuleTargetStaleTrigger MakeStaleTrigger()
        {
            return new RuleTargetStaleTrigger_Timed_Rare(10);
        }
        public override bool IsValid()
        {
            return RuleCacheManager.AllApparelThingDefs.Any(def => def.defName == apparelDefName);
        }
        public override void SetFallback(out string message)
        {
            string originalDefName = apparelDefName;
            apparelDefName = RuleCacheManager.AllApparelThingDefs.First().defName;
            message = $"{this.GetType()} is no longer valid due to unloaded Def {originalDefName}. Resetting to {apparelDefName}";
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref apparelDefName, "apparelDefName");
        }
    }
}