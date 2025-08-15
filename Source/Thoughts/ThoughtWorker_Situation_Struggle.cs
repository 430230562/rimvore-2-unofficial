using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RimVore2
{
    public abstract class ThoughtWorker_RequiredQuirk : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if(!QuirkThoughtUtility.RequiresQuirk(base.def))
            {
                RV2Log.Warning("Trying to use ThoughtWorker_RequiredQuirk for a quirk that does not have a quirk hooked up to it! This thought will never be active! " + base.def.defName, true, "Thoughts");
                return false;
            }
            QuirkManager quirks = p.QuirkManager();
            if(quirks == null) // no quirks means the pawn can not have the required enabler for this thought
                return false;
            if(!quirks.EnablesSituationalThought(base.def))
                return false;

            return true;
        }
    }

    public class ThoughtWorker_Situation_Struggle : ThoughtWorker_RequiredQuirk
    {
        protected override ThoughtState CurrentStateInternal(Pawn predator)
        {
            ThoughtState baseState = base.CurrentStateInternal(predator);
            if(!baseState.Active)
            {
                return baseState;
            }

            bool isTrackingVore = GlobalVoreTrackerUtility.IsActivePredator(predator);
            if(!isTrackingVore)
            {
                return false;
            }

            int strugglingPrey = predator.PawnData().VoreTracker.PreyStrugglingCount;
            if(strugglingPrey == 0)
            {
                return ThoughtState.Inactive;
            }
            int stagesCount = this.def.stages.Count - 1;
            // if we have more prey than we have stages, just set the last one, otherwise set the amount of prey as the current stage
            // counted prey are 1 indexed, so substract 1
            int stageToSet = Math.Min(strugglingPrey - 1, stagesCount);
            return ThoughtState.ActiveAtStage(stageToSet);
        }
    }
}