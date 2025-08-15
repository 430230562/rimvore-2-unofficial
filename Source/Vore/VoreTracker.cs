using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

/// <summary>
/// Keeps track of all pawns containing other pawns inside of any of their bodyparts.
/// Pawn.Tick() is patched to check if a pawn is being tracked, if it is, it passes the tick down to all contained prey pawns
/// </summary>

namespace RimVore2
{
    public class VoreTracker : IExposable
    {
        public Pawn pawn;
        public List<VoreTrackerRecord> VoreTrackerRecords;
        public bool IsSynchronized = false;    // not scribed, so every time the game starts up again, it will force the vore tracker to synchronize the hediffs
        public string debug_pawnName;

        public bool IsTrackingVore => VoreTrackerRecords?.Count > 0;

        public bool HasPreyReadyToRelease => VoreTrackerRecords.Any(record => record.HasReachedEnd);
        public int PreyStrugglingCount => VoreTrackerRecords.Count(record => record.StruggleManager.ShouldStruggle);

        public VoreTracker() { }

        public VoreTracker(Pawn pawn)
        {
            this.pawn = pawn;
            debug_pawnName = pawn.GetDebugName();
            VoreTrackerRecords = new List<VoreTrackerRecord>();
        }

        public int DescendentCount()
        {
            int count = 0;
            foreach(VoreTrackerRecord record in VoreTrackerRecords)
            {
                // add prey
                count++;

                // add prey of prey
                VoreTracker preyTracker = record.Prey.PawnData()?.VoreTracker;
                if(preyTracker == null)
                {
                    continue;
                }
                count += preyTracker.DescendentCount();
            }
            return count;
        }

        public void TrackVore(VoreTrackerRecord record)
        {
            if(VoreTrackerRecords.Any(trackedRecord => trackedRecord.Prey == record.Prey))
            {
                RV2Log.Warning("RimVore-2: Tried to vore track a pawn that is already being tracked. Intercepted, no errors should occur.");
                return;
            }
            VoreTrackerRecords.Add(record);
            record.Initialize();
            GlobalVoreTrackerUtility.Notify_RecordAdded(record);
        }

        public VoreTrackerRecord SplitOffNewVore(VoreTrackerRecord originalRecord, Pawn newPrey, VorePath newPath = null, int forcedPathIndex = -1, bool isPathSwitch = false)
        {
            if(newPath == null)
            {
                newPath = originalRecord.VorePath;
            }
            VoreTrackerRecord newRecord = new VoreTrackerRecord(originalRecord)
            {
                Prey = newPrey,
                VorePath = newPath,
                Initiator = originalRecord.Initiator
            };
            newRecord.IsResultOfSwitchedPath = isPathSwitch;
            PreVoreUtility.PopulateRecord(ref newRecord, true);
            if(forcedPathIndex >= 0)
            {
                newRecord.VorePathIndex = forcedPathIndex;
            }

            TrackVore(newRecord);
            if (RV2Log.ShouldLog(false, "VoreContainer"))
                RV2Log.Message($"Split off new record for {newPrey}: \nNEW: {newRecord}\nORIGINAL: {originalRecord}", "VoreContainer");
            return newRecord;
        }

        public void UntrackVore(VoreTrackerRecord record)
        {
            VoreTrackerRecords.Remove(record);
            GlobalVoreTrackerUtility.Notify_RecordRemoved(record);
            RemoveVoredHediff();
            SynchronizeHediffs();
            if(RV2Log.ShouldLog(false, "Debug"))
                RV2Log.Message($"record {record.LogLabel} took {record.VorePath.path.Sum(stage => stage.PassedRareTicks)} rare ticks", "Debug");

            void RemoveVoredHediff()
            {
                Hediff voredHediff = record.Prey.health.hediffSet.GetFirstHediffOfDef(RV2_Common.VoredHediff);
                if(voredHediff != null)
                {
                    record.Prey.health.RemoveHediff(voredHediff);
                }
            }
        }

        public IEnumerable<VoreTrackerRecord> AllRecordsIncludingNested()
        {
            foreach (VoreTrackerRecord record in VoreTrackerRecords)
            {
                yield return record;
                VoreTracker predatorTracker = record.Prey?.PawnData()?.VoreTracker;
                if (predatorTracker != null)
                {
                    foreach (VoreTrackerRecord innerRecord in predatorTracker.AllRecordsIncludingNested())
                    {
                        yield return innerRecord;
                    }
                }
            }
        }

        public void Tick()
        {
            if(!IsSynchronized)
            {
                SynchronizeHediffs();
            }
            if(!IsTrackingVore)
            {
                return;
            }
            if(pawn.Dead)
            {
                EmergencyEjectAll();
                return;
            }
            foreach(VoreTrackerRecord record in VoreTrackerRecords.ToList())
            {
                try
                {
                    record.Tick();
                }
                catch(Exception e)
                {
                    Log.Error("Exception during predator tick for record " + record.ToString() + "\n error: " + e);
                }
            }
        }

        public void TickRare()
        {
            if(!IsTrackingVore)
            {
                return;
            }
            if(pawn.Dead)
            {
                EmergencyEjectAll();
                return;
            }
            foreach(VoreTrackerRecord record in VoreTrackerRecords.ToList())
            {
                try
                {
                    record.TickRare();
                }
                catch(Exception e)
                {
                    Log.Error("Exception during predator rare tick for record " + record.ToString() + "\n error: " + e);
                }
            }
        }

        public void SynchronizeHediffs()
        {
            if(pawn.health?.hediffSet == null)
            {
                if(RV2Log.ShouldLog(false, "VoreHediffs"))
                    RV2Log.Message($"Pawn {pawn.LabelShort} has no health, no vore hediffs to apply", "VoreHediffs");
                return;
            }
            if(RV2Log.ShouldLog(true, "VoreHediffs"))
                RV2Log.Message($"Synchronizing hediffs for {pawn.LabelShort}", false, "VoreHediffs");
            ResetHediffConnections();
            foreach(VoreTrackerRecord record in VoreTrackerRecords)
            {
                ConnectHediff(record);
                EnsurePreyHediff(record.Prey);
            }
            UpdateHediffs();
            IsSynchronized = true;
        }

        private void EnsurePreyHediff(Pawn pawn)
        {
            if(!pawn.health.hediffSet.HasHediff(RV2_Common.VoredHediff))
            {
                pawn.health.AddHediff(RV2_Common.VoredHediff);
            }
        }

        private void ResetHediffConnections()
        {
            IEnumerable<Hediff_ContainingPrey> voreHediffs = pawn.health.hediffSet.hediffs
                .FindAll(hediff => hediff is Hediff_ContainingPrey)
                .Cast<Hediff_ContainingPrey>();
            voreHediffs.ForEach(hediff =>
            {
                if(RV2Log.ShouldLog(true, "VoreHediffs"))
                    RV2Log.Message($"Clearing connected records for hediff {hediff.Label}", false, "VoreHediffs");
                hediff.ConnectedVoreRecords.Clear();
            });
        }

        private void ConnectHediff(VoreTrackerRecord record)
        {
            HediffDef hediffDef = record.CurrentHediffDef;
            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
            if(hediff == null)
            {
                hediff = pawn.health.AddHediff(hediffDef, record.CurrentBodyPart);
            }
            Hediff_ContainingPrey voreHediff = (Hediff_ContainingPrey)hediff;
            voreHediff.ConnectedVoreRecords.Add(record);
            if(RV2Log.ShouldLog(true, "VoreHediffs"))
                RV2Log.Message($"Connected {record.LogLabel} to hediff {voreHediff.Label}", false, "VoreHediffs");
        }

        private void UpdateHediffs()
        {
            IEnumerable<Hediff_ContainingPrey> voreHediffs = pawn.health.hediffSet.hediffs
                .FindAll(hediff => hediff is Hediff_ContainingPrey)
                .Cast<Hediff_ContainingPrey>();
            foreach(Hediff_ContainingPrey voreHediff in voreHediffs)
            {
                if(voreHediff.ConnectedVoreRecords.Count == 0)
                {
                    if(RV2Log.ShouldLog(true, "VoreHediffs"))
                        RV2Log.Message($"Removing vore hediff without VoreTrackerConnections: {pawn.LabelShort} | {voreHediff.Label}", false, "VoreHediffs");
                    pawn.health.RemoveHediff(voreHediff);
                }
                else
                {
                    voreHediff.UpdateLabel();
                }
            }
        }

        public void EmergencyEjectAll()
        {
            foreach(VoreTrackerRecord record in VoreTrackerRecords?.ToList())
            {
                try
                {
                    EmergencyEject(record);
                }
                catch (Exception e)
                {
                    Log.Error($"Unhandled exception caught: {e}");
                }
            }
        }

        public void EmergencyEject(VoreTrackerRecord record)
        {
            bool? successfullyDroppedContents;
            try
            {
                successfullyDroppedContents = record.VoreContainer?.TryDropAllThings();
            }
            catch(Exception e)
            {
                RV2Log.Error($"Unhandled exception caught: {e}", "VoreContainer");
                successfullyDroppedContents = false;
            }
            if (successfullyDroppedContents == false)
            {
                RV2Log.Warning($"Could not drop vore container contents during emergency eject. Some contents may get lost, potentially including pawns", "VoreContainer");
            }
            UntrackVore(record);

            // probably don't want to resolve vore on a broken vore interaction
            // PostVoreUtility.ResolveVore(record, true);
            Log.Warning($"EMERGENCY EJECT for predator {record.Predator?.LabelShort} and prey {record.Prey?.LabelShort}");
        }

        public void EjectAll(bool ignoreCanEjectRestrictions = false)
        {
            foreach(VoreTrackerRecord record in VoreTrackerRecords?.ToList())
            {
                Eject(record, ignoreCanEjectRestrictions: ignoreCanEjectRestrictions);
            }
        }

        public void Eject(VoreTrackerRecord record, Pawn ejectingPawn = null, bool ignoreCanEjectRestrictions = false, bool simulateNullMap = false)
        {
            if(!ignoreCanEjectRestrictions && !record.CanEject)
            {
                return;
            }
            // TODO path reversal through pred
            if(RV2Log.ShouldLog(false, "OngoingVore"))
                RV2Log.Message($"Clearing vore for predator {record.Predator.LabelShort} and prey {record.Prey.LabelShort}", "OngoingVore");

            record.VoreContainer.TryDropAllThings(simulateNullMap);
            UntrackVore(record);
            PostVoreUtility.ResolveVore(record);
            if(ejectingPawn != null && record.IsInterrupted)
            {
                PostVoreUtility.RegisterInterruptedVoreEvent(ejectingPawn);
            }
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref VoreTrackerRecords, "TrackedVores", LookMode.Deep, new object[0]);
            Scribe_References.Look(ref pawn, "pawn", true);
            Scribe_Values.Look(ref debug_pawnName, "debug_pawnName");
            if(Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                foreach(VoreTrackerRecord record in VoreTrackerRecords.ToList())
                {
                    if(record.Predator == null)
                    {
                        Log.Error("Had to remove vore record due to missing Predator");
                        VoreTrackerRecords.Remove(record);
                        continue;
                    }
                    if(record.Prey == null)
                    {
                        Log.Error("Had to remove vore record due to missing Prey");
                        VoreTrackerRecords.Remove(record);
                        continue;
                    }
                }
                if(debug_pawnName == null)
                {
                    debug_pawnName = pawn.GetDebugName();
                }
            }
        }

        public override string ToString()
        {
            if(VoreTrackerRecords.Count == 0)
            {
                return "<EmptyVoreTracker>";
            }
            return string.Join("\n", VoreTrackerRecords.ConvertAll(v => v.ToString()));
        }
    }
}
