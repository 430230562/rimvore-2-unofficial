using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace RimVore2
{
    public static class PreferenceUtility
    {
        const float initialPreference = 0.001f;
        public static float PreferenceFor(this Pawn pawn, VoreRole role, ModifierOperation modifierOperation = ModifierOperation.Add)
        {
            if(RV2Log.ShouldLog(true, "Preferences"))
                RV2Log.Message($"Calculating {pawn.LabelShort}'s preference for role {role}", false, "Preferences");
            QuirkManager quirks = pawn.QuirkManager();
            float preference = initialPreference; //modifierOperation.DefaultModifierValue();
            if(quirks == null)
            {
                return preference;
            }
            VoreTargetSelectorRequest request = new VoreTargetSelectorRequest(true)
            {
                role = role
            };
            if(!quirks.HasTotalSelectorModifier(request))
            {
                // role rolling is a bit odd, when no preference exists, force value > 0, otherwise auto-vore will early exit due to 0 prey and 0 pred preference
                return preference;
            }
            preference = modifierOperation.Aggregate(preference, quirks.GetTotalSelectorModifierForDirect(request));
            return preference;
        }

        public static float PreferenceFor(this Pawn pawn, Pawn target, ModifierOperation modifierOperation = ModifierOperation.Add)
        {
            if(RV2Log.ShouldLog(true, "Preferences"))
                RV2Log.Message($"Calculating {pawn.LabelShort}'s preference for target {target.LabelShort}", false, "Preferences");
            QuirkManager quirks = pawn.QuirkManager();
            float preference = initialPreference; //modifierOperation.DefaultModifierValue();
            if(quirks == null)
            {
                return preference;
            }
            VoreTargetSelectorRequest request = new VoreTargetSelectorRequest(true)
            {
                raceType = target.GetRaceType()
            };
            if(RV2Log.ShouldLog(true, "Preferences"))
                RV2Log.Message($"modifying with {request.raceType} preference", false, "Preferences");
            preference = modifierOperation.Aggregate(preference, quirks.GetTotalSelectorModifierForDirect(request));
            return preference;
        }

        public static float PreferenceFor(this Pawn pawn, VoreGoalDef goal, VoreRole role, ModifierOperation modifierOperation = ModifierOperation.Add)
        {
            if(RV2Log.ShouldLog(true, "Preferences"))
                RV2Log.Message($"Calculating {pawn.LabelShort}'s preference for goal {goal.defName} as {role}", false, "Preferences");
            QuirkManager quirks = pawn.QuirkManager();
            float preference = initialPreference; //modifierOperation.DefaultModifierValue();
            if(quirks == null)
            {
                return preference;
            }
            VoreTargetSelectorRequest request = new VoreTargetSelectorRequest(true)
            {
                voreGoal = goal,
                role = role
            };
            preference = modifierOperation.Aggregate(preference, quirks.GetTotalSelectorModifierForDirect(request, modifierOperation));
            return preference;
        }

        public static float PreferenceFor(this Pawn pawn, VoreTypeDef type, VoreRole role, ModifierOperation modifierOperation = ModifierOperation.Add)
        {
            if(RV2Log.ShouldLog(true, "Preferences"))
                RV2Log.Message($"Calculating {pawn.LabelShort}'s preference for type {type.defName} as {role}", false, "Preferences");
            QuirkManager quirks = pawn.QuirkManager();
            float preference = initialPreference; //modifierOperation.DefaultModifierValue();
            if(quirks == null)
            {
                return preference;
            }
            VoreTargetSelectorRequest request = new VoreTargetSelectorRequest(true)
            {
                voreType = type,
                role = role
            };
            preference = modifierOperation.Aggregate(preference, quirks.GetTotalSelectorModifierForDirect(request, modifierOperation));
            return preference;
        }

        public static float GetChanceToAcceptProposal(VoreProposal proposal)
        {
            Pawn initiator = proposal.Initiator;
            Pawn target = proposal.PrimaryTarget;
            bool targetAutoAcceptsProposal = target.AutoAccepts(initiator);
            if(targetAutoAcceptsProposal)
            {
                if(RV2Log.ShouldLog(true, "Preferences"))
                    RV2Log.Message("Target auto-accepts proposal, returning 100% chance", false, "Preferences");
                return 1f;
            }

            float chance = RV2Mod.Settings.cheats.BaseProposalAcceptanceChance;
            if(RV2Log.ShouldLog(true, "Preferences"))
                RV2Log.Message($"base chance: {chance}", false, "Preferences");
            QuirkManager initiatorQuirks = initiator.QuirkManager();
            float initiatorSuccessModifierValue = 0f;
            if(initiatorQuirks != null)
            {
                initiatorSuccessModifierValue = initiatorQuirks.ModifyValue("VoreProposalSuccessModifier", chance) - chance;
            }
            if(RV2Log.ShouldLog(true, "Preferences"))
                RV2Log.Message($"initiator proposal success modifier (added on top of final chance): {initiatorSuccessModifierValue}", false, "Preferences");
            QuirkManager targetQuirks = target.QuirkManager();
            float targetSuccessModifierValue = 0f;
            if(targetQuirks != null)
            {
                string targetPreferences = $"target {target.LabelShortCap} preferences for proposed vore: ";
                float preferenceForRace = target.PreferenceFor(initiator);
                float preferenceScore = preferenceForRace;
                targetPreferences += $"\nFor other pawn: {preferenceForRace} -> {preferenceScore}";
                if(proposal is VoreProposal_TwoWay twoWayProposal)
                {
                    float preferenceForRole = target.PreferenceFor(proposal.RoleOf(target));
                    preferenceScore += preferenceForRole;
                    targetPreferences += $"\nFor role: {preferenceForRole} -> {preferenceScore}";

                    float preferenceForGoal = target.PreferenceFor(twoWayProposal.VorePath.voreGoal, proposal.RoleOf(target));
                    preferenceScore += preferenceForGoal;
                    targetPreferences += $"\nFor goal: {preferenceForGoal} -> {preferenceScore}";

                    float preferenceForType = target.PreferenceFor(twoWayProposal.VorePath.voreType, proposal.RoleOf(target));
                    preferenceScore += preferenceForType;
                    targetPreferences += $"\nFor type: {preferenceForType} -> {preferenceScore}";
                }
                else if(proposal is VoreProposal_Feeder_Predator)
                {
                    float feederPredatorPreference = GetPreferenceForFeederRole(target, VoreRole.Predator);
                    preferenceScore += feederPredatorPreference;
                    targetPreferences += $"\nFor being pred in feeding vore: {feederPredatorPreference} -> {preferenceScore}";
                }
                else if(proposal is VoreProposal_Feeder_Prey feederPreyProposal)
                {
                    float feederPreyPreference = GetPreferenceForFeederRole(target, VoreRole.Prey);
                    preferenceScore += feederPreyPreference;
                    targetPreferences += $"\nFor being prey in feeding vore: {feederPreyPreference} -> {preferenceScore}";

                    float preferenceForGoal = target.PreferenceFor(feederPreyProposal.VorePath.voreGoal, VoreRole.Prey);
                    preferenceScore += preferenceForGoal;
                    targetPreferences += $"\nFor goal: {preferenceForGoal} -> {preferenceScore}";

                    float preferenceForType = target.PreferenceFor(feederPreyProposal.VorePath.voreType, VoreRole.Prey);
                    preferenceScore += preferenceForType;
                    targetPreferences += $"\nFor type: {preferenceForType} -> {preferenceScore}";
                }
                if(RV2Log.ShouldLog(true, "Preferences"))
                    RV2Log.Message(targetPreferences, "Preferences");
                targetSuccessModifierValue = preferenceScore * RV2Mod.Settings.cheats.ProposalModifierPerPreference;
                targetSuccessModifierValue = Mathf.Min(targetSuccessModifierValue, RV2Mod.Settings.cheats.MaxProposalModifierViaQuirks);
            }
            if(RV2Log.ShouldLog(true, "Preferences"))
                RV2Log.Message($"target proposal success modifier: {targetSuccessModifierValue}", false, "Preferences");
            chance = chance + initiatorSuccessModifierValue + targetSuccessModifierValue;
            if(RV2Log.ShouldLog(true, "Preferences"))
                RV2Log.Message($"Total chance: {chance}", false, "Preferences");
            return chance;
        }

        private static float GetPreferenceForFeederRole(Pawn pawn, VoreRole role)
        {
            QuirkManager quirks = pawn.QuirkManager(false);
            if(quirks == null)
                return 0f;

            VoreTargetSelectorRequest request = new VoreTargetSelectorRequest()
            {
                role = role
            };
            float preferenceForFeederPredator = quirks.GetTotalSelectorModifierForFeeder(request);
            return preferenceForFeederPredator;
        }

        /// <summary>
        /// Skip if the target does not need to be convinced (high preference for proposal)
        /// </summary>
        /// <returns></returns>
        private static bool AutoAccepts(this Pawn target, Pawn initiator)
        {
            if(initiator.IsAnimal())
            {
                if(AnimalAutoConvinced(initiator, target))
                {
                    if(RV2Log.ShouldLog(false, "Preferences"))
                        RV2Log.Message("Animal convinced with high animal skill", "Preferences");
                    return true;
                }
            }
            else if(initiator.IsHumanoid())
            {
                if(HumanoidAutoConvinced(initiator, target))
                {
                    if(RV2Log.ShouldLog(false, "Preferences"))
                        RV2Log.Message("Humanoid convinced with high social skill", "Preferences");
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Skip if target is animal and initiator has good animal skill
        /// </summary>
        /// <returns></returns>
        private static bool AnimalAutoConvinced(Pawn initiator, Pawn target)
        {
            if(!target.RaceProps.Animal)
            {
                return false;
            }
            SkillRecord animalSkill = initiator.skills?.GetSkill(SkillDefOf.Animals);
            if(animalSkill == null)
            {
                return false;
            }
            return animalSkill.Level >= RV2Mod.Settings.fineTuning.AutoAcceptAnimalSkill;
        }

        /// <summary>
        /// Skip if target is human and initiator has good social skill
        /// </summary>
        /// <param name="initiator"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private static bool HumanoidAutoConvinced(Pawn initiator, Pawn target)
        {
            if(!target.IsHumanoid())
            {
                return false;
            }
            SkillRecord initiatorSocial = initiator.skills?.GetSkill(SkillDefOf.Social);
            if(initiatorSocial == null)
            {
                return false;
            }
            SkillRecord targetSocial = target.skills?.GetSkill(SkillDefOf.Social);
            int levelDifference;
            if(targetSocial == null)
            {
                levelDifference = initiatorSocial.Level;
            }
            else
            {
                levelDifference = initiatorSocial.Level - targetSocial.Level;
            }
            return levelDifference >= RV2Mod.Settings.fineTuning.AutoAcceptSocialSkillDifference;
        }

        public static bool CanBeForced(this Pawn pawn)
        {
            if(pawn.Downed && RV2Mod.Settings.rules.CanBeForcedIfDowned(pawn))
            {
                return true;
            }
            if(pawn.IsPrisonerOfColony)
            {
                return !RV2Mod.Settings.fineTuning.PrisonersMustConsentToProposal;
            }
            if(pawn.IsSlaveOfColony)
            {
                return !RV2Mod.Settings.fineTuning.SlavesMustConsentToProposal;
            }
            return false;
        }
    }
}
