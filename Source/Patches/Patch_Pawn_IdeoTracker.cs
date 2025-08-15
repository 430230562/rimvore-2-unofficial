using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimVore2
{
    [HarmonyPatch(typeof(Pawn_IdeoTracker), "SetIdeo")]
    public class Patch_Pawn_IdeoTracker_SetIdeo
    {
        [HarmonyPostfix]
        private static void RecalculateIdeoQuirksOnIdeoChange(ref Pawn ___pawn)
        {
            if(RV2Log.ShouldLog(false, "IdeoQuirks"))
                RV2Log.Message("Notifying stale ideo quirks due to SetIdeo() call", "IdeoQuirks");
            ___pawn.QuirkManager(false)?.Notify_IdeologyChanged();
        }
    }
}