#if !v1_5
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimVore2
{
    /// <summary>
    /// FloatMenuOptionProvider classes are instanced and tracked by <see cref="FloatMenuMakerMap.Init"/>
    /// </summary>
    public class FloatMenuOptionProvider_Vore : FloatMenuOptionProvider
    {
        protected override bool CanSelfTarget => true;
        protected override bool Drafted => true;
        protected override bool Undrafted => true;
        protected override bool Multiselect => false;

        public override bool SelectedPawnValid(Pawn pawn, FloatMenuContext context)
        {
            if(!RV2Mod.Settings.features.ShowRMBVoreMenu)
            {
                return false;
            }
            if(pawn.jobs == null)
            {
                return false;
            }
            return base.SelectedPawnValid(pawn, context);
        }

        public override bool TargetPawnValid(Pawn pawn, FloatMenuContext context)
        {
            if(pawn.IsBurning())
            {
                return false;
            }
            if(pawn.HostileTo(Faction.OfPlayer) && !pawn.Downed)
            {
                return false;
            }
            if(pawn.InMentalState)
            {
                return false;
            }
            Pawn firstPawn = context.FirstSelectedPawn;
            if(firstPawn != null && pawn == context.FirstSelectedPawn)
            {
                return true;
            }
            if(firstPawn.CanReach(pawn, Verse.AI.PathEndMode.ClosestTouch, Danger.Deadly))
            {
                return true;
            }
            return false;
        }

        public override IEnumerable<FloatMenuOption> GetOptionsFor(Pawn clickedPawn, FloatMenuContext context)
        {
            Pawn firstPawn = context.FirstSelectedPawn;
            if(firstPawn == null)
            {
                yield break;
            }
            if(clickedPawn == firstPawn)
            {
                foreach(FloatMenuOption item in GetOptionsForSelf(firstPawn))
                {
                    yield return item;
                }
            }
            else
            {
                FloatMenuOption targetOption = GetOptionForTarget(firstPawn, clickedPawn);
                if(targetOption != null)
                {
                    yield return targetOption;
                }
            }
        }

        private static FloatMenuOption GetOptionForTarget(Pawn selfPawn, Pawn targetPawn)
        {
            List<FloatMenuOption> options = GetOptionsForTarget(selfPawn, targetPawn).ToList();
            if(options.Any())
            {
                return new FloatMenuOption("RV2_RMB_VoreTarget".Translate(targetPawn.LabelShort), () => Find.WindowStack.Add(new FloatMenu(options)));
            }
            return null;
        }

        private static IEnumerable<FloatMenuOption> GetOptionsForSelf(Pawn pawn)
        {
            FloatMenuOption option = GetEjectOption(pawn, pawn);
            if(option != null)
            {
                yield return option;
            }
            option = GetManualPassConditionOption(pawn);
            if(option != null)
            {
                yield return option;
            }
            option = GetStageJumpOption(pawn);
            if(option != null)
            {
                yield return option;
            }
        }

        private static FloatMenuOption GetEjectOption(Pawn initiator, Pawn target)
        {
            bool isForSelf = initiator == target;
            string optionLabel = isForSelf ? "RV2_RMB_EjectPrey_Self".Translate() : "RV2_RMB_EjectPrey_Target".Translate(target.LabelShortCap);
            if(!isForSelf && VoreStatDefOf.RV2_ExternalEjectChance.Worker.IsDisabledFor(initiator))
            {
                if(RV2Mod.Settings.fineTuning.ShowInvalidRMBOptions)
                {
                    optionLabel += $" ({"RV2_RMB_EjectIncapable".Translate()})";
                    return UIUtility.DisabledOption(optionLabel);
                }
                return null;
            }
            if(GetEjectOptions(initiator, target, out List<FloatMenuOption> options))
            {
                return new FloatMenuOption(optionLabel, () => Find.WindowStack.Add(new FloatMenu(options)));
            }
            return null;
        }

        private static bool GetEjectOptions(Pawn initiator, Pawn predator, out List<FloatMenuOption> options)
        {
            bool isForSelf = initiator == predator;
            options = new List<FloatMenuOption>();
            // get the tracker for the predator, then limit the eject options to valid targets
            List<VoreTrackerRecord> records = predator.PawnData()?.VoreTracker?.VoreTrackerRecords?.FindAll(r => r.CanEject);
            // no releasable prey exists
            if(records.NullOrEmpty())
            {
                return false;
            }
            foreach(VoreTrackerRecord record in records)
            {
                Pawn prey = record.Prey;
                string optionLabel = record.DisplayLabel;
                bool canAttemptEject = isForSelf
                    || !record.WasExternalEjectAttempted
                    || RV2Mod.Settings.cheats.AllowMultipleExternalEjectAttempts;
                FloatMenuOption option;
                if(canAttemptEject)
                {
                    option = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(optionLabel, delegate ()
                    {
                        Job ejectJob;
                        if(isForSelf)
                        {
                            ejectJob = JobMaker.MakeJob(VoreJobDefOf.RV2_EjectPreySelf, prey);
                        }
                        else
                        {
                            ejectJob = JobMaker.MakeJob(VoreJobDefOf.RV2_EjectPreyForce, predator, prey);
                        }
                        initiator.jobs.TryTakeOrderedJob(ejectJob);
                    }), initiator, predator);
                }
                else
                {
                    optionLabel += $" ({"RV2_RMB_AlreadyAttemptedEject".Translate()})";
                    option = UIUtility.DisabledOption(optionLabel);
                }
                options.Add(option);
            }

            return true;
        }

        private static FloatMenuOption GetManualPassConditionOption(Pawn pawn)
        {
            VoreTracker tracker = pawn.PawnData()?.VoreTracker;
            if(tracker == null || !tracker.IsTrackingVore)
            {
                return null;
            }
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            foreach(VoreTrackerRecord record in tracker.VoreTrackerRecords)
            {
                bool recordNeedsManualPass = record.IsManuallyPassed == false   // don't allow option if record is already manually passed
                    && record.CurrentVoreStage.def.passConditions   // take all pass conditions of the current stage
                        .Any(condition => condition is StagePassCondition_Manual);  // and check if any of them is a manual pass

                if(recordNeedsManualPass)
                {
                    options.Add(new FloatMenuOption(record.Prey.LabelShortCap, () => record.IsManuallyPassed = true));
                }
            }

            if(options.Count > 0)
            {
                return new FloatMenuOption("RV2_RMB_ManualPass".Translate(), () => Find.WindowStack.Add(new FloatMenu(options)));
            }
            return null;
        }

        private static FloatMenuOption GetStageJumpOption(Pawn pawn)
        {
            VoreTracker tracker = pawn.PawnData()?.VoreTracker;
            if(tracker == null || !tracker.IsTrackingVore)
            {
                return null;
            }
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            foreach(VoreTrackerRecord record in tracker.VoreTrackerRecords)
            {
                string jumpKey = record.CurrentVoreStage.def.jumpKey;
                if(jumpKey == null) // stage has no jump capability, skip
                {
                    continue;
                }
                IEnumerable<VoreJump> jumps = JumpUtility.Jumps(record.Predator, record.Prey, jumpKey);
                if(jumps.EnumerableNullOrEmpty())  // no jump paths valid, skip
                {
                    continue;
                }
                List<FloatMenuOption> jumpOptions = jumps
                    .Where(jump => jump.path != record.VorePath.def // don't present the already running vore as an option
                    && jump.path.voreType == record.VorePath.VoreType)  // limit to paths that have the same type (prevents crossing oral -> anal for stomach e.g.)
                    .Select(jump => new FloatMenuOption(jump.path.voreGoal.label, () => jump.Jump(record)))
                    .ToList();

                options.Add(new FloatMenuOption(record.DisplayLabel, () => Find.WindowStack.Add(new FloatMenu(jumpOptions))));
            }

            if(options.Count > 0)
            {
                return new FloatMenuOption("RV2_RMB_JumpGoal".Translate(), () => Find.WindowStack.Add(new FloatMenu(options)));
            }
            return null;
        }

        private static IEnumerable<FloatMenuOption> GetOptionsForTarget(Pawn selfPawn, Pawn targetPawn)
        {
            FloatMenuOption ejectOption = GetEjectOption(selfPawn, targetPawn);
            if(ejectOption != null)
            {
                yield return ejectOption;
            }
            foreach(FloatMenuOption option in GetVoreProposalOptions(selfPawn, targetPawn))
            {
                yield return option;
            }
            foreach(FloatMenuOption option in GetDirectVoreOptions(selfPawn, targetPawn))
            {
                yield return option;
            }
            FloatMenuOption feedOption = GetDevFeedOption(selfPawn, targetPawn);
            if(feedOption != null)
            {
                yield return feedOption;
            }
        }

        private static IEnumerable<FloatMenuOption> GetVoreProposalOptions(Pawn initiator, Pawn target)
        {
            VoreInteraction initiatorAsPrey = VoreInteractionManager.Retrieve(new VoreInteractionRequest(initiator, target, VoreRole.Prey));
            VoreInteraction initiatorAsPredator = VoreInteractionManager.Retrieve(new VoreInteractionRequest(initiator, target, VoreRole.Predator));

            FloatMenuOption option = GetVoreAsPredatorOption(initiator, target, initiatorAsPredator, true);
            if(option != null)
            {
                yield return option;
            }
            option = GetVoreAsPreyOption(initiator, target, initiatorAsPrey, true);
            if(option != null)
            {
                yield return option;
            }
            foreach(FloatMenuOption feederOption in GetVoreAsFeederOptions(initiator, target))
            {
                yield return feederOption;
            }
        }

        private static List<FloatMenuOption> GetDirectVoreOptions(Pawn initiator, Pawn target)
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            if(!Prefs.DevMode)
            {
                return options;
            }
            VoreInteraction initiatorAsPrey = VoreInteractionManager.Retrieve(new VoreInteractionRequest(initiator, target, VoreRole.Prey));
            VoreInteraction initiatorAsPredator = VoreInteractionManager.Retrieve(new VoreInteractionRequest(initiator, target, VoreRole.Predator));
            if(RV2Log.ShouldLog(false, "VoreInteractions"))
            {
                RV2Log.Message(initiatorAsPredator.ToString(), true, "VoreInteractions");
                RV2Log.Message(initiatorAsPrey.ToString(), true, "VoreInteractions");
            }
            FloatMenuOption option = GetVoreAsPredatorOption(initiator, target, initiatorAsPredator);
            if(option != null)
            {
                options.Add(option);
            }
            option = GetVoreAsPreyOption(initiator, target, initiatorAsPrey);
            if(option != null)
            {
                options.Add(option);
            }
            option = GetVoreAsPredatorOption(initiator, target, initiatorAsPredator, false, true);
            if(option != null)
            {
                options.Add(option);
            }
            option = GetVoreAsPreyOption(initiator, target, initiatorAsPrey, false, true);
            if(option != null)
            {
                options.Add(option);
            }

            return options;
        }

        private static FloatMenuOption GetVoreAsPredatorOption(Pawn initiator, Pawn target, VoreInteraction interaction, bool isForProposal = false, bool isForced = false)
        {
            Pawn prey = interaction.Prey;
            string optionLabel = isForProposal ? "RV2_RMB_ProposeAsPredator" : "RV2_RMB_InitiateAsPredator";
            optionLabel = optionLabel.Translate(prey.LabelShortCap);
            bool interactionIsValid = interaction.IsValid;
            string reason = interaction.InteractionInvalidReason;
            bool mustRespectCooldown = !isForced
                && isForProposal && RV2Mod.Settings.features.RmbProposalsUseAutoVoreInterval;
            if(mustRespectCooldown)
            {
                if(!Prefs.DevMode && !WorkGiver_ProposeVore.HasCooldownPassed(initiator))
                {
                    reason = "RV2_VoreInvalidReasons_ProposalCooldown".Translate();
                    interactionIsValid = false;
                }
            }
            if(isForced)
                optionLabel += " (Forced)";
            if(interactionIsValid)
            {
                if(isForProposal && !interaction.IsValidForProposal)
                {
                    reason = interaction.PreferenceInvalidReason;
                }
                else
                {
                    JobDef jobDef = isForProposal ? VoreJobDefOf.RV2_ProposeVore : VoreJobDefOf.RV2_VoreInitAsPredator;
                    Func<VorePathDef, VoreJob> jobMaker = delegate (VorePathDef path)
                    {
                        VoreJob job = VoreJobMaker.MakeJob(jobDef, initiator, target);
                        job.targetA = target;
                        job.VorePath = path;
                        job.IsForced = isForced;
                        if(isForProposal)
                        {
                            job.Proposal = new VoreProposal_TwoWay(initiator, target, initiator, target, path);
                            if(RV2Mod.Settings.features.RmbProposalsUseAutoVoreInterval)
                            {
                                WorkGiver_ProposeVore.SetProposalTick(initiator);
                            }
                        }
                        return job;
                    };
                    List<FloatMenuOption> goalOptions = GetVoreGoalOptions(initiator, target, interaction, jobMaker, isForProposal);
                    if(goalOptions.Any())
                    {
                        return new FloatMenuOption(optionLabel, () => Find.WindowStack.Add(new FloatMenu(goalOptions)));
                    }
                }
            }

            if(!RV2Mod.Settings.fineTuning.ShowInvalidRMBOptions)
            {
                return null;
            }
            optionLabel += " (" + reason + ")";
            return UIUtility.DisabledOption(optionLabel);
        }

        private static FloatMenuOption GetVoreAsPreyOption(Pawn initiator, Pawn target, VoreInteraction interaction, bool isForProposal = false, bool isForced = false)
        {
            Pawn predator = interaction.Predator;
            string optionLabel = isForProposal ? "RV2_RMB_ProposeAsPrey" : "RV2_RMB_InitiateAsPrey";
            optionLabel = optionLabel.Translate(predator.LabelShortCap);
            bool interactionIsValid = interaction.IsValid;
            string reason = interaction.InteractionInvalidReason;
            bool mustRespectCooldown = !isForced
                && isForProposal && RV2Mod.Settings.features.RmbProposalsUseAutoVoreInterval;
            if(mustRespectCooldown)
            {
                if(!Prefs.DevMode && !WorkGiver_ProposeVore.HasCooldownPassed(initiator))
                {
                    reason = "RV2_VoreInvalidReasons_ProposalCooldown".Translate();
                    interactionIsValid = false;
                }
            }
            if(isForced)
                optionLabel += " (Forced)";
            if(interactionIsValid)
            {
                if(isForProposal && !interaction.IsValidForProposal)
                {
                    reason = interaction.PreferenceInvalidReason;
                }
                else
                {
                    JobDef jobDef = isForProposal ? VoreJobDefOf.RV2_ProposeVore : VoreJobDefOf.RV2_VoreInitAsPrey;
                    Func<VorePathDef, VoreJob> jobMaker = delegate (VorePathDef path)
                    {
                        VoreJob job = VoreJobMaker.MakeJob(jobDef, initiator, target);
                        job.targetA = target;
                        job.VorePath = path;
                        job.IsForced = isForced;
                        if(isForProposal)
                        {
                            job.Proposal = new VoreProposal_TwoWay(target, initiator, initiator, target, path);
                            if(RV2Mod.Settings.features.RmbProposalsUseAutoVoreInterval)
                            {
                                WorkGiver_ProposeVore.SetProposalTick(initiator);
                            }
                        }
                        return job;
                    };
                    List<FloatMenuOption> goalOptions = GetVoreGoalOptions(initiator, target, interaction, jobMaker, isForProposal);
                    if(goalOptions.Any())
                    {
                        return new FloatMenuOption(optionLabel, () => Find.WindowStack.Add(new FloatMenu(goalOptions)));
                    }
                }
            }

            if(!RV2Mod.Settings.fineTuning.ShowInvalidRMBOptions)
            {
                return null;
            }
            optionLabel += $" ({reason})";
            return UIUtility.DisabledOption(optionLabel);
        }

        private static WorkGiver_ProposeVore ProposeVoreWorkGiver => RV2_Common.WorkGiver_ProposeVoreDef.Worker as WorkGiver_ProposeVore;
        private static Job cachedFeederJob;
        private static int cachedFeederJobKey = -1;
        private static string cachedReason = null;
        private static IEnumerable<FloatMenuOption> GetVoreAsFeederOptions(Pawn initiator, Pawn targetPredator)
        {
            if(!RV2Mod.Settings.features.FeederVoreEnabled)
            {
                yield break;
            }

            if(ProposeVoreWorkGiver == null)
            {
                RV2Log.Warning("Could not retrieve WorkGiver for ProposeVore! Unable to determine feeding vore options.", true);
                yield break;
            }

            Job randomPreyJob = null;
            int feederJobKey = initiator.GetHashCode() + targetPredator.GetHashCode();
            if(cachedFeederJobKey == -1 || cachedFeederJobKey != feederJobKey)
            {
                cachedFeederJobKey = feederJobKey;
                // always reset the feeder job here, in case anything errors out on recalculation, we do not want to keep the cached job around
                cachedFeederJob = null;
                bool isValid = targetPredator.CanParticipateInVore(out string reason)
                    && ProposeVoreWorkGiver.CanBeFeeder(initiator, targetPredator, out reason);
                if(isValid)
                {
                    cachedFeederJob = ProposeVoreWorkGiver.TryMakeFeederJob(initiator, targetPredator);
                }
                cachedReason = reason;
            }
            randomPreyJob = cachedFeederJob;

            string randomPreyOptionLabel = "RV2_RMB_ProposeAsFeederRandomPrey".Translate(targetPredator);
            string targetedPreyOptionLabel = "RV2_RMB_ProposeAsFeederTargetPrey".Translate(targetPredator);
            bool proposalIsValid = randomPreyJob != null;
            string reasonString = $" ({cachedReason})";
            if(RV2Mod.Settings.features.RmbProposalsUseAutoVoreInterval)
            {
                if(!Prefs.DevMode && !WorkGiver_ProposeVore.HasCooldownPassed(initiator))
                {
                    reasonString = $" ({"RV2_VoreInvalidReasons_ProposalCooldown".Translate()})";
                    proposalIsValid = false;
                }
            }
            if(!proposalIsValid)
            {
                // if the main job for feeding vore with random prey is not available, feeding vore in general is unavailable
                if(!RV2Mod.Settings.fineTuning.ShowInvalidRMBOptions)
                {
                    yield break;
                }
                randomPreyOptionLabel += reasonString;
                targetedPreyOptionLabel += reasonString;
                yield return UIUtility.DisabledOption(randomPreyOptionLabel);
                yield return UIUtility.DisabledOption(targetedPreyOptionLabel);
            }
            else
            {
                // the job already randomly retrieved the targets, so no extra work is required
                Action randomPreyOptionAction = () =>
                {
                    initiator.jobs.TryTakeOrderedJob(randomPreyJob);
                    if(RV2Mod.Settings.features.RmbProposalsUseAutoVoreInterval)
                    {
                        WorkGiver_ProposeVore.SetProposalTick(initiator);
                    }
                };
                yield return new FloatMenuOption(randomPreyOptionLabel, randomPreyOptionAction);

                // the targeted prey job is a bit more tricky, requiring additional targeting logic to select the prey
                // I honestly hate how this code turned out, with three nested locals to create the job, but oh well.
                Action targetedPreyOptionAction = () =>
                {
                    List<Pawn> validWorkGiverTargets = ProposeVoreWorkGiver.ValidFeederPrey(initiator, targetPredator).ToList();
                    TargetingParameters targetingParameters = new TargetingParameters()
                    {
                        canTargetBuildings = false,
                        validator = (TargetInfo targetPrey) => targetPrey.HasThing && validWorkGiverTargets.Contains(targetPrey.Thing),
                    };
                    Action<LocalTargetInfo> selectedTargetAction = (LocalTargetInfo targetPrey) =>
                    {
                        initiator.jobs.EndCurrentJob(JobCondition.InterruptForced);
                        VoreJob job = VoreJobMaker.MakeJob(VoreJobDefOf.RV2_ProposeVore_Feeder, initiator, targetPredator, targetPrey);
                        job.Proposal = new VoreProposal_Feeder_Predator(initiator, targetPredator);
                        job.count = 1;
                        initiator.jobs.TryTakeOrderedJob(job);
                        if(RV2Mod.Settings.features.RmbProposalsUseAutoVoreInterval)
                        {
                            WorkGiver_ProposeVore.SetProposalTick(initiator);
                        }
                    };
                    Find.Targeter.BeginTargeting(targetingParameters, selectedTargetAction);
                };
                yield return new FloatMenuOption(targetedPreyOptionLabel, targetedPreyOptionAction);
            }
        }

        private static FloatMenuOption GetDevFeedOption(Pawn feeder, Pawn prey)
        {
            if(!RV2Mod.Settings.features.FeederVoreEnabled || !Prefs.DevMode)
            {
                return null;
            }
            string optionLabel = "RV2_RMB_AsFeeder".Translate(prey);
            if(!VoreFeedUtility.CanFeedToOthers(prey, out string reason))
            {
                optionLabel += " (" + reason + ")";
                return UIUtility.DisabledOption(optionLabel);
            }
            return new FloatMenuOption(optionLabel, () =>
            {
                Find.Targeter.BeginTargeting(VoreFeedUtility.PredatorParameters(feeder, prey), (LocalTargetInfo targetInfo) =>
                {
                    Pawn predator = targetInfo.Pawn;
                    VoreInteraction interaction = VoreInteractionManager.Retrieve(new VoreInteractionRequest(predator, prey, VoreRole.Predator));
                    Func<VorePathDef, VoreJob> jobMaker = (VorePathDef path) =>
                    {
                        VoreJob job = VoreJobMaker.MakeJob(VoreJobDefOf.RV2_VoreInitAsFeeder, feeder, prey, predator);
                        job.targetA = prey;
                        job.targetB = predator;
                        job.VorePath = path;
                        return job;
                    };
                    List<FloatMenuOption> goalOptions = GetVoreGoalOptions(feeder, prey, interaction, jobMaker);
                    if(goalOptions != null)
                    {
                        Find.WindowStack.Add(new FloatMenu(goalOptions));
                    }
                });
            });
        }

        private static List<FloatMenuOption> GetVoreGoalOptions(Pawn initiator, Pawn target, VoreInteraction interaction, Func<VorePathDef, VoreJob> jobMaker, bool isForProposal = false)
        {
            List<FloatMenuOption> options = GetRandomVoreOptions(initiator, target, interaction, jobMaker);

            foreach(VoreGoalDef goal in interaction.ValidGoals)
            {
                string optionLabel = goal.LabelCap;
                List<FloatMenuOption> pathOptions = GetVorePathOptions(initiator, target, interaction, jobMaker, goal, isForProposal);
                if(pathOptions.Any())
                {
                    options.Add(new FloatMenuOption(optionLabel, () => Find.WindowStack.Add(new FloatMenu(pathOptions))));
                }
            }
            if(RV2Mod.Settings.fineTuning.ShowInvalidRMBOptions)
            {
                foreach(KeyValuePair<VoreGoalDef, string> invalidGoal in interaction.InvalidGoals)
                {
                    string optionLabel = $"{invalidGoal.Key.LabelCap} ({invalidGoal.Value})";
                    options.Add(UIUtility.DisabledOption(optionLabel));
                }
            }

            return options;
        }

        private static List<FloatMenuOption> GetRandomVoreOptions(Pawn initiator, Pawn target, VoreInteraction interaction, Func<VorePathDef, VoreJob> jobMaker)
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            string randomLabel = "RV2_RMB_Random".Translate();
            VorePathDef randomPathDef = interaction.ValidPaths.RandomElementWithFallback();
            MakeOption(randomLabel, randomPathDef);

            string randomPreferredlabel = "RV2_RMB_RandomPreferred".Translate();
            VorePathDef preferredPathDef = interaction.PreferredPath;
            MakeOption(randomPreferredlabel, preferredPathDef);

            void MakeOption(string label, VorePathDef path)
            {
                if(path == null)
                {
                    if(RV2Mod.Settings.fineTuning.ShowInvalidRMBOptions)
                    {
                        options.Add(UIUtility.DisabledOption(label));
                    }
                }
                else
                {
                    Action action = () =>
                    {
                        initiator.jobs.EndCurrentJob(JobCondition.InterruptForced);
                        VoreJob job = jobMaker(path);
                        job.targetA = target;
                        job.VorePath = path;
                        initiator.jobs.TryTakeOrderedJob(job);
                    };
                    options.Add(new FloatMenuOption(label, action));
                }
            }
            return options;
        }

        private static List<FloatMenuOption> GetVorePathOptions(Pawn initiator, Pawn target, VoreInteraction interaction, Func<VorePathDef, VoreJob> jobMaker, VoreGoalDef goal, bool isForProposal = false)
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            foreach(VorePathDef pathDef in interaction.ValidPathsFor(goal))
            {
                string optionLabel = pathDef.RMBLabel;

                FloatMenuOption option = new FloatMenuOption(optionLabel, () =>
                {
                    initiator.jobs.EndCurrentJob(JobCondition.InterruptForced);
                    VoreJob job = jobMaker(pathDef);
                    job.targetA = target;
                    job.VorePath = pathDef;
                    initiator.jobs.TryTakeOrderedJob(job);
                });
                // this will disable the option with the "reserved by" text if it can't be executed
                option = FloatMenuUtility.DecoratePrioritizedTask(option, initiator, target);
                options.Add(option);
            }
            if(RV2Mod.Settings.fineTuning.ShowInvalidRMBOptions)
            {
                foreach(KeyValuePair<VorePathDef, string> invalidPath in interaction.InvalidPathsFor(goal))
                {
                    string optionLabel = $"{invalidPath.Key.RMBLabel} ({invalidPath.Value})";
                    options.Add(UIUtility.DisabledOption(optionLabel));
                }
            }
            return options;
        }
    }
}

#endif