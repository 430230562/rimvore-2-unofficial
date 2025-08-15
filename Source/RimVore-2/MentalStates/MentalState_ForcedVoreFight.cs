using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace RimVore2
{
    public abstract class MentalState_ForcedVoreFight : MentalState
    {
        public Pawn otherPawn;

        public override bool ForceHostileTo(Thing t)
        {
            if(t is Pawn pawn)
            {
                return otherPawn == pawn;
            }
            return false;
        }
    }

    public class MentalState_ForcedVoreFight_Attacker : MentalState_ForcedVoreFight
    {
        public VoreProposal_TwoWay proposal;
        FightStatus status = FightStatus.InProgress;
        public float initialAttackerInjuries = float.MinValue;
        public float initialDefenderInjuries = float.MinValue;

        private bool ShouldStopNow
        {
            get
            {
                status = CalculateFightStatus();
                return status != FightStatus.InProgress
                || otherPawn == null
                || otherPawn.Downed
                || !otherPawn.Spawned;
            }
        }

        private enum FightStatus
        {
            InProgress,
            Won,
            Lost
        }

        private Func<Pawn, float> GetHealth = (Pawn p) => p.health.summaryHealth.SummaryHealthPercent;
        const float VoreFightThreshold = 0.15f;
        private FightStatus CalculateFightStatus()
        {
            if(otherPawn.Downed)
            {
                return FightStatus.Won;
            }
            if(pawn.Downed)
            {
                return FightStatus.Lost;
            }
            if(initialAttackerInjuries == float.MinValue)
            {
                initialAttackerInjuries = GetHealth(pawn);
            }
            if(initialDefenderInjuries == float.MinValue)
            {
                initialDefenderInjuries = GetHealth(otherPawn);
            }
            float attackerDamage = initialAttackerInjuries - GetHealth(pawn);
            float defenderDamage = initialDefenderInjuries - GetHealth(otherPawn);
            float damageDifference = Math.Abs(attackerDamage - defenderDamage);
            if(damageDifference < VoreFightThreshold)
            {
                return FightStatus.InProgress;
            }
            else if(attackerDamage > defenderDamage)
            {
                return FightStatus.Lost;
            }
            else
            {
                return FightStatus.Won;
            }
        }

        public override void MentalStateTick()
        {
            if(ShouldStopNow)
            {
                if(RV2Log.ShouldLog(false, "MentalStates"))
                    RV2Log.Message($"Vore fighting over, status: {status}", "MentalStates");
                base.RecoverFromState();
                otherPawn?.MentalState?.RecoverFromState();
                if(status == FightStatus.Won)
                {
                    StartVoreJob();
                }
            }
            base.MentalStateTick();
        }

        private void StartVoreJob()
        {
            JobDef nextJobDef = proposal.RoleOf(pawn).GetInitJobDefFor();
            VoreJob nextJob = VoreJobMaker.MakeJob(nextJobDef, pawn, otherPawn);
            nextJob.IsForced = true;
            nextJob.VorePath = proposal.VorePath;
            if(RV2Log.ShouldLog(true, "MentalStates"))
                RV2Log.Message($"Starting vore initiation job: {nextJobDef.defName}", false, "MentalStates");
            this.pawn.jobs.StartJob(nextJob, JobCondition.Succeeded);
            RemoveProposalDeniedMemory();
        }

        private void RemoveProposalDeniedMemory()
        {
            List<ThoughtDef> memoriesToRemove = new List<ThoughtDef>()
            {
                VoreThoughtDefOf.RV2_DeniedVoreProposalObsessed_Mood,
                VoreThoughtDefOf.RV2_DeniedVoreProposalObsessed_Social,
                VoreThoughtDefOf.RV2_DeniedVoreProposal_Mood,
                VoreThoughtDefOf.RV2_DeniedVoreProposal_Social
            };
            List<RimWorld.Thought_Memory> memories = pawn.needs?.mood?.thoughts?.memories?.Memories;
            if(memories == null)
                return;
            foreach(RimWorld.Thought_Memory memory in memories.ToList())    // good ol list copy to prevent CollectionModifiedException
            {
                if(memoriesToRemove.Contains(memory.def))
                {
                    pawn.needs.mood.thoughts.memories.RemoveMemory(memory);
                    if(RV2Log.ShouldLog(false, "MentalStates"))
                        RV2Log.Message($"Removed memory {memory.def.defName} due to forced vore", "MentalStates");
                }
            }
        }
    }
    public class MentalState_ForcedVoreFight_Defender : MentalState_ForcedVoreFight
    {
        private bool ShouldStopNow
        {
            get
            {
                return !(otherPawn.MentalState is MentalState_ForcedVoreFight_Attacker);
            }
        }

        public override void MentalStateTick()
        {
            if(ShouldStopNow)
            {
                base.RecoverFromState();
            }
            base.MentalStateTick();
        }
    }
}
