/*using RimWorld;
using HarmonyLib;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RimVore2
{
    [HarmonyPatch(typeof(Dialog_FormCaravan), "CheckForErrors")]
    class Patch_Caravans
    {
        [HarmonyPostfix]
        public static void InvalidateCheckIfSelectedPredators(ref bool __result, List<Pawn> pawns)
        {
            bool backupReturnValue = __result;
            try
            {
                bool isError = __result;
                // already threw an error, no need to go through our check
                if(!isError)
                {
                    return;
                }
                bool anyPawnPredator = pawns.Any(pawn => pawn.IsTrackingVore());
                if(anyPawnPredator)
                {
                    Messages.Message("RV2_Caravan_PredatorBlockMessage".Translate(), MessageTypeDefOf.RejectInput, false);
                    __result = false;
                }
            }
            catch(Exception e)
            {
                Log.Error("RimVore-2 threw an error during patching:\n" + e);
                __result = backupReturnValue;
            }
        }
    }
}*/
