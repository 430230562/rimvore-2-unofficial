using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class Recipe_RemovePrey : Recipe_Surgery
    {
        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            if(!pawn.IsActivePredator())
            {
                yield break;
            }
            List<BodyPartRecord> bodyParts = new List<BodyPartRecord>();
            VoreTracker tracker = pawn.PawnData()?.VoreTracker;
            foreach(VoreTrackerRecord record in tracker.VoreTrackerRecords)
            {
                bodyParts.AddDistinct(record.CurrentBodyPart);
            }
            foreach(BodyPartRecord bodyPart in bodyParts)
            {
                yield return bodyPart;
            }
        }

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if(!pawn.IsActivePredator())
            {
                return;
            }
            VoreTracker tracker = pawn.PawnData()?.VoreTracker;
            List<VoreTrackerRecord> applicableRecords = tracker.VoreTrackerRecords
                .FindAll(record => record.CurrentBodyPart == part);
            foreach(VoreTrackerRecord record in applicableRecords)
            {
                tracker.Eject(record, billDoer, true);
            }
        }

        public override string GetLabelWhenUsedOn(Pawn pawn, BodyPartRecord part)
        {
            return "RV2_SurgeryRemovePrey".Translate(part.LabelShortCap);
        }
    }
}
