//using HarmonyLib;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection.Emit;
//using System.Text;
//using System.Threading.Tasks;
//using Verse;
//using Verse.AI;

//namespace RimVore2
//{
//    /// <summary>
//    /// This patch allows the progress bar to display for hostile pawns if they are vore-grappling.
//    /// Normally base game checks if the toil actors faction is the player and only then draws the progress bar
//    /// 
//    /// We are simply adding another jump-possibility based on the static CombatUtility.IsInvolvedInGrapple
//    /// </summary>
//    public static class Patch_ToilEffects
//    {
//        public static IEnumerable<CodeInstruction> AllowToilProgressBarForHostileGrapple(IEnumerable<CodeInstruction> instructions, ILGenerator il)
//        {
//            Label labelAfterFactionCheck = default(Label);
//            CodeInstruction breakInstruction = null;

//            if(!FindAnchors())
//            {
//                Log.Error($"RV2: Transpiler patch malfunction: label null ? {labelAfterFactionCheck == default} breakInstruction null ? {breakInstruction == null}");
//                foreach(CodeInstruction instruction in instructions)
//                {
//                    yield return instruction;
//                }
//                yield break;
//            }

//            foreach(CodeInstruction instruction in instructions)
//            {
//                yield return instruction;
//                // insert our instructions after anchor instruction
//                if(instruction == breakInstruction)
//                {
//                    yield return new CodeInstruction(OpCodes.Ldarg_0);  // load this
//                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ToilEffects), "toil"));    // load this.toil
//                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Toil), "actor")); // load this.toil.actor
//                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CombatUtility), nameof(CombatUtility.IsInvolvedInGrapple)));   // call static method with pawn
//                    yield return new CodeInstruction(OpCodes.Brtrue, labelAfterFactionCheck);   // if pawn is involved, allow progress bar -> jump to label
//                }
//            }

//            bool FindAnchors()
//            {
//                Log.Message(String.Join(", ", instructions.Select(i => i.opcode.ToString() + " " + i?.operand?.ToString())));
//                // our check is the very first equality check: if(toil.actor.Faction != Faction.OfPlayer)
//                breakInstruction = instructions.FirstOrDefault(instruction => instruction.opcode == OpCodes.Beq_S);
//                if(breakInstruction == null)
//                    return false;
//                labelAfterFactionCheck = breakInstruction.labels[0];
//                if(labelAfterFactionCheck == null)
//                    return false;

//                return true;
//            }
//        }
//    }
//}
