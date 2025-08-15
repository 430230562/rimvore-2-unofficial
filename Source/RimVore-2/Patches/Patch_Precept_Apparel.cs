#if v1_4
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2.Patches
{
    // Fix a vanilla bug that causes vore roles to break the ideology UI if any preferred apparel is chosen while having any vore role active
    [HarmonyPatch(typeof(Precept_Apparel), "GetPlayerWarning")]
    public static class Patch_RoleApparelRequirementsFix
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> IgnoreRolesWithNoApparelRequirements(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            bool success = false;
            Label? continueLabel = null; // Nullable so the whole patch fails gracefully if the label is not found for some reason

            for (int i = 0; i < codes.Count; i++)
            {
                yield return codes[i];

                // Look for the start of the loop and store the IL label from it
                if (codes[i].opcode == OpCodes.Br)
                {
                    continueLabel = (Label)codes[i].operand;
                }

                // Look for whenever the Precept_Role is stored (each time it loops over the list of roles)
                if (!success && i < codes.Count && codes[i].opcode == OpCodes.Stloc_1)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_1); // Load the Precept_Role onto the stack
                    yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Precept_Role), "ApparelRequirements")); // Get the list of apparel requirements
                    yield return new CodeInstruction(OpCodes.Brfalse, continueLabel); // If the list is null, skip the loop

                    success = true;
                }
            }

            if (!success)
            {
                RV2Log.Error("Failed to patch Precept_Apparel.GetPlayerWarning getter");
            }
        }
    }
}
#endif