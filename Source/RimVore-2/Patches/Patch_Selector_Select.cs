using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;

namespace RimVore2
{
    /// <summary>
    /// When selecting a pawn, if the Quirk UI is open, display that pawn's quirks.
    /// </summary>
    [HarmonyPatch(typeof(Selector), nameof(Selector.Select))]
    public static class Patch_Selector_Select
    {
        [HarmonyPrefix]
        public static void QuirksUpdatePawnSelection(object obj)
        {
            if(obj is Pawn pawn)
            {
                Window_Quirks previousWindow = Find.WindowStack.WindowOfType<Window_Quirks>();
                if(previousWindow == null)
                {
                    return;
                }
                QuickSearchWidget poolFilter = previousWindow.poolFilter;
                QuickSearchWidget quirkFilter = previousWindow.quirkFilter;
                UnityEngine.Rect previousRect = previousWindow.windowRect;
                previousWindow.Close();

                Window_Quirks newWindow = new Window_Quirks(pawn);
                newWindow.poolFilter = poolFilter;
                newWindow.quirkFilter = quirkFilter;
                Find.WindowStack.Add(newWindow);
                newWindow.windowRect = previousRect;
            }
        }
    }
}
