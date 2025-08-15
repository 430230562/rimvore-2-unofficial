using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;
using RimWorld;
using System.Reflection;

namespace RimVore2
{
    [HarmonyPatch(typeof(FoodUtility), "ThoughtsFromIngesting")]
    public static class RV2_InterceptSpecialFoodThoughts
    {
        [HarmonyPostfix]
        public static void InterceptSpecialFoodThoughts(Pawn ingester, Thing foodSource, ThingDef foodDef, ref List<FoodUtility.ThoughtFromIngesting> __result)
        {
            List<FoodUtility.ThoughtFromIngesting> backupResult = __result;
            try
            {
                if(__result == null)
                {
                    return;
                }

                // in order to have shared code between 1.2 and 1.3 this is one of the only times you will see 'var' in RV2.
                foreach(var thoughtOrWrapper in __result.ToList())
                {
                    ThoughtDef thought = thoughtOrWrapper.thought;
                    ThoughtDef overrideThought = null;
                    if(ingester?.QuirkManager(false)?.TryGetOverriddenThought(thought, out overrideThought) != true)
                    {
                        continue;
                    }
                    __result.Remove(thoughtOrWrapper);
                    if(RV2Log.ShouldLog(true, "IngestThoughts"))
                        RV2Log.Message($"Removing ingest thought {thoughtOrWrapper.thought} and overriding with {overrideThought}", false, "IngestThoughts");
                    FoodUtility.ThoughtFromIngesting newThought = new FoodUtility.ThoughtFromIngesting()
                    {
                        fromPrecept = thoughtOrWrapper.fromPrecept,
                        thought = overrideThought
                    };
                    __result.Add(newThought);
                }
                // Log.Message("thoughts after overrides: " + string.Join(", ", __result.ConvertAll(thought => thought.defName)));
            }
            catch(Exception e)
            {
                Log.Warning("RimVore-2: Something went wrong when intercepting thoughts from ingesting food, Error:\n" + e);
                __result = backupResult;
            }
        }
    }

    /// <summary>
    /// This patch is absolutely horrible. Mostly because the FoodUtility class was created by Satan himself,
    ///     everything is private and there is a private global static list with thoughts to apply - which is populated by numerous private methods
    /// The core issue is that the AddThoughtsFromIdeo method does not return a list of thoughts, but supplies to the global list instead, 
    ///     so we need to overwrite the original methods return value (which was copied from that static list) with the newly modified static list
    ///     
    /// What a genuinely infuriating patch to write, the food thoughts are a mess. https://www.youtube.com/watch?v=17KmNrG9pE4
    /// </summary>
    [IdeologyRelated]
    [HarmonyPatch(typeof(FoodUtility), "ThoughtsFromIngesting")]
    internal static class Patch_VoreProductConsumptionIdeoThoughts
    {
        internal static MethodInfo thoughtsFromIdeoMethod = AccessTools.Method(typeof(FoodUtility), "AddThoughtsFromIdeo");
        [HarmonyPostfix]
        internal static void VoreProductConsumptionIdeoThoughts(ref List<FoodUtility.ThoughtFromIngesting> __result, Pawn ingester, ThingDef foodDef, List<FoodUtility.ThoughtFromIngesting> ___ingestThoughts)
        {
            try
            {
                if(thoughtsFromIdeoMethod == null)
                {
                    Log.Error("RimWorld has updated and removed the FoodUtility.AddThoughtsFromIdeo method.");
                    return;
                }
                if(!ModsConfig.IdeologyActive || thoughtsFromIdeoMethod == null)
                {
                    return;
                }
                if(foodDef.HasModExtension<ConsumableVoreProductFlag>())
                {
                    // fucking private method, have to use reflection here
                    thoughtsFromIdeoMethod.Invoke(null, new object[] { IdeologyVoreEventDefOf.RV2_ConsumedVoreProduct, ingester, foodDef, MeatSourceCategory.NotMeat });
                    IdeologyPawnData data = ingester.PawnData()?.Ideology;
                    if(data != null)
                    {
                        data.UpdateKeyword("IngestedVoreProduct");
                    }
                }
                __result = ___ingestThoughts;
            }
            catch(Exception e)
            {
                Log.Error("Something went wrong: " + e);
            }
        }
    }
}