using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimVore2
{
    public abstract class StagePassCondition : IExposable
    {
        public bool decreasing = false;

        protected float CalculateProgress(float curValue, float endValue, float startValue)
        {
            if(startValue == endValue || curValue == endValue)
            {
                return 1;
            }
            float progress;
            // increasing value, take portion of end value
            if(!decreasing)
            {
                progress = curValue / endValue;
                //Log.Message("for increasing, start " + startValue + " end " + endValue + " current: " + curValue + " progress: " + progress);
            }
            // decreasing value, determine total distance and then take current distance in relation
            else
            {
                progress = (startValue - curValue) / (startValue - endValue);
                //Log.Message("for decreasing, start " + startValue + " end " + endValue + ", current: " + curValue + " progress: " + progress);
            }

            return progress.LimitClamp(0, 1);
        }

        public abstract bool IsPassed(VoreTrackerRecord record, out float progress);

        /// <summary>
        /// Used to calculate estimated vore path duration, can return very rough estimates without issues
        /// </summary>
        public abstract float AbstractDuration(StageWorker onCycle, StageWorker onStart);

        public virtual IEnumerable<string> ConfigErrors()
        {
            yield break;
        }
        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref decreasing, "increasing");
        }
    }

    public class StagePassCondition_Never : StagePassCondition
    {
        public override bool IsPassed(VoreTrackerRecord record, out float progress)
        {
            if(RV2Log.ShouldLog(true, "OngoingVore"))
                RV2Log.Message($"{record.LogLabel} - PassCondition_Never is always false", true, "OngoingVore");
            progress = -1;
            return false;
        }

        public override float AbstractDuration(StageWorker onCycle, StageWorker onStart)
        {
            return float.MaxValue;
        }
    }

    public class StagePassCondition_Manual : StagePassCondition
    {
        public override bool IsPassed(VoreTrackerRecord record, out float progress)
        {
            progress = -1;
            return record.IsManuallyPassed;
        }

        public override float AbstractDuration(StageWorker onCycle, StageWorker onStart)
        {
            return float.MaxValue;
        }
    }

    public class StagePassCondition_Timed : StagePassCondition
    {
        int duration;

        public override bool IsPassed(VoreTrackerRecord record, out float progress)
        {
            int adaptedDuration = Math.Max(1, (int)(duration / RV2Mod.Settings.cheats.VoreSpeedMultiplier));    // prevent divide by 0 exception with math.max
            int currentlyPassed = record.CurrentVoreStage.PassedRareTicks;
            progress = currentlyPassed / adaptedDuration;
            progress = CalculateProgress(currentlyPassed, adaptedDuration, 0);
            bool isPassed = record.CurrentVoreStage.PassedRareTicks >= adaptedDuration;
            if(RV2Log.ShouldLog(true, "OngoingVore"))
                RV2Log.Message($"{record.LogLabel} - PassCondition_Timed progress: {progress} ({currentlyPassed}/{duration}({adaptedDuration})), passed ? {isPassed}", true, "OngoingVore");
            return isPassed;
        }

        public override float AbstractDuration(StageWorker onCycle, StageWorker onStart)
        {
            return duration;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if(duration <= 0)
            {
                yield return "required field \"duration\" must be larger than 0";
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref duration, "duration");
        }
    }

    public class StagePassCondition_Warmup : StagePassCondition_Timed
    {
        public List<HediffDef> hediffs = new List<HediffDef>();

        public override bool IsPassed(VoreTrackerRecord record, out float progress)
        {
            if (!RV2Mod.Settings.fineTuning.SkipWarmupWhenAlreadyDigesting)
            {
                return base.IsPassed(record, out progress);
            }

            HediffSet hediffSet = record.Predator.health.hediffSet;
            bool predatorHasWarmedUp = hediffs.Any(h => hediffSet.HasHediff(h));
            if(predatorHasWarmedUp)
            {
                progress = 1;
                return true;
            }

            return base.IsPassed(record, out progress);
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if (hediffs.NullOrEmpty())
            {
                yield return $"Required list {nameof(hediffs)} is not provided";
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref hediffs, "hediffs");
        }
    }

    public abstract class TargetedStagePassCondition : StagePassCondition
    {
        VoreRole target;
        protected Pawn TargetPawn(VoreTrackerRecord record) => record.GetPawnByRole(target);

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if(target == VoreRole.Invalid)
            {
                yield return "required field \"target\" must be set";
            }
        }

        public override bool IsPassed(VoreTrackerRecord record, out float progress)
        {
            progress = -1f;
            // the prey may have died during vore, which means they would be stuck in this stage
            if(TargetPawn(record).Dead)
            {
                return true;
            }
            return false;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref target, "target");
        }
    }

    public class StagePassCondition_Need : TargetedStagePassCondition
    {
        NeedDef need;
        float targetLevel = float.MinValue;

        // TODO I think the whole dictionary approach is unnecessary, each instance of vore creates its own instances of VoreStage, 
        // which in turn instance these StagePassConditions, so logically this part of the code exists exactly once for each vore, 
        // meaning we have no collisions and don't need to find individual keys for each vore
        private Dictionary<float, float> InitialNeedLevels = new Dictionary<float, float>();
        public override bool IsPassed(VoreTrackerRecord record, out float progress)
        {
            if(base.IsPassed(record, out progress))
            {
                return true;
            }
            Pawn pawn = TargetPawn(record);
            Need pawnNeed = pawn.needs.TryGetNeed(need);
            if(pawnNeed == null)
            {
                if(RV2Log.ShouldLog(true, "OngoingVore"))
                    RV2Log.Message($"{record.LogLabel} - PassCondition_Need, but target {pawn?.Label} does not have need {need.defName} passing to prevent being stuck", true, "OngoingVore");
                progress = -1;
                return true;
            }
            float currentLevel = pawnNeed.CurLevel;

            // get a unique identifier to this pass condition in relation to the record
            //   this way we can persist the level of the very first time we are checking this condition, which is used for progress calculations
            float initialNeedKey = record.Predator.GetHashCode() * record.Prey.GetHashCode() * pawn.GetHashCode() / need.GetHashCode();
            if(!InitialNeedLevels.ContainsKey(initialNeedKey))
            {
                if(RV2Log.ShouldLog(false, "OngoingVore"))
                    RV2Log.Message($"No initial need present yet, setting to {currentLevel}", "OngoingVore");
                InitialNeedLevels.Add(initialNeedKey, currentLevel);
            }
            InitialNeedLevels.TryGetValue(initialNeedKey, out float initialLevel);
            progress = CalculateProgress(currentLevel, targetLevel, initialLevel);
            bool isPassed = currentLevel >= targetLevel;
            if(RV2Log.ShouldLog(true, "OngoingVore"))
                RV2Log.Message($"{record.LogLabel} - PassCondition_Need {pawn?.Label} | {need.defName} progress: {progress} ({currentLevel}/{targetLevel}), passed ? {isPassed}", true, "OngoingVore");
            return isPassed;
        }

        /// <summary>
        /// We guess the duration of a Need pass condition by assuming the pawn currently has need level 0.5 (average) and then 
        /// taking all of the roll influences (RollAction_IncreaseNeed & RollIncrease_StealNeed) 's average roll strength per rare tick
        /// </summary>
        public override float AbstractDuration(StageWorker onCycle, StageWorker onStart)
        {
            float influencePerRareTick = 0f;
            foreach(Roll roll in onCycle.Rolls)
            {
                if(roll.actionsOnSuccess.Any(action => Applies(action)))
                {
                    influencePerRareTick += roll.AbstractStrength();
                }
            }
            float assumedStartLevel = decreasing ? 1 : 0f; // decreasing means we go from 1 to 0, otherwise from 0 to whatever target
            float distanceToTargetLevel = Mathf.Abs(assumedStartLevel - targetLevel);
            return distanceToTargetLevel / influencePerRareTick;

            bool Applies(RollAction action)
            {
                return (
                    action is RollAction_IncreaseNeed increaseNeedAction
                        && increaseNeedAction.need == need
                ) || (
                    action is RollAction_StealNeed stealNeedAction
                        && stealNeedAction.need == need
                );
            }
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if(need == null)
            {
                yield return "Required field \"need\" not set";
            }
            if(targetLevel == float.MinValue)
            {
                yield return "Required field \"targetLevel\" not set";
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref need, "need");
            Scribe_Values.Look(ref targetLevel, "targetLevel");
        }
    }
    /// <summary>
    /// TODO
    /// </summary>
    public class StagePassCondition_PassValue : StagePassCondition
    {
        public string passValueName;
        float targetValue = float.MinValue;

        public override bool IsPassed(VoreTrackerRecord record, out float progress)
        {
            if(!record.PassValues.TryGetValue(passValueName, out float currentValue))
            {
                if(RV2Log.ShouldLog(true, "OngoingVore"))
                    RV2Log.Message($"{record.LogLabel} - PassCondition_PassValue, but record does not have PassValue {passValueName} set! Passing to prevent being stuck", true, "OngoingVore");
                progress = -1;
                return true;
            }
            if(!record.InitialPassValues.TryGetValue(passValueName, out float initialValue))
            {
                if(RV2Log.ShouldLog(true, "OngoingVore"))
                    RV2Log.Message($"{record.LogLabel} - PassCondition_PassValue, but record does not have InitialPassValue {passValueName} set! Passing to prevent being stuck", true, "OngoingVore");
                progress = -1;
                return true;
            }
            progress = CalculateProgress(currentValue, targetValue, initialValue);
            bool isPassed = progress == 1;
            if(RV2Log.ShouldLog(true, "OngoingVore"))
                RV2Log.Message($"{record.LogLabel} - PassCondition_PassValue progress: {progress} ({currentValue}/{targetValue}), passed ? {isPassed}", true, "OngoingVore");
            return isPassed;
        }

        public override float AbstractDuration(StageWorker onCycle, StageWorker onStart)
        {
            // initial values for passValue are tough, they might be provided the be roll.actions or in the onStart.rolls
            float initialValue;
            // first check the roll.actions for the set action
            RollAction_PassValue_Set initialSetter = onStart.actions
                .FirstOrDefault(action => action is RollAction_PassValue_Set setAction && setAction.name == passValueName)
                as RollAction_PassValue_Set;
            if(initialSetter != null)
            {
                initialValue = initialSetter.AbstractValue();
            }
            else
            {
                initialValue = onStart.Rolls  // take all the start rolls (one of them has to set the passValue
                    .First(roll => roll.actionsOnSuccess.Any(   // check in the actions
                        action => action is RollAction_PassValue_Set setRollAction  // all the actions that are passValue setters
                        && setRollAction.name == passValueName))    // that are setting our current passValue
                    .AbstractStrength();    // then get the rolls abstract rollStrength
            }

            float influencePerRareTick = 0f;
            foreach(Roll roll in onCycle.Rolls)
            {
                if(roll.actionsOnSuccess.Any(action => Applies(action)))
                {
                    influencePerRareTick += roll.AbstractStrength();
                }
            }

            float distanceToTargetLevel = Mathf.Abs(initialValue - targetValue);
            return distanceToTargetLevel / influencePerRareTick;

            bool Applies(RollAction action)
            {
                return (
                    action is RollAction_PassValue_Add addPassValueAction
                    && addPassValueAction.name == passValueName
                ) || (
                    action is RollAction_PassValue_Subtract subtractPassValueAction
                    && subtractPassValueAction.name == passValueName
                );
            }
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if(passValueName == null)
            {
                yield return "Required field \"passValueName\" not set";
            }
            if(targetValue == float.MinValue)
            {
                yield return "Required field \"targetValue\" not set";
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref passValueName, "passValueName");
            Scribe_Values.Look(ref targetValue, "targetValue");
        }
    }

    public class StagePassCondition_Healed : TargetedStagePassCondition
    {
        float initialTotalInjurySeverity = float.MinValue;

        public override bool IsPassed(VoreTrackerRecord record, out float progress)
        {
            if(base.IsPassed(record, out progress))
            {
                return true;
            }
            List<Hediff> injuries = TargetPawn(record).GetHealableInjuries();
            float totalInjurySeverity = injuries.Sum(injury => injury.Severity);

            if(initialTotalInjurySeverity == float.MinValue)
            {
                initialTotalInjurySeverity = totalInjurySeverity;
            }
            progress = 1 - totalInjurySeverity / initialTotalInjurySeverity;
            return totalInjurySeverity == 0f;
        }

        public override float AbstractDuration(StageWorker onCycle, StageWorker onStart)
        {
            float influencePerRareTick = 0f;
            foreach(Roll roll in onCycle.Rolls)
            {
                if(roll.actionsOnSuccess.Any(action => Applies(action)))
                {
                    influencePerRareTick += roll.AbstractStrength();
                }
            }
            float assumedInitialSeverity = 0.5f;    // truly arbitrary value, severities could be any value
            return assumedInitialSeverity / influencePerRareTick;

            bool Applies(RollAction action)
            {
                return action is RollAction_Heal healAction;
            }
        }
    }
}
