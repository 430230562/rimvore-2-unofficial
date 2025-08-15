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
    public class JobDriver_Vore_Init_AsFeeder : JobDriver
    {
        readonly TargetIndex preyIndex = TargetIndex.A;
        readonly TargetIndex predatorIndex = TargetIndex.B;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Pawn targetPredator = (Pawn)this.job.GetTarget(predatorIndex);
            Pawn targetPrey = (Pawn)this.job.GetTarget(preyIndex);

            if(!targetPredator.HasFreeCapacityFor(targetPrey))
            {
                return false;
            }
            if(!this.pawn.Reserve(targetPrey, this.job, 1, -1, null, errorOnFailed))
            {
                return false;
            }
            return this.pawn.Reserve(targetPredator, this.job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            //this.FailOnAggroMentalStateAndHostile(preyIndex);
            //this.FailOnAggroMentalStateAndHostile(predatorIndex);

            if(!RV2Mod.Settings.cheats.DisableMentalStateChecks)
            {
                this.FailOnMentalState(preyIndex);
                this.FailOnMentalState(predatorIndex);
            }
            this.FailOnBurningImmobile(preyIndex);
            this.FailOnBurningImmobile(predatorIndex);
            this.FailOnDestroyedOrNull(preyIndex);
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

            // I have no idea what the fuck this does, but without setting count above 0, the carry toil fails with an error.
            this.job.count = 1;
            Pawn feeder = this.pawn;
            Pawn prey = (Pawn)TargetA;
            Pawn predator = (Pawn)TargetB;
            VoreJob voreJob = (VoreJob)this.job;
            voreJob.targetA = this.TargetA;
            voreJob.targetB = this.TargetB;

            if(RV2Log.ShouldLog(false, "Jobs"))
                RV2Log.Message($"Starting feeder job, feeder: {feeder.LabelShort} prey: {prey.LabelShort} predator: {predator.LabelShort}", "Jobs");

            yield return Toils_Goto.GotoThing(preyIndex, PathEndMode.Touch);
            yield return Toils_Haul.StartCarryThing(preyIndex);
            yield return Toils_Goto.GotoThing(predatorIndex, PathEndMode.Touch);
            yield return Toil_Vore.SwallowToil(base.job, predator, predatorIndex);
            yield return Toil_Vore.ExecutionToil_Direct(voreJob, base.pawn, predator, prey);
            yield break;
        }


    }
}