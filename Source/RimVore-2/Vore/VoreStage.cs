using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RimVore2
{
    public class VoreStage : IExposable
    {
        public VoreStageDef def;

        public float PercentageProgress;
        public int PassedRareTicks = 0;
        public StageWorker OnStart = null;
        public StageWorker OnCycle = null;
        public StageWorker OnEnd = null;

        public VoreStage() { }

        public VoreStage(VoreStageDef def)
        {
            this.def = def;
            OnStart = new StageWorker(def.onStart);
            OnCycle = new StageWorker(def.onCycle);
            OnEnd = new StageWorker(def.onEnd);
        }

        public void Start(VoreTrackerRecord record)
        {
            if(RV2Log.ShouldLog(true, "OngoingVore"))
                RV2Log.Message("Stage Start()", false, "OngoingVore");
            OnStart.Work(record);
        }

        public void Cycle(VoreTrackerRecord record)
        {
            if(RV2Log.ShouldLog(true, "OngoingVore"))
                RV2Log.Message("Stage Cycle()", false, "OngoingVore");
            OnCycle.Work(record);
        }

        public void End(VoreTrackerRecord record)
        {
            if(RV2Log.ShouldLog(true, "OngoingVore"))
                RV2Log.Message("Stage End()", false, "OngoingVore");
            OnEnd.Work(record);
        }

        public bool PassConditionsFulfilled(VoreTrackerRecord record)
        {
            List<StagePassCondition> passConditions = record.CurrentVoreStage.def.passConditions;
            List<float> progressList = new List<float>();
            bool areAllPassed = true;
            foreach(StagePassCondition condition in passConditions)
            {
                bool isConditionPassed = condition.IsPassed(record, out float conditionProgress);
                areAllPassed &= isConditionPassed;
                progressList.Add(conditionProgress);
            }
            progressList = progressList.FindAll(progress => progress >= 0);
            if(progressList.Count > 0)
            {
                // SHOULD NO LONGER BE THE CASE -> // some progresses can run above 100%, clamp those down to 1 for the average calculation
                //PercentageProgress = progressList.ConvertAll(progress => Math.Min(progress, 1)).Average();
                PercentageProgress = progressList.Average();
            }
            // none of the conditions have progress, set overall progress to not show the label
            else
            {
                PercentageProgress = -1;
            }
            return areAllPassed;
            // previous logic
            // return record.CurrentVorePart.def.passConditions.All(condition => condition.IsPassed(record, PercentageProgress)); 
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_Values.Look(ref PercentageProgress, "PercentageProgress", -1f);
            Scribe_Values.Look(ref PassedRareTicks, "PassedRareTicks");
            Scribe_Deep.Look(ref OnStart, "onStart", new object[0]);
            Scribe_Deep.Look(ref OnCycle, "onCycle", new object[0]);
            Scribe_Deep.Look(ref OnEnd, "onEnd", new object[0]);
        }
    }
}
