using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class RV2_Ownership : IExposable
    {
        public RV2_Ownership() { }
        public RV2_Ownership(Pawn pawn)
        {
            this.pawn = pawn;
        }

        Pawn pawn;
        public Building AssignedProductReleaseBuilding;
        public Building AssignedEndoReleaseBuilding;

        public void UnassignEndoReleaseBuilding()
        {
            UnassignReleaseBuilding(AssignedEndoReleaseBuilding);
            AssignedEndoReleaseBuilding = null;
        }
        public void UnassignProductReleaseBuilding()
        {
            UnassignReleaseBuilding(AssignedProductReleaseBuilding);
            AssignedProductReleaseBuilding = null;
        }
        private void UnassignReleaseBuilding(Building building)
        {
            if(building == null)
                return;
            // if there was a previously assigned target, get the assignable comp from it and unassign from it
            ThingComp_AssignableToPawn_ReleaseSpot comp = building.TryGetComp<ThingComp_AssignableToPawn_ReleaseSpot>();
            if(comp == null)
            {
                Log.Error($"No assignable comp found, could not unassign pawn properly from {building}");
                return;
            }
            comp.ForceRemovePawn(pawn);
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref pawn, "pawn");
            Scribe_References.Look(ref AssignedProductReleaseBuilding, "AssignedDisposalTarget");
            Scribe_References.Look(ref AssignedEndoReleaseBuilding, "AssignedEndoReleaseBuilding");
        }
    }
}
