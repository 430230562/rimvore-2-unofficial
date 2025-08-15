using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimVore2
{

    [HarmonyPatch(typeof(MapInterface), "HandleMapClicks")]
    public static class Patch_MapInterface_HandleMapClicks
    {
        [HarmonyPostfix]
        private static void SubscribePauseTargeter()
        {
            Targeter_ForcePause.Targeter.ProcessInputEvents();
        }
    }
    [HarmonyPatch(typeof(MapInterface), "MapInterfaceOnGUI_BeforeMainTabs")]
    public static class Patch_MapInterface_MapInterfaceOnGUI_BeforeMainTabs
    {
        [HarmonyPostfix]
        private static void SubscribePauseTargeter()
        {
            if(!WorldRendererUtility.WorldRenderedNow)
            {
                Targeter_ForcePause.Targeter.TargeterOnGUI();
            }
            else
            {
                Targeter_ForcePause.Targeter.StopTargeting();
            }
        }
    }
    [HarmonyPatch(typeof(MapInterface), "MapInterfaceUpdate")]
    public static class Patch_MapInterface_MapInterfaceUpdate
    {
        [HarmonyPostfix]
        private static void SubscribePauseTargeter()
        {
            if(!WorldRendererUtility.WorldRenderedNow)
            {
                Targeter_ForcePause.Targeter.TargeterUpdate();
            }
        }
    }
}
