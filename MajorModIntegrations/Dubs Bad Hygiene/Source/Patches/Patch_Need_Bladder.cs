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
    /// <summary>
    /// Eject all ejectable prey when the pawn is dumping
    /// </summary>
    [HarmonyPatch(typeof(Need_Bladder), "dump")]
    internal static class Patch_Need_Bladder
    {
        [HarmonyPrefix]
        public static void EjectPreyOnDump(Pawn ___pawn)
        {
            try
            {
                VoreTracker tracker = ___pawn.PawnData().VoreTracker;
                if(tracker == null)
                {
                    return;
                }
                IEnumerable<VoreTrackerRecord> ejectableRecords = tracker.VoreTrackerRecords
                    .Where(record => record.HasReachedEnd);
                if(RV2_DBH_Settings.dbh.LimitToiletDisposalToContainers)
                {
                    ejectableRecords = ejectableRecords
                        .Where(record => Common.ValidForToiletDisposal(record));
                }
                if(ejectableRecords.EnumerableNullOrEmpty())
                {
                    if(RV2Log.ShouldLog(true, "DubsBadHygiene"))
                        RV2Log.Message($"No prey to eject for {___pawn.LabelShort}", false, "DubsBadHygiene");
                    return;
                }
                if(RV2Log.ShouldLog(false, "DubsBadHygiene")) 
                    RV2Log.Message($"Ejecting prey from {___pawn.LabelShort} because of toilet dump: {string.Join(", ", ejectableRecords.Select(r => r.Prey.LabelShort))}", "DubsBadHygiene");
                ejectableRecords
                    .ToList()   // clone list to prevent CollectionModifiedException
                    .ForEach(record => tracker.Eject(record));
            }
            catch (Exception e)
            {
                Log.Error("Something went wrong when trying eject prey during dump(): " + e);
            }
        }
    }
}
