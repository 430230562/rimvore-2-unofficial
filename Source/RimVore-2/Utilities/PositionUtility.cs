using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public static class PositionUtility
    {
        private static bool TryGetPositionForSpot(Pawn pawn, bool isProduct, out IntVec3 spotPosition)
        {
            spotPosition = new IntVec3();
            // only colonists can use release spots
            if(pawn.Faction != Faction.OfPlayer)
            {
                return false;
            }
            IEnumerable<IntVec3> spots = GetValidSpots(pawn, isProduct)
                .Where(spot => pawn.CanReach(spot, Verse.AI.PathEndMode.OnCell, Danger.Some));
            if(spots.EnumerableNullOrEmpty())
            {
                return false;
            }
            spotPosition = spots.MinBy(spot => pawn.Position.DistanceTo(spot));
            return true;
        }
        private static bool TryGetPositionForReservedBuilding(Pawn predator, Pawn reservingPawn, bool isProduct, out IntVec3 spotPosition)
        {
            spotPosition = default(IntVec3);
            RV2_Ownership ownership = reservingPawn.PawnData()?.Ownership;
            if(ownership == null)
                return false;
            Building assignedBuilding = isProduct ? ownership.AssignedProductReleaseBuilding : ownership.AssignedEndoReleaseBuilding;
            if(assignedBuilding == null)
                return false;

            spotPosition = assignedBuilding.Position;
            if(!predator.CanReach(spotPosition, Verse.AI.PathEndMode.OnCell, Danger.Deadly))
                return false;

            return true;
        }

        private static IEnumerable<ThingDef> cachedValidReleaseTargets;
        public static IEnumerable<ThingDef> ValidReleaseTargets(bool isProduct)
        {
            if(cachedValidReleaseTargets == null)
            {
                cachedValidReleaseTargets = DefDatabase<ThingDef>.AllDefsListForReading
                    .Where(thingDef => thingDef.HasModExtension<PreyReleaseTarget>());
            }
            return cachedValidReleaseTargets
                .Where(thingDef => thingDef.GetModExtension<PreyReleaseTarget>().isForProducts == isProduct);
        }
        private static IEnumerable<IntVec3> GetValidSpots(Pawn pawn, bool isProduct)
        {
            // we can only target things that are not a container, or an empty container
            bool NotContainerOrEmptyContainer(Thing t)
            {
                if(!(t is IThingHolder th))
                {
                    return true;
                }
                return th.GetDirectlyHeldThings().Count() == 0;
            }

            IEnumerable<ThingDef> spotDefs = ValidReleaseTargets(isProduct);
            return pawn.MapHeld.listerBuildings.allBuildingsColonist
                .Where(t => t.Faction == pawn.Faction
                    && NotContainerOrEmptyContainer(t)
                    && spotDefs.Contains(t.def))
                .Select(t => t.Position);
        }

        public static IntVec3 GetPreyReleasePosition(Pawn predator, Pawn prey, bool isProduct = false)
        {
            if(TryGetPositionForReservedBuilding(predator, predator, isProduct, out IntVec3 reservedBuildingPositionPredator))
            {
                return reservedBuildingPositionPredator;
            }
            if(TryGetPositionForReservedBuilding(predator, prey, isProduct, out IntVec3 reservedBuildingPositionPrey))
            {
                return reservedBuildingPositionPrey;
            }
            if(TryGetPositionForSpot(predator, isProduct, out IntVec3 spotPosition))
            {
                return spotPosition;
            }

            List<IntVec3> possiblePositions = new List<IntVec3>();
            for(int i = 0; i < 10; i++)
            {
                Predicate<IntVec3> cellValidator = delegate (IntVec3 cell)
                {
                    return predator.CanReach(new LocalTargetInfo(cell), Verse.AI.PathEndMode.OnCell, Danger.Some);
                };
                bool foundCell = CellFinder.TryFindRandomCellNear(predator.Position, predator.Map, 15, cellValidator, out IntVec3 position);
                if(foundCell && predator.CanReach(position, Verse.AI.PathEndMode.OnCell, Danger.Some) && !possiblePositions.Contains(position))
                {
                    possiblePositions.Add(position);
                }
            }
            if(possiblePositions.NullOrEmpty())
            {
                return predator.Position;
            }
            // create scores for each position and track position and its score in a dictionary
            Dictionary<IntVec3, float> scoredPositions = possiblePositions.ToDictionary(
                position => position,
                position => GetPositionScore(predator, position)
            );
            if(RV2Log.ShouldLog(true, "Positions"))
                RV2Log.Message($"calculated positions: {LogUtility.ToString(scoredPositions)}", "Positions");

            IntVec3 chosenPosition = scoredPositions
                .Where(kvp => kvp.Value == scoredPositions.Max(kvp2 => kvp2.Value))  // get all entries where position score is max
                .RandomElement()    // get any of the best positions
                .Key;   // discard score
            return chosenPosition;
        }

        private static float GetPositionScore(Pawn pawn, IntVec3 position)
        {
            float score = 0;
            Danger positionDanger = position.GetDangerFor(pawn, pawn.Map);
            if(positionDanger != Danger.None)
            {
                return -float.MaxValue;
            }
            Room room = position.GetRoom(pawn.Map);
            if(room != null)
            {
                if(room.IsDoorway) return -float.MaxValue;    //doorways are a horrible place to release a prey in
                if(room.IsPrisonCell) score -= 5;
                if(room.PsychologicallyOutdoors) score += 5;   // outdoors is good (not too sure about this)
                if(room.Owners.Contains(pawn)) score += 20;    // if room is pawn bedroom it's probably private
                RoomRoleDef role = room.Role;
                if(role == RoomRoleDefOf.Hospital) score -= 20; // do not act like an animal
                else if(role == RV2_Common.DiningRoomRoleDef) score -= 15; // do not act like an animal
                else if(role == RoomRoleDefOf.ThroneRoom) score -= 10; // do not act like an animal
                int numberOfDoors = room.Cells
                    .SelectMany(cell => cell.GetThingList(pawn.Map))
                    .Count(thing => thing.def.IsDoor);
                score -= numberOfDoors * 5; // try to use a room that has few doors - less chance of pawns walking in
            }
            if(!pawn.ComfortableTemperatureRange().Includes(position.GetTemperature(pawn.Map))) score -= 10;   // stick to comfortable temperature
            if(position.GetSurfaceType(pawn.Map) == SurfaceType.Eat) score -= 10;  // do not release on the table

            return score;
        }
    }
}
