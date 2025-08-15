using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2.Patches
{
    [HarmonyPatch]
    public static class Patch_Pawn_HealthTracker
    {
        [HarmonyPatch(typeof(Pawn_HealthTracker), "AddHediff", new Type[] { typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo), typeof(DamageWorker.DamageResult) })]
        [HarmonyPrefix]
        public static bool InterceptHediff(Pawn ___pawn, ref Hediff hediff, BodyPartRecord part)
        {
            QuirkManager quirkManager = ___pawn.QuirkManager(false);
            if(quirkManager == null)
            {
                return true;
            }
            if(!quirkManager.TryGetOverriddenHediff(hediff.def, out HediffDef overrideHediffDef))
            {
                return true;
            }
            if(overrideHediffDef == null)
            {
                // block hediff addition if we are overriding with NULL
                if(RV2Log.ShouldLog(false, "Quirks"))
                    RV2Log.Message($"Prevented hediff {hediff.def.defName} from being added to {___pawn.LabelShort} due to quirk override with NULL", "Quirks");
                return false;
            }
            Hediff overrideHediff = HediffMaker.MakeHediff(overrideHediffDef, ___pawn, part);
            if(RV2Log.ShouldLog(false, "Quirks"))
                RV2Log.Message($"Replaced hediff {hediff.def.defName} for {___pawn.LabelShort} with {overrideHediffDef.defName} due to quirk overrides", "Quirks");
            hediff = overrideHediff;

            return true;
        }
    }
}
