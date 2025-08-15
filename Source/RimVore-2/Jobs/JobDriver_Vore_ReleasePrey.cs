using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace RimVore2
{
    public class JobDriver_Vore_ReleasePrey : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        private IntVec3 TargetPosition => base.job.GetTarget(TargetIndex.A).Cell;
        private Pawn prey;  // need to apply some odd workaround here - because the prey is NULL, the game does not want to scribe it via the TargetB, so we load the TargetB into this field and scribe it ourselves
        public Pawn Prey
        {
            get
            {
                if(prey == null)
                {
                    prey = base.TargetB.Pawn;
                }
                if(prey == null)
                {
                    this.FailOn(() => true);    // fail immediately, we have lost the prey reference!
                }
                return prey;
            }
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Pawn predator = base.pawn;
            yield return Toils_Goto.GotoCell(TargetPosition, PathEndMode.OnCell);
            yield return Toil_Vore.EjectionToil(predator, Prey);
            yield return Toil_Vore.EjectToil(predator, predator, Prey);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref prey, "prey", true);
        }
    }
}
