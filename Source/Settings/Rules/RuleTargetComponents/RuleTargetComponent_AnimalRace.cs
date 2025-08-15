using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class RuleTargetComponent_AnimalRace : RuleTargetComponent
    {
        public override string ButtonTranslationKey => "RV2_Settings_Rule_RuleTargetComponent_AnimalRace";
        string targetAnimalKindDefName;
        public RuleTargetComponent_AnimalRace() : base() { }
        public RuleTargetComponent_AnimalRace(RuleTargetRole targetRole, string targetAnimalKindDefName) : base(targetRole)
        {
            this.targetAnimalKindDefName = targetAnimalKindDefName;
        }

        public override string Label => $"{ButtonTranslationKey.Translate()}: {TargetKindDef.label}";
        PawnKindDef TargetKindDef => targetAnimalKindDefName == null ? null : PawnKindDef.Named(targetAnimalKindDefName);

        protected override bool AppliesToPawnInteral(Pawn pawn)
        {
            if(!pawn.RaceProps.Animal)
                return false;
            return pawn.kindDef == TargetKindDef;
        }
        public override string PawnExplanation(Pawn pawn)
        {
            if(!pawn.RaceProps.Animal)
            {
                return "RV2_Settings_Rule_RuleExplanation_NotAnimal".Translate(pawn.LabelShortCap);
            }
            return "RV2_Settings_Rule_RuleExplanation_AnimalRace".Translate(pawn.LabelShortCap, pawn.kindDef.LabelCap);
        }

        public override object Clone()
        {
            return new RuleTargetComponent_AnimalRace()
            {
                TargetRole = TargetRole,
                inverted = inverted,
                targetAnimalKindDefName = targetAnimalKindDefName
            };
        }

        Func<PawnKindDef, string> animalLabelGetter = (PawnKindDef animal) => animal.LabelCap;
        Func<PawnKindDef, string> animalTooltipGetter = (PawnKindDef animal) => animal.description;
        public override void DrawInteractibleInternal(Listing_Standard list)
        {
            Action<PawnKindDef> animalSelection = (PawnKindDef newAnimal) => targetAnimalKindDefName = newAnimal.defName;
            list.CreateLabelledDropDown(RuleCacheManager.AllAnimalPawnKindDefs, TargetKindDef, animalLabelGetter, animalSelection, null, ButtonTranslationKey.Translate(), null, animalTooltipGetter);
        }

        public override RuleTargetStaleTrigger MakeStaleTrigger()
        {
            return new RuleTargetStaleTrigger_Never();
        }

        public override bool IsValid()
        {
            return DefDatabase<PawnKindDef>.AllDefsListForReading.Any(def => def.defName == targetAnimalKindDefName);
        }
        public override void SetFallback(out string message)
        {
            string originalDefName = targetAnimalKindDefName;
            targetAnimalKindDefName = RuleCacheManager.AllAnimalPawnKindDefs.First().defName;
            message = $"{this.GetType()} is no longer valid due to unloaded Def {originalDefName}. Resetting to {targetAnimalKindDefName}";
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref targetAnimalKindDefName, "targetAnimalKindDefName");
        }
    }
}
