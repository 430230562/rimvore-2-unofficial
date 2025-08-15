using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace RimVore2
{
    public class RollAction_GainPsyfocus : RollAction
    {
        public override bool TryAction(VoreTrackerRecord record, float rollStrength)
        {
            base.TryAction(record, rollStrength);

            if(!TargetPawn.HasPsylink)
            {
                return false;
            }
            TargetPawn.psychicEntropy.OffsetPsyfocusDirectly(rollStrength);

            if(RV2Log.ShouldLog(false, "OngoingVore"))
                RV2Log.Message($"Increasing psyfocus of {TargetPawn.LabelShort} by {rollStrength} new psyfocus: {TargetPawn.psychicEntropy.CurrentPsyfocus}", "OngoingVore");
            return true;
        }
    }
}
