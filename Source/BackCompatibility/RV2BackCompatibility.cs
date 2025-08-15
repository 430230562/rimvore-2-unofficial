using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    //[HarmonyPatch(typeof(BackCompatibility), "BackCompatibleDefName")]
    public static class RV2BackCompatibility
    {
        [HarmonyPrefix]
        public static void AddRV2BackCompatibilities()
        {
            List<BackCompatibilityConverter> conversionChain = (List<BackCompatibilityConverter>)AccessTools.Field(typeof(BackCompatibility), "conversionChain").GetValue(null);

            conversionChain.Add(new BackCompatibilityConverter_TempEndoRename());
        }
    }
}
