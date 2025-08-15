using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public static class RV2RitualUtility
    {
        static FloatRange certaintyInfluence = new FloatRange(0.3f, 0.8f);
        public static void IncreaseIdeologyBelief(LordJob_Ritual jobRitual, out string extraOutcomeDesc)
        {
            extraOutcomeDesc = null;
            Ideo ritualIdeo = jobRitual.Ritual.ideo;
            Pawn target = jobRitual.assignments.FirstAssignedPawn("target");
            if(target == null)
            {
                RV2Log.Warning("No pawn to increase ideology for - no pawn with \"target\" id found");
                return;
            }
            float influence = certaintyInfluence.RandomInRange;
            // if pawn belongs to ideo, reassure their belief
            if(target.Ideo == ritualIdeo)
            {
                target.ideo.Reassure(influence);
                extraOutcomeDesc = "RV2_OutcomeMessage_IdeologyReassured".Translate(target.LabelShortCap, ritualIdeo.name);
            }
            // otherwise attempt to convert
            else
            {
                target.ideo.IdeoConversionAttempt(influence, ritualIdeo);
                extraOutcomeDesc = "RV2_OutcomeMessage_IdeologyConversionAttempt".Translate(target.LabelShortCap, ritualIdeo.name);
            }
        }

        public static void InspirePawn(Pawn pawn, LordJob_Ritual jobRitual, out string extraOutcomeDesc)
        {
            extraOutcomeDesc = null;
            if(pawn == null)
            {
                return;
            }
            InspirationDef inspirationDef = (from i in DefDatabase<InspirationDef>.AllDefsListForReading
                                             where i.Worker.InspirationCanOccur(pawn)
                                             select i).RandomElementWithFallback(null);
            if(inspirationDef == null)
            {
                Log.Error("Could not find inspiration for pawn " + pawn.Name.ToStringFull);
                return;
            }
            if(!pawn.mindState.inspirationHandler.TryStartInspiration(inspirationDef, null, false))
            {
                Log.Error("Inspiring " + pawn.Name.ToStringFull + " failed, but the inspiration worker claimed it can occur!");
                return;
            }
            extraOutcomeDesc = "RitualOutcomeExtraDesc_ConsumableInspiration".Translate(pawn.Named("PAWN"), inspirationDef.LabelCap.Named("INSPIRATION"), jobRitual.Ritual.Label.Named("RITUAL")).CapitalizeFirst() + " " + pawn.Inspiration.LetterText;
        }

        public static bool AllPawnsPartying(LordJob_Ritual ritual)
        {
            if(ritual.assignments == null || ritual.assignments.Participants.NullOrEmpty())
            {
                RV2Log.Warning("No ritual participants could be extracted from ritual");
                return true;
            }
            bool retVal = ritual.assignments.Participants
                .Where(pawn => pawn.mindState?.duty != null)    // all pawns that can take duty
                .All(pawn => pawn.mindState.duty.def == RV2_Common.PartyDutyDef);   // are on the *parteeey* duty

            if(RV2Log.ShouldLog(false, "Rituals"))
                RV2Log.Message($"All pawns partying ? {retVal}\nPawn duties: {String.Join("\n", ritual.assignments.Participants.Select(p => p.LabelShort + ":" + p.mindState?.duty?.def?.defName))}", "Rituals");
            return retVal;
        }

        public static void RemoveVoreFeastDuty(Pawn pawn)
        {
            if(pawn.mindState.duty == null)
            {
                return;
            }
            if(pawn.mindState.duty.def == RV2_Common.VoreFeastDuty)
            {
                pawn.mindState.duty = new Verse.AI.PawnDuty(RV2_Common.PartyDutyDef);
            }
        }

        public static LordJob_Ritual ParticipatingRitual(Pawn pawn)
        {
            if(pawn.Map == null)
            {
                return null;
            }
            return pawn.Map.lordManager.lords   // get all lords on the map
                .Select(lord => lord.LordJob)   // retrieve each lords job
                .Where(lord => lord is LordJob_Ritual)  // check for rituals
                .Cast<LordJob_Ritual>() // cast to ritual
                .FirstOrDefault(ritual => ritual.assignments?.Participants?.Contains(pawn) == true);	// find the ritual that contains the pawn as a participant
        }
    }
}