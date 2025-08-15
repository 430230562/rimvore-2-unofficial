using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;

namespace RimVore2
{
    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    static class RV2_Patch_UI_Widget_GetGizmos
    {
        private readonly static Dictionary<Pawn, Gizmo> cachedGizmos = new Dictionary<Pawn, Gizmo>();

        public static void NotifyPawnStale(Pawn pawn)
        {
            if(cachedGizmos.ContainsKey(pawn))
            {
                cachedGizmos.Remove(pawn);
            }
        }
        public static void NotifyAllStale()
        {
            cachedGizmos.Clear();
        }

        [HarmonyPostfix]
        private static IEnumerable<Gizmo> RV2_Vore_Gizmo(IEnumerable<Gizmo> __result, Pawn __instance)
        {
            foreach(Gizmo gizmo in __result)
            {
                yield return gizmo;
            }
            Gizmo RV2Gizmo = null;
            try
            {
                RV2Gizmo = MakeRV2Gizmo(__instance);
            }
            catch(Exception e)
            {
                Log.Error("RimVore2 - something went wrong when trying to show RV2 widgets. Error: " + e);
            }
            if(RV2Gizmo != null)
            {
                yield return RV2Gizmo;
            }
        }
        private static Gizmo MakeRV2Gizmo(Pawn pawn)
        {

            if(!RV2Mod.Settings.features.ShowGizmo)
            {
                return null;
            }

            if(!VoreValidator.ShouldHaveGizmo(pawn))
            {
                return null;
            }
            if(cachedGizmos.ContainsKey(pawn))
            {
                return cachedGizmos[pawn];
            }
            else
            {
                Gizmo resultingGizmo;
                resultingGizmo = new SubGizmoContainer(pawn);
                cachedGizmos.Add(pawn, resultingGizmo);
                return resultingGizmo;
            }
        }
    }
}