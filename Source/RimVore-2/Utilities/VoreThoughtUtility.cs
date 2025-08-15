using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public static class VoreThoughtUtility
    {
        static readonly List<TraitDef> traitsPreventingNegativeThoughts = new List<TraitDef>()
        {
            TraitDefOf.Kind,
            TraitDefOf.Wimp,
            TraitDefOf.Psychopath
        };
        static readonly List<TraitDef> traitsPreventingPositiveThoughts = new List<TraitDef>()
        {
            TraitDefOf.Psychopath
        };

        public static bool CanReceiveNegativeThoughts(Pawn pawn)
        {
            if(!CanReceiveThoughts(pawn))
            {
                return false;
            }
            return !traitsPreventingNegativeThoughts
                .Any(trait => pawn.story?.traits?.HasTrait(trait) == true);
        }
        public static bool CanReceivePositiveThoughts(Pawn pawn)
        {
            if(!CanReceiveThoughts(pawn))
            {
                return false;
            }
            return !traitsPreventingPositiveThoughts
                .Any(trait => pawn.story?.traits?.HasTrait(trait) == true);
        }

        public static bool CanReceiveThoughts(Pawn pawn)
        {
            return pawn.needs?.mood?.thoughts?.memories != null;
        }

        // call thought calculation for ALL pawns related to prey
        public static void NotifyFatallyVored(Pawn predator, Pawn prey, VoreGoalDef goalDef = null)
        {
            // Log.Message("Pawn " + prey.LabelShort + " was fatally vored by " + predator.LabelShort + ", iterating pawn relations for social memories");
            IEnumerable<Pawn> observers = prey.relations?.PotentiallyRelatedPawns
                .Where(pawn => !pawn.Dead
                && pawn != predator);
            if(observers.EnumerableNullOrEmpty())
            {
                return;
            }
            foreach(Pawn observer in observers)
            {
                // Log.Message("calculating for " + observer.LabelShort);
                int opinion = observer.relations.OpinionOf(prey);
                bool isObsessed = goalDef?.IsObsessed(observer, VoreRole.Prey) == true
                    || goalDef?.IsObsessed(observer, VoreRole.Predator) == true;
                ThoughtDef moodToApply = null;
                ThoughtDef socialToApply = null;
                if(opinion < -20 && isObsessed)
                {
                    if(!CanReceivePositiveThoughts(observer))
                    {
                        continue;
                    }
                    // observer disliked prey and is obsessed with the vore goal, positive memories
                    moodToApply = VoreThoughtDefOf.RV2_FatallyVoredMemory_Rival_Mood;
                    socialToApply = VoreThoughtDefOf.RV2_FatallyVoredMemory_Rival_Social;
                }
                else if(opinion > 25 && !isObsessed)
                {
                    if(!CanReceiveNegativeThoughts(observer))
                    {
                        continue;
                    }
                    // observer liked prey and is not obsessed with vore goal, negative memories
                    moodToApply = VoreThoughtDefOf.RV2_FatallyVoredMemory_Mood;
                    socialToApply = VoreThoughtDefOf.RV2_FatallyVoredMemory_Social;
                }
                // Log.Message("adding social: " + socialToApply?.defName + " mood: " + moodToApply?.defName);
                if(moodToApply != null)
                {
                    observer.needs.mood.thoughts.memories.TryGainMemory(moodToApply, predator);
                }
                if(socialToApply != null)
                {
                    observer.needs.mood.thoughts.memories.TryGainMemory(socialToApply, predator);
                }
            }
        }

        // call thought calculation for involved pawns
        public static void NotifyDeniedProposal(Pawn initiator, Pawn target, VoreRole initiatorRole = VoreRole.Invalid, VoreTypeDef typeDef = null, VoreGoalDef goalDef = null)
        {
            if(!CanReceiveNegativeThoughts(initiator))
            {
                if(RV2Log.ShouldLog(true, "VoreThoughts"))
                    RV2Log.Message($"Initiator {initiator.LabelShort} either has no memories or has traits blocking negative memories", true, "VoreThoughts");
                return;
            }
            ThoughtDef socialMemoryToApply;
            ThoughtDef moodMemoryToApply;
            if(goalDef?.IsObsessed(initiator, initiatorRole) == true || typeDef?.IsObsessed(initiator, initiatorRole) == true)
            {
                socialMemoryToApply = VoreThoughtDefOf.RV2_DeniedVoreProposalObsessed_Social;
                moodMemoryToApply = VoreThoughtDefOf.RV2_DeniedVoreProposalObsessed_Mood;
            }
            else
            {
                socialMemoryToApply = VoreThoughtDefOf.RV2_DeniedVoreProposal_Social;
                moodMemoryToApply = VoreThoughtDefOf.RV2_DeniedVoreProposal_Mood;
            }
            if(socialMemoryToApply != null)
            {
                initiator.needs.mood.thoughts.memories.TryGainMemory(socialMemoryToApply, target, null);
            }
            if(moodMemoryToApply != null)
            {
                initiator.needs.mood.thoughts.memories.TryGainMemory(moodMemoryToApply, target, null);
            }
        }
    }
}
