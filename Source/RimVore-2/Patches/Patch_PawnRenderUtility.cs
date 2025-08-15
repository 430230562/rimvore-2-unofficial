#if !v1_4
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    [HarmonyPatch(typeof(PawnRenderUtility), nameof(PawnRenderUtility.CarryWeaponOpenly))]
    public static class Patch_PawnRenderer
    {
        [HarmonyPostfix]
        private static bool StopEquipmentDrawingDuringGrapple(bool __result, Pawn pawn)
        {
            if (!__result)
            {
                return false;
            }
            return !CombatUtility.IsInvolvedInGrapple(pawn);
        }
    }
}
#endif
