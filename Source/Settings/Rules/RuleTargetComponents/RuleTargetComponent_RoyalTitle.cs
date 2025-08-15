using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class RuleTargetComponent_RoyalTitle : RuleTargetComponent
    {
        public override bool RequiresRoyalty => true;
        public override string ButtonTranslationKey => "RV2_Settings_Rule_RuleTargetComponent_RoyalTitle";

        string factionDefName;
        string royalTitleDefName;
        public RuleTargetComponent_RoyalTitle() : base() { }
        public RuleTargetComponent_RoyalTitle(RuleTargetRole targetRole, string factionDefName, string royalTitleDefName) : base(targetRole)
        {
            this.factionDefName = factionDefName;
            this.royalTitleDefName = royalTitleDefName;
        }

        public override string Label => $"{ButtonTranslationKey.Translate()}: {titleLabelGetter(TargetTitle)}";
        FactionDef TargetFaction => factionDefName == null ? null : DefDatabase<FactionDef>.GetNamed(factionDefName, false);
        RoyalTitleDef TargetTitle => royalTitleDefName == null ? null : DefDatabase<RoyalTitleDef>.GetNamed(royalTitleDefName, false);
        protected override bool AppliesToPawnInteral(Pawn pawn)
        {
            if(Current.Game == null)   // check if game is running before probing the FactionManager
                return false;
            Faction faction = Find.FactionManager.FirstFactionOfDef(TargetFaction);

            if(faction == null)
                return false;

            RoyalTitleDef title = pawn.GetCurrentTitleIn(faction);
            if(title == null || TargetTitle == null)
                return false;
            return title.index >= TargetTitle.index;
        }
        public override string PawnExplanation(Pawn pawn)
        {
            Faction faction = Find.FactionManager.FirstFactionOfDef(TargetFaction);

            if(faction == null)
                return "ERR: Faction not found";

            RoyalTitleDef title = pawn.GetCurrentTitleIn(faction);
            if(title == null)
                return "RV2_Settings_Rule_RuleExplanation_RoyalTitle_NoTitle".Translate(pawn.LabelShortCap);
            return "RV2_Settings_Rule_RuleExplanation_RoyalTitle".Translate(pawn.LabelShortCap, titleLabelGetter(title));
        }
        public override object Clone()
        {
            return new RuleTargetComponent_RoyalTitle()
            {
                TargetRole = TargetRole,
                inverted = inverted,
                factionDefName = this.factionDefName,
                royalTitleDefName = this.royalTitleDefName
            };
        }

        Func<FactionDef, string> factionTooltipGetter = (FactionDef faction) => faction.description;
        Func<RoyalTitleDef, string> titleLabelGetter = (RoyalTitleDef title) => $"{title.LabelCap} (#{title.index})";
        Func<RoyalTitleDef, string> titleTooltipGetter = (RoyalTitleDef title) => title.description;
        public override void DrawInteractibleInternal(Listing_Standard list)
        {
            Action<FactionDef> factionSelection = (FactionDef newFaction) =>
            {
                royalTitleDefName = null;
                factionDefName = newFaction.defName;
            };
            Action<RoyalTitleDef> titleSelection = (RoyalTitleDef newTitle) => royalTitleDefName = newTitle.defName;
            list.CreateLabelledDropDown(RuleCacheManager.FactionTitleDictionary.Keys, TargetFaction, GetFactionLabel, factionSelection, null, "RV2_Settings_Rule_RuleTargetComponent_RoyalTitle_Faction".Translate(), null, factionTooltipGetter);
            if(TargetFaction != null)
                list.CreateLabelledDropDown(RuleCacheManager.FactionTitleDictionary[TargetFaction], TargetTitle, titleLabelGetter, titleSelection, null, ButtonTranslationKey.Translate(), null, titleTooltipGetter);
        }
        private static string GetFactionLabel(FactionDef factionDef)
        {
            if(factionDef == null)
                return "ERR: NULL faction";
            string defName = factionDef.defName;
            if(Current.Game == null)
                return defName;
            Faction faction = Find.FactionManager?.FirstFactionOfDef(factionDef);
            if(faction == null)
                return defName;
            return $"{faction.Name} ({defName})";
        }

        public override RuleTargetStaleTrigger MakeStaleTrigger()
        {
            return new RuleTargetStaleTrigger_Timed_Rare(10);
        }

        public override bool IsValid()
        {
            bool factionValid = RuleCacheManager.FactionTitleDictionary.Keys.Any(factionDef => factionDef.defName == factionDefName);
            if(!factionValid)
            {
                return false;
            }
            return RuleCacheManager.FactionTitleDictionary[TargetFaction].Any(titleDef => titleDef.defName == royalTitleDefName);
        }
        public override void SetFallback(out string message)
        {
            message = "";
            bool factionValid = RuleCacheManager.FactionTitleDictionary.Keys.Any(factionDef => factionDef.defName == factionDefName);
            if(!factionValid)
            {
                string originalFactionDefName = factionDefName;
                factionDefName = RuleCacheManager.FactionTitleDictionary.Keys.First().defName;
                message = message = $"{this.GetType()} FACTION is no longer valid due to unloaded Def {originalFactionDefName}. Resetting to {factionDefName}\n";
            }
            string originalRoyalTitleDefName = royalTitleDefName;
            royalTitleDefName = RuleCacheManager.FactionTitleDictionary[TargetFaction].First().defName;
            message += $"{this.GetType()} is no longer valid due to unloaded Def {originalRoyalTitleDefName}. Resetting to {royalTitleDefName}";
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref royalTitleDefName, "royalTitleDefName");
        }
    }
}
