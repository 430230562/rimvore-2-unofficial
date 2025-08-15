#if v1_4
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    [HarmonyPatch(typeof(PawnRenderer), "DrawEquipment")]
    public static class Patch_PawnRenderer
    {
        [HarmonyPrefix]
        private static bool StopEquipmentDrawingDuringGrapple(Pawn ___pawn)
        {
            return !CombatUtility.IsInvolvedInGrapple(___pawn);
        }
    }
}
#endif
