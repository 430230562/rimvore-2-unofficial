using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using Verse;

namespace RimVore2
{
    /// <note>
    /// There is a lot of shared logic between the different influence sources (race def, traits, quirks), and I entangled them quite harshly, I somewhat regret the decision because readability
    /// has suffered from it, but at least there is no duplicate code
    /// </note>
    public class StruggleManager : IExposable
    {
        VoreTrackerRecord record;
        public bool shouldStruggle = false;
        int requiredStruggles = -1;
        int currentStruggles = 0;

        const float sizeDifferenceMultiplier = 0.5f;

        public bool ShouldStruggle => shouldStruggle;
        // need to 0-check requiredStruggles to prevent DivideByZeroException, need to clamp in case of overflow/underflow struggles
        public float StruggleProgress => requiredStruggles == 0 ? 0 : Mathf.Clamp((float)currentStruggles / requiredStruggles, 0, 1);

        public StruggleManager() { }

        public StruggleManager(VoreTrackerRecord record)
        {
            this.record = record;
            shouldStruggle = CalculateShouldStruggle();
            CalculateRequiredStruggles();
        }

        #region struggle influences
        // used to simplify access to the various influences for float values, bools are only used once for no-struggle
        Func<Pawn, VoreRole, Func<Extension_StruggleInfluence, VoreRole, float>, float> RaceMultiplier = (Pawn pawn, VoreRole role, Func<Extension_StruggleInfluence, VoreRole, float> valueRetrieval) =>
        {
            Extension_StruggleInfluence extension = pawn.def.GetModExtension<Extension_StruggleInfluence>();
            if(extension == null)
                return 1;
            return valueRetrieval(extension, role);
        };
        Func<Pawn, VoreRole, Func<Extension_StruggleInfluence, VoreRole, int, float>, float> TraitMultiplier = (Pawn pawn, VoreRole role, Func<Extension_StruggleInfluence, VoreRole, int, float> valueRetrieval) =>
        {
            List<Trait> traits = pawn.story?.traits?.allTraits;
            float multiplier = 1f;
            if(traits.NullOrEmpty())
                return multiplier;
            foreach(Trait trait in traits)
            {
                Extension_StruggleInfluence traitExtension = trait.def.GetModExtension<Extension_StruggleInfluence>();
                if(traitExtension == null)
                    continue;
                multiplier *= valueRetrieval(traitExtension, role, trait.Degree);
            }
            return multiplier;
        };
        Func<Pawn, string, float> QuirkMultiplier = (Pawn pawn, string modifierName) =>
        {
            QuirkManager quirks = pawn.QuirkManager(false);
            if(quirks == null)
                return 1f;
            if(!quirks.TryGetValueModifier(modifierName, ModifierOperation.Multiply, out float multiplier))
                return 1f;
            return multiplier;
        };

        // and this merges the logic for requiredStruggles and struggleChance... Much to the disregard of readability...
        private void InfluenceWithRace(ref float value, ref string explanation, Func<Extension_StruggleInfluence, VoreRole, float> valueRetrieval)
        {
            float preyMultiplier = RaceMultiplier(record.Prey, VoreRole.Prey, valueRetrieval);
            value = value * preyMultiplier;
            explanation += $"\nprey race multiplier: {preyMultiplier} => {value}";

            float predatorMultiplier = RaceMultiplier(record.Predator, VoreRole.Predator, valueRetrieval);
            value = value * predatorMultiplier;
            explanation += $"\npredator race multiplier: {predatorMultiplier} => {value}";
        }
        private void InfluenceWithTraits(ref float value, ref string explanation, Func<Extension_StruggleInfluence, VoreRole, int, float> valueRetrieval)
        {
            float preyMultiplier = TraitMultiplier(record.Prey, VoreRole.Prey, valueRetrieval);
            value = value * preyMultiplier;
            explanation += $"\nprey traits multiplier: {preyMultiplier} => {value}";

            float predatorMultiplier = TraitMultiplier(record.Predator, VoreRole.Predator, valueRetrieval);
            value = value * predatorMultiplier;
            explanation += $"\npredator traits multiplier: {predatorMultiplier} => {value}";
        }
        private void InfluenceWithQuirks(ref float value, ref string explanation, string preyModifierName, string predatorModifierName)
        {
            float preyMultiplier = QuirkMultiplier(record.Prey, preyModifierName);
            value = value * preyMultiplier;
            explanation += $"\nprey quirks multiplier: {preyMultiplier} => {value}";
            float predatorMultiplier = QuirkMultiplier(record.Predator, predatorModifierName);
            value = value * predatorMultiplier;
            explanation += $"\npredator quirks multiplier: {predatorMultiplier} => {value}";
        }
        private void InfluenceWithBodySize(ref float value, ref string explanation)
        {
            float preySize = record.Prey.BodySize;
            float predatorSize = record.Predator.BodySize;
            float multiplier = preySize / predatorSize;
            /// i want to limit the influence of size on the struggle, no matter if it were < 1 or > 1, let's use two examples
            /// first example: prey size 1, predator size 1.5, so diff is 1.5
            /// 1.5 - 1 = 0.5 => 0.5 * 0.5 = 0.25 => 0.25 + 1 = 1.25, final modifier is 125% instead of 150%
            /// second example: prey size 1.5, predator size 1, so diff is 0.67
            /// 0.67 - 1 = -0.33 => -0.33 * 0.5 = -0.17 => -0.17 + 1 = 0.83,  final modifier is 83% instead of 67%
            /// Probably way too much explanation for simple math, but I'm bad at math
            multiplier = (multiplier - 1) * sizeDifferenceMultiplier + 1;
            value *= multiplier;
            explanation += $"\nBody size difference: {multiplier} [({preySize}/{predatorSize}-1)*{sizeDifferenceMultiplier}+1] => {value}";
        }

        /// <param name="postMultiplier">An additional modifier that is applied to the health percentage, allows inversion of influence by setting -1</param>
        private void InfluenceWithInjuries(ref float value, ref string explanation, Pawn pawn, float postMultiplier = 1f)
        {
            float multiplier = pawn.health.summaryHealth.SummaryHealthPercent;
            value *= multiplier * postMultiplier;
            string postMultiplierExplanation = postMultiplier != 1f ? $" ({multiplier}*{postMultiplier})" : "";
            explanation += $"\n{pawn.LabelShortCap} health scale: {multiplier}{postMultiplierExplanation} => {value}";
        }

        private bool CalculateShouldStruggle()
        {
            // settings deny struggling
            if(!RV2Mod.Settings.features.StrugglingEnabled)
            {
                if(RV2Log.ShouldLog(false, "Struggling"))
                    RV2Log.Message($"Prey {record.Prey.LabelShort} won't struggle, disabled in settings", false, "Struggling");
                return false;
            }

            if(!RV2Mod.Settings.rules.ShouldStruggle(record.Prey, record.VorePath.def, record.IsForced))
            {
                if(RV2Log.ShouldLog(false, "Struggling"))
                    RV2Log.Message($"Prey {record.Prey.LabelShort} won't struggle, path settings prevent", false, "Struggling");
                return false;
            }

            if(NoStruggleDueToQuirks())
                return false;

            if(NoStruggleDueToRace())
                return false;

            if(NoStruggleDueToTraits())
                return false;

            return true;
        }

        private bool NoStruggleDueToQuirks()
        {
            // prey never wants to struggle
            QuirkManager preyQuirks = record.Prey.QuirkManager();
            if(preyQuirks != null && preyQuirks.HasSpecialFlag("BlockStruggleAsPrey"))
            {
                if(RV2Log.ShouldLog(false, "Struggling"))
                    RV2Log.Message($"Prey {record.Prey.LabelShort} won't struggle, prey quirks prevent", false, "Struggling");
                return true;
            }

            // predator stops prey from struggling
            QuirkManager predatorQuirks = record.Predator.QuirkManager();
            if(predatorQuirks != null && predatorQuirks.HasSpecialFlag("BlockStruggleAsPredator"))
            {
                if(RV2Log.ShouldLog(false, "Struggling"))
                    RV2Log.Message($"Prey {record.Prey.LabelShort} won't struggle, predator quirks prevent", false, "Struggling");
                return true;
            }
            return false;
        }
        private bool NoStruggleDueToRace()
        {
            Func<Pawn, VoreRole, bool> RacePreventsStruggling = (Pawn pawn, VoreRole role) =>
            {
                Extension_StruggleInfluence extension = pawn.def.GetModExtension<Extension_StruggleInfluence>();
                if(extension == null)
                    return false;
                return extension.NoStrugglingRace(role);
            };

            if(RacePreventsStruggling(record.Prey, VoreRole.Prey))
            {
                if(RV2Log.ShouldLog(false, "Struggling"))
                    RV2Log.Message($"Prey {record.Prey.LabelShort} won't struggle, prey race preventing", false, "Struggling");
                return true;
            }
            if(RacePreventsStruggling(record.Predator, VoreRole.Predator))
            {
                if(RV2Log.ShouldLog(false, "Struggling"))
                    RV2Log.Message($"Prey {record.Prey.LabelShort} won't struggle, predator race preventing", false, "Struggling");
                return true;
            }

            return false;
        }
        private bool NoStruggleDueToTraits()
        {
            Func<Pawn, VoreRole, bool> AnyTraitPreventsStruggling = (Pawn pawn, VoreRole role) =>
            {
                List<Trait> traits = pawn.story?.traits?.allTraits;
                if(traits.NullOrEmpty())
                    return false;
                foreach(Trait trait in traits)
                {
                    Extension_StruggleInfluence traitExtension = trait.def.GetModExtension<Extension_StruggleInfluence>();
                    if(traitExtension == null)
                        continue;
                    if(traitExtension.NoStrugglingTraits(role, trait.Degree))
                        return true;
                }
                return false;
            };

            if(AnyTraitPreventsStruggling(record.Prey, VoreRole.Prey))
            {
                if(RV2Log.ShouldLog(false, "Struggling"))
                    RV2Log.Message($"Prey {record.Prey.LabelShort} won't struggle, prey traits preventing", false, "Struggling");
                return true;
            }
            if(AnyTraitPreventsStruggling(record.Predator, VoreRole.Predator))
            {
                if(RV2Log.ShouldLog(false, "Struggling"))
                    RV2Log.Message($"Prey {record.Prey.LabelShort} won't struggle, predator traits preventing", false, "Struggling");
                return true;
            }

            return false;
        }

        #endregion

        private void CalculateRequiredStruggles()
        {
            string explanation = "";
            float requiredStrugglesRaw = RV2Mod.Settings.rules.BaseRequiredStruggles(record.Prey, record.VorePath.def);
            explanation += $"Base struggles: {requiredStrugglesRaw}";
            float speedMultiplier = RV2Mod.Settings.cheats.VoreSpeedMultiplier;
            requiredStrugglesRaw = requiredStrugglesRaw / speedMultiplier;
            explanation += $"\nspeed multiplier {speedMultiplier} => {requiredStrugglesRaw}";
            InfluenceRequiredStrugglesWithRace();
            InfluenceRequiredStrugglesWithTraits();
            InfluenceRequiredStrugglesWithQuirks();
            InfluenceRequiredStrugglesWithBodySize();
            requiredStruggles = Mathf.RoundToInt(requiredStrugglesRaw);
            if(RV2Log.ShouldLog(false, "Struggling"))
                RV2Log.Message($"Prey {record.Prey.LabelShort} has calculated {requiredStruggles} required struggles to break free, explanation: \n{explanation}", false, "Struggling");

            void InfluenceRequiredStrugglesWithRace()
            {
                Func<Extension_StruggleInfluence, VoreRole, float> valueRetrieval = (Extension_StruggleInfluence extension, VoreRole role) => extension.RequiredStrugglesMultiplierRace(role);
                InfluenceWithRace(ref requiredStrugglesRaw, ref explanation, valueRetrieval);
            }
            void InfluenceRequiredStrugglesWithTraits()
            {
                Func<Extension_StruggleInfluence, VoreRole, int, float> valueRetrieval = (Extension_StruggleInfluence extension, VoreRole role, int degree) => extension.RequiredStrugglesMultiplierTraits(role, degree);
                InfluenceWithTraits(ref requiredStrugglesRaw, ref explanation, valueRetrieval);
            }
            void InfluenceRequiredStrugglesWithQuirks()
            {
                InfluenceWithQuirks(ref requiredStrugglesRaw, ref explanation, "RequiredStrugglesMultiplierAsPrey", "RequiredStrugglesMultiplierAsPredator");
            }
            void InfluenceRequiredStrugglesWithBodySize()
            {
                InfluenceWithBodySize(ref requiredStrugglesRaw, ref explanation);
            }
        }


        public void Tick()
        {
            if(!shouldStruggle)
                return;
            if(record.Prey.Dead)
            {
                DoAbortStrugglingInteraction();
                shouldStruggle = false;
                return;
            }
            Struggle();
            EjectIfPossible();
        }
        void DoAbortStrugglingInteraction()
        {
            PlayLogEntry_Interaction interaction = new PlayLogEntry_Interaction(VoreInteractionDefOf.RV2_FailedStruggle, record.Prey, record.Predator, new List<RulePackDef>());
            Find.PlayLog.Add(interaction);
        }

        private void Struggle()
        {
            float successChance = RV2Mod.Settings.combat.DefaultStruggleChance;
            string explanation = $"Base Chance: {successChance}";
            InfluenceStruggleChanceWithRace();
            InfluenceStruggleChanceWithTraits();
            InfluenceStruggleChanceWithQuirks();
            InfluenceStruggleChanceWithBodySize();
            InfluenceStruggleChanceWithInjuries();

            if(RV2Log.ShouldLog(true, "Struggling"))
                RV2Log.Message($"Prey {record.Prey.LabelShort} is struggling with a chance of {successChance} - explanation: \n{explanation}", false, "Struggling");
            if(!Rand.Chance(successChance))
                return;
            currentStruggles++;

            PlayLogEntry_Interaction interaction = new PlayLogEntry_Interaction(VoreInteractionDefOf.RV2_ActiveStruggle, record.Prey, record.Predator, new List<RulePackDef>());
            Find.PlayLog.Add(interaction);
            if(RV2Log.ShouldLog(true, "Struggling"))
                RV2Log.Message($"Prey {record.Prey.LabelShort} successfully struggled - progress: {StruggleProgress} ({currentStruggles}/{requiredStruggles})", false, "Struggling");

            void InfluenceStruggleChanceWithRace()
            {
                Func<Extension_StruggleInfluence, VoreRole, float> valueRetrieval = (Extension_StruggleInfluence extension, VoreRole role) => extension.StruggleChanceMultiplierRace(role);
                InfluenceWithRace(ref successChance, ref explanation, valueRetrieval);
            }
            void InfluenceStruggleChanceWithTraits()
            {
                Func<Extension_StruggleInfluence, VoreRole, int, float> valueRetrieval = (Extension_StruggleInfluence extension, VoreRole role, int degree) => extension.StruggleChanceMultiplierTraits(role, degree);
                InfluenceWithTraits(ref successChance, ref explanation, valueRetrieval);
            }
            void InfluenceStruggleChanceWithQuirks()
            {
                InfluenceWithQuirks(ref successChance, ref explanation, "StruggleChanceMultiplierAsPrey", "StruggleChanceMultiplierAsPredator");
            }
            void InfluenceStruggleChanceWithBodySize()
            {
                InfluenceWithBodySize(ref successChance, ref explanation);
            }
            void InfluenceStruggleChanceWithInjuries()
            {
                InfluenceWithInjuries(ref successChance, ref explanation, record.Prey);
            }
        }

        private void EjectIfPossible()
        {
            bool shouldEject = currentStruggles >= requiredStruggles
                && record.CanEject;
            if(shouldEject)
                FinishStruggling();
        }

        private void FinishStruggling()
        {
            if(RV2Log.ShouldLog(false, "Struggling"))
                RV2Log.Message($"Prey {record.Prey.LabelShort} has reached the required amount of successful struggles and broke free", false, "Struggling");
            string message = "RV2_Message_StrugglingBrokeFree".Translate(record.Prey.LabelShortCap, record.Predator.LabelShortCap);
            NotificationUtility.DoNotification(RV2Mod.Settings.fineTuning.StruggleBreakFreeNotification, message);
            record.IsInterrupted = true;
            record.Predator.PawnData().VoreTracker.Eject(record, record.Prey);
            FinishStruggling_Memories();
            FinishStruggling_Interactions();
        }
        private void FinishStruggling_Memories()
        {
            MemoryThoughtHandler predatorMemories = record.Predator.needs?.mood?.thoughts?.memories;
            if(predatorMemories != null)
            {
                predatorMemories.TryGainMemory(VoreThoughtDefOf.RV2_SuccessfulStruggle_Predator, record.Prey);
            }
            MemoryThoughtHandler preyMemories = record.Prey.needs?.mood?.thoughts?.memories;
            if(preyMemories != null)
            {
                preyMemories.TryGainMemory(VoreThoughtDefOf.RV2_SuccessfulStruggle_Prey, record.Predator);
            }
        }
        private void FinishStruggling_Interactions()
        {
            PlayLogEntry_Interaction interaction = new PlayLogEntry_Interaction(VoreInteractionDefOf.RV2_SuccessfulStruggle, record.Prey, record.Predator, new List<RulePackDef>());
            Find.PlayLog.Add(interaction);
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref record, "record");
            Scribe_Values.Look(ref shouldStruggle, "shouldStruggle");
            Scribe_Values.Look(ref requiredStruggles, "requiredStruggles");
            Scribe_Values.Look(ref currentStruggles, "currentStruggles");
        }
    }
}
