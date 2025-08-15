using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimRound;
using RimVore2;
using RimRound.Comps;
using RimRound.Patch;
using RimWorld;

namespace RV2_RimRound
{
    public class RollAction_IncreaseFullness : RollAction
    {
        public override bool TryAction(VoreTrackerRecord record, float rollStrength)
        {
            if(!base.TryAction(record, rollStrength))
                return false;

            FullnessAndDietStats_ThingComp fullnessComp = TargetPawn.TryGetComp<FullnessAndDietStats_ThingComp>();
            if(fullnessComp == null)
            {
                FullnessFallback(record, rollStrength);
                return false;
            }

            Thing_Ingested_HarmonyPatch.Postfix(OtherPawn, TargetPawn, ref rollStrength);
            if(RV2Log.ShouldLog(true, "RimRound"))
                RV2Log.Message($"Increased {TargetPawn.LabelShort} fullness by {rollStrength} for a new total of {fullnessComp.CurrentFullness}", false, "RimRound");
            return true;
        }

        private void FullnessFallback(VoreTrackerRecord record, float rollStrength)
        {
            if(RV2Log.ShouldLog(true, "RimRound"))
                RV2Log.Message($"Pawn {TargetPawn.LabelShort} has no fullness comp, falling back to normal food need increase", false, "RimRound");
            Need_Food foodNeed = TargetPawn.needs?.TryGetNeed<Need_Food>();
            if(foodNeed == null)
            {
                if(RV2Log.ShouldLog(true, "RimRound"))
                    RV2Log.Message($"Pawn {TargetPawn.LabelShort} has no food need either, doing nothing", false, "RimRound");
                return;
            }
            foodNeed.CurLevel += rollStrength;
        }
    }
}
