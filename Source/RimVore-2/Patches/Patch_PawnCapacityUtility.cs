using HarmonyLib;
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
    [HarmonyPatch(typeof(PawnCapacityUtility), "CalculateCapacityLevel")]
    public class Patch_PawnCapacityUtility
    {
        private static readonly FieldInfo offsetFieldInfo = AccessTools.Field(typeof(PawnCapacityModifier), "offset");
        private static readonly MethodInfo quirkModifierMethod = AccessTools.Method(typeof(Patch_PawnCapacityUtility), "ModifyOffsetWithQuirks");
        private static readonly FieldInfo pawnFieldInfo = AccessTools.Field(typeof(HediffSet), "pawn");

        /// <summary>
        /// Catch the offset retrieval during capacity calculation and modify it with the pawns quirks
        /// </summary>
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> InjectQuirkCapModModifiers(IEnumerable<CodeInstruction> instructions)
        {
            Predicate<CodeInstruction> IsAnchorInstruction = (CodeInstruction instruction) =>
            {
                return instruction.opcode == OpCodes.Ldfld 
                    && instruction.LoadsField(offsetFieldInfo);
            };

            foreach(CodeInstruction instruction in instructions)
            {
                yield return instruction;
                if(IsAnchorInstruction(instruction))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);   // load "HediffSet diffSet" from arguments
                    yield return new CodeInstruction(OpCodes.Ldfld, pawnFieldInfo);   // load pawn field from HediffSet
                    yield return new CodeInstruction(OpCodes.Ldarg_1); // load "PawnCapacityDef capacity" from arguments
                    yield return new CodeInstruction(OpCodes.Call, quirkModifierMethod);  // call method that takes old offset and returns new offset affected by quirks
                }
            }
        }

        public static float ModifyOffsetWithQuirks(float value, Pawn pawn, PawnCapacityDef capacity)
        {
            if(pawn == null || capacity == null || value == 0)
            {
                return value;
            }
            QuirkManager quirks = pawn.QuirkManager(false);
            if(quirks == null)
            {
                return value;
            }
            float modifier = quirks.CapModOffsetModifierFor(capacity);
            float newValue = modifier * value;
            if(RV2Log.ShouldLog(false, "Capacities"))
                RV2Log.Message($"Modifying {pawn.LabelShort}'s {capacity.defName} with quirks, found multiplier: {modifier} - original: {value} new: {newValue}", true, "Capacities");
            // Log.Message("TRANSPILER - calculated offset modifier for " + pawn.LabelShort + "|" + capacity.label + ": " + modifier + " - new value: " + newValue);
            return newValue;
        }
    }

    // yeaaa, this one didn't work out at all. It seems like the target of the patch is incorrect? This thing logs out like 7 instructions instead of the thousands of instructions it should
    // oh well, this is a merely visual effect, the actual capacity is modified in the above patch

    //[HarmonyPatch(typeof(HediffStatsUtility), "SpecialDisplayStats")]
    //public class Patch_HediffStatsUtility
    //{
    //    private static readonly BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;
    //    private static readonly FieldInfo offsetFieldInfo = typeof(PawnCapacityModifier).GetField("offset", flags);

    //    private static readonly FieldInfo pawnInfo = typeof(Hediff).GetField("pawn", flags);
    //    private static readonly FieldInfo capModInfo = typeof(HediffStatsUtility).GetField("capMod", flags);
    //    private static readonly FieldInfo capDefInfo = typeof(PawnCapacityModifier).GetField("capacity", flags);
    //    private static readonly MethodInfo quirkModifierMethod = typeof(QuirkUtility).GetMethod("ModifyOffsetWithQuirks", flags);

    //    [HarmonyTranspiler]
    //    private static IEnumerable<CodeInstruction> InjectQuirkCapModModifiers(IEnumerable<CodeInstruction> instructions)
    //    {
    //        List<CodeInstruction> codeInstructions= new List<CodeInstruction>(instructions);
    //        CodeInstruction loadHediff = new CodeInstruction(OpCodes.Ldarg_1); // load "Hediff instance" from arguments
    //        CodeInstruction loadPawn = new CodeInstruction(OpCodes.Ldfld, pawnInfo); // load "pawn" field from Hediff
    //        CodeInstruction loadPawnCapacityModifer = new CodeInstruction(OpCodes.Ldfld, capModInfo); // load local variable "PawnCapacityModifier capMod"
    //        CodeInstruction loadPawnCapacityDef = new CodeInstruction(OpCodes.Ldfld, capDefInfo); // load "capacity" field from PawnCapacityModifier
    //        CodeInstruction callModifyOffset = new CodeInstruction(OpCodes.Call, quirkModifierMethod);  // call method that takes old offset and returns new offset affected by quirks

    //        for (int i = 0; i < codeInstructions.Count; i++)
    //        {
    //            CodeInstruction instruction = codeInstructions[i];
    //            if(ReachedInsertionPoint(i))
    //            {
    //                Log.Message("transpiling");
    //                yield return instruction;
    //                yield return loadHediff;
    //                yield return loadPawn;
    //                yield return loadPawnCapacityModifer;
    //                yield return loadPawnCapacityDef;
    //                yield return callModifyOffset;
    //            }
    //            yield return instruction;
    //        }

    //        // our insertion point is when the offset is multiplied by 100 to create a display % - there is also a check if offset is not 0, but we don't care for that - 0 modified is always 0
    //        bool ReachedInsertionPoint(int index)
    //        {
    //            Log.Message("checking instruction " + codeInstructions[index]);
    //            if(index < 0 || index >= codeInstructions.Count)
    //            {
    //                return false;
    //            }
    //            if(codeInstructions[index].LoadsField(offsetFieldInfo)){
    //                Log.Message("field load detected");
    //                Log.Message("Next instruction: " + codeInstructions[index + 1] + " operand: " + codeInstructions[index+1].operand);
    //            }
    //            return false;
    //        }
    //    }
    //}
}
