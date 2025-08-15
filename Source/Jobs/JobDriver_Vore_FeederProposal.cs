using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimVore2
{
    /// <note>
    /// The way I understand it is that TargetQueueA / B is only to be used for the corresponding TargetA / B
    /// Because our predator is TargetA we "should" use the TargetQueueB to populate the prey in TargetB
    /// </note>
    public class JobDriver_Vore_FeederProposal : JobDriver
    {
        TargetIndex predatorIndex = TargetIndex.A;
        TargetIndex preyIndex = TargetIndex.B;

        VoreJob VoreJob => (VoreJob)job;    // if the cast fails, we want an exception to happen!

        Pawn Predator => job.GetTarget(predatorIndex).Pawn;
        Pawn Prey => job.GetTarget(preyIndex).Pawn;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if(VoreJob.Proposal == null)
            {
                RV2Log.Error("No proposal for proposal job, the job must be filled with a proposal during the job creation");
                return false;
            }

            return this.pawn.Reserve(Predator, this.job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(predatorIndex);
            this.FailOnAggroMentalStateAndHostile(predatorIndex);
            if(!RV2Mod.Settings.cheats.DisableMentalStateChecks)
            {
                this.FailOnMentalState(predatorIndex);
            }
            this.FailOnBurningImmobile(predatorIndex);
            this.FailOn(() => !base.pawn.CanReach(Predator, PathEndMode.Touch, Danger.Deadly)); // TODO consider tick delay for check
            this.AddFinishAction((JobCondition jobCondition) =>
            {
                // for some reason the game does not remove the VoreJob from the pawns curJob, doing it manually this way
                base.pawn.jobs.curJob = null;

                // remove the prey pawns used in this job from the global proposal cache
                List<Pawn> preyPawns = new List<Pawn>();
                preyPawns.Add(job.GetTarget(preyIndex).Pawn);
                if(!job.targetQueueB.NullOrEmpty())
                {
                    preyPawns.AddRange(job.targetQueueB
                        .Select(target => target.Pawn));
                }
                this.AddFinishAction((JobCondition jobCondition2) =>
                {
                    foreach(Pawn prey in preyPawns)
                    {
                        if(prey == null)
                            RV2Log.Warning($"VoreJob {job} prey pawn was null");
                        else
                            ReservedProposalTargetCache.Remove(prey);
                    }
                });
            });


            //this.FailOnAggroMentalStateAndHostile(preyIndex);
            //this.FailOnMentalState(preyIndex);
            //this.FailOnBurningImmobile(preyIndex);

            // ask predator if they are okay with the feeder proposal
            yield return Toils_Goto.GotoThing(predatorIndex, PathEndMode.Touch);
            yield return Toils_General.WaitWith(predatorIndex, RV2Mod.Settings.fineTuning.ProposalTimeTicks, true, true);
            Toil predatorProposalToil = new Toil()
            {
                initAction = delegate ()
                {
                    bool proposalPassed = VoreJob.Proposal.TryProposal();
                    VoreJob.IsForced = VoreJob.Proposal.IsForced;
                    if(!proposalPassed)
                    {
                        AddEndCondition(() => JobCondition.Succeeded);
                    }
                },
                socialMode = RandomSocialMode.SuperActive
            };
            yield return predatorProposalToil;

            // remove active prey from queue so the loop can properly pull it from the queue again
            yield return Toils_JobTransforms.MoveCurrentTargetIntoQueue(preyIndex);
            Func<Thing, bool> validator = StillValidPrey;
            // remove invalid prey from queue
            Toil initExtractPreyFromQueue = Toils_JobTransforms.ClearDespawnedNullOrForbiddenQueuedTargets(TargetIndex.B, validator);
            yield return initExtractPreyFromQueue;
            yield return Toils_JobTransforms.SucceedOnNoTargetInQueue(preyIndex);
            // get the next prey as TargetIndexB
            yield return Toils_JobTransforms.ExtractNextTargetFromQueue(preyIndex, true);
            Func<bool> ReservationPredicate = () =>
            {
                if(base.pawn.Reserve(TargetB, this.job))
                    return true;
                ReservedProposalTargetCache.Remove(TargetB.Pawn);
                return false;
            };
            yield return new Toil() // if prey could not be reserved, pick next prey
            {
                initAction = delegate ()
                {
                    if(!ReservationPredicate())
                        JumpToToil(initExtractPreyFromQueue);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };

            yield return Toils_Goto.GotoThing(preyIndex, PathEndMode.Touch);
            Toil preyWaitToil = Toils_General.WaitWith(preyIndex, RV2Mod.Settings.fineTuning.ProposalTimeTicks, true, true);
            preyWaitToil.handlingFacing = true;
            preyWaitToil.AddPreTickAction(() => pawn.rotationTracker.FaceTarget(Prey));
            preyWaitToil.socialMode = RandomSocialMode.SuperActive;
            yield return preyWaitToil;
            yield return new Toil()
            {
                initAction = delegate ()
                {
                    VoreJob.Proposal = GeneratePreyProposal();
                    VoreJob.Proposal.TryProposal();
                    ReservedProposalTargetCache.Remove(VoreJob.Proposal.PrimaryTarget);
                    VoreJob.IsForced = VoreJob.Proposal.IsForced;
                    if(!VoreJob.Proposal.IsPassed)
                    {
                        JumpToToil(initExtractPreyFromQueue);
                    }

                }
            };
            yield return Toils_Haul.StartCarryThing(preyIndex);
            yield return Toils_Goto.GotoThing(predatorIndex, PathEndMode.Touch);
            yield return Toil_Vore.SwallowToil(base.job, Predator, predatorIndex);
            yield return Toil_Vore.ExecutionToil_Feeder(VoreJob, base.pawn, predatorIndex, preyIndex);
            Toil socialInteractionToil = new Toil()
            {
                initAction = () =>
                {
                    DoFeederInteraction();
                    DoFeederSocialMemories();
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield return socialInteractionToil;

            yield return Toils_Jump.Jump(initExtractPreyFromQueue);
        }

        private void DoFeederInteraction()
        {
            // rule packs infer that the feeder is participating directly in vore. This would need new keys to ensure that doesn't happen
            //List<RulePackDef> rulePacks = VoreJob.VorePath.voreType.relatedRulePacks;
            //rulePacks.AddRange(VoreJob.VorePath.voreGoal.relatedRulePacks);
            List<RulePackDef> rulePacks = new List<RulePackDef>();

            PlayLogEntry_Interaction predatorEntry = new PlayLogEntry_Interaction(VoreInteractionDefOf.RV2_Feeding_Predator, base.pawn, Predator, rulePacks);
            PlayLogEntry_Interaction preyEntry = new PlayLogEntry_Interaction(VoreInteractionDefOf.RV2_Feeding_Prey, base.pawn, Prey, rulePacks);
            Find.PlayLog.Add(predatorEntry);
            Find.PlayLog.Add(preyEntry);
        }

        const float veryBadPreferenceCutOff = -9;
        const float badPreferenceCutoff = -4;
        const float goodPreferenceCutOff = 5;
        const float veryGoodPreferenceCutOff = 10;
        private void DoFeederSocialMemories()
        {
            ApplySocialMemoriesFor(VoreRole.Predator);
            ApplySocialMemoriesFor(VoreRole.Prey);

            void ApplySocialMemoriesFor(VoreRole role)
            {
                Pawn pawn = role == VoreRole.Predator ? Predator : Prey;
                Pawn otherPawn = role == VoreRole.Predator ? Prey : Predator;
                MemoryThoughtHandler memories = pawn.needs?.mood?.thoughts?.memories;
                if(memories == null)
                    return;
                float totalPreference = pawn.PreferenceFor(role)
                    + pawn.PreferenceFor(otherPawn)
                    + pawn.PreferenceFor(VoreJob.VorePath.voreType, role)
                    + pawn.PreferenceFor(VoreJob.VorePath.voreGoal, role);

                ThoughtDef memoryDef;
                if(totalPreference <= veryBadPreferenceCutOff)
                    memoryDef = VoreThoughtDefOf.RV2_FedMePrey_VeryBad;
                else if(totalPreference <= badPreferenceCutoff)
                    memoryDef = VoreThoughtDefOf.RV2_FedMePrey_Bad;
                else if(totalPreference <= goodPreferenceCutOff)
                    memoryDef = null;
                else if(totalPreference <= veryGoodPreferenceCutOff)
                    memoryDef = VoreThoughtDefOf.RV2_FedMePrey_Good;
                else
                    memoryDef = VoreThoughtDefOf.RV2_FedMePrey_VeryGood;

                if(memoryDef == null)
                    return;

                pawn.needs.mood.thoughts.memories.TryGainMemory(memoryDef, base.pawn);
            }


        }

        private VoreProposal GeneratePreyProposal()
        {
            // calculate valid paths for predator and prey
            VoreInteractionRequest request = new VoreInteractionRequest(Predator, Prey, VoreRole.Predator, isForAuto: true, isForProposal: true);
            VoreInteraction interaction = VoreInteractionManager.Retrieve(request);
            VoreRole preferenceSourceRole = VoreRole.Feeder;  // TODO SETTING
            Pawn preferenceSourcePawn = PawnForRole(preferenceSourceRole);
            // get the preferred path of whichever pawn is considered the source of the preference
            VorePathDef preferredPath = interaction.PreferredPathFor(preferenceSourcePawn);
            if(preferredPath == null)
            {
                // if no preferred path was found, use a random valid path (there should always be at least one)
                preferredPath = interaction.ValidPaths.RandomElementWithFallback();
                if(preferredPath == null)
                {
                    Log.Error("Fallback vorepath was still null, wtf is the IsValid for interactions doing?");
                }
                if(RV2Log.ShouldLog(false, "Jobs"))
                    RV2Log.Message($"Pawn {preferenceSourcePawn.LabelShort} with role {preferenceSourceRole} could not pick a valid preferred path, falling back to a random valid path: {preferredPath?.defName}", "Jobs");
            }
            VoreJob.VorePath = preferredPath;
            // persist the proposal so that later toils can retrieve them
            return new VoreProposal_Feeder_Prey(pawn, Predator, Prey, preferredPath);
        }

        private Pawn PawnForRole(VoreRole role)
        {
            switch(role)
            {
                case VoreRole.Predator:
                    return Predator;
                case VoreRole.Prey:
                    return Prey;
                case VoreRole.Feeder:
                    return base.pawn;
                default:
                    Log.Warning("Unexpected VoreRole " + role);
                    return base.pawn;

            }
        }

        private bool StillValidPrey(Thing targetThing)
        {
            if(!(targetThing is Pawn prey))
                return false;
            if(!base.pawn.CanReserveAndReach(prey, PathEndMode.Touch, Danger.Deadly))
                return false;
            if(!Predator.CanVore(prey, out _))
                return false;
            return true;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref predatorIndex, "predatorIndex");
            Scribe_Values.Look(ref preyIndex, "preyIndex");

            if(Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                ReservedProposalTargetCache.Add(job.targetA.Pawn);
                ReservedProposalTargetCache.Add(job.targetB.Pawn);
                if(!job.targetQueueB.NullOrEmpty())
                    ReservedProposalTargetCache.AddRange(job.targetQueueB.Select(t => t.Pawn));
            }
        }
    }
}
