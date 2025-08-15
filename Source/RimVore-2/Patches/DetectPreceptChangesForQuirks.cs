using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

/// <summary>
/// This class watches precept changes and detects the closing the ideos tab, from which any changes are made. 
/// If other mods allow changes to precepts outside of the main tab, it'll require its own patch to properly notify the quirks
/// </summary>
namespace RimVore2
{
    [HarmonyPatch(typeof(Ideo), "AddPrecept")]
    public class Patch_Ideo_AddPrecept
    {
        public static bool AnyQuirkCompsAdded = false;

        [HarmonyPostfix]
        private static void PrepareIdeologyQuirkNotify(Precept precept)
        {
            try
            {
                if(precept.def.comps?.Any(comp => comp is PreceptComp_Quirk) == true)
                {
                    if(RV2Log.ShouldLog(false, "IdeoQuirks"))
                        RV2Log.Message("A precept with quirk comps was added", "IdeoQuirks");
                    AnyQuirkCompsAdded = true;
                }
            }
            catch(Exception e)
            {
                Log.Error("Something went wrong: " + e);
            }
        }
    }
    [HarmonyPatch(typeof(Ideo), "RemovePrecept")]
    public class Patch_Ideo_RemovePrecept
    {
        public static bool AnyQuirkCompsRemoved = false;

        [HarmonyPostfix]
        private static void PrepareIdeologyQuirkNotify(Precept precept)
        {
            try
            {
                if(precept.def.comps?.Any(comp => comp is PreceptComp_Quirk) == true)
                {
                    if(RV2Log.ShouldLog(false, "IdeoQuirks"))
                        RV2Log.Message("A precept with quirk comps was removed", "IdeoQuirks");
                    AnyQuirkCompsRemoved = true;
                }
            }
            catch(Exception e)
            {
                Log.Error("Something went wrong: " + e);
            }
        }
    }
    [HarmonyPatch(typeof(Window), "PostClose")]
    public class Patch_Window_PostClose
    {
        [HarmonyPostfix]
        private static void NotifyIdeologyQuirksStale(Window __instance)
        {
            try
            {
                if(__instance is MainTabWindow_Ideos)
                {
                    if(Patch_Ideo_AddPrecept.AnyQuirkCompsAdded || Patch_Ideo_RemovePrecept.AnyQuirkCompsRemoved)
                    {
                        if(RV2Log.ShouldLog(false, "IdeoQuirks"))
                            RV2Log.Message("Notifying all pawns ideo quirks stale", "IdeoQuirks");
                        Patch_Ideo_AddPrecept.AnyQuirkCompsAdded = false;
                        Patch_Ideo_RemovePrecept.AnyQuirkCompsRemoved = false;
                        foreach(Map map in Find.Maps)
                        {
                            foreach(Pawn pawn in map.mapPawns.AllPawns)
                            {
                                if(pawn.Ideo != null)
                                {
                                    pawn.QuirkManager(false)?.Notify_IdeologyChanged();
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Log.Error("Something went wrong: " + e);
            }
        }
    }
}