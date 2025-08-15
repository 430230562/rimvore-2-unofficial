using System.Reflection;
using HarmonyLib;
using Verse;
using System.Collections.Generic;

namespace RV2_RJW
{
    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            Harmony.DEBUG = false;
            Harmony harmony = new Harmony("rv2_rjw");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            if(ModLister.AnyFromListActive(new List<string>() { "rjw.cum" }))
            {
                // Let's just hope RJW doesn't just update the name of the type :) because RJW is so stable, so absolutely no way that happens :)))
                harmony.Patch(AccessTools.Method(AccessTools.TypeByName("rjwcum.CumHelper"), "cumOn"), new HarmonyMethod(AccessTools.Method(typeof(Patch_SemenHelper), "AddCumReservesIfAvailable")));
            }
        }
    }
}
//[HarmonyPatch(typeof(SemenHelper), "cumOn")]
