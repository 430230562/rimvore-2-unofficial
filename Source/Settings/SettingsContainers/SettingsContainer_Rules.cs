using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimVore2
{


    public class SettingsContainer_Rules : SettingsContainer
    {
        public SettingsContainer_Rules()
        {
        }

        public Dictionary<string, VoreRulePreset> Presets = new Dictionary<string, VoreRulePreset>();
        private List<RuleEntry> rules;
        public List<RuleEntry> Rules
        {
            get
            {
                if(rules.EnumerableNullOrEmpty())
                {
                    ResetRulesList();
                }
                return rules;
            }
            set
            {
                NotifyStale();
                rules = value;
            }
        }
        public static List<Texture2D> RuleStateIcons
        {
            get
            {
                if(_ruleStateIcons == null)
                {
                    _ruleStateIcons = new List<Texture2D>()
                    {
                        UITextures.CheckOnTexture,
                        UITextures.CheckOffTexture,
                        UITextures.CopyTexture
                    };
                }
                return _ruleStateIcons;
            }
        }
        public static List<Texture2D> _ruleStateIcons;

        static readonly Func<VoreRule, RuleState> pathRulesStateGetter = (VoreRule rule) => rule.UseVorePathRules;
        static readonly Func<VoreRule, RuleState> considerMinimumAgeGetter = (VoreRule rule) => rule.ConsiderMinimumAge;
        static readonly Func<VoreRule, RuleState> allowedInAutoVoreGetter = (VoreRule rule) => rule.AllowedInAutoVore;
        static readonly Func<VoreRule, RuleState> useVorePathRulesGetter = (VoreRule rule) => rule.UseVorePathRules;
        static readonly Func<VoreRule, RuleState> canBeProposedToGetter = (VoreRule rule) => rule.CanBeProposedTo;
        static readonly Func<VoreRule, RuleState> canForceFailedProposalsGetter = (VoreRule rule) => rule.CanForceFailedProposals;
        static readonly Func<VoreRule, RuleState> canBeFOrcedFailedProposalsGetter = (VoreRule rule) => rule.CanBeForcedFailedProposals;
        static readonly Func<VoreRule, RuleState> canBeForcedIfDownedGetter = (VoreRule rule) => rule.CanBeForcedIfDowned;


        Dictionary<RV2DesignationDef, Func<VoreRule, RuleState>> designationGetters;
        Dictionary<RV2DesignationDef, Func<VoreRule, RuleState>> DesignationGetters
        {
            get
            {
                if(designationGetters == null)
                {
                    designationGetters = new Dictionary<RV2DesignationDef, Func<VoreRule, RuleState>>();
                    foreach(RV2DesignationDef des in DefDatabase<RV2DesignationDef>.AllDefsListForReading)
                    {
                        Func<VoreRule, RuleState> getter = (VoreRule rule) => rule.DesignationStates[des.defName];
                        designationGetters.SetOrAdd(des, getter);
                    }
                }
                return designationGetters;
            }
        }


        public void NotifyStale()
        {
            RuleCacheManager.Notify_AllPawnsStale();
            RV2_Patch_UI_Widget_GetGizmos.NotifyAllStale();
            RecacheAllDesignations();
            VoreInteractionManager.ClearCachedInteractions();
        }

        public void MoveRuleDown(int index)
        {
            Rules = Rules
                .Move(index, index + 1)
                .ToList();
        }

        public void MoveRuleUp(int index)
        {
            Rules = Rules
                .Move(index, index - 1)
                .ToList();
        }

        public void RecacheAllDesignations()
        {
            try
            {
#if v1_5
                if(!WorldRendererUtility.WorldRenderedNow && Find.CurrentMap == null)
#else
                if(!WorldRendererUtility.WorldRendered && Find.CurrentMap == null)
#endif
                {
                    // player is on the main menu, no designations to recache
                    return;
                }

                IEnumerable<PawnData> pawnDataList = RV2Mod.RV2Component.AllPawnData;
                if(pawnDataList.EnumerableNullOrEmpty())
                {
                    if(RV2Log.ShouldLog(true, "Designations"))
                        RV2Log.Message("No pawn data to recache", false, "Designations");
                    return;
                }
                if(RV2Log.ShouldLog(true, "Designations"))
                    RV2Log.Message($"Recaching pawndatas: {string.Join(", ", pawnDataList.Select(pd => pd.Pawn?.LabelShort))}", false, "Designations");
                foreach(PawnData pawnData in pawnDataList)
                {
                    RecacheDesignations(pawnData);
                }
            }
            catch(Exception e)
            {
                RV2Log.Error("The accursed RecacheAllDesignations fucked up. Pawn designations were not properly recached, reason: " + e);
            }
        }

        public void RecacheDesignationsFor(Pawn pawn)
        {
            PawnData pawnData = pawn.PawnData();
            if(pawnData == null)
                return;
            RecacheDesignations(pawnData);
        }

        private void RecacheDesignations(PawnData pawnData)
        {
            if(RV2Log.ShouldLog(true, "Designations"))
                RV2Log.Message($"Recaching designations for  {pawnData.Pawn?.LabelShort}", false, "Designations");
            List<RV2DesignationDef> designations = RV2_Common.VoreDesignations;
            if(designations.NullOrEmpty())
            {
                RV2Log.Error($"{pawnData.Pawn?.LabelShort}: No designationDefs to recache");
                return;
            }
            if(!pawnData.IsValid)
            {
                if(RV2Log.ShouldLog(true, "Designations"))
                    RV2Log.Warning($"Tried to recache designations for invalid pawn", true, "Designations");
                return;
            }
            foreach(RV2DesignationDef designation in designations)
            {
                VoreRule appliedRule = RuleCacheManager.GetFinalRule(pawnData.Pawn, RuleTargetRole.All, DesignationGetters[designation]);
                if(appliedRule == null)
                {
                    if(RV2Log.ShouldLog(true, "Designations"))
                        RV2Log.Message("Fatal error, no rule to recalculate designations with", true, "Designations");
                    return;
                }
                bool designationActive = DesignationGetters[designation](appliedRule) == RuleState.On;
                if(RV2Log.ShouldLog(true, "Designations"))
                    RV2Log.Message($"{pawnData.Pawn?.LabelShort}: Set auto designation for {designation.label} to {designationActive}", true, "Designations");
                pawnData.Designations[designation].enabledAuto = designationActive;
            }
        }

        #region outside communication

        public bool DesignationActive(Pawn pawn, RV2DesignationDef designation)
        {
            if(RV2Log.ShouldLog(true, "Designations"))
                RV2Log.Message($"calculating enabled designation for {pawn.LabelShort}", true, "Designations");
            string key = designation.defName;
            VoreRule finalRule = RuleCacheManager.GetFinalRule(pawn, designation.assignedTo, DesignationGetters[designation]);
            // Log.Message(LogUtility.ToString(finalRule.DesignationStates));
            return finalRule.DesignationStates.TryGetValue(key) == RuleState.On;
        }
        public bool VorePathEnabled(Pawn pawn, RuleTargetRole role, VorePathDef path, bool isForAuto = false)
        {
            VoreRule rule = RuleCacheManager.GetFinalRule(pawn, role, useVorePathRulesGetter);
            VorePathRule pathRule = rule.GetPathRule(path.defName);
            bool enabled = pathRule.Enabled;
            if(isForAuto)
            {
                // Log.Message("Checking for auto-vore validity, is auto enabled? " + pathRule.AutoVoreEnabled);
                enabled &= pathRule.AutoVoreEnabled;
            }
            return enabled;
        }

        public ThingDef GetContainer(Pawn predator, VorePathDef pathDef)
        {
            VoreRule finalRule = RuleCacheManager.GetFinalRule(predator, RuleTargetRole.Predator, pathRulesStateGetter);
            VorePathRule pathRule = finalRule.GetPathRule(pathDef.defName);
            ThingDef container = pathRule.Container;
            if(container == RV2_Common.VoreContainerNone)
            {
                return null;
            }
            return container;
        }
        public bool HasValidAge(Pawn pawn, RuleTargetRole role = RuleTargetRole.All)
        {
            int requiredAge = GetValidAge(pawn, role);
            return pawn.ageTracker?.AgeBiologicalYears >= requiredAge;
        }

        public int GetValidAge(Pawn pawn, RuleTargetRole role = RuleTargetRole.All)
        {
            VoreRule activeRule = RuleCacheManager.GetFinalRule(pawn, role, considerMinimumAgeGetter);
            bool needsAgeCheck = activeRule.ConsiderMinimumAge == RuleState.On;
            if(!needsAgeCheck)
            {
                return -1;
            }
            return activeRule.MinimumAge;
        }

        public bool AllowedInAutoVore(Pawn pawn)
        {
            VoreRule activeRule = RuleCacheManager.GetFinalRule(pawn, RuleTargetRole.All, allowedInAutoVoreGetter);
            return activeRule.AllowedInAutoVore == RuleState.On;
        }
        public bool ShouldStruggle(Pawn prey, VorePathDef pathDef, bool isForced)
        {
            VoreRule finalRule = RuleCacheManager.GetFinalRule(prey, RuleTargetRole.Prey, useVorePathRulesGetter);
            VorePathRule pathRule = finalRule.GetPathRule(pathDef.defName);
            return pathRule.ShouldStruggle(isForced);
        }
        public int BaseRequiredStruggles(Pawn prey, VorePathDef pathDef)
        {
            VoreRule finalRule = RuleCacheManager.GetFinalRule(prey, RuleTargetRole.Prey, useVorePathRulesGetter);
            VorePathRule pathRule = finalRule.GetPathRule(pathDef.defName);
            return pathRule.RequiredStruggles;
        }

        public CorpseProcessingType GetCorpseProcessingType(Pawn pawn, VorePathDef path)
        {

            QuirkManager quirkManager = pawn.QuirkManager(false);
            if(quirkManager?.HasSpecialFlag("DestroyPawn") == true)
            {
                return CorpseProcessingType.Destroy;
            }

            VoreRule finalRule = RuleCacheManager.GetFinalRule(pawn, RuleTargetRole.Predator, (VoreRule rule) => rule.UseVorePathRules);
            string pathKey = path.defName;
            if(finalRule != null)
            {
                VorePathRule pathRule = finalRule.GetPathRule(pathKey);
                return pathRule.CorpseProcessingType;
            }

            return CorpseProcessingType.Dessicate;
        }

        public bool CanBeProposedTo(Pawn pawn, RuleTargetRole role = RuleTargetRole.All)
        {
            VoreRule activeRule = RuleCacheManager.GetFinalRule(pawn, role, canBeProposedToGetter);
            bool canBeProposedTo = activeRule.CanBeProposedTo == RuleState.On;
            return canBeProposedTo;
        }

        public bool CanForceFailedProposal(Pawn initiator, Pawn target, RuleTargetRole initiatorRole = RuleTargetRole.All, RuleTargetRole targetRole = RuleTargetRole.All)
        {
            VoreRule initiatorRule = RuleCacheManager.GetFinalRule(initiator, initiatorRole, canForceFailedProposalsGetter);
            VoreRule targetRule = RuleCacheManager.GetFinalRule(target, targetRole, canBeFOrcedFailedProposalsGetter);
            bool valid = initiatorRule.CanForceFailedProposals == RuleState.On && targetRule.CanBeForcedFailedProposals == RuleState.On;
            //Log.Message(initiator.LabelShort + " can vore " + target.LabelShort + " ? " + valid);
            return valid;
        }

        public bool CanBeForcedIfDowned(Pawn pawn, RuleTargetRole role = RuleTargetRole.All)
        {
            VoreRule activeRule = RuleCacheManager.GetFinalRule(pawn, role, canBeForcedIfDownedGetter);
            return activeRule.CanBeForcedIfDowned == RuleState.On;
        }

        #endregion

        public override void Reset()
        {
            rules = null;
            NotifyStale();
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref rules, "rules", LookMode.Deep);

            //ScribeUtilities.ScribeVariableDictionary(ref rules, "rules", LookMode.Deep, LookMode.Deep);
            ScribeUtilities.ScribeVariableDictionary(ref Presets, "presets", LookMode.Value, LookMode.Deep);
        }

        private void ResetRulesList()
        {
            rules?.Clear();
            rules = new List<RuleEntry>()
            {
                {
                    new RuleEntry(new RuleTarget(), new VoreRule(RuleState.On))
                }
            };
        }

        public override void DefsLoaded()
        {
            if(rules.NullOrEmpty())
            {
                ResetRulesList();
            }
            if(rules.Any(rule => rule.Target == null || rule.Rule == null))
            {
                Log.Error($"RV2: Detected broken rule settings, the cause is currently unknown. Resetting your rule settings so that your game hopefully works. Apologies for the inconvenience");
                ResetRulesList();
            }

            foreach(RuleEntry entry in Rules)
            {
                entry.Rule.DefsLoaded();
                entry.Target.DefsLoaded();
            }
        }

        public override void EnsureSmartSettingDefinition()
        {
            // nothing to do here for rules settings
        }
    }
}
