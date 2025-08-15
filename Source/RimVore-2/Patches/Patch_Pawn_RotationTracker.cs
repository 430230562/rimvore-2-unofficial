using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimVore2
{
    /// <summary>
    /// The rotation tracker constantly updates the grapple defenders rotation to face away from the attacker
    /// I don't know how to fix this without having to make a custom defender job that specifically sets the "handlesFacing" field in a toil
    /// 
    /// Fuck that. Stop rotating my pawns. https://i.kym-cdn.com/entries/icons/original/000/033/189/tumblr_33caa6fa2d9060d1ebf32b7f13a3bf38_59ae975d_1280.png
    /// </summary>
    [HarmonyPatch(typeof(Pawn_RotationTracker), "UpdateRotation")]
    public class Patch_Pawn_RotationTracker
    {
        [HarmonyPrefix]
        private static bool PreventRotationDuringGrapple(Pawn ___pawn)
        {
            try
            {
                if(CombatUtility.IsInvolvedInGrapple(___pawn))
                {
                    if(RV2Log.ShouldLog(true, "VoreCombatGrapple"))
                        RV2Log.Message($"Prevented pawn {___pawn.LabelShort} from rotating on their own while involved in grapple", true, "VoreCombatGrapple");
                    return false;
                }
            }
            catch(Exception e)
            {
                Log.Warning("RimVore-2: Something went wrong when trying to prevent grappling pawns from rotating: " + e);
            }
            return true;
        }
    }
}