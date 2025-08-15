using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    [HarmonyPatch(typeof(GeneDef), "GetDescriptionFull")]
    public static class Patch_GeneDef
    {
        private const string listEntryPrefix = "  - ";

        [HarmonyPostfix]
        public static void AddGeneDefExtensionData(GeneDef __instance, ref string __result)
        {
            try
            {
                GeneDefExtension_ForcedHediff extension = __instance.GetModExtension<GeneDefExtension_ForcedHediff>();
                if(extension == null)
                {
                    return;
                }
                __result += $"\n\n{"RV2_AddsHediffs".Translate().Colorize(ColoredText.TipSectionTitleColor)}";
                foreach(HediffDef hediff in extension.hediffs)
                {
                    __result += $"\n{listEntryPrefix}{hediff.label}";
                }
            }
            catch(Exception e)
            {
                RV2Log.Error($"Exception during GeneDef description extension: {e}");
                return;
            }
        }
    }
}