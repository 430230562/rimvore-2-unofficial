//using System;
//using System.Collections.Generic;
//using System.Linq;
//using HarmonyLib;
//using RimWorld;
//using Verse;
//using Verse.AI;

//namespace RimVore2
//{
//    [HarmonyPatch(typeof(HediffSet))]
//    [HarmonyPatch("BleedRateTotal", MethodType.Getter)]
//    public class PatchTemplate
//    {
//        [HarmonyPrefix]
//        private static bool StopBleedingForPreyBeingHealed(ref HediffSet __instance, ref float __result)
//        {
//            try
//            {
//                Pawn pawn = __instance.pawn;
//                VoreTrackerRecord record = GlobalVoreTrackerUtility.GetRecordForPrey(pawn);
//                if(record == null)
//                {
//                    return true;
//                }
//                if(record.VorePath.VoreGoal != VoreGoalDefOf.Heal)
//                {
//                    return true;
//                }
//                __result = 0f;
//                return false;
//            }
//            catch(Exception e)
//            {
//                Log.Warning("RimVore-2: Something went wrong when trying to stop prey from bleeding during heal vore" + e);
//                return true;
//            }
//        }
//    }
//}