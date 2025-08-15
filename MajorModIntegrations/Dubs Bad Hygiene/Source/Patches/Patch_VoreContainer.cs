using DubsBadHygiene;
using HarmonyLib;
using RimVore2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RV2_DBH
{
    [HarmonyPatch(typeof(VoreContainer), "TryDropToMap")]
    internal static class Patch_VoreContainer
    {
        [HarmonyPrefix]
        public static void InterceptToiletDisposal(ref IntVec3 position, Map map)
        {
            try
            {
                Building_BaseToilet toilet = (Building_BaseToilet)position.GetThingList(map)
                    .Find(building => building is Building_BaseToilet);

                if(toilet == null)
                {
                    if(RV2Log.ShouldLog(true, "DubsBadHygiene"))
                        RV2Log.Message("Not dropping into a toilet building, falling back", true, "DubsBadHygiene");
                    // no toilet at position, just carry on with base behaviour
                    return;
                }
                // we found a toilet, neat, now we need to check if the toilet is connected to a sewage pipe and then to an outlet
                // we are going to drop to the outlet if available, otherwise just fall back to base behavior
                IEnumerable<CompSewageOutlet> sewers = toilet.pipe?.pipeNet?.Sewers
                    .Where(sewer => sewer is CompSewageOutlet)
                    .Cast<CompSewageOutlet>();
                if(sewers.EnumerableNullOrEmpty())
                {
                    // toilet is not connected to any sewer, base behaviour
                    if(RV2Log.ShouldLog(true, "DubsBadHygiene"))
                        RV2Log.Message($"No sewer found for toilet {toilet.ThingID}, fallback", true, "DubsBadHygiene");
                    return;
                }
                CompSewageOutlet chosenSewer = sewers.RandomElement();
                chosenSewer.GetSewageDumpSpot(out position); 
                if(RV2Log.ShouldLog(true, "DubsBadHygiene"))
                    RV2Log.Message($"Injected new position of sewage outlet {position} for toilet {toilet.ThingID}, falling back to base behaviour", true, "DubsBadHygiene");
                return;
            }
            catch(Exception e)
            {
                Log.Error("Something went wrong when injecting the sewer outlet spot as spawn-location for disposal: " + e);
            }
        }
    }
}
