
// disabled until further notice. Pre- and Postfixing FreeHumanlikesOfFaction breaks too many parts of the base game, a transpiler patch for the ColonistBarDrawer must be written to inject vored pawns.

/*using HarmonyLib;
using Verse;
using System.Collections.Generic;
using RimWorld;
using System.Linq;
using System;
using UnityEngine;

namespace RimVore2
{
    [HarmonyPatch(typeof(MapPawns), "FreeHumanlikesOfFaction")]
    public static class RV2_Patch_MapPawns_FreeHumanlikesOfFaction
    {
        /// <summary>
        /// Get the list for the colonist bar and add despawned colonists that are vored by other colonists
        /// </summary>
        [HarmonyPostfix]
        public static void AddVoredPawns(ref MapPawns __instance, ref IEnumerable<Pawn> __result)
        {
            IEnumerable<Pawn> backupResult = __result;
            try
            {
                List<Pawn> allPawns = __result.ToList();
                // go through each pawn and add their prey to the list if they have any
                List<Pawn> predatorPawns = GlobalVoreTrackerManager.AllVoringPredators; //allPawns.FindAll(pawn => pawn.IsTrackingVore());
                foreach (Pawn pawn in predatorPawns)
                {
                    // only apply to predators on current map
                    if(pawn.Map != allPawns.Find(p => p.Map != null).Map) {
                        continue;
                    }
                    PawnData pawnData = SaveStorage.DataStore?.GetPawnData(pawn);
                    List<Pawn> allPreyForPawn = pawnData?.VoreTracker?.TrackedVores
                        .ConvertAll(record => record.Prey)  // take the prey
                        // .FindAll(p => p?.Dead == false)   // prey is null if transformed to products
                        .FindAll(p => p.IsColonist);    // don't want to draw animals or raiders in colonist bar
                    allPawns.AddRange(allPreyForPawn);
                }
                __result = allPawns;
            }
            catch (Exception e)
            {
                // Exceptions prevent the entire UI from being drawn, so just "skip" the patch and yell at the player
                Log.Error("Something went wrong when RimVore-2 tried to determine vored pawns to put into ColonistBar:\n" + e);
                __result = backupResult;
            }
        }
    }
    [HarmonyPatch(typeof(ColonistBarColonistDrawer), "DrawColonist")]
    public static class RV2_Patch_ColonistBarColonistDrawer_DrawColonist
    {
        /// <summary>
        /// Let the game draw the colonist icon and then draw a small icon in the upper right corner on top of it
        /// </summary>
        [HarmonyPostfix]
		public static void DrawVoredPawn(ref ColonistBarColonistDrawer __instance, Rect rect, Pawn colonist, Map pawnMap, bool highlight, bool reordering)
        {
            try
            {
                VoreTrackerRecord record = GlobalVoreTrackerManager.GetRecordForPrey(colonist);
                if(record == null)
                {
                    return;
                }
                // size of the vore icon in relation to the pawn, do not set over 1f
                float iconSizeFraction = 0.5f;
                // upper right corner
                Rect preyIconRect = new Rect    
                (
                    rect.x + (rect.width - rect.width * iconSizeFraction),
                    rect.y,
                    rect.width * iconSizeFraction,
                    rect.height * iconSizeFraction
                );
                Texture voreIcon = record.VoreIcon;
                GUI.DrawTexture(preyIconRect, voreIcon);
            }
            catch(Exception e)
            {
                Log.Error("Something went wrong when RimVore-2 tried to draw a vored pawn:\n" + e);
            }
        }
    }
    [HarmonyPatch(typeof(ColonistBar), "ColonistOrCorpseAt")]
    public static class CameraToPredIfPreyClicked
    {
        [HarmonyPostfix]
        public static void FindPred(ref Thing __result)
        {
            Thing backupResult = __result;
            try
            {
                Thing prey = __result;
                VoreTrackerRecord record = GlobalVoreTrackerManager.GetRecordForPrey((Pawn)prey);
                if(record == null)
                {
                    return;
                }
                __result = record.Predator;
            }
            catch(Exception e)
            {
                Log.Error("Something went wrong when RimVore-2 tried to move to the predator of the clicked prey:\n" + e);
                __result = backupResult;
            }
        }
    }
}

/* recursive method for nested vore. Disabled for now, we just block nested vore currently
private static List<Pawn> GetAllPrey(Pawn pawn)
{
    PawnData pawnData = SaveStorage.DataStore.GetPawnData(pawn);
    // pre-emptive exit if VoreTracker is null or no vore is being tracked
    VoreTracker voreTracker = pawnData.VoreTracker;
    if(voreTracker?.IsTrackingVore != true)
    {
        return new List<Pawn>();
    }
    List<Pawn> preys = voreTracker.TrackedVores
        .ConvertAll(record => record.Prey)  // take all prey
        .FindAll(p => p != null);   // prey is null if dead and removed
    List<Pawn> outPreys = new List<Pawn>();
    foreach(Pawn preyin preys)
    {
        outPrey.AddRange(GetAllPrey(prey));
    }
    preys.AddRange(outPreys);
    return preys;
}*/
