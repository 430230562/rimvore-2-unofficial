//using System;
//using System.Collections.Generic;
//using System.Linq;
//using HarmonyLib;
//using RimWorld;
//using Verse;
//using Verse.AI;

//namespace RimVore2
//{
//    [HarmonyPatch(typeof(TickManager), "DoSingleTick")]
//    public class Patch_TickManager
//    {
//        [HarmonyPostfix]
//        private static void RV2Ticks()
//        {
//            if(GenTicks.TicksAbs % GenTicks.TickRareInterval == 0)
//                RareTick();
//        }

//        private static void RareTick()
//        {
//            try
//            {
//                RuleCacheManager.RareTick();
//            }
//            catch (Exception e)
//            {
//                Log.Warning("RimVore-2: Something went wrong during RV2 rare ticks: " + e);
//                return;
//            }
//        }
//    }
//}