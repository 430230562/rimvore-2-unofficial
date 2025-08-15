using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;


/// <summary>
/// I am constantly unhappy with how the RMB menu is built. The current approach tries to prevent empty float menus and has the invalid options properly baked in
/// Most menu functions return a true if they offer any options, which is checked in their calling function - this should make the entire system more maintaineable, but I am not sure
/// I will happily accept any suggestions for a more "robust" system that is equally variable
/// 
/// I have begun to hate this class and everything in it. If you dare delve into the magnomious depravity that are the inner machinations of my mentally insane neurons, take this to steel your resolve: https://media.tenor.com/BVESjhxVxrgAAAAd/bath-time-tippy-tapping.gif
/// </summary>
namespace RimVore2
{
    [HarmonyPatch(typeof(FloatMenuMakerMap), "ChoicesAtFor")]
    static class RV2_Patch_UI_RMB_AddHumanlikeOrders
    {
        private static readonly TargetingParameters voreTargetParameters = new TargetingParameters()
        {
            canTargetPawns = true,
            canTargetBuildings = false,
            canTargetItems = true,
            canTargetAnimals = true,
            mapObjectTargetsMustBeAutoAttackable = true
        };

        [HarmonyPostfix]
        private static void AddVoreOptions(List<FloatMenuOption> __result, Vector3 clickPos, Pawn pawn)
        {
            List<FloatMenuOption> backupResult = __result;
            try
            {
                if(!RV2Mod.Settings.features.ShowRMBVoreMenu)
                {
                    return;
                }
                if(pawn.jobs == null)
                {
                    return;
                }
                List<FloatMenuOption> voreOptions = new List<FloatMenuOption>();
                List<LocalTargetInfo> validTargets = GenUI.TargetsAt(clickPos, voreTargetParameters).ToList();
                validTargets = validTargets
                    .FindAll(target => target.Pawn != null
                    && !target.Pawn.IsBurning()
                    && !target.Pawn.HostileTo(Faction.OfPlayer) || target.Pawn.Downed
                    && !target.Pawn.InMentalState);

                if(!validTargets.NullOrEmpty())
                {
                    foreach(LocalTargetInfo target in validTargets)
                    {
                        if(target.Pawn == pawn)
                        {
                            if(DoSelfOptions(pawn, out List<FloatMenuOption> optionsToAdd))
                            {
                                voreOptions.AddRange(optionsToAdd);
                            }
                        }
                        else if(pawn.CanReach(target, PathEndMode.ClosestTouch, Danger.Deadly))
                        {
                            if(DoTargetOptions(pawn, target.Pawn, out List<FloatMenuOption> optionsToAdd))
                            {
                                voreOptions.AddRange(optionsToAdd);
                            }
                        }
                    }
                }

                if(!voreOptions.NullOrEmpty())
                {
                    FloatMenuOption voreOption = new FloatMenuOption("RV2_RMB_Home".Translate(), () => Find.WindowStack.Add(new FloatMenu(voreOptions)));

                    __result.Add(voreOption);
                }
            }
            catch(Exception e)
            {
                Log.Error("Something went wrong when RimVore-2 tried to add vore options to RMB:\n" + e);
                __result = backupResult;
            }
        }

        private static bool DoSelfOptions(Pawn pawn, out List<FloatMenuOption> options)
        {
            options = new List<FloatMenuOption>();
            FloatMenuOption option;
            if(DoEjectOption(pawn, pawn, out option))
            {
                options.Add(option);
            }
            if(DoManualPassConditionOption(pawn, out option))
            {
                options.Add(option);
            }
            if(DoStageJumpOption(pawn, out option))
            {
                options.Add(option);
            }
            return !options.NullOrEmpty();
        }

        private static bool DoTargetOptions(Pawn initiator, Pawn target, out List<FloatMenuOption> options)
        {
            options = new List<FloatMenuOption>();
            FloatMenuOption option;
            List<FloatMenuOption> optionsToAdd;
            if(DoEjectOption(initiator, target, out option))
            {
                options.Add(option);
            }
            if(DoVoreProposalOptions(initiator, target, out optionsToAdd))
            {
                options.AddRange(optionsToAdd);
            }
            if(DoDirectVoreOptions(initiator, target, out optionsToAdd))
            {
                options.AddRange(optionsToAdd);
            }
            if(DoDevFeedOption(initiator, target, out option))
            {
                options.Add(option);
            }
            if(options.NullOrEmpty())
            {
                return false;
            }
            return true;
        }

        private static bool DoEjectOption(Pawn initiator, Pawn target, out FloatMenuOption option)
        {
            option = null;
            bool isForSelf = initiator == target;
            string optionLabel = isForSelf ? "RV2_RMB_EjectPrey_Self".Translate() : "RV2_RMB_EjectPrey_Target".Translate(target.LabelShortCap);
            if(!isForSelf && VoreStatDefOf.RV2_ExternalEjectChance.Worker.IsDisabledFor(initiator))
            {
                if(RV2Mod.Settings.fineTuning.ShowInvalidRMBOptions)
                {
                    optionLabel += $" ({"RV2_RMB_EjectIncapable".Translate()})";
                    option = UIUtility.DisabledOption(optionLabel);
                    return true;
                }
                return false;
            }
            if(DoEjectOptions(initiator, target, out List<FloatMenuOption> options))
            {
                option = new FloatMenuOption(optionLabel, () => Find.WindowStack.Add(new FloatMenu(options)));
                return true;
            }
            return false;
        }

        private static bool DoEjectOptions(Pawn initiator, Pawn predator, out List<FloatMenuOption> options)
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

        private static bool DoDevFeedOption(Pawn feeder, Pawn prey, out FloatMenuOption option)
        {
            option = null;
            if(!RV2Mod.Settings.features.FeederVoreEnabled || !Prefs.DevMode)
            {
                return false;
            }
            string optionLabel = "RV2_RMB_AsFeeder".Translate(prey);
            if(!VoreFeedUtility.CanFeedToOthers(prey, out string reason))
            {
                optionLabel += " (" + reason + ")";
                option = new FloatMenuOption(optionLabel, () => { })
                {
                    Disabled = true
                };
                return true;
            }
            option = new FloatMenuOption(optionLabel, () =>
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
                    if(DoVoreGoalOptions(feeder, prey, interaction, jobMaker, out List<FloatMenuOption> options))
                    {
                        Find.WindowStack.Add(new FloatMenu(options));
                    }
                });
            });
            return true;
        }

        private static bool DoManualPassConditionOption(Pawn pawn, out FloatMenuOption option)
        {
            option = null;
            VoreTracker tracker = pawn.PawnData()?.VoreTracker;
            if(tracker == null || !tracker.IsTrackingVore)
            {
                return false;
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
                option = new FloatMenuOption("RV2_RMB_ManualPass".Translate(), () => Find.WindowStack.Add(new FloatMenu(options)));
            }
            return option != null;
        }

        private static bool DoStageJumpOption(Pawn pawn, out FloatMenuOption option)
        {
            option = null;
            VoreTracker tracker = pawn.PawnData()?.VoreTracker;
            if(tracker == null || !tracker.IsTrackingVore)
            {
                return false;
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
                option = new FloatMenuOption("RV2_RMB_JumpGoal".Translate(), () => Find.WindowStack.Add(new FloatMenu(options)));
            }
            return option != null;
        }

        private static bool DoDirectVoreOptions(Pawn initiator, Pawn target, out List<FloatMenuOption> options)
        {
            options = new List<FloatMenuOption>();
            FloatMenuOption option;
            if(!Prefs.DevMode)
            {
                return false;
            }
            VoreInteraction initiatorAsPrey = VoreInteractionManager.Retrieve(new VoreInteractionRequest(initiator, target, VoreRole.Prey));
            VoreInteraction initiatorAsPredator = VoreInteractionManager.Retrieve(new VoreInteractionRequest(initiator, target, VoreRole.Predator));
            if(RV2Log.ShouldLog(false, "VoreInteractions"))
            {
                RV2Log.Message(initiatorAsPredator.ToString(), true, "VoreInteractions");
                RV2Log.Message(initiatorAsPrey.ToString(), true, "VoreInteractions");
            }
            if(DoVoreAsPredatorOption(initiator, target, initiatorAsPredator, out option))
            {
                options.Add(option);
            }
            if(DoVoreAsPreyOption(initiator, target, initiatorAsPrey, out option))
            {
                options.Add(option);
            }
            if(DoVoreAsPredatorOption(initiator, target, initiatorAsPredator, out option, false, true))
            {
                options.Add(option);
            }
            if(DoVoreAsPreyOption(initiator, target, initiatorAsPrey, out option, false, true))
            {
                options.Add(option);
            }

            return !options.NullOrEmpty();
        }

        private static bool DoVoreProposalOptions(Pawn initiator, Pawn target, out List<FloatMenuOption> options)
        {
            options = new List<FloatMenuOption>();

            FloatMenuOption option;

            VoreInteraction initiatorAsPrey = VoreInteractionManager.Retrieve(new VoreInteractionRequest(initiator, target, VoreRole.Prey));
            VoreInteraction initiatorAsPredator = VoreInteractionManager.Retrieve(new VoreInteractionRequest(initiator, target, VoreRole.Predator));

            if(DoVoreAsPredatorOption(initiator, target, initiatorAsPredator, out option, true))
            {
                options.Add(option);
            }
            if(DoVoreAsPreyOption(initiator, target, initiatorAsPrey, out option, true))
            {
                options.Add(option);
            }
            if(DoVoreAsFeederOptions(initiator, target, out List<FloatMenuOption> feederOptions))
            {
                options.AddRange(feederOptions);
            }

            return !options.NullOrEmpty();
        }

        private static bool DoVoreAsPredatorOption(Pawn initiator, Pawn target, VoreInteraction interaction, out FloatMenuOption option, bool isForProposal = false, bool isForced = false)
        {
            option = null;
            Pawn prey = interaction.Prey;
            string optionLabel = isForProposal ? "RV2_RMB_ProposeAsPredator" : "RV2_RMB_InitiateAsPredator";
            optionLabel = optionLabel.Translate(prey.LabelShortCap);
            bool interactionIsValid = interaction.IsValid;
            string reason = interaction.InteractionInvalidReason;
            bool mustRespectCooldown = !isForced
                && isForProposal && RV2Mod.Settings.features.RmbProposalsUseAutoVoreInterval;
            if (mustRespectCooldown)
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
                    if(DoVoreGoalOptions(initiator, target, interaction, jobMaker, out List<FloatMenuOption> options, isForProposal))
                    {
                        option = new FloatMenuOption(optionLabel, () => Find.WindowStack.Add(new FloatMenu(options)));
                        return true;
                    }
                }
            }

            if(!RV2Mod.Settings.fineTuning.ShowInvalidRMBOptions)
            {
                return false;
            }
            optionLabel += " (" + reason + ")";
            option = UIUtility.DisabledOption(optionLabel);
            return true;
        }

        private static bool DoVoreAsPreyOption(Pawn initiator, Pawn target, VoreInteraction interaction, out FloatMenuOption option, bool isForProposal = false, bool isForced = false)
        {
            option = null;
            Pawn predator = interaction.Predator;
            string optionLabel = isForProposal ? "RV2_RMB_ProposeAsPrey" : "RV2_RMB_InitiateAsPrey";
            optionLabel = optionLabel.Translate(predator.LabelShortCap);
            bool interactionIsValid = interaction.IsValid;
            string reason = interaction.InteractionInvalidReason;
            bool mustRespectCooldown = !isForced
                && isForProposal && RV2Mod.Settings.features.RmbProposalsUseAutoVoreInterval;
            if (mustRespectCooldown)
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
                    if(DoVoreGoalOptions(initiator, target, interaction, jobMaker, out List<FloatMenuOption> options, isForProposal))
                    {
                        option = new FloatMenuOption(optionLabel, () => Find.WindowStack.Add(new FloatMenu(options)));
                        return true;
                    }
                }
            }

            if(!RV2Mod.Settings.fineTuning.ShowInvalidRMBOptions)
            {
                return false;
            }
            optionLabel += $" ({reason})";
            option = UIUtility.DisabledOption(optionLabel);
            return true;
        }

        private static WorkGiver_ProposeVore ProposeVoreWorkGiver => RV2_Common.WorkGiver_ProposeVoreDef.Worker as WorkGiver_ProposeVore;
        private static Job cachedFeederJob;
        private static int cachedFeederJobKey = -1;
        private static string cachedReason = null;
        private static bool DoVoreAsFeederOptions(Pawn initiator, Pawn targetPredator, out List<FloatMenuOption> options)
        {
            options = new List<FloatMenuOption>();
            if(!RV2Mod.Settings.features.FeederVoreEnabled)
            {
                return false;
            }

            if(ProposeVoreWorkGiver == null)
            {
                RV2Log.Warning("Could not retrieve WorkGiver for ProposeVore! Unable to determine feeding vore options.", true);
                return false;
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
                    return false;
                }
                randomPreyOptionLabel += reasonString;
                targetedPreyOptionLabel += reasonString;
                options.Add(UIUtility.DisabledOption(randomPreyOptionLabel));
                options.Add(UIUtility.DisabledOption(targetedPreyOptionLabel));
                return true;
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
                options.Add(new FloatMenuOption(randomPreyOptionLabel, randomPreyOptionAction));

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
                    Action<LocalTargetInfo> selectedTargetAction = (LocalTargetInfo targetPrey) => {
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
                options.Add(new FloatMenuOption(targetedPreyOptionLabel, targetedPreyOptionAction));
                return true;
            }
        }

        private static bool DoVoreGoalOptions(Pawn initiator, Pawn target, VoreInteraction interaction, Func<VorePathDef, VoreJob> jobMaker, out List<FloatMenuOption> options, bool isForProposal = false)
        {
            options = new List<FloatMenuOption>();

            DoRandomVoreOptions(initiator, target, interaction, jobMaker, options);

            foreach(VoreGoalDef goal in interaction.ValidGoals)
            {
                string optionLabel = goal.LabelCap;
                if(DoVorePathOptions(initiator, target, interaction, jobMaker, goal, out List<FloatMenuOption> goalOptions, isForProposal))
                {
                    options.Add(new FloatMenuOption(optionLabel, () => Find.WindowStack.Add(new FloatMenu(goalOptions))));
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

            return !options.NullOrEmpty();
        }

        private static void DoRandomVoreOptions(Pawn initiator, Pawn target, VoreInteraction interaction, Func<VorePathDef, VoreJob> jobMaker, List<FloatMenuOption> options)
        {
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
        }

        private static bool DoVorePathOptions(Pawn initiator, Pawn target, VoreInteraction interaction, Func<VorePathDef, VoreJob> jobMaker, VoreGoalDef goal, out List<FloatMenuOption> options, bool isForProposal = false)
        {
            options = new List<FloatMenuOption>();
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
            return !options.NullOrEmpty();
        }
    }
}