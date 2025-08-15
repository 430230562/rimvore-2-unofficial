using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    [HarmonyPatch(typeof(ColonistBar), "CheckRecacheEntries")]
    public static class Patch_ColonistBar
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> AddVoredPawnsToBar(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codeInstructions = instructions.ToList();
            bool shouldAttemptCodeInjection = true;

            // our anchor is the `this.cachedReorderableGroups.Clear();` call, which begins the "wrapping up" part of the method, we should inject our own entries before that starts
            FieldInfo cachedReorderableGroupsInfo = AccessTools.Field(typeof(ColonistBar), "cachedReorderableGroups");
            MethodInfo clearInfo = AccessTools.Method(typeof(List<int>), "Clear");

            FieldInfo cachedEntriesInfo = AccessTools.Field(typeof(ColonistBar), "cachedEntries");

            for(int i = 0; i < codeInstructions.Count; i++)
            {
                CodeInstruction instruction = codeInstructions[i];
                if(ShouldInjectOn(i))
                {
                    // all we need is the internal list of entries and the local counter that keeps track of the group number
                    yield return new CodeInstruction(OpCodes.Ldarg_0);  //this
                    yield return new CodeInstruction(OpCodes.Ldfld, cachedEntriesInfo); //.cachedEntriesInfo
                    yield return new CodeInstruction(OpCodes.Ldloca, 0); //groupNumber
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_ColonistBar), nameof(AddVoredColonistsToBar)));
                }
                yield return instruction;
            }

            bool ShouldInjectOn(int i)
            {
                if(!shouldAttemptCodeInjection)
                {
                    return false;
                }
                // i      this.
                // i+1    cachedReorderableGroups
                // i+2    .Clear() 
                if(i >= codeInstructions.Count - 2)
                {
                    Log.Error($"Could not find anchor, hit end of stack without matching criteria, transpiler will not work");
                    shouldAttemptCodeInjection = false;
                    return false;
                }
                bool isAnchor = codeInstructions[i + 1].LoadsField(cachedReorderableGroupsInfo) && codeInstructions[i + 2].Calls(clearInfo);
                if(isAnchor)
                {
                    shouldAttemptCodeInjection = false;
                }
                return isAnchor;
            }
        }

        public static void AddVoredColonistsToBar(List<ColonistBar.Entry> entries, ref int lastGroup)
        {
            if(!RV2Mod.Settings.features.AddVoredPawnsToColonistBar)
            {
                return;
            }
            IEnumerable<ColonistBar.Entry> voredPawns = VoredColonistsAsEntries(lastGroup);
            entries.AddRange(voredPawns);
            if(RV2Log.ShouldLog(true, "ColonistBar"))
                RV2Log.Message($"Added vored colonists to bar: {string.Join(", ", voredPawns.Select(p => p.pawn.LabelShort))}", "ColonistBar");
            lastGroup++;
        }

        private static IEnumerable<ColonistBar.Entry> VoredColonistsAsEntries(int group)
        {
            // should be pretty performant because RV2 already does the caching logic for active prey
            return GlobalVoreTrackerUtility.ActivePreyWithRecord.Keys
                .Where(pawn => pawn.IsColonist)
                .Select(pawn => new ColonistBar.Entry(pawn, null, group));
        }
    }

    //[HarmonyPatch(typeof(ColonistBarDrawLocsFinder), "CalculateColonistsInGroup", new Type[] { typeof(int) })]
    //public static class fijisda
    //{
    //    [HarmonyPrefix]
    //    public static void hfusejd(int groupsCount, List<int> ___entriesInGroup)
    //    {
    //        Log.Message($"groupCounmt: {groupsCount}, entries in group: {___entriesInGroup.Count}, entries in bar: {Find.ColonistBar.Entries.Count}");
    //    }
    //}
}
