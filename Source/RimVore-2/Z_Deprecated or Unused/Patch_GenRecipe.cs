/// this feature does not work, passing by reference for the return __result results in new objects being created
/// which makes no sense to me, but it means that we add the comps to a different Thing than the Thing that is later being placed on the map


//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using HarmonyLib;
//using RimWorld;
//using Verse;
//using Verse.AI;

//namespace RimVore2
//{
//    [HarmonyPatch(typeof(GenRecipe), "MakeRecipeProducts")]
//    public class Patch_GenRecipe
//    {
//        [HarmonyPostfix]
//        private static void AppendDeadPreyInformation(IEnumerable<Thing> __result, List<Thing> ingredients)
//        {
//            Log.Message($"Pre: {string.Join(", ", GetAllThingsAndInnerThings(__result))}");
//            List<Thing> createdThings = __result.ToList();
//            try
//            {
//                Log.Message($"Mid: {string.Join(", ", GetAllThingsAndInnerThings(createdThings))}");
//                ThingComp_DeadPreyInformation sourceComp = FindDeadPreyInformationSource(ingredients);
//                if(sourceComp == null)
//                    return;
//                RV2Log.Message("Adding dead prey information to the items created by recipe: " + String.Join(", ", __result.Select(t => t.LabelShort)), "Crafting");
//                IEnumerable<ThingComp_DeadPreyInformation> targetComps = GetAllDeadPreyCompTargets(createdThings);

//                foreach (ThingComp_DeadPreyInformation targetComp in targetComps)
//                {
//                    targetComp.CopyFrom(sourceComp);
//                    RV2Log.Message("Copied comp info to " + targetComp.ToString());
//                }

//                __result = createdThings;
//                Log.Message($"Post: {string.Join(", ", GetAllThingsAndInnerThings(__result))}");
//            }
//            catch (Exception e)
//            {
//                Log.Warning("RimVore-2: Something went wrong when trying to check a created recipe for VoreProductContainers to move dead prey information: " + e);
//                return;
//            }
//        }
//        private static ThingComp_DeadPreyInformation FindDeadPreyInformationSource(List<Thing> things)
//        {
//            return things
//                .Where(t => t is ThingWithComps)    // all things that have comps
//                .Cast<ThingWithComps>()
//                .SelectMany(t => t.AllComps)    // get all comps of all things
//                .First(c => c is ThingComp_DeadPreyInformation) // retrieve info comp
//                as ThingComp_DeadPreyInformation;   // cast
//        }

//        private static IEnumerable<ThingComp_DeadPreyInformation> GetAllDeadPreyCompTargets(IEnumerable<Thing> things)
//        {
//            foreach(Thing thing in GetAllThingsAndInnerThings(things))
//            {
//                if(!(thing is ThingWithComps thingWithComps))
//                    continue;
//                ThingComp_DeadPreyInformation deadPreyComp = thingWithComps.GetComp<ThingComp_DeadPreyInformation>();
//                if(deadPreyComp == null)
//                    continue;
//                yield return deadPreyComp;
//            }
//        }

//        private static IEnumerable<Thing> GetAllThingsAndInnerThings(IEnumerable<Thing> things)
//        {
//            foreach(Thing thing in things)
//            {
//                yield return thing;
//                if(thing is MinifiedThing minifiedThing)
//                {
//                    yield return minifiedThing.InnerThing;
//                }
//            }
//        }

//        [DebugAction("RimVore-2", "Debug", false, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
//        private static void ExplosionBomb()
//        {
//            IntVec3 cell = UI.MouseCell();
//            foreach(Thing t in cell.GetThingList(Find.CurrentMap))
//            {
//                if(t is ThingWithComps thingWithComps)
//                    Log.Message($"Comps: {thingWithComps.AllComps.Count} - {string.Join(", ", thingWithComps.AllComps)}");
//                if(t is MinifiedThing miniThing)
//                {
//                    Thing innerThing = miniThing.InnerThing;
//                    Log.Message($"Minified, inner thing: {miniThing.InnerThing}");
//                    if(innerThing is ThingWithComps innerThingWithComps)
//                        Log.Message($"Inner thing has comps: {innerThingWithComps.AllComps.Count} - {string.Join(", ", innerThingWithComps.AllComps)}");
//                }
//                Log.Message($"Desc: {t.LabelShort}: {t.DescriptionFlavor}");
//            }
//        }
//    }
//}