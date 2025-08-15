using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RimVore2
{
    public class ThoughtWorker_Situation_Vore : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn predator)
        {
            bool isTrackingVore = GlobalVoreTrackerUtility.IsActivePredator(predator);
            if(!isTrackingVore)
            {
                return false;
            }

            List<VoreTrackerRecord> records = predator.PawnData()?.VoreTracker?.VoreTrackerRecords;
            if(records == null)
            {
                return ThoughtState.Inactive;
            }
            // ORIGINAL VERSION: trackedVores = trackedVores.FindAll(record => record.CurrentVoreStage.def.predatorThoughtDef == this.def);
            // reduce tracked vores to only the ones that are currently applying THIS thought
            records = records.FindAll(record => record.CurrentVoreStage.def.CurrentThought(predator) == this.def);
            if(records == null)
            {
                return ThoughtState.Inactive;
            }
            int numberOfPrey = records.Count;
            if(numberOfPrey == 0)
            {
                return ThoughtState.Inactive;
            }
            int stagesCount = this.def.stages.Count - 1;
            // if we have more prey than we have stages, just set the last one, otherwise set the amount of prey as the current stage
            // counted prey are 1 indexed, so substract 1
            int stageToSet = Math.Min(numberOfPrey - 1, stagesCount);
            return ThoughtState.ActiveAtStage(stageToSet);
        }
    }
}