/// No longer directly used from the ThinKTree since Vore became a work type and proposals are generated in WorkGiver_ProposeVore

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
    public class JobGiver_RequestVore : ThinkNode_JobGiver
    {
        protected virtual IEnumerable<Pawn> GetValidPawns(Pawn pawn)
        {
            Map map = pawn.Map;
            if(map == null)
            {
                if(RV2Log.ShouldLog(true, "AutoVore"))
                    RV2Log.Message("Early exit: no map determinable", false, "AutoVore");
                return null;
            }
            List<Pawn> validTargets = map.mapPawns.FreeColonistsSpawned;
            validTargets.AddDistinctRange(map.mapPawns.PrisonersOfColonySpawned);
            if(RV2Mod.Settings.features.AnimalsEnabled)
            {
                validTargets.AddDistinctRange(map.mapPawns.SpawnedColonyAnimals);
            }
            validTargets = validTargets
                .Where(t => ValidTargetFor(t, pawn))
                .ToList();    // check if vore is even possible between the pawns

            // remove self from targets
            validTargets.Remove(pawn);
            return validTargets;
        }

        public static bool ValidTargetFor(Pawn potentialTarget, Pawn initiator)
        {
            return !potentialTarget.InAggroMentalState    // do not propose to hostile pawns
                && initiator.CanReserveAndReach(potentialTarget, PathEndMode.Touch, Danger.Some)    // check pathing and reservation ability
                && (initiator.CanVore(potentialTarget, out _) || potentialTarget.CanVore(initiator, out _))
                && RV2Mod.Settings.rules.CanBeProposedTo(potentialTarget);
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            if(RV2Log.ShouldLog(true, "AutoVore"))
                RV2Log.Message("Tried to give request-vore job!", false, "AutoVore");
            IEnumerable<Pawn> validTargets = GetValidPawns(pawn);
            if(validTargets.EnumerableNullOrEmpty())
            {
                if(RV2Log.ShouldLog(true, "AutoVore"))
                    RV2Log.Message("Early exit: no targets at all", false, "AutoVore");
                return null;
            }
            if(ModsConfig.IdeologyActive)
            {
                if(TryAttemptIdeologyJobInterception(pawn, validTargets, out VoreJob ideologyJob))
                {
                    return ideologyJob;
                }
            }
            Pawn target = RollForTarget(pawn, validTargets);
            if(target == null)
            {
                if(RV2Log.ShouldLog(true, "AutoVore"))
                    RV2Log.Message("Early exit: no target determinable", false, "AutoVore");
                return null;
            }

            VoreInteractionRequest directRequest = new VoreInteractionRequest(pawn, target, VoreRole.Invalid, isForAuto: true, isForProposal: true);
            VoreInteraction interaction = VoreInteractionManager.Retrieve(directRequest);
            if(interaction.PreferredPath == null)
            {
                if(RV2Log.ShouldLog(true, "AutoVore"))
                    RV2Log.Message("Early exit: no preferred path", false, "AutoVore");
                return null;
            }
            VorePathDef pathDef = interaction.PreferredPath;
            if(RV2Log.ShouldLog(true, "AutoVore"))
                RV2Log.Message($"Picked path for vore proposal: {pathDef.defName}", false, "AutoVore");
            return CreateJob(interaction.Predator, interaction.Prey, pawn, target, pathDef);
        }

        private bool TryAttemptIdeologyJobInterception(Pawn pawn, IEnumerable<Pawn> targets, out VoreJob job)
        {
            job = null;
            Ideo ideo = pawn.Ideo;
            if(ideo == null)
            {
                if(RV2Log.ShouldLog(true, "AutoVore"))
                    RV2Log.Message("Pawn has no ideo, skipping ideology intercept", false, "AutoVore");
                return false;
            }
            Pawn alphaPredator = targets.FirstOrDefault(t => ideo.GetRole(t)?.def == RV2_Common.IdeoRolePredator);
            Pawn chosenPrey = targets.FirstOrDefault(t => ideo.GetRole(t)?.def == RV2_Common.IdeoRolePrey);
            if(alphaPredator != null && !alphaPredator.CanVore(pawn, out string reason))
            {
                if(RV2Log.ShouldLog(true, "AutoVore"))
                    RV2Log.Message($"Alpha predator found, but they can not vore the current pawn, reason: {reason}", false, "AutoVore");
                alphaPredator = null;
            }
            if(chosenPrey != null && !pawn.CanVore(chosenPrey, out reason))
            {
                if(RV2Log.ShouldLog(true, "AutoVore"))
                    RV2Log.Message($"Chosen prey found, but they can not be vored by the current pawn, reason: {reason}", false, "AutoVore");
                chosenPrey = null;
            }
            if(alphaPredator == null && chosenPrey == null)
            {
                if(RV2Log.ShouldLog(true, "AutoVore"))
                    RV2Log.Message("No ideology role target, skipping ideology intercept", false, "AutoVore");
                return false;
            }
            if(!Rand.Chance(RV2Mod.Settings.ideology.AutoVoreInterceptionChance))
            {
                if(RV2Log.ShouldLog(true, "AutoVore"))
                    RV2Log.Message($"Random chance for ideology intercept failed, chance was {RV2Mod.Settings.ideology.AutoVoreInterceptionChance}%", false, "AutoVore");
                return false;
            }
            VoreRole forcedRole;
            Pawn target;
            if(alphaPredator != null && chosenPrey != null)
            {
                if(Rand.Chance(0.5f))
                {
                    if(RV2Log.ShouldLog(true, "AutoVore"))
                        RV2Log.Message("Chosen Prey and Alpha Predator available, picked alpha predator", false, "AutoVore");
                    target = alphaPredator;
                    forcedRole = VoreRole.Prey;
                }
                else
                {
                    if(RV2Log.ShouldLog(true, "AutoVore"))
                        RV2Log.Message("Chosen Prey and Alpha Predator available, picked chosen prey", false, "AutoVore");
                    target = chosenPrey;
                    forcedRole = VoreRole.Predator;
                }
            }
            else if(alphaPredator != null)
            {
                if(RV2Log.ShouldLog(true, "AutoVore"))
                    RV2Log.Message("Alpha Predator available and picked", false, "AutoVore");
                target = alphaPredator;
                forcedRole = VoreRole.Prey;
            }
            else
            {
                if(RV2Log.ShouldLog(true, "AutoVore"))
                    RV2Log.Message("Chosen Prey available and picked", false, "AutoVore");
                target = chosenPrey;
                forcedRole = VoreRole.Predator;
            }
            if(RV2Log.ShouldLog(true, "AutoVore"))
                RV2Log.Message($"Target: {target.LabelShort} initiator role: {forcedRole}", false, "AutoVore");
            VoreInteractionRequest request = new VoreInteractionRequest(pawn, target, forcedRole, isForAuto: true, isForProposal: true);
            VoreInteraction interaction = VoreInteractionManager.Retrieve(request);
            if(interaction.PreferredPath == null)
            {
                if(RV2Log.ShouldLog(true, "AutoVore"))
                    RV2Log.Message("No path available", false, "AutoVore");
                return false;
            }
            VorePathDef pathDef = interaction.PreferredPath;
            if(RV2Log.ShouldLog(true, "AutoVore"))
                RV2Log.Message($"Picked path for vore proposal: {pathDef.defName}", false, "AutoVore");
            job = CreateJob(interaction.Predator, interaction.Prey, pawn, target, pathDef);
            return true;
        }

        protected virtual VoreJob CreateJob(Pawn predator, Pawn prey, Pawn initiator, Pawn target, VorePathDef path)
        {
            VoreProposal_TwoWay proposal = new VoreProposal_TwoWay(predator, prey, initiator, target, path);
            VoreJob job = VoreJobMaker.MakeJob(VoreJobDefOf.RV2_ProposeVore, initiator, target);
            job.targetA = target;
            job.Proposal = proposal;
            job.VorePath = proposal.VorePath;
            return job;
        }

        private Pawn RollForTarget(Pawn pawn, IEnumerable<Pawn> targets)
        {
            Dictionary<Pawn, float> weightedTargets;
            if(!TryMakeWeightedPreferences(pawn, targets, out weightedTargets))
            {
                return null;
            }
            if(RV2Log.ShouldLog(true, "AutoVore"))
                RV2Log.Message($"weighted targets: {LogUtility.ToString(weightedTargets)}", false, "AutoVore");

            Pawn target = weightedTargets.RandomElementByWeight(kvp => kvp.Value).Key;
            if(RV2Log.ShouldLog(true, "AutoVore"))
                RV2Log.Message($"Picked target {target.LabelShort}", false, "AutoVore");
            return target;
        }

        private bool TryMakeWeightedPreferences(Pawn pawn, IEnumerable<Pawn> targets, out Dictionary<Pawn, float> weightedPreferences)
        {
            weightedPreferences = new Dictionary<Pawn, float>();
            if(targets.EnumerableNullOrEmpty())
            {
                return false;
            }
            foreach(Pawn target in targets)
            {
                float preference = pawn.PreferenceFor(target);
                if(preference >= 0)
                {
                    //preference++;
                    weightedPreferences.Add(target, preference);
                }
                else
                {
                    if(RV2Log.ShouldLog(true, "AutoVore"))
                        RV2Log.Message("Preference for target is below 0, not adding to valid target list", false, "AutoVore");
                }
            }
            return !weightedPreferences.EnumerableNullOrEmpty();
        }
    }
}
