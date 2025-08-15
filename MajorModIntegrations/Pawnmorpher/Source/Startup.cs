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
            Harmony harmony = new Harmony("rv2_pawnmorpher");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
