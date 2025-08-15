using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class RuleTargetComponent_Race : RuleTargetComponent
    {
        public override string ButtonTranslationKey => "RV2_Settings_Rule_RuleTargetComponent_Race";
        string targetRaceDefName;
        public RuleTargetComponent_Race() : base() { }
        public RuleTargetComponent_Race(RuleTargetRole targetRole, string targetRaceDefName) : base(targetRole)
        {
            this.targetRaceDefName = targetRaceDefName;
        }


        public override string Label => $"{ButtonTranslationKey.Translate()}: {TargetRace.label}";
        ThingDef TargetRace => targetRaceDefName == null ? null : ThingDef.Named(targetRaceDefName);

        protected override bool AppliesToPawnInteral(Pawn pawn)
        {
            return pawn.def == TargetRace;
        }
        public override string PawnExplanation(Pawn pawn)
        {
            return "RV2_Settings_Rule_RuleExplanation_Race".Translate(pawn.LabelShortCap, pawn.def.defName);
        }
        public override object Clone()
        {
            return new RuleTargetComponent_Race()
            {
                TargetRole = TargetRole,
                inverted = inverted,
                targetRaceDefName = targetRaceDefName
            };
        }

        Func<ThingDef, string> raceLabelGetter = (ThingDef race) => race.LabelCap;
        Func<ThingDef, string> raceTooltipGetter = (ThingDef race) => race.description;
        public override void DrawInteractibleInternal(Listing_Standard list)
        {
            Action<ThingDef> raceSelection = (ThingDef newRace) => targetRaceDefName = newRace.defName;
            list.CreateLabelledDropDown(RV2_Common.AllAlienRaces, TargetRace, raceLabelGetter, raceSelection, null, "RV2_Settings_Rule_RuleTargetComponent_Race".Translate(), null, raceTooltipGetter);
        }

        public override RuleTargetStaleTrigger MakeStaleTrigger()
        {
            return new RuleTargetStaleTrigger_Never();
        }

        public override bool IsValid()
        {
            return RV2_Common.AllAlienRaces.Any(def => def.defName == targetRaceDefName);
        }
        public override void SetFallback(out string message)
        {
            string originalDefName = targetRaceDefName;
            targetRaceDefName = RV2_Common.AllAlienRaces.First().defName;
            message = $"{this.GetType()} is no longer valid due to unloaded Def {originalDefName}. Resetting to {targetRaceDefName}";
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref targetRaceDefName, "targetRaceDefName");
        }
    }
}
