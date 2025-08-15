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
    public class StatPart_VoredWeight : StatPart
    {
        public override string ExplanationPart(StatRequest req)
        {
            VoreTracker tracker = TryGetPawn(req)?.PawnData(false)?.VoreTracker;
            if(tracker == null || tracker.VoreTrackerRecords.NullOrEmpty())
            {
                return null;
            }
            float totalMass = TotalVoredWeight(tracker);
            string explanation = $"{"RV2_StatsReport_VoredPrey".Translate()}: {totalMass.ToStringMassOffset()}";
            foreach(VoreTrackerRecord record in tracker.VoreTrackerRecords)
            {
                string subExplanation = $"    {record.Prey.LabelShortCap}: {record.VoreContainer.TotalMass.ToStringMassOffset()}";
                explanation += $"\n{subExplanation}";
            }
            return explanation;
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            VoreTracker tracker = TryGetPawn(req)?.PawnData(false)?.VoreTracker;
            if(tracker == null)
            {
                return;
            }
            val += TotalVoredWeight(tracker);
        }

        private float TotalVoredWeight(VoreTracker tracker)
        {
            float totalWeight = tracker.VoreTrackerRecords.Sum(record => record.VoreContainer.TotalMass);
            return totalWeight;
        }

        private Pawn TryGetPawn(StatRequest req)
        {
            if(req.Pawn != null)
            {
                return req.Pawn;
            }
            if(req.Thing is Pawn pawn)
            {
                return pawn;
            }
            return null;
        }
    }
}
