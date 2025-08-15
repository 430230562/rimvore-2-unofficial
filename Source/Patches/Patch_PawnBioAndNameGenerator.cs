using HarmonyLib;
using Verse;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimVore2
{
    [HarmonyPatch]
    public static class Patch_PawnBioAndNameGenerator
    {
        [HarmonyPatch(typeof(PawnBioAndNameGenerator), "BackstorySelectionWeight")]
        [HarmonyPostfix]
        public static void ModifyWithBackstoryCommonality(ref float __result, BackstoryDef bs)
        {
            if(!(bs is RV2_BackstoryDef rv2Backstory))
            {
                return;
            }
            __result *= rv2Backstory.commonality;
        }

    }
}