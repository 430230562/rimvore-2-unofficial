using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Steamworks;
using Verse;
using Verse.AI;

namespace RimVore2
{
    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            Harmony.DEBUG = false;
            Harmony harmony = new Harmony("rv2");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            ReflectionUtility.StartUp();
            BoomDayManager.StartUp(harmony);
            //harmony.Patch(AccessTools.Method(AccessTools.Inner(typeof(ToilEffects), "<>c__DisplayClass10_0"), "<WithProgressBar>b__0"), null, null, new HarmonyMethod(typeof(Patch_ToilEffects), "AllowToilProgressBarForHostileGrapple"));
        }
    }
}
