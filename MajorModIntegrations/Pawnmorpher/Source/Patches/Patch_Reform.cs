using HarmonyLib;
using RimVore2;
using Pawnmorph;
using Pawnmorph.TfSys;
using Verse;

namespace RV2_PawnMorpher
{
    /// <summary>
    /// Patch by Error: String Expected, Got Nil
    /// </summary>
    [HarmonyPatch(typeof(RollAction_Reform))]
    public class Patch_Reform
    {
        private static bool CanReformToSapientHuman(RollAction_Reform rollAction, out Pawn targetPawn)
        {
            targetPawn = null;
            if(!RV2_PM_Settings.pm.PawnmorpherAnimalReformation)
                return false;

            Traverse traverse = Traverse.Create(rollAction);
            Pawn sourcePawn = traverse.Property("ResultSourcePawn").GetValue<Pawn>();
            if(!VoreKeywordUtility.IsAnimal(sourcePawn))
            {
                return false;
            }

            targetPawn = traverse.Property("TargetPawn").GetValue<Pawn>();
            bool targetCanBeSapientHuman = VoreKeywordUtility.IsHumanoid(targetPawn) || targetPawn.IsFormerHuman();
            return targetCanBeSapientHuman;
        }

        [HarmonyPatch("MakeReformedPawn")]
        [HarmonyPostfix]
        public static void FormerHumanReformPostfix(RollAction_Reform __instance, Pawn __result)
        {
            if(!CanReformToSapientHuman(__instance, out Pawn targetPawn))
            {
                return;
            }

            float sapienceLevel = 1;
            if(targetPawn.IsFormerHuman())
            {
                sapienceLevel = (float)targetPawn.GetSapienceLevel();
            }

            FormerHumanUtilities.MakeAnimalSapient(targetPawn, __result, sapienceLevel);
            // for reasons unknown to me, the above method doesn't automatically generate the TransformedPawn too, so the original pawn
            // isn't tracked! this needs to be done manually, which is below (copied from pre-existing Pawnmorpher code)
            // figuring this out cost me... far too many hours of bugfixing
            TransformedPawnSingle inst = new TransformedPawnSingle
            {
                original = targetPawn,
                animal = __result,
                mutagenDef = MutagenDefOf.defaultMutagen
            };
            PawnmorphGameComp gameComp = Find.World.GetComponent<PawnmorphGameComp>();
            gameComp.AddTransformedPawn(inst);
        }

        // the StripAndDestroyOriginalPawn() method is usually performed after generating a reformed pawn
        // however, a former human cannot be reverted if their original pawn has been destroyed, so we stop that from happening by doing 
        // the method's normal functions without destroying the pawn, in the case of former human reformation
        [HarmonyPatch("StripAndDestroyOriginalPawn")]
        [HarmonyPrefix]
        public static bool StopDestroyOnFormerHumanReformation(RollAction_Reform __instance, VoreTrackerRecord record)
        {
            if(!CanReformToSapientHuman(__instance, out Pawn targetPawn))
            {
                return true;
            }

            record.VoreContainer.TryAddOrTransfer(targetPawn.apparel?.WornApparel);
            record.VoreContainer.TryAddOrTransfer(targetPawn.equipment?.AllEquipmentListForReading);
            return false;
        }
    }
}
