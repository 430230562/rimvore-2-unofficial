using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace RimVore2
{
    public class RollAction_StealNeed : RollAction_Steal
    {
        public NeedDef need;

        public override bool TryAction(VoreTrackerRecord record, float rollStrength)
        {
            base.TryAction(record, rollStrength);
            if(TakerPawn.Dead)
            {
                return false;
            }
            if(invert)
            {
                rollStrength *= -1;
            }
            if(rollStrength < 0)
            {
                RV2Log.Warning("rollStrength is negative for stealing need. This may lead to undesired effects", "OngoingVore");
            }
            bool takerCanTakeNeed = TakerPawn.needs?.TryGetNeed(need) != null    // taker has need
                && TakerPawn.needs.TryGetNeed(need).CurLevel + rollStrength < 1;    // and taker has capacity in the need to steal from giver
            if(!takerCanTakeNeed)
            {
                if(RV2Log.ShouldLog(true, "OngoingVore"))
                    RV2Log.Message($"No need to steal need {need.defName} - {TakerPawn.LabelShort} has no capacity for {rollStrength}", false, "OngoingVore");
                return false;
            }

            /// If we are stealing food and the setting to prevent draining food is on, don't drain predator need
            /// otherwise, try to decrease the need ot the giver
            /// if it failed, but we are allowed to "steal" from nothing, we are free to give to the taker
            bool canGiveToTaker = need == NeedDefOf.Food && RV2Mod.Settings.cheats.PreventDrainingPredatorFood;
            if(!canGiveToTaker)
            {
                canGiveToTaker = RV2PawnUtility.TryIncreaseNeed(GiverPawn, need, -rollStrength);
                canGiveToTaker = canGiveToTaker || RV2Mod.Settings.cheats.CanStealFromNullNeed;
            }
            
            if(canGiveToTaker)
            {
                return RV2PawnUtility.TryIncreaseNeed(TakerPawn, need, rollStrength);
            }
            return false;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if(need == null)
            {
                yield return "required field \"need\" not set";
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref need, "need");
        }
    }
}
