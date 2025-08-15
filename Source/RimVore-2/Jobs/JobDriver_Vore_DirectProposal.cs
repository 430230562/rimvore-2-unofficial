using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using RimWorld;

namespace RimVore2
{
    public class JobDriver_Vore_DirectProposal : JobDriver
    {
        readonly TargetIndex targetIndex = TargetIndex.A;

        VoreJob voreJob;
        VoreJob VoreJob
        {
            get
            {
                if(voreJob == null)
                {
                    if(job is VoreJob parsedVoreJob)
                    {
                        voreJob = parsedVoreJob;
                        voreJob.targetA = this.TargetA;
                        voreJob.targetB = this.TargetB;
                        voreJob.targetC = this.TargetC;
                    }
                    else
                    {
                        throw new Exception("Job for JobDriver_Vore_GotoAndProposeVore is not VoreJob! Aborting!");
                    }
                }
                return voreJob;
            }
        }
        Pawn TargetPawn => this.job.GetTarget(targetIndex).Pawn;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if(VoreJob.Proposal == null)
            {
                RV2Log.Error("No proposal for proposal job, the job must be filled with a proposal during the job creation");
                return false;
            }
            Pawn predator = voreJob.Proposal.RoleFor(VoreRole.Predator);
            if(!predator.HasFreeCapacityFor(TargetPawn))
            {
                return false;
            }
            return this.pawn.Reserve(TargetPawn, this.job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(targetIndex);
            this.FailOnAggroMentalStateAndHostile(targetIndex);
            if(!RV2Mod.Settings.cheats.DisableMentalStateChecks)
            {
                this.FailOnMentalState(targetIndex);
            }
            this.FailOnBurningImmobile(targetIndex);
            this.FailOnDestroyedOrNull(targetIndex);

            // for some reason the game does not remove the VoreJob from the pawns curJob, doing it manually this way
#if v1_4
            globalFinishActions.Add(() =>
#else
            globalFinishActions.Add((JobCondition jobCondition) =>
#endif
            {
                base.pawn.jobs.curJob = null;
            });

            yield return Toils_Goto.GotoThing(targetIndex, PathEndMode.Touch);
            yield return Toils_General.WaitWith(targetIndex, RV2Mod.Settings.fineTuning.ProposalTimeTicks, true, true);
            Toil proposalToil = new Toil()
            {
                initAction = delegate ()
                {
                    bool proposalPassed = VoreJob.Proposal.TryProposal();
                    if(!proposalPassed)
                    {
                        AddEndCondition(() => JobCondition.Succeeded);
                    }
                }
            };
            proposalToil.socialMode = RandomSocialMode.SuperActive;
            yield return proposalToil;
            Toil voreToil = new Toil()
            {
                initAction = delegate ()
                {
                    JobDef nextJobDef = VoreJob.Proposal.RoleOf(pawn).GetInitJobDefFor();
                    VoreJob nextJob = VoreJobMaker.MakeJob(nextJobDef, pawn, this.TargetA, this.TargetB, this.TargetC);
                    nextJob.VorePath = VoreJob.VorePath;
                    nextJob.IsForced = VoreJob.Proposal.IsForced;
                    if(RV2Log.ShouldLog(true, "Jobs"))
                        RV2Log.Message($"Starting vore initiation job: {nextJobDef.defName}", false, "Jobs");
                    this.pawn.jobs.StartJob(nextJob, JobCondition.Succeeded);
                }
            };
            yield return voreToil;
            //yield return Toil_Vore.SwallowToil(Predator, Prey, targetIndex);
            //yield return Toil_Vore.ExecutionToil(VoreJob, Predator, Prey);
        }
    }
}
