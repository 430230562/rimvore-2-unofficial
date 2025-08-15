using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class RuleTargetComponent_WorkType : RuleTargetComponent
    {
        public override string ButtonTranslationKey => "RV2_Settings_Rule_RuleTargetComponent_WorkType";
        string workTypeDefName;
        public RuleTargetComponent_WorkType() : base() { }
        public RuleTargetComponent_WorkType(RuleTargetRole targetRole, string workTypeDefName) : base(targetRole)
        {
            this.workTypeDefName = workTypeDefName;
        }

        public override string Label => $"{ButtonTranslationKey.Translate()}: {labelGetter(TargetWorkType)}";
        WorkTypeDef TargetWorkType => workTypeDefName == null ? null : DefDatabase<WorkTypeDef>.GetNamed(workTypeDefName);

        protected override bool AppliesToPawnInteral(Pawn pawn)
        {
            if(pawn.workSettings == null)
                return false;
            if(!pawn.workSettings.EverWork)
                return false;
            return pawn.workSettings.WorkIsActive(TargetWorkType);
        }
        public override string PawnExplanation(Pawn pawn)
        {
            if(pawn.workSettings == null || !pawn.workSettings.EverWork)
                return "RV2_Settings_Rule_RuleExplanation_WorkType_NeverWorks".Translate(pawn.LabelShortCap);
            return "RV2_Settings_Rule_RuleExplanation_WorkType".Translate(pawn.LabelShortCap, RuleCacheManager.ActiveWorkTypes);
        }
        public override object Clone()
        {
            return new RuleTargetComponent_WorkType()
            {
                TargetRole = TargetRole,
                inverted = inverted,
                workTypeDefName = this.workTypeDefName
            };
        }

        Func<WorkTypeDef, string> labelGetter = (WorkTypeDef workType) => workType.gerundLabel.CapitalizeFirst();
        Func<WorkTypeDef, string> tooltipGetter = (WorkTypeDef workType) => workType.description;
        public override void DrawInteractibleInternal(Listing_Standard list)
        {
            Action<WorkTypeDef> selection = (WorkTypeDef newWorkType) => workTypeDefName = newWorkType.defName;
            list.CreateLabelledDropDown(RuleCacheManager.AllWorkTypeDefs, TargetWorkType, labelGetter, selection, null, ButtonTranslationKey.Translate(), null, tooltipGetter);
        }

        public override RuleTargetStaleTrigger MakeStaleTrigger()
        {
            return new RuleTargetStaleTrigger_Timed_Rare(3);
        }

        public override bool IsValid()
        {
            return RuleCacheManager.AllWorkTypeDefs.Any(def => def.defName == workTypeDefName);
        }
        public override void SetFallback(out string message)
        {
            string originalDefName = workTypeDefName;
            workTypeDefName = RuleCacheManager.AllWorkTypeDefs.First().defName;
            message = $"{this.GetType()} is no longer valid due to unloaded Def {originalDefName}. Resetting to {workTypeDefName}";
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref workTypeDefName, "workTypeDefName");
        }
    }
}
