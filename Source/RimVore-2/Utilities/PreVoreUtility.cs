using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RimVore2
{
    public static class PreVoreUtility
    {
        /// <param name="internalSplitOff">Whether or not the new record is created from an existing record, in which case we do not want to act as if the predator has actually directly vored the prey (no records, etc)</param>
        public static void PopulateRecord(ref VoreTrackerRecord record, bool internalSplitOff = false)
        {
            Pawn prey = record.Prey;
            Pawn predator = record.Predator;

            record.PreyStartedNaked = prey.IsNaked();

            record.VoreContainer = new VoreContainer(predator, record.VorePath.def, prey);
            record.VoreContainer.TryAddOrTransferPrey(prey);

            if(record.VorePath.def.feedsPredator)
            {
                predator.AddFood(VoreCalculationUtility.CalculatePreyNutrition(prey, predator));
            }
            if(!internalSplitOff)
            {
                IncrementRecords(record);
                TriggerInteractions(record);
            }
            record.PathToJumpTo = PredatorPreferredPathForJump(record);
        }
        public static void IncrementRecords(VoreTrackerRecord record)
        {
            record.Predator.records?.Increment(RV2_Common.predatorRecordDef);
            record.Prey.records?.Increment(RV2_Common.preyRecordDef);
            record.VoreType.IncrementRecords(record.Predator, record.Prey);
        }

        public static void TriggerInteractions(VoreTrackerRecord record)
        {
            InteractionDef interactionDef = record.VoreGoal.IsLethal ? VoreInteractionDefOf.RV2_FatalVore : VoreInteractionDefOf.RV2_EndoVore;

            // the list at the end is a free selection of rule packs! We can use this to insert the goal / type of vore!
            // This kinda creates messy rules, but interactions are an overcomplicated mess anyways...
            List<RulePackDef> rulePacks = ExtraRulePacks(record);
            PlayLogEntry_Interaction logEntry = new PlayLogEntry_Interaction(interactionDef, record.Predator, record.Prey, rulePacks);
            Find.PlayLog.Add(logEntry);
        }
        private static List<RulePackDef> ExtraRulePacks(VoreTrackerRecord record)
        {
            List<RulePackDef> list = new List<RulePackDef>();
            List<RulePackDef> typeRulePacks = record.VorePath.VoreType.relatedRulePacks;
            if(typeRulePacks != null)
            {
                list.AddRange(typeRulePacks);
            }
            List<RulePackDef> goalRulePacks = record.VorePath.VoreGoal.relatedRulePacks;
            if(goalRulePacks != null)
            {
                list.AddRange(goalRulePacks);
            }
            return list;
        }

        /// <note>
        /// The target jump path must use the same VoreType as the original one to prevent odd interactions where oral would be swapped to anal in the stomach, which logically concludes the same way
        /// </note>
        private static VorePathDef PredatorPreferredPathForJump(VoreTrackerRecord record)
        {
            if(record.Predator?.QuirkManager()?.HasSpecialFlag("EnableGoalSwitching") != true)
            {
                if(RV2Log.ShouldLog(true, "VoreJump"))
                    RV2Log.Message($"{record.Predator.LabelShort} not considering vore switching - pawn doesn't have the quirk", false, "VoreJump");
                return null;
            }
            if(record.IsResultOfSwitchedPath)
            {
                if(RV2Log.ShouldLog(true, "VoreJump"))
                    RV2Log.Message($"{record.Predator.LabelShort} not considering vore switching - current vore has already switched", false, "VoreJump");
                return null;
            }
            // player forced paths prevent the pawn from switching the goal on their own
            if(record.IsPlayerForced && !RV2Mod.Settings.cheats.AllowSelfVoreJumpOnPlayerForcedVore)
                return null;
            // pawn forced paths are also not switchable - the pawn must have been a prisoner or downed, so vore was already picked without their consent
            if(record.IsForced)
            {
                if(RV2Log.ShouldLog(true, "VoreJump"))
                    RV2Log.Message($"{record.Predator.LabelShort} not considering vore switching because vore was forced", false, "VoreJump");
                return null;
            }

            bool shouldIgnoreDesignations = RV2Mod.Settings.features.IgnoreDesignationsGoalSwitching;
            VoreInteractionRequest request = new VoreInteractionRequest(record.Predator, record.Prey, VoreRole.Predator, isForAuto: !record.IsPlayerForced, shouldIgnoreDesignations: shouldIgnoreDesignations);
            VoreInteraction interaction = VoreInteractionManager.Retrieve(request);
            List<VoreGoalDef> bestPreferredGoals = new List<VoreGoalDef>();
            float maxPreference = float.MinValue;
            List<string> jumpKeysForThisPath = record.VorePath.def.stages
                .Select(stage => stage.jumpKey)
                .Where(key => key != null)
                .ToList();
            IEnumerable<VoreGoalDef> potentialJumpGoals = interaction.ValidPathsFor(record.VoreType)    // keep the type the same so that stage-logic can't reverse
                .Where(path => jumpKeysForThisPath  // only use paths that can be "reached" with vore jumps from the original
                    .Any(key => JumpUtility.HasJumpKey(path, key))
                )
                .Select(path => path.voreGoal);
            foreach(VoreGoalDef goal in potentialJumpGoals)
            {
                // no need to consider role, prey race or type, just roll for the goal alone
                float currentPreference = record.Predator.PreferenceFor(goal, VoreRole.Predator);
                if(currentPreference > maxPreference)
                {
                    bestPreferredGoals.Clear();
                    maxPreference = currentPreference;
                }
                if(currentPreference == maxPreference)
                    bestPreferredGoals.Add(goal);
            }
            VoreGoalDef goalToSwitchTo = bestPreferredGoals.RandomElementWithFallback();
            if(RV2Log.ShouldLog(true, "VoreJump"))
                RV2Log.Message($"{record.Predator.LabelShort} is considering these goals for switching: {(bestPreferredGoals.NullOrEmpty() ? "NONE" : string.Join(", ", bestPreferredGoals.Select(g => g.defName)))}", false, "VoreJump");
            if(goalToSwitchTo == null)
            {
                if(RV2Log.ShouldLog(true, "VoreJump"))
                    RV2Log.Message($"{record.Predator.LabelShort} would have tried to switch goals, but no preferred goals were found", false, "VoreJump");
                return null;
            }
            if(goalToSwitchTo == record.VorePath.def.voreGoal)
            {
                if(RV2Log.ShouldLog(true, "VoreJump"))
                    RV2Log.Message($"{record.Predator.LabelShort} picked {goalToSwitchTo.defName}, which they are already doing, so no need to switch", false, "VoreJump");
                return null;
            }
            VorePathDef pickedPath = interaction.ValidPathsFor(goalToSwitchTo, record.VorePath.VoreType)
                .RandomElementWithFallback();
            if(RV2Log.ShouldLog(false, "VoreJump"))
                RV2Log.Message($"{record.Predator.LabelShort} picked {pickedPath.defName} to jump to", "VoreJump");
            return pickedPath;
        }
    }
}
