using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace RimVore2
{
    public class MentalStateWorker_VoreTargetsAvailable : MentalStateWorker
    {
        public VoreMentalStateDef VoreMentalStateDef => (VoreMentalStateDef)base.def;

        public override bool StateCanOccur(Pawn pawn)
        {
            if(RV2Log.ShouldLog(true, "MentalStates"))
                RV2Log.Message($"Pawn {pawn.LabelShort} is trying to use mentalState {this.def}", "MentalStates");
            int targetCount = VoreMentalStateDef.targetCountToVore;
            if(!base.StateCanOccur(pawn))
            {
                if(RV2Log.ShouldLog(true, "MentalStates"))
                    RV2Log.Message("base state already returned false", "MentalStates");
                return false;
            }
            //Log.Message("mental state " + def.defName + " retrieving pawns");
            MentalState_VoreTargeter mentalState = (MentalState_VoreTargeter)Activator.CreateInstance(def.stateClass);
            mentalState.pawn = pawn;
            mentalState.def = base.def;

            IEnumerable<Pawn> matchingPawns = TargetUtility.GetVorablePawns(pawn, mentalState.Request, targetCount);
            if(matchingPawns.EnumerableNullOrEmpty())
            {
                if(RV2Log.ShouldLog(false, "MentalStates"))
                    RV2Log.Message("No pawns matched mental state criteria", "MentalStates");
                return false;
            }
            //Log.Message("pawns available for " + def.defName + ": " + matchingPawns.Count() + " / " + targetCount);
            if(matchingPawns.Count() < targetCount)
            {
                if(RV2Log.ShouldLog(false, "MentalStates"))
                    RV2Log.Message($"Pawn {pawn.LabelShort} does not have enough targets for {this.def} - {matchingPawns.Count()} / {targetCount}", true, "MentalStates");
                return false;
            }
            float minimumRequiredCapacity = matchingPawns
                .Select(p => p.BodySize)
                .OrderBy(p => p)
                .Take(targetCount)
                .Sum();
            float pawnCapacity = pawn.CalculateVoreCapacity();
            if(pawnCapacity < minimumRequiredCapacity)
            {
                if(RV2Log.ShouldLog(false, "MentalStates"))
                    RV2Log.Message($"Pawn {pawn.LabelShort} does not have enough vore capacity for {this.def} - {pawnCapacity} / {minimumRequiredCapacity}", true, "MentalStates");
                return false;
            }
            return true;
        }
    }
}
