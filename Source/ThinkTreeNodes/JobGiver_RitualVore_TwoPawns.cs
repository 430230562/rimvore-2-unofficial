using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimVore2
{
    public abstract class JobGiver_RitualVore : ThinkNode_JobGiver
    {
        protected virtual VoreRole ForcedRole => VoreRole.Invalid;
        protected virtual bool ForceFatal => false;
        protected virtual bool ForceEndo => false;

        public JobGiver_RitualVore() { }

        protected override Job TryGiveJob(Pawn pawn)
        {
            Pawn target = pawn.mindState.duty.focusSecond.Pawn;
            if(!pawn.CanReach(target, PathEndMode.ClosestTouch, Danger.None))
            {
                if(RV2Log.ShouldLog(false, "Rituals"))
                    RV2Log.Message("Target not reachable", "Rituals");
                return null;
            }
            List<VoreGoalDef> validGoals = DefDatabase<VoreGoalDef>.AllDefsListForReading
                .Where(goal => IsAllowed(goal))
                .ToList();
            if(RV2Log.ShouldLog(false, "Rituals"))
                RV2Log.Message($"Force endo? {ForceEndo} force fatal? {ForceFatal} Calculated these goals as valid: {String.Join(", ", validGoals.Select(g => g.defName))}", false, "Rituals");
            VoreInteractionRequest request = new VoreInteractionRequest(pawn, target, ForcedRole, goalWhitelist: validGoals);
            VoreInteraction interaction = VoreInteractionManager.Retrieve(request);
            if(interaction.PreferredPath == null)
            {
                if(RV2Log.ShouldLog(false, "Rituals"))
                    RV2Log.Message("No preferred path available for interaction", "Rituals");
                pawn.MentalState.RecoverFromState();
                return null;
            }
            if(RV2Log.ShouldLog(false, "Rituals"))
                RV2Log.Message(interaction.ToString(), "Rituals");
            VorePathDef path = interaction.PreferredPath;
            JobDef voreJobDef = interaction.RoleOf(pawn).GetInitJobDefFor();
            if(voreJobDef == null)
            {
                if(RV2Log.ShouldLog(false, "Rituals"))
                    RV2Log.Message("No job def found", "Rituals");
                return null;
            }
            VoreJob job = VoreJobMaker.MakeJob(voreJobDef, pawn, target);
            job.targetA = target;
            job.VorePath = path;
            job.IsRitualRelated = true;

            if(RV2Log.ShouldLog(false, "Rituals"))
                RV2Log.Message($"Giving job {job}", "Rituals");
            return job;

            bool IsAllowed(VoreGoalDef goal)
            {
                if(ForceFatal && !goal.IsLethal)
                {
                    return false;
                }
                if(ForceEndo && goal.IsLethal)
                {
                    return false;
                }
                return true;
            }
        }
    }

    /// <summary>
    /// base games job givers can apparently not take fields from XMLs, I would have just passed these parameters as XML fields, but that doesn't work. Thanks for nothing, Ludeon
    /// </summary>
    public class JobGiver_RitualVore_FatalPredator : JobGiver_RitualVore
    {
        protected override VoreRole ForcedRole => VoreRole.Predator;
        protected override bool ForceFatal => true;
    }
    public class JobGiver_RitualVore_FatalPrey : JobGiver_RitualVore
    {
        protected override VoreRole ForcedRole => VoreRole.Prey;
        protected override bool ForceFatal => true;
    }
    public class JobGiver_RitualVore_EndoPredator : JobGiver_RitualVore
    {
        protected override VoreRole ForcedRole => VoreRole.Predator;
        protected override bool ForceEndo => true;
    }
    public class JobGiver_RitualVore_EndoPrey : JobGiver_RitualVore
    {
        protected override VoreRole ForcedRole => VoreRole.Prey;
        protected override bool ForceEndo => true;
    }
}
