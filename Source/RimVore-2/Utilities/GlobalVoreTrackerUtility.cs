using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimVore2
{
    public static class GlobalVoreTrackerUtility
    {
        public static List<Pawn> ActivePredators = new List<Pawn>();
        public static List<VoreTracker> ActiveVoreTrackers = new List<VoreTracker>();
        public static Dictionary<Pawn, VoreTrackerRecord> ActivePreyWithRecord = new Dictionary<Pawn, VoreTrackerRecord>();
        
        public static void Initialize()
        {
            ActivePredators.Clear();
            ActivePreyWithRecord.Clear();
            ActiveVoreTrackers.Clear();

            IEnumerable<PawnData> allPawnData = RV2Mod.RV2Component.AllPawnData;
            foreach(PawnData pawnData in allPawnData)
            {
                VoreTracker voreTracker = pawnData.VoreTracker;
                ActiveVoreTrackers.Add(voreTracker);
                if(voreTracker.IsTrackingVore)
                {
                    ActivePredators.Add(pawnData.Pawn);
                    foreach(VoreTrackerRecord record in voreTracker.VoreTrackerRecords)
                    {
                        ActivePreyWithRecord.Add(record.Prey, record);
                    }
                }
            }
        }

        public static void Notify_RecordAdded(VoreTrackerRecord record)
        {
            if(record == null || record.Prey == null || record.Predator == null)
            {
                Log.Warning($"Tried to add record with NULL data to global tracking cache, skipping");
                return;
            }
            if (RV2Log.ShouldLog(true, "GlobalVoreTracker"))
                RV2Log.Message($"Adding record to caches: {record}", "GlobalVoreTracker");

            if (!ActivePredators.Contains(record.Predator))
            {
                ActivePredators.Add(record.Predator);
            }

            VoreTracker predatorTracker = record.VoreTracker;
            if(!ActiveVoreTrackers.Contains(predatorTracker))
            {
                ActiveVoreTrackers.Add(predatorTracker);
            }
            if(!ActivePreyWithRecord.ContainsKey(record.Prey))
            {
                ActivePreyWithRecord.Add(record.Prey, record);
            }
        }

        public static void Notify_RecordRemoved(VoreTrackerRecord record)
        {
            if(record == null || record.Prey == null || record.Predator == null)
            {
                Log.Warning($"Tried to remove record with NULL data from global tracking cache, skipping");
                return;
            }
            if (RV2Log.ShouldLog(true, "GlobalVoreTracker"))
                RV2Log.Message($"Removing record from caches: {record}", "GlobalVoreTracker");

            bool predatorHasNoMoreVore = record.VoreTracker.VoreTrackerRecords.NullOrEmpty();
            if (predatorHasNoMoreVore) 
            {
                if(ActivePredators.Contains(record.Predator))
                {
                    ActivePredators.Remove(record.Predator);
                }

                VoreTracker predatorTracker = record.VoreTracker;
                if(ActiveVoreTrackers.Contains(predatorTracker))
                {
                    ActiveVoreTrackers.Remove(predatorTracker);
                }
            }
            if(ActivePreyWithRecord.ContainsKey(record.Prey))
            {
                if(ActiveVoreTrackers.Any(tracker => tracker.VoreTrackerRecords.Any(r => r.Prey == record.Prey)))
                {
                    if (RV2Log.ShouldLog(false, "GlobalVoreTracker"))
                        RV2Log.Message($"Not removing prey from caches yet, because prey is still vored", "GlobalVoreTracker");
                }
                else
                {
                    ActivePreyWithRecord.Remove(record.Prey);
                }
            }
        }

        public static bool IsActivePredator(this Pawn pawn)
        {
            return ActivePredators.Contains(pawn);
        }

        public static bool IsPreyOf(this Pawn prey, Pawn predator)
        {
            VoreTrackerRecord activeRecord = ActivePreyWithRecord.TryGetValue(prey);
            if(activeRecord == null)
            {
                return false;
            }
            return activeRecord.Predator == predator;
        }

        public static VoreTrackerRecord GetVoreRecord(this Pawn prey)
        {
            if(prey == null)
            {
                return null;
            }
            return ActivePreyWithRecord.TryGetValue(prey);
        }

        public static bool IsActivePrey(this Pawn pawn)
        {
            return ActivePreyWithRecord.ContainsKey(pawn);
        }
    }
}
