using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class ThingComp_AssignableToPawn_ReleaseSpot : CompAssignableToPawn
    {
        bool IsForProducts => parent.def.GetModExtension<PreyReleaseTarget>()?.isForProducts == true;
        public override IEnumerable<Pawn> AssigningCandidates
        {
            get
            {
                if(!base.parent.Spawned)
                {
                    yield break;
                }
                List<Pawn> allPawns = parent.Map.mapPawns.FreeColonistsAndPrisoners;
                allPawns.AddRange(parent.Map.mapPawns.SlavesOfColonySpawned);
                if(!assignedPawns.NullOrEmpty())
                {
                    foreach(Pawn pawn in assignedPawns)
                        allPawns.Remove(pawn);
                }

                foreach(Pawn pawn in allPawns)
                {
                    if(!CanAssignTo(pawn))
                        continue;
                    if(IdeoligionForbids(pawn))
                        continue;
                    yield return pawn;
                }
            }
        }
        private RV2_Ownership GetOwnershipData(Pawn pawn)
        {
            if(pawn == null)
                return null;
            return pawn.PawnData()?.Ownership;
        }

        public override bool AssignedAnything(Pawn pawn)
        {
            RV2_Ownership ownership = GetOwnershipData(pawn);
            if(IsForProducts)
                return ownership.AssignedProductReleaseBuilding != null;
            else
                return ownership.AssignedEndoReleaseBuilding != null;
        }

        /// <summary>
        /// for some reason the game does not call this method, which means the assigning pawns are not shown.
        /// 
        /// I can not for the life of my figure out *why*
        /// </summary>
        public override void DrawGUIOverlay()
        {
            base.DrawGUIOverlay();
        }

        public override void TryAssignPawn(Pawn pawn)
        {
            // unassign previous target if present
            RV2_Ownership ownership = GetOwnershipData(pawn);
            if(ownership == null)
            {
                Log.Error($"Could not assign {pawn.LabelShort}, no RV2_Ownership data was found");
                return;
            }

            if(IsForProducts)
            {
                ownership.UnassignProductReleaseBuilding();
                if(ownership.AssignedProductReleaseBuilding != null)
                {
                    Log.Warning($"Pawn {pawn?.LabelShort} has release building {ownership.AssignedProductReleaseBuilding?.LabelShort} assigned that could not be unassigned. Still assigning new building {base.parent?.LabelShort}, but the old one may not be cleaned up.");
                }
                ownership.AssignedProductReleaseBuilding = (Building)base.parent;
            }
            else
            {
                ownership.UnassignEndoReleaseBuilding();
                if(ownership.AssignedEndoReleaseBuilding != null)
                {
                    Log.Error($"Pawn {pawn?.LabelShort} has release building {ownership.AssignedEndoReleaseBuilding?.LabelShort} assigned that could not be unassigned. Still assigning new building {base.parent?.LabelShort}, but the old one may not be cleaned up.");
                }
                ownership.AssignedEndoReleaseBuilding = (Building)base.parent;
            }

            base.TryAssignPawn(pawn);
        }

        public override void TryUnassignPawn(Pawn pawn, bool sort = true, bool uninstall = false)
        {
            base.TryUnassignPawn(pawn, sort);

            RV2_Ownership ownership = GetOwnershipData(pawn);
            if(ownership == null)
                return;
            if(IsForProducts)
                ownership.UnassignProductReleaseBuilding();
            else
                ownership.UnassignEndoReleaseBuilding();
        }
    }
}
