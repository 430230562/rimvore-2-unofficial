using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimVore2
{
    /// <summary>
    /// Dumbed down copy of UniqueIDsManager from base game because it is impossible to extend
    /// </summary>
    public class RuntimeUniqueIDsManager : IExposable
    {
        private int nextVoreTrackerRecordId;

        public RuntimeUniqueIDsManager(int nextVoreTrackerRecordId)
        {
            this.nextVoreTrackerRecordId = nextVoreTrackerRecordId;
        }

        public RuntimeUniqueIDsManager() { }

        public int GetNextVoreTrackerRecordID()
        {
            return GetNextID(ref nextVoreTrackerRecordId);
        }

        private int GetNextID(ref int nextID)
        {
            if(Scribe.mode == LoadSaveMode.LoadingVars)
            {
                Log.Warning("Getting next unique ID during LoadingVars before UniqueIDsManager was loaded. Assigning a random value.");
                return Rand.Int;
            }
            if(Scribe.mode == LoadSaveMode.Saving)
            {
                Log.Warning("Getting next unique ID during saving This may cause bugs.");
            }
            int result = nextID;
            nextID++;
            if(nextID == 2147483647)
            {
                Log.Warning("Next ID is at max value. Resetting to 0. This may cause bugs.");
                nextID = 0;
            }
            return result;
        }

        public void ExposeData()
        {
            try
            {
                Scribe_Values.Look(ref nextVoreTrackerRecordId, "nextVoreTrackerRecordId");
            }
            catch(Exception e)
            {
                Log.Warning("RimVore-2: Something went wrong: " + e);
                return;
            }
        }
    }
}