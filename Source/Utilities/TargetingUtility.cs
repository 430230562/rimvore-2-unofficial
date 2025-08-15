using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace RimVore2
{
    public static class TargetUtility
    {
        public static IEnumerable<Pawn> GetVorablePawns(Pawn initiator, VoreTargetRequest request, int targetPawnCount = -1)
        {
            if(request == null)
            {
                RV2Log.Error("Tried to get vorable pawns for NULL request");
                return new List<Pawn>();
            }
            IEnumerable<Pawn> allPawns = initiator?.Map?.mapPawns?.AllPawnsSpawned;
            if(allPawns.EnumerableNullOrEmpty())
            {
                RV2Log.Warning("Tried to get vorable pawns, but pawn, map or map pawns are NULL");
                return new List<Pawn>();
            }
            List<Pawn> targets = new List<Pawn>();
            // use inRandomOrder so we don't always consider the exact same pawns
            foreach(Pawn pawn in allPawns.InRandomOrder())
            {
                if(pawn == initiator)
                    continue;
                if(request.IsValid(initiator, pawn, out string reason))
                {
                    targets.Add(pawn);
                    // if we found enough targets, end loop
                    if(targetPawnCount != -1 && targets.Count >= targetPawnCount)
                    {
                        break;
                    }
                }
                else
                {
                    if(RV2Log.ShouldLog(false, "Targeting"))
                        RV2Log.Message($"Pawn {pawn.LabelShort} is not a valid target, reason: {reason}", "Targeting");
                }
            }
            return targets;
        }
    }
}
