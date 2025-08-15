using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    [HarmonyPatch(typeof(Dialog_BeginRitual), "BlockingIssues")]
    public static class Patch_Dialog_BeginRitual
    {
        /// <summary>
        /// This was originally a pass-through enumeration patch, but it produces a NRE for base game rituals
        /// I have no idea why that in particular malfunctions, but I will blame Harmony for it
        /// </summary>
        /// <param name="__result"></param>
        /// <param name="___ritual"></param>
        /// <param name="___assignments"></param>
        [HarmonyPostfix]
        private static void AddVoreValidatorIssues(ref IEnumerable<string> __result, Precept_Ritual ___ritual, RitualRoleAssignments ___assignments)
        {
            try
            {
                List<string> issues = __result != null ? __result.ToList() : new List<string>();
                VoreRitualValidator validator = ___ritual?.behavior?.def?.GetModExtension<VoreRitualValidator>();
                if(validator == null)
                {
                    if(RV2Log.ShouldLog(true, "Rituals"))
                        RV2Log.Message($"no validator for {___ritual?.def?.defName}", true, "Rituals");
                    return;
                }
                if(!validator.IsValid(___assignments, out string reason))
                {
                    issues.Add(reason);
                }
                __result = issues.AsEnumerable();
            }
            catch(Exception e)
            {
                RV2Log.Error("Something went wrong when gathering vore ritual issues: " + e, true);
            }
        }
    }
}