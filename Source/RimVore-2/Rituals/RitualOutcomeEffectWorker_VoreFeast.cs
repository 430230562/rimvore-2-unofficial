using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;


namespace RimVore2
{
    /// <summary>
    /// This one basically takes all the participants and then has a low chance of applying an inspiration to them
    /// </summary>
    public class RitualOutcomeEffectWorker_VoreFeast : RitualOutcomeEffectWorker_Consumable
    {

        public RitualOutcomeEffectWorker_VoreFeast()
        {
        }

        public RitualOutcomeEffectWorker_VoreFeast(RitualOutcomeEffectDef def) : base(def)
        {
        }

#if v1_4
        protected override void ApplyExtraOutcome(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, OutcomeChance outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
#else
        protected override void ApplyExtraOutcome(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, RitualOutcomePossibility outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
#endif
        {
            extraOutcomeDesc = String.Empty;
            // if ritual went badly, never apply extra
            if(!outcome.Positive)
            {
                return;
            }

            float inspirationChance = outcome.BestPositiveOutcome(jobRitual) ? 0.1f : 0.05f;

            foreach(KeyValuePair<Pawn, int> pawnPresence in totalPresence)
            {
                Pawn pawn = pawnPresence.Key;
                // additional chance to apply inspiration (base game sets this at 50%)
                if(Rand.Chance(inspirationChance))
                {
                    RV2RitualUtility.InspirePawn(pawn, jobRitual, out string inspireDesc);
                    if(inspireDesc != null)
                    {
                        extraOutcomeDesc += " " + inspireDesc;
                    }
                }
            }
        }
    }
}