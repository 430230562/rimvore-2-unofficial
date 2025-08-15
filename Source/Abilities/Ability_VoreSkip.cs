using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace RimVore2
{
    public class Ability_VoreSkip : Ability_MultiTarget
    {
        public Ability_VoreSkip() : base() { }
        public Ability_VoreSkip(Pawn pawn, Precept sourcePrecept) : base(pawn, sourcePrecept) { }
        public Ability_VoreSkip(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(pawn, sourcePrecept, def) { }
        public Ability_VoreSkip(Pawn pawn) : base(pawn) { }
        public Ability_VoreSkip(Pawn pawn, AbilityDef def) : base(pawn, def) { }

        Pawn Prey => initialTarget.Pawn;
        AbilityExtension_VoreSkip VoreSkipExtension => def.GetModExtension<AbilityExtension_VoreSkip>();
        public override Predicate<TargetInfo> AdditionalTargetValidator => (TargetInfo target) =>
        {
            if(Prey == null)
                return false;
            if(!(target.Thing is Pawn predator))
                return false;
            if(!predator.CanVore(Prey, out _))
                return false;
            VoreInteractionRequest request = new VoreInteractionRequest(predator, Prey, VoreRole.Predator);
            VoreInteraction interaction = VoreInteractionManager.Retrieve(request);
            return interaction.IsValid;
        };

        public override void Notify_AllTargetsPicked()
        {
            base.Notify_AllTargetsPicked();
            //Log.Message($"Picked targets: {string.Join(", ", targets.Select(t => t.Label))}");
            if(targets.Count != 2)
            {
                RV2Log.Warning("Ability_VoreSkip trying to run without the correct amount of targets", "Psycast");
                return;
            }
            Pawn predator = targets[1].Pawn;
            VoreInteractionRequest request = new VoreInteractionRequest(predator, Prey, VoreRole.Predator);
            VoreInteraction interaction = VoreInteractionManager.Retrieve(request);
            VoreGoalDef goal;
            if(VoreSkipExtension.allowChoosingVoreGoal)
            {
                // this would require more force-pause logic that I currently am unwilling to force on the game.
                // it all would have worked if it was easier to force-pause the game, but this would require a parallel implementation
                // to the Targeter_ForcePause, which is already enough of a hack job to make me hate it
                Log.Error("Nabber never bothered to implement this.");
            }
            goal = interaction.ValidGoals.RandomElementWithFallback();
            if(goal == null)
            {
                if(RV2Log.ShouldLog(false, "Psycast"))
                    RV2Log.Message("No vore goal picked", "Psycast");
                return;
            }
            if(!DetermineRandomValidPathIndexForSkip(interaction, goal, out VorePathDef path, out int pathIndex))
                return;
            VoreTrackerRecord record = new VoreTrackerRecord(predator, Prey, true, pawn, new VorePath(path), pathIndex, false);
            PreVoreUtility.PopulateRecord(ref record);
            predator.PawnData().VoreTracker.TrackVore(record);
        }

        private bool DetermineRandomValidPathIndexForSkip(VoreInteraction interaction, VoreGoalDef goal, out VorePathDef path, out int index)
        {
            index = -1;
            path = interaction.ValidPathsFor(goal).RandomElementWithFallback();
            if(path == null)
            {
                if(RV2Log.ShouldLog(false, "Psycast"))
                    RV2Log.Message("no path found", "Psycast");
                return false;
            }
            VoreStageDef stage = path.stages
                .Where(s => s.canReverseDirection)
                .RandomElementWithFallback();
            if(stage == null)
            {
                if(RV2Log.ShouldLog(false, "Psycast"))
                    RV2Log.Message("no stage found", "Psycast");
                return false;
            }
            index = path.stages.IndexOf(stage);
            return true;
        }
    }
}
