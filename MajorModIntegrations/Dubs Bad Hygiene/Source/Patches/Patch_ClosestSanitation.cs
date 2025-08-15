using DubsBadHygiene;
using HarmonyLib;
using RimVore2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace RV2_DBH
{
    /// patched manually from <see cref="Startup"/> because it's FUCKING INTERNAL
    public static class Patch_ClosestSanitation
    {
        /// <notes>
        /// Capitalized method arguments, ugh
        /// </notes>
        public static void InjectAllowedToiletsForDisposal(ref LocalTargetInfo __result, Pawn pawn, bool Urgent, float shortRange)
        {
            Thing toilet = __result.Thing;
            if(toilet == null)
            {
                return;
            }

            ThingComp_DesignatableDumpToilet dumpToiletDesignation = __result.Thing.TryGetComp<ThingComp_DesignatableDumpToilet>();
            if(dumpToiletDesignation == null || dumpToiletDesignation.isEnabled)
            {
                // if no designation or the designation exists and allows disposals, always allow the toilet
                return;
            }

            List<VoreTrackerRecord> records = pawn?.PawnData(false)?.VoreTracker?.VoreTrackerRecords;
            if(records.NullOrEmpty() || !records.Any(r => Common.ValidForToiletDisposal(r)))
            {
                // if the pawn has no prey to release, the designation doesn't matter
                return;
            }

            if(RV2Log.ShouldLog(false, "DBH"))
                RV2Log.Message("Detected attempt to dump prey in toilet not designated for it, trying to find backup", "DBH");

            IEnumerable<Thing> toilets = SanitationUtil.AllFixtures(pawn.Map)
                .Where(t => IsValidToiletForPreyRelease(t));
            /// so DBH uses a customGlobalSearchSet where it loads its cached fixtures. But the game just ignores the set and runs the normal search anyways
            /// I can't waste the time on investigating or trouble shooting this, so whatever
            __result = GenClosest.ClosestThingReachable
            (
                root: pawn.Position, 
                map: pawn.Map, 
                thingReq: ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), 
                peMode: PathEndMode.OnCell, 
                traverseParams: TraverseParms.For(pawn), 
                maxDistance: shortRange, 
                validator: IsValidToiletForPreyRelease, 
                customGlobalSearchSet: toilets, 
                searchRegionsMin: 0, 
                searchRegionsMax: 10, 
                forceAllowGlobalSearch: true
            );

            if(RV2Log.ShouldLog(false, "DBH"))
            {
                if(__result.HasThing)
                {
                    RV2Log.Message($"Found toilet to dump prey in at position {__result.Thing.Position}", "DBH");
                }
                else
                {
                    RV2Log.Message($"Could not find valid toilet to dump prey in", "DBH");
                }
            }

            bool IsValidToiletForPreyRelease(Thing thing)
            {
                if(!(thing is Building_BaseToilet))
                {
                    return false;
                }
                if(!IsEverUsable(thing, pawn, pawn, Urgent, new FixtureType[] { FixtureType.Toilet }, false))
                {
                    return false;
                }
                ThingComp_DesignatableDumpToilet comp = thing.TryGetComp<ThingComp_DesignatableDumpToilet>();
                return comp != null && comp.isEnabled;
            }
        }

        

        public static MethodInfo IsEverUsableInfo = AccessTools.Method(Common.ClosestSanitationType, "IsEverUsable");
        /// <summary>
        /// Fffffffucking internal classes. Detour via reflection.
        /// </summary>
        public static bool IsEverUsable(Thing x, Pawn fetcher, Pawn user, bool Urgent, FixtureType[] Fixtures, bool CaresAboutContamination = true)
        {
            return (bool)IsEverUsableInfo.Invoke(null, new object[] { x, fetcher, user, Urgent, Fixtures, CaresAboutContamination });
        }
    }
}
