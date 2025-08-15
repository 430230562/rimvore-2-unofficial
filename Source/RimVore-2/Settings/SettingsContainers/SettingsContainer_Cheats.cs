using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimVore2
{
    public class SettingsContainer_Cheats : SettingsContainer
    {
        public SettingsContainer_Cheats() { }

        private FloatSmartSetting voreSpeedMultiplier;
        private FloatSmartSetting bodySizeToVoreCapacity;
        private FloatSmartSetting minimumVoreCapacity;
        private FloatSmartSetting baseInitAsPredatorChance;
        private FloatSmartSetting baseStripChance;
        private FloatSmartSetting fallbackNutritionValue;
        private BoolSmartSetting preventStarvingPrey;
        private BoolSmartSetting preventDrainingPredatorFood;
        private BoolSmartSetting canStealFromNullNeed;
        private FloatSmartSetting internalTemperature;
        private FloatSmartSetting acidSavingThrowChance;
        private FloatSmartSetting acidPityHealthFactor;
        private FloatSmartSetting acidImplantDamage;
        private FloatSmartSetting reformedPawnTraitCount;
        private FloatSmartSetting baseProposalAcceptanceChance;
        private FloatSmartSetting maxProposalModifierViaQuirks;
        private FloatSmartSetting proposalModifierPerPreference;
        private FloatSmartSetting maxPawnTraits;
        private BoolSmartSetting traitStealIgnoresLearningFactor;
        private BoolSmartSetting disableFactionImpact;
        private FloatSmartSetting implantDestructionChance;
        private BoolSmartSetting disableMentalStateChecks;
        private BoolSmartSetting allowSelfVoreJumpOnPlayerForcedVore;
        private BoolSmartSetting externalEjectAlwaysSucceeds;
        private BoolSmartSetting allowMultipleExternalEjectAttempts;

        public float VoreSpeedMultiplier => voreSpeedMultiplier.value;
        public float BodySizeToVoreCapacity => bodySizeToVoreCapacity.value;
        public float MinimumVoreCapacity => minimumVoreCapacity.value;
        public float BaseInitAsPredatorChance => baseInitAsPredatorChance.value / 100;
        public float BaseStripChance => baseStripChance.value / 100;
        public float FallbackNutritionValue => fallbackNutritionValue.value;
        public bool PreventStarvingPrey => preventStarvingPrey.value;
        public bool PreventDrainingPredatorFood => preventDrainingPredatorFood.value;
        public bool CanStealFromNullNeed => canStealFromNullNeed.value;
        public float InternalTemperature => internalTemperature.value;
        public float AcidSavingThrowChance => acidSavingThrowChance.value / 100;
        public float AcidPityHealthFactor => acidPityHealthFactor.value / 100;
        public float AcidImplantDamage => acidImplantDamage.value / 100;
        public int ReformedPawnTraitCount => (int)reformedPawnTraitCount.value;
        public float BaseProposalAcceptanceChance => baseProposalAcceptanceChance.value / 100;
        public float MaxProposalModifierViaQuirks => maxProposalModifierViaQuirks.value / 100;
        public float ProposalModifierPerPreference => proposalModifierPerPreference.value / 100;
        public int MaxPawnTraits => (int)maxPawnTraits.value;
        public bool TraitStealIgnoresLearningFactor => traitStealIgnoresLearningFactor.value;
        public bool DisableFactionImpact => disableFactionImpact.value;
        public float ImplantDestructionChance => implantDestructionChance.value / 100;
        public bool DisableMentalStateChecks => disableMentalStateChecks.value;
        public bool AllowSelfVoreJumpOnPlayerForcedVore => allowSelfVoreJumpOnPlayerForcedVore.value;
        public bool ExternalEjectAlwaysSucceeds => externalEjectAlwaysSucceeds.value;
        public bool AllowMultipleExternalEjectAttempts => allowMultipleExternalEjectAttempts.value;

        public override void Reset()
        {
            voreSpeedMultiplier = null;
            bodySizeToVoreCapacity = null;
            minimumVoreCapacity = null;
            baseInitAsPredatorChance = null;
            baseStripChance = null;
            fallbackNutritionValue = null;
            preventStarvingPrey = null;
            preventDrainingPredatorFood = null;
            canStealFromNullNeed = null;
            internalTemperature = null;
            acidSavingThrowChance = null;
            acidPityHealthFactor = null;
            acidImplantDamage = null;
            reformedPawnTraitCount = null;
            baseProposalAcceptanceChance = null;
            maxProposalModifierViaQuirks = null;
            proposalModifierPerPreference = null;
            maxPawnTraits = null;
            traitStealIgnoresLearningFactor = null;
            disableFactionImpact = null;
            implantDestructionChance = null;
            disableMentalStateChecks = null;
            allowSelfVoreJumpOnPlayerForcedVore = null;
            externalEjectAlwaysSucceeds = null;
            allowMultipleExternalEjectAttempts = null;

            EnsureSmartSettingDefinition();
        }

        public override void EnsureSmartSettingDefinition()
        {
            if(voreSpeedMultiplier == null || voreSpeedMultiplier.IsInvalid())
                voreSpeedMultiplier = new FloatSmartSetting("RV2_Settings_Cheats_VoreSpeedMultiplier", 1f, 1f, 0.01f, 20f, "RV2_Settings_Cheats_VoreSpeedMultiplier_Tip");
            if(bodySizeToVoreCapacity == null || bodySizeToVoreCapacity.IsInvalid())
                bodySizeToVoreCapacity = new FloatSmartSetting("RV2_Settings_Cheats_BodySizeToVoreCapacity", 1.5f, 1.5f, 0f, 10f, "RV2_Settings_Cheats_BodySizeToVoreCapacity_Tip");
            if(minimumVoreCapacity == null || minimumVoreCapacity.IsInvalid())
                minimumVoreCapacity = new FloatSmartSetting("RV2_Settings_Cheats_MinimumVoreCapacity", 0f, 0f, 0f, 35f, "RV2_Settings_Cheats_MinimumVoreCapacity_Tip");
            if(baseInitAsPredatorChance == null || baseInitAsPredatorChance.IsInvalid())
                baseInitAsPredatorChance = new FloatSmartSetting("RV2_Settings_Cheats_BaseInitAsPredatorChance", 50f, 50f, 0f, 100f, "RV2_Settings_Cheats_BaseInitAsPredatorChance_Tip", "0", "%");
            if(baseStripChance == null || baseStripChance.IsInvalid())
                baseStripChance = new FloatSmartSetting("RV2_Settings_Cheats_BaseStripChance", 50f, 50f, 0f, 100f, "RV2_Settings_Cheats_BaseStripChance_Tip", "0", "%");
            if(fallbackNutritionValue == null || fallbackNutritionValue.IsInvalid())
                fallbackNutritionValue = new FloatSmartSetting("RV2_Settings_Cheats_FallbackNutritionValue", 3f, 3f, 0f, 50f, "RV2_Settings_Cheats_FallbackNutritionValue_Tip");
            if(preventStarvingPrey == null || preventStarvingPrey.IsInvalid())
                preventStarvingPrey = new BoolSmartSetting("RV2_Settings_Cheats_PreventStarvingPrey", false, false, "RV2_Settings_Cheats_PreventStarvingPrey_Tip");
            if(preventDrainingPredatorFood == null || preventDrainingPredatorFood.IsInvalid())
                preventDrainingPredatorFood = new BoolSmartSetting("RV2_Settings_Cheats_PreventDrainingPredatorFood", false, false, "RV2_Settings_Cheats_PreventDrainingPredatorFood_Tip");
            if(canStealFromNullNeed == null || canStealFromNullNeed.IsInvalid())
                canStealFromNullNeed = new BoolSmartSetting("RV2_Settings_Cheats_CanStealFromNullNeed", false, false, "RV2_Settings_Cheats_CanStealFromNullNeed_Tip");
            if(internalTemperature == null || internalTemperature.IsInvalid())
                internalTemperature = new FloatSmartSetting("RV2_Settings_Cheats_InternalTemperature", 30f, 30f, 0f, 100f, "RV2_Settings_Cheats_InternalTemperature_Tip", "0.00", "°C");
            if(acidSavingThrowChance == null || acidSavingThrowChance.IsInvalid())
                acidSavingThrowChance = new FloatSmartSetting("RV2_Settings_Cheats_AcidSavingThrowChance", 30f, 30f, 0f, 100f, "RV2_Settings_Cheats_AcidSavingThrowChance_Tip", "0", "%");
            if(acidPityHealthFactor == null || acidPityHealthFactor.IsInvalid())
                acidPityHealthFactor = new FloatSmartSetting("RV2_Settings_Cheats_AcidPityHealthFactor", 20f, 20f, 0f, 100f, "RV2_Settings_Cheats_AcidPityHealthFactor_Tip", "0", "%");
            if(acidImplantDamage == null || acidImplantDamage.IsInvalid())
                acidImplantDamage = new FloatSmartSetting("RV2_Settings_Cheats_AcidImplantDamage", 50f, 50f, 0f, 100f, "RV2_Settings_Cheats_AcidImplantDamage_Tip", "0", "%");
            if(reformedPawnTraitCount == null || reformedPawnTraitCount.IsInvalid())
                reformedPawnTraitCount = new FloatSmartSetting("RV2_Settings_Cheats_ReformedPawnTraitCount", 3f, 3f, 0f, 5f, "RV2_Settings_Cheats_ReformedPawnTraitCount_Tip", "0");
            if(baseProposalAcceptanceChance == null || baseProposalAcceptanceChance.IsInvalid())
                baseProposalAcceptanceChance = new FloatSmartSetting("RV2_Settings_Cheats_BaseProposalAcceptanceChance", 50f, 50f, 0f, 100f, "RV2_Settings_Cheats_BaseProposalAcceptanceChance_Tip", "0", "%");
            if(maxProposalModifierViaQuirks == null || maxProposalModifierViaQuirks.IsInvalid())
                maxProposalModifierViaQuirks = new FloatSmartSetting("RV2_Settings_Cheats_MaxProposalModifierViaQuirks", 35f, 35f, 0f, 100f, "RV2_Settings_Cheats_MaxProposalModifierViaQuirks_Tip", "0", "%");
            if(proposalModifierPerPreference == null || proposalModifierPerPreference.IsInvalid())
                proposalModifierPerPreference = new FloatSmartSetting("RV2_Settings_Cheats_ProposalModifierPerPreference", 5f, 5f, 0f, 100f, "RV2_Settings_Cheats_ProposalModifierPerPreference_Tip", "0", "%");
            if(maxPawnTraits == null || maxPawnTraits.IsInvalid())
                maxPawnTraits = new FloatSmartSetting("RV2_Settings_Cheats_MaxPawnTraits", 3, 3, 1, 20, "RV2_Settings_Cheats_MaxPawnTraits_Tip", "0");
            if(traitStealIgnoresLearningFactor == null || traitStealIgnoresLearningFactor.IsInvalid())
                traitStealIgnoresLearningFactor = new BoolSmartSetting("RV2_Settings_Cheats_TraitStealIgnoresLearningFactor", false, false, "RV2_Settings_Cheats_TraitStealIgnoresLearningFactor_Tip");
            if(disableFactionImpact == null || disableFactionImpact.IsInvalid())
                disableFactionImpact = new BoolSmartSetting("RV2_Settings_Cheats_DisableFactionImpact", false, false, "RV2_Settings_Cheats_DisableFactionImpact_Tip");
            if(implantDestructionChance == null || implantDestructionChance.IsInvalid())
                implantDestructionChance = new FloatSmartSetting("RV2_Settings_Cheats_ImplantDestructionChance", 10f, 10f, 0, 100, "RV2_Settings_Cheats_ImplantDestructionChance_Tip", "0", "%");
            if(disableMentalStateChecks == null || disableMentalStateChecks.IsInvalid())
                disableMentalStateChecks  = new BoolSmartSetting("RV2_Settings_Cheats_DisableMentalStateChecks", false, false, "RV2_Settings_Cheats_DisableMentalStateChecks_Tip");
            if(allowSelfVoreJumpOnPlayerForcedVore == null || allowSelfVoreJumpOnPlayerForcedVore.IsInvalid())
                allowSelfVoreJumpOnPlayerForcedVore = new BoolSmartSetting("RV2_Settings_Cheats_AllowSelfVoreJumpOnPlayerForcedVore", false, false, "RV2_Settings_Cheats_AllowSelfVoreJumpOnPlayerForcedVore_Tip");
            if(externalEjectAlwaysSucceeds == null || externalEjectAlwaysSucceeds.IsInvalid())
                externalEjectAlwaysSucceeds = new BoolSmartSetting("RV2_Settings_Cheats_ExternalEjectAlwaysSucceeds", false, false, "RV2_Settings_Cheats_ExternalEjectAlwaysSucceeds_Tip");
            if(allowMultipleExternalEjectAttempts == null || allowMultipleExternalEjectAttempts.IsInvalid())
                allowMultipleExternalEjectAttempts = new BoolSmartSetting("RV2_Settings_Cheats_AllowMultipleExternalEjectAttempts", false, false, "RV2_Settings_Cheats_AllowMultipleExternalEjectAttempts_Tip");
        }

        private bool heightStale = true;
        private float height = 0f;
        private Vector2 scrollPosition;
        public void FillRect(Rect inRect)
        {
            Rect outerRect = inRect;
            UIUtility.MakeAndBeginScrollView(outerRect, height, ref scrollPosition, out Listing_Standard list);

            if(list.ButtonText("RV2_Settings_Reset".Translate()))
                Reset();

            voreSpeedMultiplier.DoSetting(list);
            bodySizeToVoreCapacity.DoSetting(list);
            minimumVoreCapacity.DoSetting(list);
            baseInitAsPredatorChance.DoSetting(list);
            baseStripChance.DoSetting(list);
            fallbackNutritionValue.DoSetting(list);
            preventStarvingPrey.DoSetting(list);
            preventDrainingPredatorFood.DoSetting(list);
            canStealFromNullNeed.DoSetting(list);
            internalTemperature.DoSetting(list);
            acidSavingThrowChance.DoSetting(list);
            if(AcidSavingThrowChance > 0)
            {
                acidPityHealthFactor.DoSetting(list);
            }
            acidImplantDamage.DoSetting(list);
            reformedPawnTraitCount.DoSetting(list);
            baseProposalAcceptanceChance.DoSetting(list);
            maxProposalModifierViaQuirks.DoSetting(list);
            proposalModifierPerPreference.DoSetting(list);
            maxPawnTraits.DoSetting(list);
            traitStealIgnoresLearningFactor.DoSetting(list);
            disableFactionImpact.DoSetting(list);
            implantDestructionChance.DoSetting(list);
            disableMentalStateChecks.DoSetting(list);
            allowSelfVoreJumpOnPlayerForcedVore.DoSetting(list);
            externalEjectAlwaysSucceeds.DoSetting(list);
            if(!ExternalEjectAlwaysSucceeds)
            {
                allowMultipleExternalEjectAttempts.DoSetting(list);
            }

            list.EndScrollView(ref height, ref heightStale);
        }

        public override void ExposeData()
        {
            if(Scribe.mode == LoadSaveMode.Saving || Scribe.mode == LoadSaveMode.LoadingVars)
            {
                EnsureSmartSettingDefinition();
            }
            Scribe_Deep.Look(ref voreSpeedMultiplier, "voreSpeedMultiplier", new object[0]);
            Scribe_Deep.Look(ref bodySizeToVoreCapacity, "bodySizeToVoreCapacity", new object[0]);
            Scribe_Deep.Look(ref minimumVoreCapacity, "minimumVoreCapacity", new object[0]);
            Scribe_Deep.Look(ref baseInitAsPredatorChance, "baseInitAsPredatorChance", new object[0]);
            Scribe_Deep.Look(ref baseStripChance, "baseStripChance", new object[0]);
            Scribe_Deep.Look(ref fallbackNutritionValue, "fallbackNutritionValue", new object[0]);
            Scribe_Deep.Look(ref preventStarvingPrey, "preventStarvingPrey", new object[0]);
            Scribe_Deep.Look(ref preventDrainingPredatorFood, "preventDrainingPredatorFood", new object[0]);
            Scribe_Deep.Look(ref canStealFromNullNeed, "canStealFromNullNeed", new object[0]);
            Scribe_Deep.Look(ref internalTemperature, "internalTemperature", new object[0]);
            Scribe_Deep.Look(ref acidSavingThrowChance, "acidSavingThrowChance", new object[0]);
            Scribe_Deep.Look(ref acidPityHealthFactor, "acidPityHealthFactor", new object[0]);
            Scribe_Deep.Look(ref acidImplantDamage, "acidImplantDamage", new object[0]);
            Scribe_Deep.Look(ref reformedPawnTraitCount, "reformedPawnTraitCount", new object[0]);
            Scribe_Deep.Look(ref baseProposalAcceptanceChance, "baseProposalAcceptanceChance", new object[0]);
            Scribe_Deep.Look(ref maxProposalModifierViaQuirks, "maxProposalModifierViaQuirks", new object[0]);
            Scribe_Deep.Look(ref proposalModifierPerPreference, "proposalModifierPerPreference", new object[0]);
            Scribe_Deep.Look(ref maxPawnTraits, "maxPawnTraits", new object[0]);
            Scribe_Deep.Look(ref traitStealIgnoresLearningFactor, "traitStealIgnoresLearningFactor", new object[0]);
            Scribe_Deep.Look(ref disableFactionImpact, "disableFactionImpact", new object[0]);
            Scribe_Deep.Look(ref implantDestructionChance, "implantDestructionChance", new object[0]);
            Scribe_Deep.Look(ref disableMentalStateChecks, "disableMentalStateChecks", new object[0]);
            Scribe_Deep.Look(ref allowSelfVoreJumpOnPlayerForcedVore, "allowSelfVoreJumpOnPlayerForcedVore", new object[0]);
            Scribe_Deep.Look(ref externalEjectAlwaysSucceeds, "externalEjectAlwaysSucceeds", new object[0]);
            Scribe_Deep.Look(ref allowMultipleExternalEjectAttempts, "allowMultipleExternalEjectAttempts", new object[0]);

            PostExposeData();
        }
    }
}
