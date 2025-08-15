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
    /// Special case where we need IDs available at settings-time, before the user has loaded a game
    /// </summary>
    public class SettingsUniqueIDsManager : IExposable
    {
        private int nextRuleTargetId;

        public int GetNextRuleTargetID()
        {
            return GetNextID(ref nextRuleTargetId);
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
                Scribe_Values.Look(ref nextRuleTargetId, "nextRuleTargetId");
            }
            catch(Exception e)
            {
                Log.Warning("RimVore-2: Something went wrong: " + e);
                return;
            }
        }
    }
}