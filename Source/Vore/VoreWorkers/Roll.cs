using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using UnityEngine;
using System.Linq;

namespace RimVore2
{
    public class Roll : IExposable, IRollable
    {
        public Roll() { }

        public Roll(RollPresetDef def)
        {
            this.presetDef = def;
            this.interval = def.interval;
            this.successChance = def.successChance;
            this.chanceModifiers = new List<RollModifier>(def.chanceModifiers);
            this.strength = def.strength;
            this.strengthModifiers = new List<RollModifier>(def.strengthModifiers);
            this.actionsOnSuccess = new List<RollAction>(def.actionsOnSuccess);
            this.actionsOnFailure = new List<RollAction>(def.actionsOnFailure);
        }

        public RollPresetDef presetDef;    // does not need to be set!

        public int interval = 1;
        public float successChance = 1f;
        public List<RollModifier> chanceModifiers = new List<RollModifier>();
        FloatRange strength = new FloatRange(0, 0);
        public List<RollModifier> strengthModifiers = new List<RollModifier>();

        public List<RollAction> actionsOnSuccess = new List<RollAction>();
        public List<RollAction> actionsOnFailure = new List<RollAction>();

        public float GetRollStrength(VoreTrackerRecord record)
        {
            float rollStrength = strength.RandomInRange;
            foreach(RollModifier modifier in strengthModifiers)
            {
                rollStrength = modifier.ModifyValue(rollStrength, record);
            }
            return rollStrength;
        }

        /// <summary>
        /// Provides an average of this rolls strength, used to calculate the estimated time a VorePath takes
        /// </summary>
        public float AbstractStrength()
        {
            float rollStrength = strength.Average;
            foreach(RollModifier modifier in strengthModifiers)
            {
                rollStrength = modifier.AbstractModifyValue(rollStrength);
            }
            // if the roll only doesn't happen every roll, reduce the strength of the roll to average out on a per-tick basis
            float rollChanceModifier = successChance;
            foreach(RollModifier modifier in chanceModifiers)
            {
                rollChanceModifier = modifier.AbstractModifyValue(rollChanceModifier);
            }
            rollChanceModifier = Mathf.Clamp(rollChanceModifier, 0f, 1f);
            return rollStrength * rollChanceModifier;
        }

        public float GetRollChance(VoreTrackerRecord record)
        {
            float chance = successChance;
            foreach(RollModifier modifier in chanceModifiers)
            {
                chance = modifier.ModifyValue(chance, record);
            }
            return Mathf.Clamp(chance, 0f, 1f);
        }

        public bool IntervalValid(VoreTrackerRecord record)
        {
            return record.CurrentVoreStage.PassedRareTicks % interval == 0;
        }

        public virtual void Work(VoreTrackerRecord record)
        {
            if(!IntervalValid(record))
            {
                return;
            }
            float rollStrength = GetRollStrength(record);
            float rollSuccessChance = GetRollChance(record);
            if(Rand.Chance(rollSuccessChance))
            {
                foreach(RollAction action in actionsOnSuccess)
                {
                    bool success = action.TryAction(record, rollStrength);
                    if(!success && action.CanBlockNextActions)
                    {
                        if(RV2Log.ShouldLog(false, "OngoingVore"))
                            RV2Log.Message("Further actions / rolls in RollInstruction have been blocked due to an action returning false", "OngoingVore");
                        return;
                    }
                }
            }
            else
            {
                foreach(RollAction action in actionsOnFailure)
                {
                    bool success = action.TryAction(record, rollStrength);
                    if(!success && action.CanBlockNextActions)
                    {
                        if(RV2Log.ShouldLog(false, "OngoingVore"))
                            RV2Log.Message("Further actions / rolls in RollInstruction have been blocked due to an action returning false", "OngoingVore");
                        return;
                    }
                }
            }
        }

        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref strength, "strength");
            Scribe_Collections.Look(ref strengthModifiers, "strengthModifiers", LookMode.Deep);
            Scribe_Values.Look(ref successChance, "successChance");
            Scribe_Collections.Look(ref chanceModifiers, "chanceModifiers", LookMode.Deep);
            Scribe_Values.Look(ref interval, "interval");
            Scribe_Collections.Look(ref actionsOnSuccess, "actionsOnSuccess", LookMode.Deep);
            Scribe_Collections.Look(ref actionsOnFailure, "actionsOnFailure", LookMode.Deep);
        }

        public virtual IEnumerable<string> ConfigErrors()
        {
            foreach(RollModifier modifier in strengthModifiers)
            {
                foreach(string error in modifier.ConfigErrors())
                {
                    yield return error;
                }
            }
            foreach(RollModifier modifier in chanceModifiers)
            {
                foreach(string error in modifier.ConfigErrors())
                {
                    yield return error;
                }
            }
            foreach(RollAction action in actionsOnSuccess)
            {
                foreach(string error in action.ConfigErrors())
                {
                    yield return error;
                }
            }
            foreach(RollAction action in actionsOnFailure)
            {
                foreach(string error in action.ConfigErrors())
                {
                    yield return error;
                }
            }
        }

        public override string ToString()
        {
            try
            {
                string returnValue = base.ToString();
                if(presetDef != null)
                    returnValue += $"\nPresetDef: {presetDef.defName}";
                returnValue += $"\nInterval: {interval}, successChance: {successChance}, strength: {strength}";
                Func<List<RollAction>, string> actionsToString = (List<RollAction> actions) =>
                {
                    if(actions.NullOrEmpty())
                        return "NONE";
                    return "\n\t" + string.Join("\n\t", actions.Select(action => action.ToString()));
                };
                returnValue += $"\nactionsOnSuccess: {actionsToString(actionsOnSuccess)}";
                returnValue += $"\nactionsOnFailure: {actionsToString(actionsOnFailure)}";
                return returnValue;
            }
            catch(Exception e)
            {
                RV2Log.Error("Exception when trying to calculate ToString for " + base.ToString() + ": " + e);
                return base.ToString();
            }
        }
    }
}
