using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace RimVore2
{
    public class RV2Component : GameComponent
    {
        private Dictionary<int, PawnData> pawnData = new Dictionary<int, PawnData>();
        public RuntimeUniqueIDsManager RuntimeUniqueIDsManager = new RuntimeUniqueIDsManager();

        public IEnumerable<PawnData> AllPawnData => pawnData.Values
            .Where(value => value != null);

        public RV2Component(Game game)
        {
            // assign component to data utility for faster referencing
            RV2Mod.RV2Component = this;
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            try
            {
                DebugTools.Reset.ResyncAllHediffs();
                GlobalVoreTrackerUtility.Initialize();
            }
            catch(Exception e)
            {
                Log.Error($"Caught exception in FinalizeInt(): {e}");
            }
        }

        public PawnData GetPawnData(Pawn pawn)
        {
            PawnData data = pawnData.TryGetValue(pawn.thingIDNumber);
            if(data == null)
            {
                if(RV2Log.ShouldLog(false, "Debug"))
                    RV2Log.Message($"Initialized new PawnData for pawn {pawn.LabelShort} with ID {pawn.thingIDNumber}", "Debug");
                data = new PawnData(pawn);
                pawnData.Add(pawn.thingIDNumber, data);
            }
            return data;
        }

        public bool HasPawnData(Pawn pawn)
        {
            return pawnData.ContainsKey(pawn.thingIDNumber);
        }

        public void HardReset()
        {
            pawnData.Clear();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref RuntimeUniqueIDsManager, nameof(RuntimeUniqueIDsManager));
            if(Scribe.mode == LoadSaveMode.Saving)
            {
                RemoveInvalidPawnData();
            }
            Scribe_Collections.Look(ref pawnData, nameof(pawnData), LookMode.Value, LookMode.Deep);
            if(Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if(RuntimeUniqueIDsManager == null)
                {
                    RuntimeUniqueIDsManager = new RuntimeUniqueIDsManager();
                }
                if(pawnData == null)
                {
                    pawnData = new Dictionary<int, PawnData>();
                }
            }
        }

        private void RemoveInvalidPawnData()
        {
            if(pawnData.NullOrEmpty())
            {
                return;
            }
            int removedEntries = pawnData.RemoveAll(pawnData => !pawnData.Value.IsValid);
            if(removedEntries > 0 && RV2Log.ShouldLog(false, "Debug"))
                RV2Log.Message($"Removed {removedEntries} invalid PawnData entries", "Debug");
        }

        public void MigrateFromOldPawnData(Dictionary<int, PawnData> oldPawnData)
        {
            foreach(KeyValuePair<int, PawnData> oldPawnDataEntry in oldPawnData)
            {
                pawnData.SetOrAdd(oldPawnDataEntry.Key, oldPawnDataEntry.Value);
            }
            Log.Message($"Migrated {oldPawnData.Count} entries to new PawnData storage");
        }
    }
}
