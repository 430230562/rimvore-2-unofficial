using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimVore2
{
    /// <summary>
    /// Copy of JobDriver_TakeAndExitMap, but with vore instead of StartCarryThing
    /// </summary>
    public class JobDriver_Vore_VoreAndExitMap : JobDriver
    {
        private readonly TargetIndex preyIndex = TargetIndex.A;
        private readonly TargetIndex exitLocation = TargetIndex.B;
        private Pawn prey;
        private Pawn Prey
        {
            get
            {
                if(prey == null)
                {
                    prey = base.job.GetTarget(preyIndex).Pawn;
                }
                if(prey == null)
                {
                    this.FailOn(() => true);
                }
                return prey;
            }
        }
        private VoreJob VoreJob => (VoreJob)job;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(Prey, this.job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(preyIndex);
            this.FailOn(() => this.Prey == null || (!this.Prey.Downed && this.Prey.Awake()));

            // for some reason the game does not remove the VoreJob from the pawns curJob, doing it manually this way
            this.AddFinishAction((JobCondition jobCondition) =>
            {
                base.pawn.jobs.curJob = null;
            });

            VoreJob.IsForced = true;

            yield return Toils_Goto.GotoThing(preyIndex, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(preyIndex);
            yield return Toil_Vore.SwallowToil(base.job, this.pawn, preyIndex, 1);
            VoreJob.targetA = this.TargetA;
            Toil executionToil = Toil_Vore.ExecutionToil_Direct(VoreJob, this.pawn, this.pawn, Prey);
            executionToil.AddFinishAction(delegate ()
            {
                // remove the instant swallow quirk that was added by the raider generation
                pawn.QuirkManager()?.RemovePersistentQuirk(QuirkDefOf.Cheat_InstantSwallow);
            });
            yield return executionToil;
            Toil gotoMapEdgeToil = Toils_Goto.GotoCell(exitLocation, PathEndMode.OnCell);
            gotoMapEdgeToil.AddPreTickAction(delegate
            {
                if(base.Map.exitMapGrid.IsExitCell(this.pawn.Position))
                {
                    this.pawn.ExitMap(true, CellRect.WholeMap(base.Map).GetClosestEdge(this.pawn.Position));
                }
            });
            gotoMapEdgeToil.FailOn(() => this.job.failIfCantJoinOrCreateCaravan && !CaravanExitMapUtility.CanExitMapAndJoinOrCreateCaravanNow(this.pawn));
            yield return gotoMapEdgeToil;
            Toil leaveMapToil = new Toil()
            {
                initAction = delegate ()
                {
                    if(this.pawn.Position.OnEdge(this.pawn.Map) || this.pawn.Map.exitMapGrid.IsExitCell(this.pawn.Position))
                    {
                        this.pawn.ExitMap(true, CellRect.WholeMap(base.Map).GetClosestEdge(this.pawn.Position));
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            leaveMapToil.AddFinishAction(delegate ()
            {
                if(VoreJob.IsKidnapping)
                {
                    try
                    {
                        KidnapRecursively(Prey.GetVoreRecord());
                    }
                    catch(Exception e)
                    {
                        Log.Error("Exception during recursive kidnapping: " + e);
                    }
                }
            });
            leaveMapToil.FailOn(() => this.job.failIfCantJoinOrCreateCaravan && !CaravanExitMapUtility.CanExitMapAndJoinOrCreateCaravanNow(this.pawn));
            yield return leaveMapToil;
            yield break;
        }

        private void KidnapRecursively(VoreTrackerRecord record, int infiniteLoopLock = 0)
        {
            if(++infiniteLoopLock > 50)
            {
                Log.Error("Infinite recursion prevention triggered!");
                return;
            }
            Pawn predator = record.Predator;
            Pawn prey = record.Prey;
            if(pawn.Faction.HostileTo(prey.Faction))
            {
                pawn.Faction.kidnapped.Kidnap(prey, predator);
            }

            VoreTracker tracker = predator.PawnData().VoreTracker;
            tracker.Eject(record, null, false, true);
            VoreTracker preyTracker = prey.PawnData().VoreTracker;
            if(preyTracker != null)
            {
                foreach(VoreTrackerRecord subRecord in preyTracker.VoreTrackerRecords)
                {
                    KidnapRecursively(subRecord, infiniteLoopLock);
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref prey, "prey", true);
        }
    }
}
