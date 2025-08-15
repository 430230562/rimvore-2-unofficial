using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using RimWorld;
using Verse.AI.Group;

namespace RimVore2
{
    public class JobGiver_RequestVore_VoreFeast : JobGiver_RequestVore
    {
        private static Dictionary<Pawn, int> attemptedVoreCounts = new Dictionary<Pawn, int>();
        private int GetVoreAttemptCount(Pawn pawn)
        {
            if(!attemptedVoreCounts.ContainsKey(pawn))
            {
                attemptedVoreCounts.Add(pawn, 0);
            }
            return attemptedVoreCounts[pawn];
        }
        private void IncreaseVoreAttemptCount(Pawn pawn)
        {
            if(!attemptedVoreCounts.ContainsKey(pawn))
            {
                attemptedVoreCounts.SetOrAdd(pawn, 1);
            }
            else
            {
                attemptedVoreCounts[pawn]++;
            }
        }
        public static void ClearAttemptedVoreCounts(LordJob_Ritual ritual)
        {
            List<Pawn> participants = ritual.assignments.Participants;
            foreach(Pawn pawn in participants)
            {
                attemptedVoreCounts.Remove(pawn);
            }
            if(RV2Log.ShouldLog(false, "Rituals"))
                RV2Log.Message($"Cleared vore attempt counts for: {String.Join(", ", participants.Select(p => p.LabelShort))}", "Rituals");
        }

        protected override IEnumerable<Pawn> GetValidPawns(Pawn pawn)
        {
            if(!(pawn.GetLord()?.LordJob is LordJob_Ritual ritual))
            {
                return new List<Pawn>();
            }
            int availableParticipants = ritual.assignments.Participants
                .Where(p => p.Spawned)
                .Count();

            if(GetVoreAttemptCount(pawn) >= availableParticipants * 2) // honestly arbitrary attempt limit, take all available ritual participants and try at least twice
            {
                if(RV2Log.ShouldLog(false, "Rituals"))
                    RV2Log.Message($"{pawn.LabelShort} reached max attempts, forcing empty target list", "Rituals");
                return new List<Pawn>();
            }
            return ritual.assignments.Participants
                .Where(otherPawn => otherPawn != pawn
                    && !otherPawn.IsReserved());
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            if(pawn.IsReserved())
            {
                return JobMaker.MakeJob(JobDefOf.Wait, GenTicks.TickRareInterval);
            }
            IncreaseVoreAttemptCount(pawn);
            Job job = base.TryGiveJob(pawn);
            if(job == null)
            {
                if(RV2Log.ShouldLog(false, "Rituals"))
                    RV2Log.Message($"{pawn.LabelShort} could not create feast job, applying party duty", "Rituals");
                pawn.mindState.duty = new PawnDuty(RV2_Common.PartyDutyDef);
            }
            return job;
        }

        protected override VoreJob CreateJob(Pawn predator, Pawn prey, Pawn initiator, Pawn target, VorePathDef path)
        {
            VoreJob job = base.CreateJob(predator, prey, initiator, target, path);
            if(job != null)
            {
                job.IsRitualRelated = true;
            }

            return job;
        }
    }
}