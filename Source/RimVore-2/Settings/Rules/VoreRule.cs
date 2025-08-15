using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;
using RimWorld;

namespace RimVore2
{
    public class VoreRule : IExposable, ICloneable
    {
        public RuleState ConsiderMinimumAge = RuleState.Copy;
        public int MinimumAge = 18;
        public RuleState AllowedInAutoVore = RuleState.Copy;
        public RuleState CanBeProposedTo = RuleState.Copy;
        public RuleState CanForceFailedProposals = RuleState.Copy;
        public RuleState CanBeForcedFailedProposals = RuleState.Copy;
        public RuleState CanBeForcedIfDowned = RuleState.Copy;

        private Dictionary<string, RuleState> designationStates;
        public Dictionary<string, RuleState> DesignationStates
        {
            get
            {
                if(designationStates.EnumerableNullOrEmpty())
                {
                    List<string> designationKeys = RV2_Common.VoreDesignations != null ? RV2_Common.VoreDesignations.ConvertAll(def => def.defName) : new List<string>();
                    ScribeUtilities.SyncKeys(ref designationStates, designationKeys);
                }
                return designationStates;
            }
        }

        public RuleState UseVorePathRules = RuleState.Copy;

        public Dictionary<string, VorePathRule> pathRules;
        public VorePathRule GetPathRule(string pathName)
        {
            if(pathRules == null)
            {
                pathRules = new Dictionary<string, VorePathRule>();
            }
            if(!pathRules.ContainsKey(pathName) || pathRules.TryGetValue(pathName) == null)
            {
                pathRules.SetOrAdd(pathName, new VorePathRule(pathName));
            }
            return pathRules.TryGetValue(pathName);
        }
        public IEnumerable<VorePathRule> AllPathRules()
        {
            return pathRules.Values;
        }
        //public Dictionary<VorePathDef, VorePathRule> ResolvedPathRules => PathRules.ToDictionary(kvp => DefDatabase<VorePathDef>.GetNamed(kvp.Key), kvp => kvp.Value);

        public VoreRule() { }

        public VoreRule(RuleState state)
        {
            ConsiderMinimumAge = state;
            AllowedInAutoVore = state;
            CanBeProposedTo = state;
            CanForceFailedProposals = state;
            CanBeForcedFailedProposals = state;
            CanBeForcedIfDowned = state;
            ScribeUtilities.SyncKeys<string, RuleState>(ref designationStates, RV2_Common.VoreDesignations.ConvertAll(d => d.defName), state);
            Func<string, VorePathRule> vorePathRuleValueCreator = delegate (string pathName)
            {
                return new VorePathRule(pathName);
            };
            ScribeUtilities.SyncKeys<string, VorePathRule>(ref pathRules, RV2_Common.VorePaths.ConvertAll(path => path.defName), null, vorePathRuleValueCreator);
            UseVorePathRules = state;
        }

        public void DefsLoaded()
        {
            ScribeUtilities.SyncKeys(ref designationStates, RV2_Common.VoreDesignations.ConvertAll(d => d.defName)); 
            Func<string, VorePathRule> vorePathRuleValueCreator = delegate (string pathName)
            {
                return new VorePathRule(pathName);
            };
            ScribeUtilities.SyncKeys<string, VorePathRule>(ref pathRules, RV2_Common.VorePaths.ConvertAll(path => path.defName), null, vorePathRuleValueCreator);

            foreach(VorePathRule pathRule in pathRules.Values)
                pathRule.DefsLoaded();
        }

        public object Clone()
        {
            return new VoreRule()
            {
                ConsiderMinimumAge = this.ConsiderMinimumAge,
                MinimumAge = this.MinimumAge,
                AllowedInAutoVore = this.AllowedInAutoVore,
                CanBeProposedTo = this.CanBeProposedTo,
                CanForceFailedProposals = this.CanForceFailedProposals,
                CanBeForcedFailedProposals = this.CanBeForcedFailedProposals,
                CanBeForcedIfDowned = this.CanBeForcedIfDowned,
                designationStates = this.designationStates == null ? null : new Dictionary<string, RuleState>(this.designationStates),
                UseVorePathRules = this.UseVorePathRules,
                pathRules = pathRules == null ? null : new Dictionary<string, VorePathRule>(this.pathRules)
            };
        }

        public void ValidateContainers()
        {
            pathRules.ForEach(rule => rule.Value.ValidateContainers());
        }

        public void DoControls(Rect inRect, RuleTargetRole targetRole = RuleTargetRole.All, bool isFirstRule = false)
        {
            bool showPathControls = UseVorePathRules == RuleState.On;
            int columnCount = 1;
            if(showPathControls)
            {
                columnCount++;
            }
            List<Rect> columns = UIUtility.CreateColumns(inRect, columnCount, out _, 0, 0);
            DoFirstColumn(columns[0], targetRole, isFirstRule);
            if(showPathControls)
            {
                DoPaths(columns[1]);
            }
        }

        private Vector2 firstColumnScrollPosition;
        private float firstColumnHeight = 0f;
        private bool firstColumnHeightStale = true;
        private void DoFirstColumn(Rect inRect, RuleTargetRole targetRole = RuleTargetRole.All, bool isFirstRule = false)
        {
            UIUtility.MakeAndBeginScrollView(inRect, firstColumnHeight, ref firstColumnScrollPosition, out Listing_Standard list);
            DoGeneralRules(list, isFirstRule);
            DoDesignationRules(list, targetRole, isFirstRule);
            list.EndScrollView(ref firstColumnHeight, ref firstColumnHeightStale);
        }

        private Vector2 pathsScrollPosition;
        private float pathsHeight = 0f;
        private bool pathsHeightStale = true;
        private int filteredPathsHash = 0;
        private void DoPaths(Rect inRect)
        {
            Listing_Standard outerList = new Listing_Standard()
            {
                ColumnWidth = inRect.width,
                maxOneColumn = true
            };
            outerList.Begin(inRect);
            outerList.HeaderLabel("RV2_Settings_Rules_VorePathsHeader".Translate());
            Rect outerPathsRect = outerList.GetRect(inRect.height - outerList.CurHeight);
            UIUtility.MakeAndBeginScrollView(outerPathsRect, pathsHeight, ref pathsScrollPosition, out Listing_Standard innerList);
            DoRuleFilterControls(innerList);
            IEnumerable<KeyValuePair<string, VorePathRule>> filteredRules = pathRules.Where(kvp => pathFilter.filter.Matches(kvp.Key));
            // cache the unique list so the scroll view can update with it
            int newFilteredPathsHash = filteredRules.GetHashCode();
            if(newFilteredPathsHash != filteredPathsHash)
            {
                filteredPathsHash = newFilteredPathsHash;
                pathsHeightStale = true;
            }
            DoAllPathButtons(innerList, filteredRules);
            foreach(KeyValuePair<string, VorePathRule> rule in filteredRules)
            {
                rule.Value.DoControls(innerList, AllowedInAutoVore, ref pathsHeightStale);
            }
            innerList.EndScrollView(ref pathsHeight, ref pathsHeightStale);
            outerList.End();
        }

        private QuickSearchWidget pathFilter = new QuickSearchWidget();
        private void DoRuleFilterControls(Listing_Standard list)
        {
            Rect rowRect = list.GetRect(Text.LineHeight);
            pathFilter.OnGUI(rowRect);
        }

        private void DoAllPathButtons(Listing_Standard list, IEnumerable<KeyValuePair<string, VorePathRule>> paths)
        {
            List<string> buttonLabels = new List<string>
            {
                "RV2_Settings_Rules_VorePathsEnableAll".Translate(),
                "RV2_Settings_Rules_VorePathsDisableAll".Translate()
            };
            list.ButtonRow(buttonLabels, out int indexClicked, PairAlignment.Equal);
            bool stateToSet = false;
            switch(indexClicked)
            {
                case 0:
                    stateToSet = true;
                    break;
                case 1:
                    stateToSet = false;
                    break;
                default:
                    return;
            }
            paths
                .Select(kvp => kvp.Value)
                .ForEach(path => path.Enabled = stateToSet);
        }

        private void DoGeneralRules(Listing_Standard list, bool isFirstRule = false)
        {
            list.HeaderLabel("RV2_Settings_Rules_General".Translate());
            DoRuleState(list, "RV2_Settings_Rules_ConsiderMinimumAge".Translate(), ref ConsiderMinimumAge, isFirstRule, null);
            if(ConsiderMinimumAge == RuleState.On)
            {
                list.SliderLabeled("RV2_Settings_Rules_MinimumAge".Translate(), ref MinimumAge, 0, 99);
            }
            DoRuleState(list, "RV2_Settings_Rules_AllowedInAutoVore".Translate(), ref AllowedInAutoVore, isFirstRule, "RV2_Settings_Rules_AllowedInAutoVore_Tip".Translate());
            if(!isFirstRule)
            {
                DoRuleState(list, "RV2_settings_Rules_UsesVorePathRules".Translate(), ref UseVorePathRules, isFirstRule, "RV2_settings_Rules_UsesVorePathRules_Tip".Translate());
            }
            DoRuleState(list, "RV2_settings_Rules_CanBeProposedTo".Translate(), ref CanBeProposedTo, isFirstRule, "RV2_settings_Rules_CanBeProposedTo_Tip".Translate());
            DoRuleState(list, "RV2_settings_Rules_CanForceFailedProposals".Translate(), ref CanForceFailedProposals, isFirstRule, "RV2_settings_Rules_CanForceFailedProposals_Tip".Translate());
            DoRuleState(list, "RV2_settings_Rules_CanBeForcedFailedProposals".Translate(), ref CanBeForcedFailedProposals, isFirstRule, "RV2_settings_Rules_CanBeForcedFailedProposals_Tip".Translate());
            DoRuleState(list, "RV2_settings_Rules_CanBeForcedIfDowned".Translate(), ref CanBeForcedIfDowned, isFirstRule, "RV2_settings_Rules_CanBeForcedIfDowned_Tip".Translate());
        }

        private void DoRuleState(Listing_Standard list, string label, ref RuleState state, bool isFirstRule = false, string tooltip = null)
        {
            if(isFirstRule)
            {
                bool isEnabled = state == RuleState.On;
                list.CheckboxLabeled(label, ref isEnabled, tooltip);
                state = isEnabled ? RuleState.On : RuleState.Off;
            }
            else
            {
                SettingsTab_Rules.DoLabeledRuleStateControl(list, label, ref state);
            }
        }

        private void DoDesignationRules(Listing_Standard list, RuleTargetRole targetRole = RuleTargetRole.All, bool isFirstRule = false)
        {
            IEnumerable<RV2DesignationDef> designations = RV2_Common.VoreDesignations
                .Where(designation => designation.AppliesToRole(targetRole));
            foreach(RV2DesignationDef designation in designations)
            {
                if(!DesignationStates.ContainsKey(designation.defName))
                {
                    DesignationStates.Add(designation.defName, RuleState.On);
                }
                RuleState currentState = DesignationStates[designation.defName];
                DoRuleState(list, designation.label, ref currentState, isFirstRule, designation.description);
                DesignationStates[designation.defName] = currentState;
            }
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref ConsiderMinimumAge, "ConsiderMinimumAge");
            Scribe_Values.Look(ref MinimumAge, "MinimumAge");
            Scribe_Values.Look(ref AllowedInAutoVore, "AllowedInAutoVore");
            Scribe_Values.Look(ref CanBeProposedTo, "CanBeProposedTo");
            Scribe_Values.Look(ref CanForceFailedProposals, "CanForceFailedProposals");
            Scribe_Values.Look(ref CanBeForcedFailedProposals, "CanBeForcedFailedProposals");
            Scribe_Values.Look(ref CanBeForcedIfDowned, "CanBeForcedIfDowned");
            Scribe_Values.Look(ref UseVorePathRules, "UseVorePathRules");
            ScribeUtilities.ScribeVariableDictionary<string, VorePathRule>(ref pathRules, "pathRules", LookMode.Value, LookMode.Deep);
            ScribeUtilities.ScribeVariableDictionary(ref designationStates, "designationStates", LookMode.Value, LookMode.Value);
        }

    }
}
