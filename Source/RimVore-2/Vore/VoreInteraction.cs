using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    /// <summary>
    /// VoreInteraction is used to keep track of all possible and the most preferred way of doing vore between a predator and prey
    /// The predator and prey MUST be determined before a VoreInteraction is created, usually by the VoreInteractionManager with a VoreInteractionRequest with InitiatorRole.Invalid
    /// </summary>
    public class VoreInteraction
    {
        public readonly Pawn Initiator;
        public readonly Pawn Target;
        public Pawn Predator;
        public Pawn Prey;
        public string InteractionInvalidReason = null;
        public string PreferenceInvalidReason = null;
        public List<VorePathDef> ValidPaths = new List<VorePathDef>();
        public List<VoreGoalDef> ValidGoals = new List<VoreGoalDef>();
        public List<VoreTypeDef> ValidTypes = new List<VoreTypeDef>();
        public Dictionary<VorePathDef, string> InvalidPaths = new Dictionary<VorePathDef, string>();
        public Dictionary<VoreGoalDef, string> InvalidGoals = new Dictionary<VoreGoalDef, string>();
        public Dictionary<VoreTypeDef, string> InvalidTypes = new Dictionary<VoreTypeDef, string>();

        bool wasPreferenceCalculationAttempted = false;
        private VorePathDef preferredPath;
        public VorePathDef PreferredPath
        {
            get
            {
                if(!wasPreferenceCalculationAttempted && IsValid)   // only calculate once and if we are have a valid interaction
                {
                    CalculatePreference();
                    wasPreferenceCalculationAttempted = true;
                }
                return preferredPath;
            }
        }

        private VoreInteractionRequest request;

        static List<VoreRole> calculatableVoreRoles = new List<VoreRole>() {
            VoreRole.Predator,
            VoreRole.Prey
        };

        public VoreInteraction(VoreInteractionRequest request)
        {
            this.request = request;
            Initiator = request.Initiator;
            Target = request.Target;

            if(!calculatableVoreRoles.Contains(request.InitiatorRole))
            {
                InteractionInvalidReason = "RV2_VoreInvalidReasons_NoValidRole".Translate();
                if(RV2Log.ShouldLog(true, "VoreInteractions"))
                    RV2Log.Message($"Tried to calculate for invalid initiator role {request.InitiatorRole} - this means the previous code was unable to determine an initiator role and thus the interaction will yield no valid vore paths.", "VoreInteractions");
                return;
            }

            CalculateValidity();
        }

        public bool IsValid => InteractionInvalidReason == null;
        public bool IsValidForProposal => PreferredPath != null && PreferenceInvalidReason == null; // poke the PreferredPath property to calculate preferences and check if preference calculations were valid
        public IEnumerable<VorePathDef> ValidPathsFor(VoreGoalDef goal) => ValidPaths
            .Where(path => path.voreGoal == goal);
        public IEnumerable<VorePathDef> ValidPathsFor(VoreTypeDef type) => ValidPaths
            .Where(path => path.voreType == type);
        public IEnumerable<KeyValuePair<VorePathDef, string>> InvalidPathsFor(VoreGoalDef goal) => InvalidPaths
            .Where(kvp => kvp.Key.voreGoal == goal);
        private IEnumerable<VoreTypeDef> ValidTypesFor(VoreGoalDef goal) => ValidPathsFor(goal)
            .Select(path => path.voreType)
            .Distinct();
        public IEnumerable<VorePathDef> ValidPathsFor(VoreGoalDef goal, VoreTypeDef type) => ValidPaths
            .Where(path => path.voreGoal == goal && path.voreType == type);



        #region Validity Calculation

        private void CalculateValidity()
        {
            SetPawns();
            if(!Predator.CanVore(Prey, out InteractionInvalidReason))
            {
                return;
            }
            if(request.IsForAuto && request.IsForProposal)
            {
                if(!RV2Mod.Settings.fineTuning.AllowWildAnimalsAsAutoProposalTargets && request.Target.IsWildAnimal())
                {
                    InteractionInvalidReason = "RV2_VoreInvalidReasons_WildAnimalForAuto".Translate();
                    return;
                }
                bool hasRelations = request.Target.TryGetColonyRelations(out List<RelationKind> relations);
                if(hasRelations && !RV2Mod.Settings.fineTuning.AllowVisitorsAsAutoProposalTargets && relations.Contains(RelationKind.Visitor))
                {
                    InteractionInvalidReason = "RV2_VoreInvalidReasons_VisitorForAuto".Translate();
                    return;
                }
            }
            foreach(VorePathDef path in RV2_Common.VorePaths)
            {
                if(IsPathValid(path, out string pathInvalidReason))
                    ValidPaths.Add(path);
                else
                    InvalidPaths.Add(path, pathInvalidReason);
            }
            foreach(VoreGoalDef goal in RV2_Common.VoreGoals)
            {
                if(IsGoalValid(goal, out string goalInvalidReason))
                    ValidGoals.Add(goal);
                else
                    InvalidGoals.Add(goal, goalInvalidReason);
            }
            if(ValidGoals.NullOrEmpty())
            {
                InteractionInvalidReason = "RV2_VoreInvalidReasons_NoGoals".Translate();
                return;
            }
            foreach(VoreTypeDef type in RV2_Common.VoreTypes)
            {
                if(IsTypeValid(type, out string typeInvalidReason))
                    ValidTypes.Add(type);
                else
                    InvalidTypes.Add(type, typeInvalidReason);
            }
            if(ValidTypes.NullOrEmpty())
            {
                InteractionInvalidReason = "RV2_VoreInvalidReasons_NoTypes".Translate();
            }

        }

        IEnumerable<string> InvalidPathReasons(Predicate<VorePathDef> predicate = null)
        {
            if(predicate == null)
                predicate = (VorePathDef) => true;  // return all vore paths if no predicate set
            return InvalidPaths
                .Where(kvp => predicate(kvp.Key))
                .Select(kvp => kvp.Value)
                .Distinct();
        }

        bool IsPathValid(VorePathDef path, out string reason)
        {
            if(!WhiteBlackListValid(path, out reason))
            {
                return false;
            }
            else if(!path.IsValid(Predator, Prey, out reason, request.IsForAuto, request.ShouldIgnoreDesignations))
            {
                return false;
            }
            reason = null;
            return true;
        }
        bool IsGoalValid(VoreGoalDef goal, out string reason)
        {
            // goal blacklisted or not whitelisted
            if(!WhiteBlackListValid(goal, out reason))
            {
                return false;
            }
            // goal itself is invalid
            if(!goal.IsValid(Predator, Prey, out reason, request.ShouldIgnoreDesignations))
            {
                return false;
            }
            // goal itself is valid, but there may be no paths!;
            if(ValidPathsFor(goal).Count() <= 0)
            {
                Predicate<VorePathDef> predicate = (VorePathDef path) => path.voreGoal == goal;
                IEnumerable<string> goalInvalidReasons = InvalidPathReasons(predicate);
                reason = "RV2_VoreInvalidReasons_NoPaths".Translate(string.Join(", ", goalInvalidReasons));
                return false;
            }
            // goal is valid
            reason = null;
            return true;
        }
        bool IsTypeValid(VoreTypeDef type, out string reason)
        {
            if(!WhiteBlackListValid(type, out reason))
            {
                return false;
            }
            if(!type.IsValid(Predator, Prey, out reason))
            {
                return false;
            }
            if(ValidPathsFor(type).Count() <= 0)
            {
                Predicate<VorePathDef> predicate = (VorePathDef path) => path.voreType == type;
                IEnumerable<string> typeInvalidReasons = InvalidPathReasons(predicate);
                reason = "RV2_VoreInvalidReasons_NoPaths".Translate(string.Join(", ", typeInvalidReasons));
                return false;
            }
            reason = null;
            return true;
        }

        private void SetPawns()
        {
            switch(request.InitiatorRole)
            {
                case VoreRole.Predator:
                    Predator = request.Initiator;
                    Prey = request.Target;
                    break;
                case VoreRole.Prey:
                    Predator = request.Target;
                    Prey = request.Initiator;
                    break;
                default:
                    throw new Exception($"Invalid initiator role for VoreInteraction calculation: {request.InitiatorRole}");
            }
        }

        bool WhiteBlackListValid(VorePathDef path, out string reason)
        {
            if(!WhiteBlackListValid(path.voreGoal, out reason))
            {
                return false;
            }
            if(!WhiteBlackListValid(path.voreType, out reason))
            {
                return false;
            }
            if(request.PathBlacklist != null && request.PathBlacklist.Contains(path))
            {
                reason = "RV2_VoreInvalidReasons_PathBlacklisted".Translate();
                return false;
            }
            if(request.PathWhitelist != null && !request.PathWhitelist.Contains(path))
            {
                reason = "RV2_VoreInvalidReasons_PathNotWhitelisted".Translate();
                return false;
            }
            reason = null;
            return true;
        }

        bool WhiteBlackListValid(VoreTypeDef type, out string reason)
        {
            if(request.TypeBlacklist != null && request.TypeBlacklist.Contains(type))
            {
                reason = "RV2_VoreInvalidReasons_TypeBlacklisted".Translate();
                return false;
            }
            if(request.TypeWhitelist != null && !request.TypeWhitelist.Contains(type))
            {
                reason = "RV2_VoreInvalidReasons_TypeNotWhitelisted".Translate();
                return false;
            }
            reason = null;
            return true;
        }

        bool WhiteBlackListValid(VoreGoalDef goal, out string reason)
        {
            if(request.GoalBlacklist != null && request.GoalBlacklist.Contains(goal))
            {
                reason = "RV2_VoreInvalidReasons_GoalBlacklisted".Translate();
                return false;
            }
            if(request.GoalWhitelist != null && !request.GoalWhitelist.Contains(goal))
            {
                reason = "RV2_VoreInvalidReasons_GoalNotWhitelisted".Translate();
                return false;
            }
            if(request.DesignationBlacklist != null)
            {
                bool anyDesignationBlacklisted = goal.requiredDesignations
                    .Where(des => des.assignedTo != RuleTargetRole.Predator)    // only check prey designations
                    .Any(des => request.DesignationBlacklist.Contains(des));
                if(anyDesignationBlacklisted)
                {
                    reason = "RV2_VoreInvalidReasons_DesignationBlacklisted".Translate();
                    return false;
                }
            }
            if(request.DesignationWhitelist != null)
            {
                bool allDesignationsWhitelisted = goal.requiredDesignations
                    .Where(des => des.assignedTo != RuleTargetRole.Predator)    // only check prey designations
                    .All(des => request.DesignationWhitelist.Contains(des));
                if(!allDesignationsWhitelisted)
                {
                    reason = "RV2_VoreInvalidReasons_DesignationNotWhitelisted".Translate();
                    return false;
                }
            }
            reason = null;
            return true;
        }

        #endregion

        #region Preference Calculation

        private void CalculatePreference()
        {
            preferredPath = PreferredPathFor(Initiator);
        }

        public VorePathDef PreferredPathFor(Pawn pawn)
        {
            VoreGoalDef preferredGoal = RollForPreferredGoal(pawn);
            if(preferredGoal == null)
            {
                PreferenceInvalidReason = "RV2_VoreInvalidReasons_NoPreferredGoal".Translate();
                return null;
            }
            VoreTypeDef preferredType = RollForPreferredType(pawn, preferredGoal);
            if(preferredType == null)
            {
                PreferenceInvalidReason = "RV2_VoreInvalidReasons_NoPreferredType".Translate();
                return null;
            }
            IEnumerable<VorePathDef> preferredPathSelection = ValidPathsFor(preferredGoal, preferredType);
            if(preferredPathSelection.EnumerableNullOrEmpty())
            {
                PreferenceInvalidReason = "RV2_VoreInvalidReasons_NoPreferredPath".Translate();
                return null;
            }
            return preferredPathSelection.RandomElement();
        }

        private VoreGoalDef RollForPreferredGoal(Pawn pawn)
        {
            Dictionary<VoreGoalDef, float> weightedGoals;
            if(!TryMakeWeightedPreferences<VoreGoalDef>(pawn, ValidGoals, out weightedGoals))
            {
                return null;
            }

            if(RV2Log.ShouldLog(true, "Preferences"))
                RV2Log.Message($"Weighted goals: {LogUtility.ToString(weightedGoals)}", false, "Preferences");
            VoreGoalDef preferredGoals = weightedGoals.RandomElementByWeight(weightedGoal => weightedGoal.Value).Key;
            if(RV2Log.ShouldLog(true, "Preferences"))
                RV2Log.Message($"Preferred {preferredGoals}", false, "Preferences");
            return preferredGoals;
        }

        private VoreTypeDef RollForPreferredType(Pawn pawn, VoreGoalDef goal)
        {
            Dictionary<VoreTypeDef, float> weightedTypes;
            if(!TryMakeWeightedPreferences<VoreTypeDef>(pawn, ValidTypesFor(goal), out weightedTypes))
            {
                return null;
            }

            if(RV2Log.ShouldLog(true, "Preferences"))
                RV2Log.Message($"Weighted types: {LogUtility.ToString(weightedTypes)}", false, "Preferences");
            VoreTypeDef preferredType = weightedTypes.RandomElementByWeight(weightedType => weightedType.Value).Key;
            if(RV2Log.ShouldLog(true, "Preferences"))
                RV2Log.Message($"Preferred {preferredType}", false, "Preferences");
            return preferredType;
        }
        private bool TryMakeWeightedPreferences<T>(Pawn pawn, IEnumerable<T> preferenceSource, out Dictionary<T, float> weightedPreferences) where T : IPreferrable
        {
            weightedPreferences = new Dictionary<T, float>();
            if(preferenceSource.EnumerableNullOrEmpty())
            {
                return false;
            }
            if(RV2Log.ShouldLog(true, "Preferences"))
                RV2Log.Message($"Available {typeof(T)}: {string.Join(", ", preferenceSource.Select(s => s.GetName()))}", false, "Preferences");
            foreach(T source in preferenceSource)
            {
                float preference = source.GetPreference(pawn, RoleOf(pawn));
                if(preference >= 0)
                {
                    //preference++;
                    weightedPreferences.Add(source, preference);
                }
                else
                {
                    if(RV2Log.ShouldLog(true, "Preferences"))
                        RV2Log.Message($"Preference for {typeof(T)} {source.GetName()} is {preference}, which is below 0, not adding to valid target list", false, "Preferences");
                }
            }
            return !weightedPreferences.EnumerableNullOrEmpty();
        }

        #endregion

        public VoreRole RoleOf(Pawn pawn)
        {
            if(pawn == Predator)
                return VoreRole.Predator;
            if(pawn == Prey)
                return VoreRole.Prey;
            return VoreRole.Feeder;
        }

        public bool AppliesTo(VoreInteractionRequest request)
        {
            return this.request.Equals(request);
        }

        public override string ToString()
        {
            return $@"Initiator: {Initiator.LabelShortCap}, Target: {Target.LabelShortCap} - Valid ? {(InteractionInvalidReason == null ? "Yes" : $"No: {InteractionInvalidReason}")}
Predator: {Predator?.LabelShort}, Prey: {Prey?.LabelShort}
Valid Paths:
- {(ValidPaths.NullOrEmpty() ? "NONE" : String.Join("\n- ", ValidPaths.Select(p => p.defName)))}

Invalid Paths:
- {(InvalidPaths.EnumerableNullOrEmpty() ? "NONE" : String.Join("\n- ", InvalidPaths.Select(kvp => $"{kvp.Key.defName}: {kvp.Value}")))}

Valid Goals:
- {(ValidGoals.NullOrEmpty() ? "NONE" : String.Join("\n- ", ValidGoals.Select(g => g.defName)))}

Invalid Goals:
- {(InvalidGoals.EnumerableNullOrEmpty() ? "NONE" : String.Join("\n- ", InvalidGoals.Select(kvp => $"{kvp.Key.defName}: {kvp.Value}")))}

Valid Types:
- {(ValidTypes.NullOrEmpty() ? "NONE" : String.Join("\n- ", ValidTypes.Select(t => t.defName)))}

Invalid Goals:
- {(InvalidTypes.EnumerableNullOrEmpty() ? "NONE" : String.Join("\n- ", InvalidTypes.Select(kvp => $"{kvp.Key.defName}: {kvp.Value}")))}
";
        }
    }
}
