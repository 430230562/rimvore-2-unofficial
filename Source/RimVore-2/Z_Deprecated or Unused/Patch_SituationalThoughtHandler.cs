
/// this patch doesn't work as hoped. The globally cached ThoughtUtility.situationalNonSocialThoughtDefs constantly re-apply the patched thought
/// because the game thinks it needs to apply the overridden thought. Going with a different implementation instead


//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection.Emit;
//using HarmonyLib;
//using RimWorld;
//using Verse;
//using Verse.AI;

//namespace RimVore2
//{
//    [HarmonyPatch(typeof(SituationalThoughtHandler))]
//    public class Patch_SituationalThoughtHandler
//    {
//        [HarmonyPatch("TryCreateSocialThought")]
//        [HarmonyPrefix]
//        private static void ReplaceOverridenSocialThoughts(ref ThoughtDef def, SituationalThoughtHandler __instance, HashSet<ThoughtDef> ___tmpCachedSocialThoughts)
//        {
//            ReplaceOverridenThoughts(ref def, __instance.pawn, ___tmpCachedSocialThoughts);
//        }

//        [HarmonyPatch("TryCreateThought")]
//        [HarmonyPrefix]
//        private static void ReplaceOverridenMoodThoughts(ref ThoughtDef def, SituationalThoughtHandler __instance, HashSet<ThoughtDef> ___tmpCachedThoughts)
//        {
//            ReplaceOverridenThoughts(ref def, __instance.pawn, ___tmpCachedThoughts);
//        }

//        private static void ReplaceOverridenThoughts(ref ThoughtDef def, Pawn pawn, HashSet<ThoughtDef> cachedThoughts)
//        {
//            QuirkManager quirks = pawn.QuirkManager(false);
//            if(quirks == null)
//                return;
//            if(quirks.TryGetOverriddenThought(def, out ThoughtDef overrideThought))
//            {
//                RV2Log.Message($"{pawn.LabelShort} replaced base situational thought {def.defName} with {overrideThought.defName}", true, true, "Quirks");
//                def = overrideThought;
//                cachedThoughts.Remove(def);
//                cachedThoughts.Add(overrideThought);
//            }
//        }
//    }
//}