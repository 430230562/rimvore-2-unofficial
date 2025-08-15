using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2.Patches
{
    public static class AnimalVoreEventUtility
    {
        public static void ManipulateEventForVeneratedAnimals(Ideo ideo, ref HistoryEvent ev)
        {
            if(ideo == null || ev.def == null)
            {
                return;
            }
            if(ev.def != IdeologyVoreEventDefOf.RV2_AnimalPrey && ev.def != IdeologyVoreEventDefOf.RV2_AnimalPredator)
            {
                return;
            }
            bool animalIsPredator = ev.def == IdeologyVoreEventDefOf.RV2_AnimalPredator;

            List<ThingDef> veneratedAnimals = ideo.VeneratedAnimals;
            bool canRetrieveAnimal = ev.args.TryGetArg(HistoryEventArgsNames.Subject, out ThingDef animalDef);
            if(!canRetrieveAnimal)
            {
                RV2Log.Warning($"Could not retrieve animal participant (NamedArgument SUBJECT) in vore event {ev.def.defName}");
                return;
            }

            if(!veneratedAnimals.Contains(animalDef))
            {
                ev.def = animalIsPredator ? IdeologyVoreEventDefOf.RV2_AnimalPredator : IdeologyVoreEventDefOf.RV2_AnimalPrey;
            }
            else
            {
                ev.def = animalIsPredator ? IdeologyVoreEventDefOf.RV2_AnimalPredator_Venerated : IdeologyVoreEventDefOf.RV2_AnimalPrey_Venerated;
            }
        }
    }

    [HarmonyPatch(typeof(Ideo), "Notify_MemberTookAction")]
    public static class Patch_Ideo_Notify_MemberTookAction
    {
        [HarmonyPrefix]
        public static void InterceptAnimalVoreEventForIdeo(Ideo __instance, ref HistoryEvent ev)
        {
            AnimalVoreEventUtility.ManipulateEventForVeneratedAnimals(__instance, ref ev);
        }
    }

    [HarmonyPatch(typeof(Ideo), "Notify_MemberKnows")]
    public static class Patch_Ideo_Notify_MemberKnows
    {
        [HarmonyPrefix]
        public static void InterceptAnimalVoreEventForIdeo(Ideo __instance, ref HistoryEvent ev)
        {
            AnimalVoreEventUtility.ManipulateEventForVeneratedAnimals(__instance, ref ev);
        }
    }
}