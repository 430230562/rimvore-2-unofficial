using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimVore2
{
    class JobDriver_Vore_EjectPrey_Self : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return base.pawn.Reserve(base.pawn, base.job, 1, -1, null, errorOnFailed);
        }

        private Pawn ejectPawn;
        private Pawn EjectPawn
        {
            get
            {
                if(ejectPawn == null)
                {
                    ejectPawn = TargetA.Pawn;
                }
                if(ejectPawn == null)
                {
                    this.FailOn(() => true);
                }
                return ejectPawn;
            }
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Pawn initiatorPawn = base.pawn;

            if(RV2Log.ShouldLog(false, "Jobs"))
                RV2Log.Message($"Initiating self eject toil for predator {base.pawn.LabelShort} and prey {EjectPawn.LabelShort}", "Jobs");

            yield return Toil_Vore.EjectionToil(initiatorPawn, EjectPawn);
            yield return Toil_Vore.EjectToil(initiatorPawn, initiatorPawn, EjectPawn, true);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref ejectPawn, "ejectPawn", true);
        }
    }
}
