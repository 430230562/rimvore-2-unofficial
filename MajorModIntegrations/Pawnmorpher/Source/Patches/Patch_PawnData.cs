using HarmonyLib;
using RimVore2;
using Pawnmorph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RV2_PawnMorpher
{
    /// <summary>
    /// Pawns that were transformed by PawnMorpher need to acces their previous pawn and clone the quirk manager of that original pawn
    /// </summary>
    [HarmonyPatch(typeof(PawnData), "QuirkManager")]
    public static class Patch_PawnData
    {
        [HarmonyPrefix]
        public static void InitializeQuirkManagerFromOriginalPawn(PawnData __instance, bool initializeIfNull, ref QuirkManager ___quirkManager)
        {
            // only need to perform a hook-in if we actually generate a quirk manager
            if(!initializeIfNull)
                return;
            // if a quirk manager for this pawn already exists, we have no business replacing it
            if(___quirkManager != null)
                return;

            Pawn pawn = __instance.Pawn;
            Pawn formerPawn = FormerHumanUtilities.GetOriginalPawnOfFormerHuman(pawn);
            // no former human means we generate quirk manager as usual
            if(formerPawn == null)
                return;
            QuirkManager formerQuirkManager = formerPawn.QuirkManager(false);
            // previous pawn had no quirk manager either, let RV2 generate normally
            if(formerQuirkManager == null)
                return;
            // now we actually replace the variable
            ___quirkManager = (QuirkManager)formerQuirkManager.Clone();
        }
    }
}
