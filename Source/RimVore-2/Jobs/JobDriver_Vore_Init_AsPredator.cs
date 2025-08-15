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
    public class JobDriver_Vore_Init_AsPredator : JobDriver
    {
        readonly TargetIndex preyIndex = TargetIndex.A;
        Pawn Prey => (Pawn)job.GetTarget(preyIndex);

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Pawn targetPawn = (Pawn)base.job.GetTarget(preyIndex);
            if(!pawn.HasFreeCapacityFor(targetPawn))
            {
                return false;
            }
            return base.pawn.Reserve(targetPawn, base.job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(preyIndex);
            //this.FailOnAggroMentalStateAndHostile(preyIndex);
            bool isTargetGrappled = Prey.health != null
                && Prey.health.hediffSet.HasHediff(RV2_Common.GrappledHediff);
            bool shouldIgnoreMentalState = RV2Mod.Settings.cheats.DisableMentalStateChecks || isTargetGrappled;
            if(!shouldIgnoreMentalState)
            {
                this.FailOnMentalState(preyIndex);
            }
            this.FailOnBurningImmobile(preyIndex);
            this.FailOnDestroyedOrNull(preyIndex);

            // for some reason the game does not remove the VoreJob from the pawns curJob, doing it manually this way
#if v1_4
            this.AddFinishAction(() =>
#else
            this.AddFinishAction((JobCondition jobCondition) =>
#endif
            {
                base.pawn.jobs.curJob = null;
            });

            Pawn predator = base.pawn;
            Pawn prey = (Pawn)TargetA;
            VoreJob voreJob = (VoreJob)base.job;
            voreJob.targetA = base.TargetA;

            if(RV2Log.ShouldLog(false, "Jobs"))
                RV2Log.Message($"Job started with prey: {prey.LabelShort} and pred {predator.LabelShort}", "Jobs");

            yield return Toils_Goto.GotoThing(preyIndex, PathEndMode.Touch);

            yield return Toil_Vore.SwallowToil(base.job, predator, preyIndex);
            yield return Toil_Vore.ExecutionToil_Direct(voreJob, predator, predator, prey);
        }
    }
}