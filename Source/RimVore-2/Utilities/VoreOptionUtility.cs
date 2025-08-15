using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimVore2
{
    /// <summary>
    /// Manage valid vore types for various criteria
    /// </summary>
    public static class VoreOptionUtility
    {
        public static List<VorePathDef> ValidPaths(Pawn predator, Pawn prey, VoreSource source)
        {
            switch(source)
            {
                case VoreSource.Manual:
                    return RV2_Common.VorePaths.FindAll(vorePath => vorePath.IsValid(predator, prey, out string reason));
                default:
                    throw new NotImplementedException("Unknown source to calculate vore types for: " + source);
            }
        }

        public static List<VoreGoalDef> ValidGoalsInPaths(List<VorePathDef> vorePaths)
        {
            return vorePaths.Select(path => path.voreGoal)
                .Distinct()
                .ToList();
        }

        public static List<VoreTypeDef> ValidTypesInPaths(List<VorePathDef> vorePaths, VoreGoalDef voreGoal)
        {
            return vorePaths.FindAll(path => path.voreGoal == voreGoal)
                .Select(path => path.voreType)
                .Distinct()
                .ToList();
        }

        public static VorePathDef GetPath(List<VorePathDef> vorePaths, VoreTypeDef voreType, VoreGoalDef voreGoal)
        {
            return vorePaths.Find(path => path.voreType == voreType && path.voreGoal == voreGoal);
        }

        public static VorePath MakeVorePath(List<VorePathDef> vorePaths, VoreTypeDef voreType, VoreGoalDef voreGoal)
        {
            VorePathDef vorePathDef = GetPath(vorePaths, voreType, voreGoal);
            return new VorePath(vorePathDef);
        }

        public enum VoreSource
        {
            Manual,
            Auto
        }

        public static ForcedState GetForcedState(this VoreJob job, Pawn predator, Pawn prey, bool isForced = false)
        {
            return GetForcedState(job.Initiator, predator, prey, isForced);
        }
        public static ForcedState GetForcedState(Pawn initiator, Pawn predator, Pawn prey, bool isForced = false)
        {
            if(!isForced)
            {
                return ForcedState.Willing;
            }
            if(initiator == predator)
            {
                return ForcedState.ForcedByPredator;
            }
            if(initiator == prey)
            {
                return ForcedState.ForcedByPrey;
            }
            else
            {
                return ForcedState.ForcedByFeeder;
            }
        }

        /// <summary>
        /// If the pawn is a predator animal, only use paths that feed the predator
        /// </summary>
        public static List<VorePathDef> ConditionalPathWhitelistForPredatorAnimals(Pawn pawn)
        {
            if(pawn.IsAnimal() && pawn.RaceProps.predator)
            {
                if(RV2Mod.Settings.fineTuning.PredatorAnimalsOnlyUseDigestGoal)
                {
                    return RV2_Common.VorePathsFeedingPredator;
                }
            }
            return null;
        }
    }
}
