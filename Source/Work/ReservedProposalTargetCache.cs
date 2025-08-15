using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public static class ReservedProposalTargetCache
    {
        const int cacheTimeout = 60000;    // full in-game day
        static Dictionary<Pawn, int> cachedPawns = new Dictionary<Pawn, int>();

        public static bool Contains(Pawn pawn)
        {
            // pawn not cached
            if(!cachedPawns.ContainsKey(pawn))
                return false;
            // pawn is cached, check caching tick timeout
            KeyValuePair<Pawn, int> cachedPawn = cachedPawns.First(kvp => kvp.Key == pawn);
            if(GenTicks.TicksGame - cachedPawn.Value > cacheTimeout)
            {
                if(RV2Log.ShouldLog(false, "Debug"))
                    RV2Log.Message($"Pawn {pawn.LabelShort} was cached as proposal target, but timed out", "Debug");
                cachedPawns.Remove(pawn);
                return false;
            }
            return true;
        }
        public static void Add(Pawn pawn)
        {
            cachedPawns.SetOrAdd(pawn, GenTicks.TicksGame);
        }
        public static void AddRange(IEnumerable<Pawn> pawns)
        {
            foreach(Pawn pawn in pawns)
                Add(pawn);
        }
        public static void Remove(Pawn pawn)
        {
            cachedPawns.Remove(pawn);
        }
    }
}
