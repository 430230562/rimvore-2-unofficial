using RimWorld;
using Verse;
using Verse.AI;

namespace RimVore2
{
    public class JobGiver_VoreFighting : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if(pawn.RaceProps.Humanlike && pawn.WorkTagIsDisabled(WorkTags.Violent))
            {
                return null;
            }
            Pawn otherPawn = ((MentalState_ForcedVoreFight)pawn.MentalState).otherPawn;
            Verb verbToUse;
#if v1_5
            if(!InteractionUtility.TryGetRandomVerbForSocialFight(pawn, out verbToUse))
#else
            if(!SocialInteractionUtility.TryGetRandomVerbForSocialFight(pawn, out verbToUse))
#endif
            {
                return null;
            }
            Job job = JobMaker.MakeJob(JobDefOf.SocialFight, otherPawn);
            job.maxNumMeleeAttacks = 1;
            job.verbToUse = verbToUse;
            return job;
        }
    }
}
