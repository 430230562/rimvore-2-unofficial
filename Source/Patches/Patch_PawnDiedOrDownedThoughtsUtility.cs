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
    [HarmonyPatch(typeof(TaleUtility), "Notify_PawnDied")]
    public class Patch_TaleUtility
    {
        private static readonly BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;
        private static readonly MethodInfo isColonistMethod = typeof(Pawn).GetMethod("get_IsColonist", flags);
        private static readonly MethodInfo colonistWasWillinglyVoredMethod = typeof(Patch_TaleUtility).GetMethod("WasForciblyVored", flags);
        /// <summary>
        /// Catch the colonist killed tale record, check if they were vored willingly and skip tale recording if vore was willing
        /// </summary>
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> AbortColonistKilledTaleOnWillingVore(IEnumerable<CodeInstruction> instructions)
        {
            CodeInstruction loadPawn = new CodeInstruction(OpCodes.Ldarg_0);   // load "Pawn victim" from arguments
            CodeInstruction callForcedVoreCheck = new CodeInstruction(OpCodes.Call, colonistWasWillinglyVoredMethod);  // call static method in this class to check if victim was vored forcibly
            CodeInstruction andOp = new CodeInstruction(OpCodes.And);   // evaluate two boolean values on stack
            List<CodeInstruction> codeInstructions = instructions.ToList();

            //Log.Message(isColonistMethod.ToString());
            //Log.Message(colonistWasWillinglyVoredMethod.ToString());

            foreach(CodeInstruction instruction in codeInstructions)
            {
                yield return instruction;
                if(instruction.Calls(isColonistMethod))
                {
                    yield return loadPawn;
                    yield return callForcedVoreCheck;
                    yield return andOp;
                }
            }
        }

        public static bool WasForciblyVored(Pawn pawn)
        {
            VoreTrackerRecord record = pawn.GetVoreRecord();
            if(record == null)  // no record means the pawn did not die from vore
            {
                return false;
            }
            return record.IsForced;
        }
    }
}
