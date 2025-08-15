using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse.AI;

namespace RimVore2
{
    /// <summary>
    /// The master class for accessing calculated pawn flags and checking for overrides or validity between pawns
    /// </summary>
    public static class VoreValidator
    {
        // -------- General checks --------
        public static bool CanParticipateInVore(this Pawn pawn, out string reason)
        {
            if(pawn == null)
            {
                RV2Log.Error("Tried to check NULL pawn");
                reason = "PAWN IS NULL";
                return false;
            }
            // update the pawns age data
            //pawn.UpdateAgeLock();
            // if pawn ignores minimum age or is above qualifying age
            if(!RV2Mod.Settings.rules.HasValidAge(pawn))
            {
                reason = "RV2_VoreInvalidReasons_TooYoung".Translate();
                return false;
            }
            // animal participation is disabled and pawn is animal
            if(!RV2Mod.Settings.features.AnimalsEnabled && pawn.RaceProps.Animal)
            {
                reason = "RV2_VoreInvalidReasons_Animal".Translate();
                return false;
            }
            reason = null;
            return true;
        }

        public static bool CanHaveQuirks(this Pawn pawn, out string reason)
        {
            if(!RV2Mod.Settings.features.VoreQuirksEnabled)
            {
                reason = "RV2_VoreInvalidReasons_QuirksDisabled".Translate();
                return false;
            }
            if(!pawn.CanParticipateInVore(out reason))
            {
                return false;
            }
            return true;
        }

        public static bool WantsToProposeTo(this Pawn pawn, Thing target, out string reason)
        {
            reason = "";
            if(!(target is Pawn targetPawn))
            {
                reason = "not a pawn";
                return false;
            }
            if(targetPawn == null || targetPawn.Map == null || targetPawn.Dead)
            {
                reason = "target or targets map null";
                return false;
            }
            if(targetPawn == pawn)
            {
                reason = "is self";
                return false;
            }
            if(pawn.Map == null || !pawn.IsColonist || !pawn.IsHumanoid())
            {
                reason = "initiator is null or not a humanoid colonist";
                return false;
            }
            if(targetPawn.HostileTo(pawn))
            {
                reason = "is hostile";
                return false;
            }
            bool isBlockedByRules = !RV2Mod.Settings.rules.AllowedInAutoVore(pawn)
                || !RV2Mod.Settings.rules.AllowedInAutoVore(targetPawn)
                || !RV2Mod.Settings.rules.CanBeProposedTo(targetPawn);
            if(isBlockedByRules)
            {
                reason = "rules forbid";
                return false;
            }
            if(targetPawn.InAggroMentalState)
            {
                reason = "target in aggro mental state";
                return false;
            }
            if(!pawn.CanReserveAndReach(targetPawn, PathEndMode.Touch, Danger.Some))
            {
                reason = "target not reachable or reservable";
                return false;
            }
            float preference = pawn.PreferenceFor(targetPawn);
            if(preference <= 0)
            {
                reason = $"Preference too low: {preference}";
                return false;
            }
            return true;
        }

        // -------- DESIGNATION QUALIFICATION checks --------
        public static bool CanBePredator(this Pawn pawn, out string reason)
        {
            // consider age and pawn type
            if(!pawn.CanParticipateInVore(out reason))
            {
                return false;
            }
            // if all forms of vore are disabled
            if(!RV2Mod.Settings.features.FatalVoreEnabled && !RV2Mod.Settings.features.EndoVoreEnabled)
            {
                reason = "RV2_VoreInvalidReasons_FatalAndEndoDisabled".Translate();
                return false;
            }
            QuirkManager quirkManager = pawn.QuirkManager();
            // check quirks if pawn has quirks
            if(quirkManager != null)
            {
                // if fatal vore is disabled and pawn can only vore fatally
                if(!RV2Mod.Settings.features.FatalVoreEnabled && quirkManager.HasSpecialFlag("FatalPredatorOnly"))
                {
                    reason = "RV2_VoreInvalidReasons_FatalDisabledAndQuirkFatalOnlyPredator".Translate();
                    return false;
                }
                // if endo vore is disabled and pawn can only vore non-fatally
                if(!RV2Mod.Settings.features.EndoVoreEnabled && quirkManager.HasSpecialFlag("EndoPredatorOnly"))
                {
                    reason = "RV2_VoreInvalidReasons_EndoDisabledAndQuirkEndoOnlyPredator".Translate();
                    return false;
                }
                if(quirkManager.HasSpecialFlag("NeverPredator"))
                {
                    reason = "RV2_VoreInvalidReasons_QuirkNeverPredator".Translate();
                    return false;
                }
            }
            return true;
        }
        public static bool NeverWantsToBePredator(this Pawn pawn, out string reason)
        {
            if(pawn.CanBePredator(out reason))
            {
                return false;
            }
            QuirkManager quirks = pawn.QuirkManager();
            if(quirks != null)
            {
                VoreTargetSelectorRequest request = new VoreTargetSelectorRequest()
                {
                    role = VoreRole.Predator
                };
                float preferenceForPredator = quirks.GetTotalSelectorModifierForDirect(request);
                return preferenceForPredator == 0f;
            }
            return true;
        }

        public static bool CanBePrey(this Pawn pawn, out string reason)
        {
            if(!pawn.CanParticipateInVore(out reason))
            {
                return false;
            }
            // if all forms of vore are disabled
            if(!RV2Mod.Settings.features.FatalVoreEnabled && !RV2Mod.Settings.features.EndoVoreEnabled)
            {
                reason = "RV2_VoreInvalidReasons_FatalAndEndoDisabled".Translate();
                return false;
            }
            QuirkManager quirkManager = pawn.QuirkManager();
            // check quirks if pawn has quirks
            if(quirkManager != null)
            {
                if(quirkManager.HasSpecialFlag("NeverPrey"))
                {
                    reason = "RV2_VoreInvalidReasons_QuirkNeverPrey".Translate();
                    return false;
                }
            }
            return true;
        }
        public static bool NeverWantsToBePrey(this Pawn pawn, out string reason)
        {
            if(pawn.CanBePrey(out reason))
            {
                return false;
            }
            QuirkManager quirks = pawn.QuirkManager();
            if(quirks != null)
            {
                VoreTargetSelectorRequest request = new VoreTargetSelectorRequest()
                {
                    role = VoreRole.Prey
                };
                float preferenceForPrey = quirks.GetTotalSelectorModifierForDirect(request);
                return preferenceForPrey == 0f;
            }
            return true;
        }
        public static bool CanBeEndoPrey(this Pawn pawn, out string reason)
        {
            if(!RV2Mod.Settings.features.EndoVoreEnabled)
            {
                reason = "RV2_VoreInvalidReasons_EndoDisabled".Translate();
                return false;
            }
            if(!pawn.CanBePrey(out reason))
            {
                return false;
            }
            return true;
        }
        public static bool CanBeFatalPrey(this Pawn pawn, out string reason)
        {
            if(!RV2Mod.Settings.features.EndoVoreEnabled)
            {
                reason = "RV2_VoreInvalidReasons_EndoDisabled".Translate();
                return false;
            }
            if(!pawn.CanBePrey(out reason))
            {
                return false;
            }
            return true;
        }

        public static bool CanDoRole(this VoreRole role, Pawn initiator, Pawn target)
        {
            switch(role)
            {
                case VoreRole.Prey:
                    return target.CanVore(initiator, out _);
                case VoreRole.Predator:
                    return initiator.CanVore(target, out _);
                case VoreRole.Feeder:
                    return true;
                default:
                    RV2Log.Warning("Unknown vore role: " + role);
                    return false;
            }
        }

        // -------- ability checks --------
        public static bool CanVore(this Pawn predator, Pawn prey, out string reason)
        {
            if(predator == prey)
            {
                reason = "RV2_VoreInvalidReasons_SelfVore".Translate();
                return false;
            }
            if(!predator.CanParticipateInVore(out reason))
            {
                reason = "RV2_VoreInvalidReasons_PredatorCantVore".Translate() + reason;
                return false;
            }
            if(!prey.CanParticipateInVore(out reason))
            {
                reason = "RV2_VoreInvalidReasons_PreyCantBeVored".Translate() + reason;
                return false;
            }
            if(!predator.IsPredator(out reason))
            {
                reason = "RV2_VoreInvalidReasons_PredatorCantVore".Translate() + reason;
                return false;
            }
            if(!predator.HasFreeCapacityFor(prey))
            {
                reason = "RV2_VoreInvalidReasons_NoCapacity".Translate();
                return false;
            }
            if(predator.InMentalState && !predator.HasValidMentalStateForVore(VoreRole.Predator))
            {
                reason = "RV2_VoreInvalidReasons_MentalState".Translate();
                return false;
            }
            if(prey.InMentalState && !prey.HasValidMentalStateForVore(VoreRole.Prey))
            {
                reason = "RV2_VoreInvalidReasons_MentalState".Translate();
                return false;
            }
            if(CurrentlyDoingVoreJob(prey))
            {
                reason = "RV2_VoreInvalidReasons_CurrentlyInitiatingVore".Translate();
                return false;
            }
            if(!RV2Mod.Settings.features.FatalVoreEnabled && !RV2Mod.Settings.features.EndoVoreEnabled)
            {
                reason = "RV2_VoreInvalidReasons_FatalAndEndoDisabled".Translate();
                return false;
            }
            if(RitualBlocking(predator))
            {
                reason = "RV2_VoreInvalidReasons_PredatorInRitual".Translate();
                return false;
            }
            if(RitualBlocking(prey))
            {
                reason = "RV2_VoreInvalidReasons_PreyInRitual".Translate();
                return false;
            }
            return true;
        }

        public static bool RitualBlocking(Pawn pawn)
        {
            if(!ModsConfig.IdeologyActive)
            {
                return false;
            }
            LordJob_Ritual ritual = RV2RitualUtility.ParticipatingRitual(pawn);
            if(ritual == null)
            {
                return false;
            }
            RitualBehaviorDef behaviorDef = ritual.Ritual?.behavior?.def;
            if(behaviorDef == null)
            {
                return false;
            }
            return !behaviorDef.HasModExtension<VoreRitualFlag>();
        }
        public static bool CurrentlyDoingVoreJob(Pawn pawn)
        {
            return pawn?.jobs?.curJob is VoreJob;
        }

        public static bool CanEndoVore(this Pawn predator, Pawn prey, out string reason, bool checkCanVore = false)
        {
            if(!RV2Mod.Settings.features.EndoVoreEnabled)
            {
                reason = "RV2_VoreInvalidReasons_EndoDisabled".Translate();
                return false;
            }
            if(checkCanVore && !predator.CanVore(prey, out reason))
            {
                return false;
            }
            if(!IsPredator(predator, out reason))
            {
                return false;
            }
            if(!IsEndoPrey(prey, out reason))
            {
                return false;
            }
            return true;
            /*if(!CanVore(predator, prey, out reason))
            {
                return false;
            }
            if(!prey.IsDesignatedEndoPrey(out reason))
            {
                return false;
            }
            if(!predator.CanBeEndoPredator(out reason))
            {
                return false;
            }
            if(!prey.CanBeEndoPrey(out reason))
            {
                return false;
            }
            return true;*/
        }

        public static bool CanFatalVore(this Pawn predator, Pawn prey, out string reason, bool checkCanVore = false)
        {
            if(!RV2Mod.Settings.features.FatalVoreEnabled)
            {
                reason = "RV2_VoreInvalidReasons_FatalDisabled".Translate();
                return false;
            }
            if(checkCanVore && !predator.CanVore(prey, out reason))
            {
                return false;
            }
            if(!IsPredator(predator, out reason))
            {
                return false;
            }
            if(!IsFatalPrey(prey, out reason))
            {
                return false;
            }
            return true;
            /*if(!CanVore(predator, prey, out reason))
            {
                return false;
            }
            if(!prey.IsDesignatedFatalPrey(out reason))
            {
                return false;
            }
            if(!predator.CanBeFatalPredator(out reason))
            {
                return false;
            }
            if(!prey.CanBeFatalPrey(out reason))
            {
                return false;
            }
            return true;*/
        }

        public static bool HasFreeCapacityFor(this Pawn predator, Pawn prey)
        {
            if(predator == null || prey == null)
            {
                RV2Log.Error("Can not calculate capacity, predator or prey are NULL, pred: " + (predator == null) + " prey: " + (prey == null));
            }
            float totalCapacity = predator.CalculateVoreCapacity();
            // a certain capacity becomes ridiculous, the predator is obviously meant to have infinite capacity
            if(totalCapacity >= RV2_Common.VoreStorageCapacityToBeConsideredInfinite)
            {
                return true;
            }
            VoreTracker tracker = predator.PawnData()?.VoreTracker;
            if(tracker == null)
            {
                Log.Error("Vore tracker doesn't exist for pawn " + predator?.Label + " - could not calculate free capacity");
                return false;
            }
            float usedCapacity = tracker.VoreTrackerRecords.Sum(record => record.Prey.BodySize);
            float freeCapacity = totalCapacity - usedCapacity;
            // factor in the settings, where the minimum free capacity can be overridden
            bool hasFreeCapacity = freeCapacity >= prey.BodySize;
            // Log.Message("Predator " + predator.Label + " has total cap " + totalCapacity + ", used cap " + usedCapacity + " and prey needs cap " + prey.BodySize);
            return hasFreeCapacity;
        }

        public static float CalculateVoreCapacity(this Pawn pawn)
        {
            float capacity = pawn.BodySize;
            capacity *= RV2Mod.Settings.cheats.BodySizeToVoreCapacity;
            QuirkManager quirkManager = pawn.QuirkManager();
            if(quirkManager != null)
            {
                if(quirkManager.HasComp<QuirkComp_ValueModifier>())
                {
                    capacity = quirkManager.ModifyValue("StorageCapacity", capacity);
                }
            }
            // override the calculated capacity if the user set a special lower limit
            capacity = Math.Max(capacity, RV2Mod.Settings.cheats.MinimumVoreCapacity);
            return capacity;
        }

        // -------- designation check --------
        public static bool IsPredator(this Pawn pawn, out string reason)
        {
            if(!CanBePredator(pawn, out reason))
            {
                return false;
            }

            bool anyPredatorDesignationSet = RV2_Common.VoreDesignations
                .Where(des => des.AppliesToRole(RuleTargetRole.Predator)) // take all predator designations
                .Any(des => des.IsEnabledFor(pawn, out _)); // check if any of those designations is currently active for the pawn
            if(!anyPredatorDesignationSet)
            {
                reason = "RV2_VoreInvalidReasons_NoPredatorDesignations".Translate(pawn.Label);
                return false;
            }
            reason = null;
            return true;
        }

        public static bool IsPrey(this Pawn pawn, out string reason)
        {
            if(!CanBePrey(pawn, out reason))
            {
                return false;
            }

            bool anyPreyDesignationSet = RV2_Common.VoreDesignations
                .Where(des => des.AppliesToRole(RuleTargetRole.Prey)) // take all prey designations
                .Any(des => des.IsEnabledFor(pawn, out _)); // check if any of those designations is currently active for the pawn
            if(!anyPreyDesignationSet)
            {
                reason = "RV2_VoreInvalidReasons_NoPreyDesignations".Translate(pawn.Label);
                return false;
            }
            reason = null;
            return true;
        }

        public static bool IsEndoPrey(this Pawn pawn, out string reason)
        {
            if(!CanBeEndoPrey(pawn, out reason))
            {
                return false;
            }
            if(!RV2DesignationDefOf.endo.IsEnabledFor(pawn, out reason))
            {
                return false;
            }
            reason = null;
            return true;
        }

        public static bool IsFatalPrey(this Pawn pawn, out string reason)
        {
            if(!CanBeFatalPrey(pawn, out reason))
            {
                return false;
            }
            if(!RV2DesignationDefOf.fatal.IsEnabledFor(pawn, out reason))
            {
                return false;
            }
            reason = null;
            return true;
        }

        // -------- other --------

        /// <remark>
        /// Provides a hook for Overstuffed to intercept and remove enabler validations (absorption enabled = assimilation enabled && amalgamation enabled)
        /// </remark>
        public static List<VoreTargetSelectorRequest> PredatorPassesVoreEnablerSelectors(List<VoreTargetSelectorRequest> requests) {
            return requests;
        }

        public static Dictionary<VoreTargetSelectorRequest, string> PrecomputeRequestFailureReasons(List<VoreTargetSelectorRequest> requests)
        {
            return PredatorPassesVoreEnablerSelectors(requests)
                .Where(request => RV2_Common.VoreCombinationsRequiringEnablers.Any(selector => selector.Matching(request)))
                .ToDictionary(
                    request => request,
                    request => {
                        string reason = "RV2_RequirementInvalidReasons_MissingQuirk".Translate(String.Join(", ", RV2_Common.GetRequiredQuirksForRequest(request).Select(q => q.label)));
                        return reason;
                    }
                );
        }

        public static Dictionary<RaceType, Dictionary<VoreTargetSelectorRequest, string>> PrecomputePreyRequestFailureReasons(Func<RaceType, List<VoreTargetSelectorRequest>> generator)
        {
            return GeneralUtility.GetValues<RaceType>().ToDictionary(
                (race => race),
                (race => PrecomputeRequestFailureReasons(generator.Invoke(race)))
            );
        }

        public static bool PredatorHasVoreEnablerQuirks(Pawn predator, Dictionary<VoreTargetSelectorRequest, string> requests, out string reason)
        {
            //LogUtility.MessageOnce("Checking requests " + string.Join(", ", requests.ConvertAll(r => r.ToString())));
            QuirkManager predatorQuirks = predator.QuirkManager();
            foreach(KeyValuePair<VoreTargetSelectorRequest, string> pair in requests)
            {
                if(predatorQuirks == null)
                {
                    reason = "RV2_VoreInvalidReasons_QuirksDisabled".Translate();
                    return false;
                }
                if(!predatorQuirks.HasVoreEnabler(pair.Key))
                {
                    //RV2Log.Message($"Pawn {predator.LabelShort} does not have voreEnabler for " + request, true, true, "VoreInteractions");
                    reason = pair.Value;
                    return false;
                }
                //RV2Log.Message($"Pawn {predator.LabelShort} has voreEnabler for " + request, true, true, "VoreInteractions");
            }
            reason = null;
            return true;
        }

        public static bool ShouldHaveGizmo(this Pawn pawn)
        {
            if(!CanParticipateInVore(pawn, out string reason))
            {
                if(RV2Log.ShouldLog(false, "Gizmo"))
                {
                    RV2Log.Message($"Pawn {pawn.LabelShort} gets no gizmo: {reason}", true, "Gizmo");
                }
                return false;
            }
            return true;
        }

        public static bool StripBeforeVore(Pawn predator, Pawn prey, ForcedState forced = ForcedState.Willing)
        {
            // early exit if prey has no apparel or is already stripped
            if(!prey.AnythingToStrip())
            {
                return false;
            }
            // check relations of pawn, block stripping if listed as a never-strip relation in RV2_Common
            if(ColonyRelationUtility.TryGetColonyRelations(prey, out List<RelationKind> preyRelationKinds))
            {
                bool isNonStripRelationKind = preyRelationKinds.Any(kind => RV2_Common.relationKindsToNeverStrip.Contains(kind));
                if(isNonStripRelationKind)
                {
                    if(RV2Log.ShouldLog(false, "Preferences"))
                        RV2Log.Message($"Not stripping pawn {prey.LabelShort} because one of their relation kinds is tracked " +
                            $"as a never-strip kind: {string.Join(", ", preyRelationKinds)} | non strip kinds: {string.Join(", ", RV2_Common.relationKindsToNeverStrip)}", "Preferences");
                    return false;
                }
            }

            // this makes it so manhunting animals never strip their prey (causes issues for traders getting vored due to strip method causing faction penalty)
            if(forced == ForcedState.ForcedByPredator && predator.IsAnimal())
            {
                return false;
            }

            float totalPredatorStripDesire;
            float totalPreyStripDesire;
            if(predator.QuirkManager() != null && predator.QuirkManager().HasValueModifier("PredatorStripDesire"))
            {
                totalPredatorStripDesire = predator.QuirkManager().ModifyValue("PredatorStripDesire", 1);
            }
            else
            {
                // if the predator has no preference, set their desire to 0, this way the default stripping behaviour for forced vore can trigger
                totalPredatorStripDesire = 0f;
            }
            if(prey.QuirkManager() != null && prey.QuirkManager().HasValueModifier("PreyStripDesire"))
            {
                totalPreyStripDesire = prey.QuirkManager().ModifyValue("PreyStripDesire", 1);
            }
            else
            {
                totalPreyStripDesire = 0f;
            }
            // Log.Message("prey strip desire " + totalPreyStripDesire + " predator strip desire " + totalPredatorStripDesire);
            switch(forced)
            {
                case ForcedState.ForcedByPredator:
                    if(totalPredatorStripDesire >= 1)
                    {
                        if(RV2Log.ShouldLog(true, "Preferences"))
                            RV2Log.Message("Decided to strip because of predator preference and forced by predator", false, "Preferences");
                        return true;
                    }
                    break;
                case ForcedState.ForcedByPrey:
                    if(totalPreyStripDesire >= 1)
                    {
                        if(RV2Log.ShouldLog(true, "Preferences"))
                            RV2Log.Message("Decided to strip because of prey preference and forced by prey", false, "Preferences");
                        return true;
                    }
                    break;
                case ForcedState.Willing:
                    float combinedStripDesire = totalPredatorStripDesire + totalPreyStripDesire;
                    // Log.Message("combined desire " + combinedStripDesire);
                    if(combinedStripDesire >= 1)
                    {
                        if(RV2Log.ShouldLog(true, "Preferences"))
                            RV2Log.Message("Decided to strip because of combined preference and willing", false, "Preferences");
                        return true;
                    }
                    else if(combinedStripDesire <= -1)
                    {
                        if(RV2Log.ShouldLog(true, "Preferences"))
                            RV2Log.Message("Decided not to strip because of combined preference and willing", false, "Preferences");
                        return false;
                    }
                    // if we got to this point, fall back to default behaviour
                    break;
            }
            if(RV2Log.ShouldLog(true, "Preferences"))
                RV2Log.Message($"Default behaviour for stripping: {RV2Mod.Settings.fineTuning.DefaultStripBehaviour}", false, "Preferences");
            switch(RV2Mod.Settings.fineTuning.DefaultStripBehaviour)
            {
                case DefaultStripSetting.Random:
                    return Rand.Chance(RV2Mod.Settings.cheats.BaseStripChance);
                case DefaultStripSetting.AlwaysStrip:
                    return true;
                case DefaultStripSetting.NeverStrip:
                    return false;
                default:
                    return false;
            }
        }
    }
}
