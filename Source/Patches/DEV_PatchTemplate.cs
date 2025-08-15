//using System;
//using System.Collections.Generic;
//using System.Linq;
//using HarmonyLib;
//using RimWorld;
//using Verse;
//using Verse.AI;

//namespace RimVore2
//{
//    [HarmonyPatch(typeof(CLASS1), "METHOD")]
//    public class PatchTemplate
//    {
//        [HarmonyPostfix]
//        private static void NAME(ref CLASS1 __instance, ref CLASS2 __result)
//        {
//            CLASS2 backupResult = __result;
//            try
//            {
//                CODE HERE
//            }
//            catch(Exception e)
//            {
//                __result = backupResult;
//                Log.Warning("RimVore-2: Something went wrong " + e);
//                return;
//            }
//        }
//    }
//}