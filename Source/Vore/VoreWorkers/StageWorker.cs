using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class StageWorker : IExposable
    {
        public List<RollAction> actions = new List<RollAction>();
        List<Roll> rolls = new List<Roll>();
        List<RollPresetDef> rollPresets = new List<RollPresetDef>();
        List<Roll> allRolls = null;
        public List<Roll> Rolls
        {
            get
            {
                if(allRolls == null)
                {
                    allRolls = new List<Roll>();
                    allRolls.AddRange(new List<Roll>(rolls));
                    allRolls.AddRange(rollPresets.Select(preset => new Roll(preset)));
                }
                return allRolls;
            }
        }

        public StageWorker() { }

        public StageWorker(StageWorker original)
        {
            if(original == null)
            {
                return;
            }
            actions = new List<RollAction>(original.actions);
            rolls = new List<Roll>(original.rolls);
            rollPresets = new List<RollPresetDef>(original.rollPresets);
        }

        public void Work(VoreTrackerRecord record)
        {
            foreach(RollAction action in actions)
            {
                try
                {
                    action.TryAction(record, 1);
                }
                catch(Exception e)
                {
                    RV2Log.Error("Uncaught exception in roll, cancelling vore " + action.ToString() + " --- Error: " + e);
                    record.Predator?.PawnData()?.VoreTracker?.EmergencyEject(record);
                }
            }
            foreach(Roll roll in Rolls)
            {
                try
                {
                    roll.Work(record);
                }
                catch(Exception e)
                {
                    RV2Log.Error("Uncaught exception in roll, cancelling vore " + roll.ToString() + " --- Error: " + e);
                    record.Predator?.PawnData()?.VoreTracker?.EmergencyEject(record);
                }
            }
        }

        public IEnumerable<string> ConfigErrors()
        {
            foreach(RollAction action in actions)
            {
                foreach(string error in action.ConfigErrors())
                {
                    yield return error;
                }
            }
            foreach(Roll roll in rolls)
            {
                foreach(string error in roll.ConfigErrors())
                {
                    yield return error;
                }
            }
            foreach(RollPresetDef rollPreset in rollPresets)
            {
                foreach(string error in rollPreset.ConfigErrors())
                {
                    yield return error;
                }
            }
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref actions, "actions", LookMode.Deep, new object[0]);
            Scribe_Collections.Look(ref rolls, "rolls", LookMode.Deep, new object[0]);
            Scribe_Collections.Look(ref rollPresets, "rollPresets", LookMode.Def);
        }
    }
}
