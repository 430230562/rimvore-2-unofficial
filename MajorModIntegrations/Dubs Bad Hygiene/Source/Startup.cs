using System.Reflection;
using HarmonyLib;
using Verse;

namespace RV2_DBH
{
    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            Harmony.DEBUG = false;
            Harmony harmony = new Harmony("rv2_dbh");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            harmony.Patch(AccessTools.Method(Common.ClosestSanitationType, "FindBestToilet"), null, new HarmonyMethod(AccessTools.Method(typeof(Patch_ClosestSanitation), nameof(Patch_ClosestSanitation.InjectAllowedToiletsForDisposal))));
        }
    }
}
