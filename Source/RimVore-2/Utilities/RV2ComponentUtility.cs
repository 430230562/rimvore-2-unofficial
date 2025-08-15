using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public static class RV2ComponentUtility
    {
        public static PawnData PawnData(this Pawn pawn, bool initializeIfNull = true)
        {
            if(pawn == null)
            {
                RV2Log.Error("Tried to get pawn data for NULL pawn!");
                return null;
            }
            if(!initializeIfNull && !RV2Mod.RV2Component.HasPawnData(pawn))
            {
                return null;
            }
            PawnData pawnData = RV2Mod.RV2Component.GetPawnData(pawn);
            if(pawnData.Pawn == null)
            {
                RV2Log.Error($"Retrieved pawn data for pawn {(pawn == null ? "NULL" : pawn.LabelShort)}, but their PawnData had a NULL pawn, returning nothing");
                return null;
            }
            return pawnData;
        }
    }
}
