using JetBrains.Annotations;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class VoreRitualFlag : DefModExtension { }
    public class VoreRitualValidator : VoreRitualFlag
    {
        [UsedImplicitly]
        string predatorId;
        [UsedImplicitly]
        string preyId;
        [UsedImplicitly]
        bool forceEndo;
        [UsedImplicitly]
        bool forceFatal;

        private KeyValuePair<float, string> cachedResult = new KeyValuePair<float, string>(0, "null");
        public bool IsValid(RitualRoleAssignments assignments, out string reason)
        {
            if(assignments == null)
            {
                reason = "RV2_RitualInvalidReasons_NoAssignments".Translate();
                return false;
            }
            Pawn predator = assignments.FirstAssignedPawn(predatorId);
            Pawn prey = assignments.FirstAssignedPawn(preyId);
            float hashKey = (predatorId + predator?.ThingID + preyId + prey?.ThingID).GetHashCode();
            if(cachedResult.Key == hashKey)
            {
                reason = cachedResult.Value;
                if(RV2Log.ShouldLog(false, "Rituals"))
                    RV2Log.Message("Retrieving cached result: " + reason, true, "Rituals");
                return reason == null;
            }
            else
            {
                bool valid = IsValid(predator, prey, out reason);
                cachedResult = new KeyValuePair<float, string>(hashKey, reason);
                if(RV2Log.ShouldLog(false, "Rituals"))
                    RV2Log.Message("Calculated new cached result: " + reason, true, "Rituals");
                return valid;
            }
        }

        private bool IsValid(Pawn predator, Pawn prey, out string reason)
        {
            if(predator == null)
            {
                reason = "RV2_RitualInvalidReasons_PredatorNotSet".Translate();
                return false;
            }
            if(prey == null)
            {
                reason = "RV2_RitualInvalidReasons_PreyNotSet".Translate();
                return false;
            }

            List<VoreGoalDef> validGoals = DefDatabase<VoreGoalDef>.AllDefsListForReading
                .Where(goal => IsAllowed(goal))
                .ToList();
            if(RV2Log.ShouldLog(true, "Rituals"))
                RV2Log.Message($"Force endo? {forceEndo} force fatal? {forceFatal} - Calculated these goals as valid: {String.Join(", ", validGoals.Select(g => g.defName))}", false, "Rituals");
            VoreInteractionRequest request = new VoreInteractionRequest(predator, prey, VoreRole.Predator, goalWhitelist: validGoals);
            VoreInteraction interaction = VoreInteractionManager.Retrieve(request);
            if(interaction.PreferredPath == null)
            {
                reason = "RV2_RitualInvalidReasons_NoPath".Translate();
                return false;
            }
            reason = null;
            return true;

            bool IsAllowed(VoreGoalDef goal)
            {
                if(forceFatal && !goal.IsLethal)
                {
                    return false;
                }
                if(forceEndo && goal.IsLethal)
                {
                    return false;
                }
                return true;
            }
        }
    }
}