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
    public abstract class JobGiver_RitualVore_Feeder : JobGiver_RitualVore
    {
        public JobGiver_RitualVore_Feeder() { }

        private bool DeterminePawns(Pawn feeder, out Pawn predator, out Pawn prey)
        {
            predator = null;
            prey = null;
            LordJob_Ritual ritual = RV2RitualUtility.ParticipatingRitual(feeder);
            if(ritual == null)
            {
                if(RV2Log.ShouldLog(false, "Rituals"))
                    RV2Log.Message($"No ritual found for pawn {feeder.LabelShort}", "Rituals");
                return false;
            }
            predator = ritual.PawnWithRole("predator");
            if(predator == null)
            {
                if(RV2Log.ShouldLog(false, "Rituals"))
                    RV2Log.Message("No pawn with ID prey found", "Rituals");
                return false;
            }
            prey = ritual.PawnWithRole("prey");
            if(predator == null)
            {
                if(RV2Log.ShouldLog(false, "Rituals"))
                    RV2Log.Message("No pawn with ID prey found", "Rituals");
                return false;
            }
            return true;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            DeterminePawns(pawn, out Pawn predator, out Pawn prey);
            if(!pawn.CanReach(prey, PathEndMode.ClosestTouch, Danger.None))
            {
                if(RV2Log.ShouldLog(false, "Rituals"))
                    RV2Log.Message("Prey not reachable", "Rituals");
                return null;
            }
            if(!pawn.CanReach(predator, PathEndMode.ClosestTouch, Danger.None))
            {
                if(RV2Log.ShouldLog(false, "Rituals"))
                    RV2Log.Message("Predator not reachable", "Rituals");
                return null;
            }
            List<VoreGoalDef> validGoals = DefDatabase<VoreGoalDef>.AllDefsListForReading
                .Where(goal => IsAllowed(goal))
                .ToList();
            if(RV2Log.ShouldLog(true, "Rituals"))
                RV2Log.Message($"Force endo? {ForceEndo} force fatal? {ForceFatal} Calculated these goals as valid: {String.Join(", ", validGoals.Select(g => g.defName))}", false, "Rituals");
            VoreInteractionRequest request = new VoreInteractionRequest(predator, prey, VoreRole.Predator, goalWhitelist: validGoals);
            VoreInteraction interaction = VoreInteractionManager.Retrieve(request);
            if(interaction.PreferredPath == null)
            {
                return null;
            }
            if(RV2Log.ShouldLog(false, "Rituals"))
                RV2Log.Message(interaction.ToString(), "Rituals");
            VorePathDef path = interaction.ValidPaths.RandomElementWithFallback();
            if(path == null)
            {
                if(RV2Log.ShouldLog(false, "Rituals"))
                    RV2Log.Message("No path available for interaction", "Rituals");
                pawn.MentalState.RecoverFromState();
                return null;
            }
            JobDef voreJobDef = VoreJobDefOf.RV2_VoreInitAsFeeder;
            if(voreJobDef == null)
            {
                if(RV2Log.ShouldLog(false, "Rituals"))
                    RV2Log.Message("No job def found", "Rituals");
                return null;
            }
            VoreJob job = VoreJobMaker.MakeJob(voreJobDef, pawn, prey, predator);
            job.targetA = prey;
            job.targetB = predator;
            job.VorePath = path;
            job.IsRitualRelated = true;

            if(RV2Log.ShouldLog(false, "Rituals"))
                RV2Log.Message($"Giving job {job}", "Rituals");
            return job;

            bool IsAllowed(VoreGoalDef goal)
            {
                if(!goal.validForRituals)
                {
                    return false;
                }
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
    public class JobGiver_RitualVore_EndoFeeder : JobGiver_RitualVore_Feeder
    {
        protected override bool ForceEndo => true;
    }
    public class JobGiver_RitualVore_FatalFeeder : JobGiver_RitualVore_Feeder
    {
        protected override bool ForceFatal => true;
    }
}