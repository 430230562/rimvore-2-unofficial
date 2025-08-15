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
    public class JobDriver_Vore_Init_AsPrey : JobDriver
    {
        readonly TargetIndex predatorIndex = TargetIndex.A;
        Pawn Predator => (Pawn)job.GetTarget(predatorIndex);

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Pawn targetPawn = (Pawn)this.job.GetTarget(predatorIndex);
            if(!targetPawn.HasFreeCapacityFor(this.pawn))
            {
                return false;
            }
            return this.pawn.Reserve(targetPawn, this.job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(predatorIndex);
            //this.FailOnAggroMentalStateAndHostile(predatorIndex);
            bool isTargetGrappled = Predator.health != null
                && Predator.health.hediffSet.HasHediff(RV2_Common.GrappledHediff);
            bool shouldIgnoreMentalState = RV2Mod.Settings.cheats.DisableMentalStateChecks || isTargetGrappled;
            if(!shouldIgnoreMentalState)
            {
                this.FailOnMentalState(predatorIndex);
            }
            this.FailOnBurningImmobile(predatorIndex);
            this.FailOnDestroyedOrNull(predatorIndex);

            // for some reason the game does not remove the VoreJob from the pawns curJob, doing it manually this way
#if v1_4
            this.AddFinishAction(() =>
#else
            this.AddFinishAction((JobCondition jobCondition) =>
#endif
            {
                base.pawn.jobs.curJob = null;
            });

            Pawn predator = (Pawn)TargetA;
            Pawn prey = this.pawn;
            VoreJob voreJob = (VoreJob)this.job;
            voreJob.targetA = this.TargetA;

            if(RV2Log.ShouldLog(false, "Jobs"))
                RV2Log.Message($"Job started with prey: {prey.LabelShort} and pred {predator.LabelShort}", "Jobs");

            yield return Toils_Goto.GotoThing(predatorIndex, PathEndMode.Touch);

            yield return Toil_Vore.SwallowToil(base.job, predator, predatorIndex);
            yield return Toil_Vore.ExecutionToil_Direct(voreJob, prey, predator, prey);
        }
    }
}