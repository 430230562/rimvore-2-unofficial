using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;


namespace RimVore2
{
    public class RitualOutcomeEffectWorker_VoreBonding : RitualOutcomeEffectWorker_Consumable
    {
        public RitualOutcomeEffectWorker_VoreBonding() { }

        public RitualOutcomeEffectWorker_VoreBonding(RitualOutcomeEffectDef def) : base(def) { }

        protected override void ApplyExtraOutcome(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, RitualOutcomePossibility outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
        {
            extraOutcomeDesc = null;
            // if ritual went badly, never apply extra
            if(!outcome.Positive)
            {
                return;
            }
            // with how base game sets up the constants the best outcome will guarantee application, otherwise only 50% - I am using the constants here in case base game ever updates
            if(Rand.Chance(outcome.BestPositiveOutcome(jobRitual) ? 1 - InspirationGainChanceBestOutcome : 1 - InspirationGainChanceBestOutcome))
            {
                return;
            }

            RV2RitualUtility.IncreaseIdeologyBelief(jobRitual, out extraOutcomeDesc);

            // additional chance to apply inspiration (base game sets this at 50%)
            if(Rand.Chance(InspirationGainChance))
            {
                Pawn pawn = (from p in totalPresence.Keys
                             where !p.Inspired && DefDatabase<InspirationDef>.AllDefsListForReading.Any((InspirationDef i) => i.Worker.InspirationCanOccur(p))
                             select p).RandomElementWithFallback(null);
                RV2RitualUtility.InspirePawn(pawn, jobRitual, out string inspireDesc);
                if(inspireDesc != null)
                {
                    extraOutcomeDesc += " " + inspireDesc;
                }
            }
        }

    }
}