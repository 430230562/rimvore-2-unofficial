using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RimVore2
{
    [HarmonyPatch(typeof(MainButtonDef), "get_Icon")]
    public static class Patch_MainButtonDef
    {
        [HarmonyPostfix]
        private static void ChangeIcon(MainButtonDef __instance, ref Texture2D __result)
        {
            AlternativeMainTabIcon alternativeIconExtension = __instance.GetModExtension<AlternativeMainTabIcon>();
            if(alternativeIconExtension == null)
                return;
            if(!RV2Mod.Settings.fineTuning.UseAlternativeMainTabIcon)
                return;
            __result = alternativeIconExtension.Icon;
        }
    }
}
