using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;

namespace RimVore2
{
    [HarmonyPatch(typeof(Dialog_BeginRitual), "DrawRoleSelection")]
    static class Patch_Dialog_BeginRitual_DrawRoleSelection
    {
        [HarmonyPrefix]
        static bool PreventNREInUI(Pawn pawn, RitualRoleAssignments ___assignments)
        {
            if(___assignments.RoleChangeSelection == null)
            {
                RV2Log.Warning("Base game fix because Ludeon doesn't know how to NULL check before using a field.");
                ___assignments.SetRoleChangeSelection(pawn.Ideo?.RolesListForReading?.First());
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(RitualOutcomeEffectWorker_FromQuality), "GiveMemoryToPawn")]
    static class Patch_RitualOutcomeEffectWorker_FromQuality_GiveMemoryToPawn
    {
        [HarmonyPrefix]
        static bool PreventNREFromMoodlessPawns(Pawn pawn)
        {
            if(pawn.needs?.mood?.thoughts?.memories == null)
            {
                RV2Log.Warning("Base game fix because Ludeon forgot to check for a pawns ability to even take memories before applying them");
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(RitualRoleTag), "AppliesToRole")]
    static class Patch_RitualRoleTag_AppliesToRole
    {
        [HarmonyPrefix]
        static bool PreventNREFromMissingIdeo(ref bool __result, RitualRoleTag __instance, ref string reason, Precept_Ritual ritual, Pawn p, ref bool skipReason)
        {
            if(ritual != null && p != null && p.Ideo != ritual.ideo)
            {
                if(ritual.ideo?.memberName == null && !skipReason)
                {
                    reason = "MessageRitualRoleMustHaveIdeoToDoRole".Translate("ERR", __instance.Label);
                }
                return false;
            }
            return true;
        }
    }
}