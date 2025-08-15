using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace RimVore2
{
    public class RollAction_IncreaseGenitalPartSize : RollAction
    {
        string genitalHediffAlias;
        float maxTotalSeverity = 1.5f;
        float minTotalSeverity = 0f;
        const float preyBodySizeToSeverityModifier = 0.1f;
        float additionalMultiplier = 1f;

        public override bool TryAction(VoreTrackerRecord record, float rollStrength)
        {
            base.TryAction(record, rollStrength);
            Pawn predator = record.Predator;
            Pawn prey = record.Prey;
            Hediff genital = BodyPartUtility.GetHediffByAlias(predator, genitalHediffAlias);
            if(genital == null)
            {
                if(RV2Log.ShouldLog(false, "OngoingVore"))
                    RV2Log.Message("Tried to increase genital size, but no genitals found", "OngoingVore");
                return false;
            }
            float severityChange = prey.BodySize * preyBodySizeToSeverityModifier;
            severityChange *= additionalMultiplier;
            float newSeverity = genital.Severity + severityChange;
            newSeverity = newSeverity.LimitClamp(minTotalSeverity, maxTotalSeverity);
            if(RV2Log.ShouldLog(false, "OngoingVore"))
                RV2Log.Message($"Increasing genital size of {genital.def.defName} by {severityChange} new severity: {newSeverity}", "OngoingVore");
            genital.Severity = newSeverity;
            return true;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if(genitalHediffAlias == null)
            {
                yield return "Required field \"genitalHediffAlias\" is not set";
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref genitalHediffAlias, "genitalHediffAlias");
        }
    }
}
