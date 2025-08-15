using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace RimVore2
{
    public static class VoreKeywordUtility
    {
        public static List<string> RecordKeywords(this VoreTrackerRecord record)
        {
            List<string> keywords = new List<string>();
            Pawn prey = record.Prey;
            Pawn predator = record.Predator;

            // general
            if(record.IsInterrupted) keywords.Add("IsInterrupted");
            keywords.Add("VoreType_" + record.VorePath.def.voreType);
            keywords.Add("VoreGoal_" + record.VorePath.def.voreGoal);

            // initiator
            if(record.Initiator == predator) keywords.AddDistinct("PredatorInitiated");
            else if(record.Initiator == prey) keywords.AddDistinct("PreyIntiated");
            else if(record.Initiator != null) keywords.AddDistinct("FeederInitiated");

            // goal abstraction
            if(record.VoreGoal.IsLethal) keywords.AddDistinct("FatalVore");
            else keywords.AddDistinct("EndoVore");

            // ------ prey ------ 

            // race
            keywords.AddRange(NaturalKeywords(prey, "Prey"));

            // quirk role preference
            if(prey.QuirkManager() != null)
            {
                float preyPreferenceAsPrey = prey.QuirkManager().GetTotalSelectorModifier(VoreRole.Prey);
                float preyPreferenceAsPredator = prey.QuirkManager().GetTotalSelectorModifier(VoreRole.Predator);
                // in the case of both being "preferred", take the stronger one as the actual preference
                if(preyPreferenceAsPredator > 1 && preyPreferenceAsPrey > 1)
                {
                    if(preyPreferenceAsPrey > preyPreferenceAsPredator) keywords.AddDistinct("PreyPrefersBeingPrey");
                    else keywords.AddDistinct("PreyPrefersBeingPredator");
                }
                else if(preyPreferenceAsPrey > 1) keywords.AddDistinct("PreyPrefersBeingPrey");
                else if(preyPreferenceAsPredator > 1) keywords.AddDistinct("PreyPrefersBeingPredator");
            }

            // misc
            if(record.PreyStartedNaked) keywords.AddDistinct("PreyIsNaked");
            else keywords.AddDistinct("PreyIsNotNaked");
            if(prey.ageTracker != null)
            {
                if(prey.ageTracker.AgeBiologicalYears < predator.ageTracker.AgeBiologicalYears) keywords.AddDistinct("PreyIsYounger");
                else keywords.AddDistinct("PreyIsOlder");
            }
            if(prey.GetStatValue(StatDefOf.PawnBeauty) > 1) keywords.AddDistinct("PreyIsBeautiful");
            if(prey.GetStatValue(StatDefOf.PsychicSensitivity) > 1.2f) keywords.AddDistinct("PreyIsPsychic");
            if(ModAdapter.Genitals.IsFertile(prey)) keywords.AddDistinct("PreyIsFertile");

            // skills
            keywords.AddRange(prey.SkillKeywords(false));

            // traits
            keywords.AddRange(prey.TraitKeywords(false));


            // ------ predator ------ 

            // race
            keywords.AddRange(NaturalKeywords(predator, "Predator"));

            // preference
            if(predator.QuirkManager() != null)
            {
                float predatorPreferenceAsPrey = predator.QuirkManager().GetTotalSelectorModifier(VoreRole.Prey);
                float predatorPreferenceAsPredator = predator.QuirkManager().GetTotalSelectorModifier(VoreRole.Predator);
                // in the case of both being "preferred", take the stronger one as the actual preference
                if(predatorPreferenceAsPredator > 1 && predatorPreferenceAsPrey > 1)
                {
                    if(predatorPreferenceAsPrey > predatorPreferenceAsPredator) keywords.AddDistinct("PredatorPrefersBeingPrey");
                    else keywords.AddDistinct("PredatorPrefersBeingPredator");
                }
                else if(predatorPreferenceAsPrey > 1) keywords.AddDistinct("PredatorPrefersBeingPrey");
                else if(predatorPreferenceAsPredator > 1) keywords.AddDistinct("PredatorPrefersBeingPredator");
            }

            // skills
            keywords.AddRange(predator.SkillKeywords(true));

            // traits
            keywords.AddRange(predator.TraitKeywords(true));


            // ------ combined ------

            // social standing
            prey.IsFriendOrRivalTowards(predator, out bool predatorIsFriend, out bool predatorIsRival);
            if(predatorIsFriend) keywords.AddDistinct("PredatorIsFriend");
            else if(predatorIsRival) keywords.AddDistinct("PredatorIsRival");

            predator.IsFriendOrRivalTowards(prey, out bool preyIsFriend, out bool preyIsRival);
            if(preyIsFriend) keywords.AddDistinct("PreyIsFriend");
            else if(preyIsRival) keywords.AddDistinct("PreyIsRival");

            // relation
            if(prey.IsFamilyByBlood(predator)) keywords.AddDistinct("PredatorIsFamilyByBlood");
            else if(prey.IsFamilyByChoice(predator)) keywords.AddDistinct("PredatorIsFamilyByChoice");

            if(predator.IsFamilyByBlood(prey)) keywords.AddDistinct("PreyIsFamilyByBlood");
            else if(predator.IsFamilyByChoice(prey)) keywords.AddDistinct("PreyIsFamilyByChoice");

            return keywords;
        }

        public static List<string> PawnKeywords(this Pawn pawn, bool ignoreRules = false)
        {
            List<string> keywords = new List<string>();
            List<VorePathDef> validPaths = RV2_Common.VorePaths.FindAll(path => path.IsValid(pawn, null, out _, false, ignoreRules));
            if(validPaths != null)
            {
                keywords.AddRange(validPaths.ConvertAll(path => "PawnCanVoreType_" + path.voreType.defName).Distinct());
                keywords.AddRange(validPaths.ConvertAll(path => "PawnCanVoreGoal_" + path.voreGoal.defName).Distinct());
            }

            keywords.AddRange(NaturalKeywords(pawn, "Pawn"));

            return keywords;
        }

        public static IEnumerable<string> NaturalKeywords(Pawn pawn, string prefix)
        {
            if(pawn.RaceProps == null)
            {
                yield break;
            }
            if(pawn.IsAnimal()) yield return prefix + "IsAnimal";
            else yield return prefix + "IsNotAnimal";
            if(pawn.IsMechanoid()) yield return prefix + "IsMechanoid";
            else yield return prefix + "IsNotMechanoid";
            if(pawn.IsHumanoid()) yield return prefix + "IsHumanoid";
            else yield return prefix + "IsNotHumanoid";
            if(pawn.IsInsectoid()) yield return prefix + "IsInsectoid";
            else yield return prefix + "IsNotInsectoid";
            if (pawn.gender == Gender.Male) yield return prefix + "IsMale";
            else if (pawn.gender == Gender.Female) yield return prefix + "IsFemale";
        }

        private static void IsFriendOrRivalTowards(this Pawn pawn, Pawn otherPawn, out bool isFriend, out bool isRival)
        {
            isFriend = false;
            isRival = false;
            if(pawn.relations == null)
            {
                return;
            }
            int friendThreshold = Pawn_RelationsTracker.FriendOpinionThreshold;
            int rivalThreshold = Pawn_RelationsTracker.RivalOpinionThreshold;
            int opinion = pawn.relations.OpinionOf(otherPawn);
            // this will set the pawns to be friends if friendThreshold and rivalThreshold are the same value
            if(opinion >= friendThreshold)
            {
                isFriend = true;
            }
            else if(opinion <= rivalThreshold)
            {
                isRival = true;
            }
        }

        private static bool IsFamilyByBlood(this Pawn pawn, Pawn otherPawn)
        {
            return pawn.relations?.FamilyByBlood?.Contains(otherPawn) == true;
        }

        private static bool IsFamilyByChoice(this Pawn pawn, Pawn otherPawn)
        {
            // base game doesn't care about family by choice, so custom check
            List<PawnRelationDef> relations = DefDatabase<RimVore2.DefList_PawnRelationDef>.GetNamed("RelationsToConsiderFamilyByChoice")?.relations;
            if(relations == null)
            {
                RV2Log.Warning("RelationsToConsiderFamilyByChoice is not set, unable to determine if pawns are family by choice");
                return false;
            }
            return pawn.GetRelations(otherPawn)?.Any(relation => relations.Contains(relation)) == true;
        }

        public static bool IsHumanoid(this Pawn pawn)
        {
            return pawn.RaceProps.Humanlike;
        }
        public static bool IsAnimal(this Pawn pawn)
        {
            return pawn.RaceProps.Animal;
        }
        public static bool IsWildAnimal(this Pawn pawn)
        {
            return pawn.IsAnimal() && pawn.Faction == null;
        }
        public static bool IsInsectoid(this Pawn pawn)
        {
            return pawn.RaceProps.FleshType == FleshTypeDefOf.Insectoid;
        }
        public static bool IsMechanoid(this Pawn pawn)
        {
            return pawn.RaceProps.FleshType == FleshTypeDefOf.Mechanoid
                || pawn.RaceProps.IsMechanoid;
        }

        public static RaceType GetRaceType(this Pawn pawn)
        {
            if(pawn.IsHumanoid())
            {
                return RaceType.Humanoid;
            }
            if(pawn.IsAnimal())
            {
                return RaceType.Animal;
            }
            if(pawn.IsInsectoid())
            {
                return RaceType.Insectoid;
            }
            if(pawn.IsMechanoid())
            {
                return RaceType.Mechanoid;
            }
            return RaceType.Invalid;
        }

        private static List<string> SkillKeywords(this Pawn pawn, bool isPredator)
        {
            List<SkillRecord> skills = pawn?.skills?.skills;
            if(skills.NullOrEmpty())
            {
                return new List<string>();
            }
            string prefix = isPredator ? "PredatorIsSkilledAt_" : "PreyIsSkilledAt_";
            return skills // take all skills
                .FindAll(skill => skill.Level > 15) // get the skills that have level 16 or higher
                .ConvertAll(skill => "PreyIsSkilledAt_" + skill.def.defName);   // convert the skill names to keywords
        }

        private static List<string> TraitKeywords(this Pawn pawn, bool isPredator)
        {
            List<Trait> traits = pawn?.story?.traits?.allTraits;
            if(traits.NullOrEmpty())
            {
                return new List<string>();
            }
            string prefix = isPredator ? "PredatorHasTrait_" : "PreyHasTrait_";
            return pawn.story.traits.allTraits.ConvertAll(trait => prefix + trait.def.defName);
        }

        public static bool IsNaked(this Pawn pawn)
        {
            // if prey can't have apparel or prey is not wearing any apparel
            if(pawn.apparel == null || pawn.apparel.WornApparelCount == 0)
            {
                return true;
            }
            return false;
        }
    }
}
