//using System;
//using System.Collections.Generic;
//using System.Linq;
//using HarmonyLib;
//using RimWorld;
//using Verse;
//using Verse.AI;

//namespace RimVore2
//{
//    [HarmonyPatch(typeof(PawnBioAndNameGenerator), "FillBackstorySlotShuffled")]
//    public class PatchTemplate
//    {
//        [HarmonyPostfix]
//        private static void ReplaceBackstoryIfRV2KeywordsInvalid(Pawn pawn, BackstorySlot slot, ref Backstory backstory)
//        {
//            try
//            {
//                string identifier = slot == BackstorySlot.Adulthood ? pawn.story?.adulthood?.identifier : pawn.story?.childhood?.identifier;
//                if(identifier == null)
//                {
//                    Log.Warning("identifier for backstory was null, can not determine if backstory is RV2_Backstory");
//                    return;
//                }
//                RV2_BackstoryDef rv2Backstory = DefDatabase<RV2_BackstoryDef>.GetNamedSilentFail(identifier);
//                if(rv2Backstory == null)
//                {
//                    Log.Message("Backstory not RV2_Backstory, returning");
//                    return;
//                }
//                List<string> pawnKeywords = VoreKeywordUtility.PawnKeywords(pawn);
//                if(!rv2Backstory.requiredKeywords.NullOrEmpty())
//                {
//                    bool hasAllRequiredKeywords = rv2Backstory.requiredKeywords
//                        .TrueForAll(requiredKeyword => pawnKeywords.Contains(requiredKeyword));
//                    if(!hasAllRequiredKeywords)
//                    {
//                        Log.Message("Reshuffling due to missing required keyword, pawn has keywords: " + 
//                            string.Join(", ", pawnKeywords) + 
//                            " and requires : " + 
//                            string.Join(", ", rv2Backstory.requiredKeywords));
//                        ReshuffleRV2Backstory(pawn, slot, ref backstory);
//                        return;
//                    }
//                }
//                if(!rv2Backstory.blockingKeywords.NullOrEmpty())
//                {
//                    bool hasAnyBlockingKeyword = rv2Backstory.blockingKeywords
//                        .Any(blockingKeyword => pawnKeywords.Contains(blockingKeyword));
//                    if(hasAnyBlockingKeyword)
//                    {
//                        Log.Message("Reshuffling due to blocking keyword, pawn has keywords: " +
//                            string.Join(", ", pawnKeywords) +
//                            " and blocking keywords are : " +
//                            string.Join(", ", rv2Backstory.blockingKeywords));
//                        ReshuffleRV2Backstory(pawn, slot, ref backstory);
//                        return;
//                    }
//                }
//            }
//            catch(Exception e)
//            {
//                Log.Warning("RimVore-2: Something went wrong " + e);
//                return;
//            }
//        }

//        private static void ReshuffleRV2Backstory(Pawn pawn, BackstorySlot slot, ref Backstory backstory)
//        {
//            Log.Message("Invalid backstory due to keyword conflict, reshuffling");
//        }
//    }
//}