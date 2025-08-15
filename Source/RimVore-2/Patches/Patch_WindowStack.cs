using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    [HarmonyPatch(typeof(WindowStack), "get_WindowsForcePause")]
    public static class Patch_WindowStack
    {
        [HarmonyPostfix]
        private static void ForcePauseIfPauseTargeterActive(ref bool __result)
        {
            if(Targeter_ForcePause.Targeter?.IsTargeting == true)
                __result = true;
        }
    }
}
