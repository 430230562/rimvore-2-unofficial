using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimVore2
{
    [HarmonyPatch(typeof(Thing))]
    [HarmonyPatch("AmbientTemperature", MethodType.Getter)]
    public class Patch_AmbientTemperature
    {
        static Dictionary<string, int> thingIdTickRetrieved = new Dictionary<string, int>();
        [HarmonyPostfix]
        private static void SetPreyAmbientTempToPredInternalTemp(Thing __instance, ref float __result)
        {
            try
            {
                if(!(__instance is Pawn pawn))
                {
                    return;
                }
                if(pawn.Spawned)    // spawned pawns can't be vored
                {
                    return;
                }
                VoreTrackerRecord record = pawn.GetVoreRecord();
                if(record == null)
                {
                    return;
                }
                Pawn predator = record.Predator;
                float predatorTemperature = predator.GetInternalTemperature();
                __result = predatorTemperature;
                if(RV2Log.ShouldLog(true, "OngoingVore"))
                    RV2Log.Message($"Injecting predator {predator.LabelShort}'s internal temperature of {predatorTemperature} as the ambient temperature for prey {pawn.LabelShort}",  true, "OngoingVore");
                return;
            }
            catch(Exception e)
            {
                Log.Warning("RimVore-2: Something went wrong when trying to inject the preys ambient temperature" + e);
                return;
            }
        }
    }
    [IdeologyRelated]
    [HarmonyPatch(typeof(Thing), "Ingested")]
    internal static class Patch_Ingested
    {
        [HarmonyPostfix]
        private static void AddVoreligionEvents(ref Thing __instance, Pawn ingester, float nutritionWanted)
        {
            // for some reason this part fails if ideology is not installed, even though ideology itself isn't required for history events
            if(!ModsConfig.IdeologyActive)
                return;
            if(__instance.def.HasModExtension<ConsumableVoreProductFlag>())
            {
                Find.HistoryEventsManager.RecordEvent(new HistoryEvent(IdeologyVoreEventDefOf.RV2_ConsumedVoreProduct, ingester.Named(HistoryEventArgsNames.Doer)), false);
            }
        }
    }
}