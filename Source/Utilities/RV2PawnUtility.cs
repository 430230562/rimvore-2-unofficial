using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public static class RV2PawnUtility
    {
        public static Trait GenerateTrait(Pawn pawn, PawnGenerationRequest request = default(PawnGenerationRequest))
        {
            Func<TraitDef, float> weightSelector = (TraitDef t) => t.GetGenderSpecificCommonality(pawn.gender);
            Func<Trait> traitPicker = delegate ()
            {
                TraitDef def = DefDatabase<TraitDef>.AllDefsListForReading.RandomElementByWeight(weightSelector);
                if(def.degreeDatas.Count > 0)
                {
                    //Log.Message("degree data: " + string.Join(", ", def.degreeDatas.ConvertAll(dd => dd.degree + ", " + dd.label)));
                    int randomDegree = PawnGenerator.RandomTraitDegree(def);
                    //Log.Message("Set degree to " + randomDegree);
                    return new Trait(def, randomDegree);
                }
                else
                {
                    //Log.Message("No need to set degree");
                    return new Trait(def);
                }
            };

            Trait trait = traitPicker();
            int recursionLock = 10;
            int originalRecursionLock = recursionLock;
            while(!TraitIsValid(pawn, trait, request))
            {
                trait = traitPicker();

                if(recursionLock-- == 0)
                {
                    RV2Log.Error("Tried " + originalRecursionLock + " different traitDefs, all were invalid.", "Traits");
                    return null;
                }
            }
            return trait;
        }

        public static void SetTraits(Pawn pawn, List<Trait> newTraits)
        {
            TraitSet pawnTraits = pawn.story?.traits;
            if(pawnTraits == null)
            {
                if(RV2Log.ShouldLog(false, "Traits"))
                    RV2Log.Message($"Tried to set traits, but pawn {pawn.LabelShort} does not have traits", "Traits");
                return;
            }
            foreach(Trait newTrait in newTraits)
            {

                Trait existingTrait = pawnTraits.GetTrait(newTrait.def);
                if(existingTrait != null)
                {
                    if(existingTrait.Degree != newTrait.Degree)
                    {
                        RemoveTrait(pawnTraits, existingTrait);
                    }
                    else
                    {
                        // the trait already exists in its intended degree, skip
                        continue;
                    }
                }

                if(!TraitIsValid(pawn, newTrait))
                {
                    if(RV2Log.ShouldLog(false, "Traits"))
                        RV2Log.Message($"Tried to set trait {newTrait.Label} but it is not valid, skipping", "Traits");
                    continue;
                }

                if(pawnTraits.allTraits.Count >= RV2Mod.Settings.cheats.ReformedPawnTraitCount)
                {
                    // the pawn has too many traits, remove a random one
                    Trait randomTrait = pawnTraits.allTraits.RandomElement();
                    if(RV2Log.ShouldLog(false, "Traits"))
                        RV2Log.Message($"removing trait {randomTrait.def.defName} due to overflow", "Traits");
                    RemoveTrait(pawnTraits, randomTrait);
                }
                if(RV2Log.ShouldLog(false, "Traits"))
                    RV2Log.Message($"adding trait {newTrait.def.defName} with degree {newTrait.Degree}", "Traits");
                pawnTraits.GainTrait(newTrait);
            }
        }

        public static void RemoveTrait(TraitSet set, Trait trait)
        {
            set.RemoveTrait(trait);
        }

        public static void GenerateOrTrimTraits(Pawn pawn, List<Trait> traits, int targetCount, PawnGenerationRequest request = default(PawnGenerationRequest))
        {
            if(traits.Count > targetCount)
            {
                traits = traits.GetRange(0, targetCount - 1);
                if(RV2Log.ShouldLog(false, "Traits"))
                    RV2Log.Message($"Trimmed traits down to {string.Join(", ", traits.ConvertAll(t => t.def.defName))}", "Traits");
            }
            if(traits.Count < targetCount)
            {
                int loopLock = 3 * (targetCount - traits.Count);  // each missing trait has 3 generation attempts
                while(traits.Count < targetCount && loopLock-- > 0)
                {
                    Trait newTrait = RV2PawnUtility.GenerateTrait(pawn, request);
                    if(RV2Log.ShouldLog(false, "Traits"))
                        RV2Log.Message($"Created new trait {newTrait.def.defName}", "Traits");
                    if(newTrait != null)
                    {
                        traits.Add(newTrait);
                    }
                }
                if(RV2Log.ShouldLog(false, "Traits"))
                    RV2Log.Message($"Generated traits to: {string.Join(", ", traits.ConvertAll(t => t.def.defName))}", "Traits");
            }
        }

        public static bool TraitIsValid(Pawn pawn, Trait trait, PawnGenerationRequest request = default(PawnGenerationRequest))
        {
            if(!TraitIsValid(pawn, trait.def, trait.Degree, request))
            {
                return false;
            }

            return true;
        }
        public static bool TraitIsValid(Pawn pawn, TraitDef traitDef, int degree = 0, PawnGenerationRequest request = default(PawnGenerationRequest))
        {
            if(traitDef == null)
            {
                RV2Log.Error("Could not create trait for " + pawn?.Label + ", resulting TraitDef is NULL");
                return false;
            }
            // pawn already has trait
            if(pawn.story?.traits?.HasTrait(traitDef) == true)
            {
                if(RV2Log.ShouldLog(false, "TraitGeneration"))
                    RV2Log.Message("trait invalid - pawn already has trait", "TraitGeneration");
                return false;
            }
            // request prohibits trait
            if(request.ProhibitedTraits?.Contains(traitDef) == true)
            {
                if(RV2Log.ShouldLog(false, "TraitGeneration"))
                    RV2Log.Message("trait invalid - request prohibits", "TraitGeneration");
                return false;
            }
            if(request.KindDef != null)
            {
                // the requests PawnKind blocks the trait
                if(request.KindDef?.disallowedTraits?.Contains(traitDef) == true)
                {
                    if(RV2Log.ShouldLog(false, "TraitGeneration"))
                        RV2Log.Message("trait invalid - request pawnkind disallows", "TraitGeneration");
                    return false;
                }
                // the requests PawnKind required a workTag that is blocked by the trait
                if((request.KindDef.requiredWorkTags & traitDef.disabledWorkTags) != WorkTags.None)
                {
                    if(RV2Log.ShouldLog(false, "TraitGeneration"))
                        RV2Log.Message("trait invalid - would block required workTag", "TraitGeneration");
                    return false;
                }
                if(traitDef == TraitDefOf.Gay && !request.AllowGay)
                {
                    if(RV2Log.ShouldLog(false, "TraitGeneration"))
                        RV2Log.Message("trait invalid - gae not allowed", "TraitGeneration");
                    return false;
                }
            }
            // trait is blocked for this currently hostile pawns
            if(!traitDef.allowOnHostileSpawn && pawn.Faction?.HostileTo(Faction.OfPlayer) == true)
            {
                if(RV2Log.ShouldLog(false, "TraitGeneration"))
                    RV2Log.Message("trait invalid - not allowed on hostile", "TraitGeneration");
                return false;
            }
            // any of the pawns existing traits conflicts with the new trait
            if(pawn.story?.traits?.allTraits?.Any(t => t.def.conflictingTraits.Contains(traitDef)) == true)
            {
                if(RV2Log.ShouldLog(false, "TraitGeneration"))
                    RV2Log.Message("trait invalid - conflicts with another trait", "TraitGeneration");
                return false;
            }
            // trait requires work type that the pawn is not capable of
            if(traitDef.requiredWorkTypes != null && pawn.OneOfWorkTypesIsDisabled(traitDef.requiredWorkTypes))
            {
                if(RV2Log.ShouldLog(false, "TraitGeneration"))
                    RV2Log.Message("trait invalid - requires worktype the pawn can't do", "TraitGeneration");
                return false;
            }
            // trait requires work tag that the pawn is not capable of
            if(pawn.WorkTagIsDisabled(traitDef.requiredWorkTags))
            {
                if(RV2Log.ShouldLog(false, "TraitGeneration"))
                    RV2Log.Message("trait invalid - required worktag that pawn can't do", "TraitGeneration");
                return false;
            }

            // if trait has forced worktype passions and pawn can work
            if(traitDef.forcedPassions != null && pawn.workSettings != null)
            {
                Predicate<SkillDef> skillCheck = (SkillDef s) => s.IsDisabled(pawn.story.DisabledWorkTagsBackstoryAndTraits, pawn.GetDisabledWorkTypes(true));
                // the trait tries to set a passion for a work type that the pawn is not capable of
                if(traitDef.forcedPassions.Any(skill => !skillCheck(skill)))
                {
                    if(RV2Log.ShouldLog(false, "TraitGeneration"))
                        RV2Log.Message("trait invalid - sets passion for worktype that pawn can't do", "TraitGeneration");
                    return false;
                }
            }
            BackstoryDef childhood = pawn.story?.Childhood;
            BackstoryDef adulthood = pawn.story?.Adulthood;
            // degree of trait is disallowed by backstories
            if(childhood?.DisallowsTrait(traitDef, degree) == true)
            {
                if(RV2Log.ShouldLog(false, "TraitGeneration"))
                    RV2Log.Message("trait invalid - childhood disallows", "TraitGeneration");
                return false;
            }
            if(adulthood?.DisallowsTrait(traitDef, degree) == true)
            {
                if(RV2Log.ShouldLog(false, "TraitGeneration"))
                    RV2Log.Message("trait invalid - adulthood disallows", "TraitGeneration");
                return false;
            }
            if(RV2Log.ShouldLog(false, "TraitGeneration"))
                RV2Log.Message("trait valid", "TraitGeneration");
            return true;
        }

        public static bool TryIncreaseNeed(Pawn pawn, NeedDef needDef, float value)
        {
            Need pawnNeed = pawn?.needs?.TryGetNeed(needDef);
            if(pawnNeed == null)
            {
                if(RV2Log.ShouldLog(false, "OngoingVore"))
                    RV2Log.Message($"Pawn {pawn?.Label} does not have need {needDef.label} - nothing to increase/decrease", "OngoingVore");
                return false;
            }
            float originalLevel = pawnNeed.CurLevel;
            pawnNeed.CurLevel += value;
            // if the need did not change, return false
            if(pawnNeed.CurLevel == originalLevel)
            {
                return false;
            }
            if(RV2Log.ShouldLog(true, "MentalStates"))
                RV2Log.Message("Changed " + pawn.Label + "'s need " + needDef.label + " by " + value + ", new level: " + pawnNeed.CurLevel, "OngoingVore");
            return true;
        }

        public static List<Hediff> GetHealableInjuries(this Pawn pawn)
        {
            return pawn.health?.hediffSet?.hediffs?
                .FindAll(hed => hed is Hediff_Injury
                && !hed.IsPermanent());
        }

        public static bool HasValidMentalStateForVore(this Pawn pawn, VoreRole role)
        {
            if(RV2Mod.Settings.cheats.DisableMentalStateChecks)
            {
                if(RV2Log.ShouldLog(true, "MentalStates"))
                    RV2Log.Message("Mental state forced to valid - cheat setting is currently ON", false, "MentalStates");
                return true;
            }
            if(pawn.CurJobDef == VoreJobDefOf.RV2_VoreGrapple)
            {
                if(RV2Log.ShouldLog(true, "MentalStates"))
                    RV2Log.Message("Mental state forced to valid - the pawn is currently grappling another pawn", false, "MentalStates");
                return true;
            }

            MentalStateDef stateDef = pawn.MentalStateDef;
            Extension_ValidMentalStateForVore extension = stateDef.GetModExtension<Extension_ValidMentalStateForVore>();
            if(extension == null)
            {
                return false;
            }
            return extension.IsValid(role);
        }

        public static float GetHungerRate(this Pawn pawn)
        {
            return pawn.needs.food.FoodFallPerTickAssumingCategory(HungerCategory.Fed);
        }

        public static float GetInternalTemperature(this Pawn pawn)
        {
            float temperature = GetInternalBaseTemperature(pawn);
            QuirkManager quirks = pawn.QuirkManager(false);
            if(quirks != null && quirks.HasValueModifier("InternalTemperature"))
            {
                temperature = quirks.ModifyValue("InternalTemperature", temperature);
                if(RV2Log.ShouldLog(true, "OngoingVore"))
                    RV2Log.Message($"Modified internal temperature of {pawn.LabelShort} because of quirks, new temperature: {temperature}", true, "OngoingVore");
            }
            return temperature;
        }

        private static float GetInternalBaseTemperature(Pawn pawn)
        {
            InternalTemperatureExtension temperatureExtension = pawn.def.GetModExtension<InternalTemperatureExtension>();
            if(temperatureExtension == null)
            {
                return RV2Mod.Settings.cheats.InternalTemperature;
            }
            return pawn.def.GetModExtension<InternalTemperatureExtension>().temperature;
        }

        /// <summary>
        /// Pawns love to become NULL, so we scribe the label of the pawn when we have them and log it out if the tracker becomes NULL
        /// </summary>
        public static string GetDebugName(this Pawn pawn)
        {
            if(pawn == null)
            {
                return "NULL";
            }
            return $"{pawn.LabelShort} ({pawn.GetUniqueLoadID()})";
        }
    }
}
