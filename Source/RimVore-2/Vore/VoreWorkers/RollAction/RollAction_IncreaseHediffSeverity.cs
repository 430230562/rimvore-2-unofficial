using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace RimVore2
{
    public class RollAction_IncreaseHediffSeverity : RollAction_AddHediff
    {
        bool addHediffIfNotPresent = false;

        public override bool TryAction(VoreTrackerRecord record, float rollStrength)
        {
            this.record = record;
            if(invert)
            {
                rollStrength *= -1;
            }

            HediffDef hediffDefToUse = hediff;
            QuirkManager quirkManager = TargetPawn.QuirkManager(false);
            // if the pawn has quirks, there may be replacements for the hediff, we want to affect that hediff in this circumstance
            if(quirkManager != null)
            {
                if(quirkManager.TryGetOverriddenHediff(hediff, out HediffDef potentialOverrideHediff))
                {
                    if(potentialOverrideHediff == null)
                    {
                        if(RV2Log.ShouldLog(false, "OngoingVore"))
                            RV2Log.Message($"{TargetPawn.LabelShort} has override with NULL for hediff {hediff.defName}, not increasing severity", "OngoingVore");
                        return false;
                    }

                    if(RV2Log.ShouldLog(false, "OngoingVore"))
                        RV2Log.Message($"Found override hediff {potentialOverrideHediff.defName} for {TargetPawn.LabelShort}'s {hediff.defName}, increasing its severity instead of original", "OngoingVore");
                    hediffDefToUse = potentialOverrideHediff;
                }
            }

            if(!TargetPawn.health.hediffSet.HasHediff(hediffDefToUse))
            {
                if(addHediffIfNotPresent)
                {
                    if(RV2Log.ShouldLog(false, "OngoingVore"))
                        RV2Log.Message($"Adding non-present hediff {hediffDefToUse} to {TargetPawn.LabelShort}", "OngoingVore");
                    // if the hediff add didn't work, don't try to increase severity
                    if(!base.TryAction(record, rollStrength))
                    {
                        if(RV2Log.ShouldLog(false, "OngoingVore"))
                            RV2Log.Message("Failed to add hediff, cancelling severity increase", "OngoingVore");
                        return false;
                    }
                }
                else
                    return false;
            }
            Hediff targetHediff = base.TargetPawn.health.hediffSet.hediffs
                .FindAll(hed => hed.def == hediffDefToUse)
                .RandomElement();
            if(targetHediff == null)
            {
                return false;
            }
            targetHediff.Severity += rollStrength;
            if(RV2Log.ShouldLog(false, "OngoingVore"))
                RV2Log.Message($"Increased {TargetPawn.Label}'s {targetHediff.def.label} by {rollStrength} to {targetHediff.Severity}", "OngoingVore");
            return true;
        }
    }
}
