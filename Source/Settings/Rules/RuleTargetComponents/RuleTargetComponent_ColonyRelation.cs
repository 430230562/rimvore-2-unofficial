using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class RuleTargetComponent_ColonyRelation : RuleTargetComponent
    {
        public override string ButtonTranslationKey => "RV2_Settings_Rule_RuleTargetComponent_ColonyRelation";

        RelationKind targetRelation;

        public RuleTargetComponent_ColonyRelation() : base()
        {
            targetRelation = RelationKind.Colonist;
        }
        public RuleTargetComponent_ColonyRelation(RuleTargetRole targetRole, RelationKind targetRelation) : base(targetRole)
        {
            this.targetRelation = targetRelation;
        }

        public override string Label => $"{ButtonTranslationKey.Translate()}: {labelGetter(targetRelation)}";

        protected override bool AppliesToPawnInteral(Pawn pawn)
        {
            List<RelationKind> pawnRelations;
            if(!pawn.TryGetColonyRelations(out pawnRelations))
            {
                return false;
            }
            return pawnRelations.Contains(this.targetRelation);
        }
        public override string PawnExplanation(Pawn pawn)
        {
            string relations = string.Join(", ", ColonyRelationUtility.GetRelationKinds(pawn));
            return "RV2_Settings_Rule_RuleExplanation_ColonyRelation".Translate(pawn.LabelShortCap, relations);
        }

        public override object Clone()
        {
            return new RuleTargetComponent_ColonyRelation()
            {
                TargetRole = TargetRole,
                inverted = inverted,
                targetRelation = targetRelation
            };
        }

        private static List<RelationKind> SelectableBlacklist = new List<RelationKind>()
        {
            RelationKind.Invalid
        };
        Func<RelationKind, string> labelGetter = (RelationKind kind) => UIUtility.EnumPresenter(kind.ToString());
        public override void DrawInteractibleInternal(Listing_Standard list)
        {
            Action<RelationKind> selection = (RelationKind newKind) => targetRelation = newKind;
            list.CreateLabelledDropDown(RuleCacheManager.AllColonyRelationKinds, targetRelation, labelGetter, selection, null, ButtonTranslationKey.Translate());
        }

        public override RuleTargetStaleTrigger MakeStaleTrigger()
        {
            return new RuleTargetStaleTrigger_Timed_Rare(20);
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
            Scribe_Values.Look(ref targetRelation, "targetRelation");
        }
    }
}
