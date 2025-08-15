using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimVore2
{
    public class SettingsContainer_FineTuning : SettingsContainer
    {
        public SettingsContainer_FineTuning() { }

        private BoolSmartSetting showInvalidRMBOptions;
        private BoolSmartSetting forbidDigestedCorpse;
        private EnumSmartSetting<DefaultStripSetting> defaultStripBehaviour;
        private BoolSmartSetting autoHaulVoreContainer;
        private BoolSmartSetting autoOpenVoreContainer;
        private BoolSmartSetting forbidNonFactionVoreContainer;
        private BoolSmartSetting reformHumanoidsAsNewborn;
        private BoolSmartSetting skipWarmupWhenAlreadyDigesting; // ESEGN
        private BoolSmartSetting manhuntersVoreDownedPawns;
        private FloatSmartSetting huntingAnimalsVoreChance;
        private FloatSmartSetting failedTameVoreChance;
        private FloatSmartSetting raiderVorenappingChance;
        private FloatSmartSetting minVoreProposalCooldown;
        private FloatSmartSetting maxVoreProposalCooldown;
        private FloatSmartSetting proposalTimeTicks;
        private FloatSmartSetting forcedVoreChanceOnFailedProposal;
        private BoolSmartSetting prisonersMustConsentToProposal;
        private BoolSmartSetting slavesMustConsentToProposal;
        private BoolSmartSetting autoAcceptAnimal;
        private FloatSmartSetting autoAcceptAnimalSkill;
        private BoolSmartSetting autoAcceptSocial;
        private FloatSmartSetting autoAcceptSocialSkillDifference;
        private FloatSmartSetting feederVoreMinPrey;
        private FloatSmartSetting feederVoreMaxPrey;
        private BoolSmartSetting unwrapOnDisposalContainerRelease;
        private BoolSmartSetting mentalBreaksUseAutoVoreRules;
        private BoolSmartSetting strugglingCreatesInteractions;
        private BoolSmartSetting useAlternativeMainTabIcon;
        private FloatSmartSetting ejectStunDuration;
        private BoolSmartSetting blockProposalInitiationDuringWorkTime;
        private BoolSmartSetting allowVisitorsAsAutoProposalTargets;
        private BoolSmartSetting allowWildAnimalsAsAutoProposalTargets;
        private BoolSmartSetting predatorAnimalsOnlyUseDigestGoal;
        private FloatSmartSetting proposalRange;

        private StruggleForceMode previousStruggleForceMode = StruggleForceMode.Invalid;    // just used for value comparison
        private EnumSmartSetting<StruggleForceMode> defaultStruggleForceMode;
        private EnumSmartSetting<NotificationType> proposalDeniedNotification;
        private EnumSmartSetting<NotificationType> proposalAcceptedNotification;
        private EnumSmartSetting<NotificationType> fatalProposalAcceptedNotification;
        private EnumSmartSetting<NotificationType> struggleBreakFreeNotification;
        private EnumSmartSetting<NotificationType> goalSwitchNotification;
        private EnumSmartSetting<NotificationType> failedTameVoreNotification;

        public bool ShowInvalidRMBOptions => showInvalidRMBOptions.value;
        public bool ForbidDigestedCorpse => forbidDigestedCorpse.value;
        public DefaultStripSetting DefaultStripBehaviour => defaultStripBehaviour.value;
        public bool AutoHaulVoreContainer => autoHaulVoreContainer.value;
        public bool AutoOpenVoreContainer => autoOpenVoreContainer.value;
        public bool ForbidNonFactionVoreContainer => forbidNonFactionVoreContainer.value;
        public bool ReformHumanoidsAsNewborn => reformHumanoidsAsNewborn.value;
        public bool SkipWarmupWhenAlreadyDigesting => skipWarmupWhenAlreadyDigesting.value; // ESEGN
        public bool ManhuntersVoreDownedPawns => manhuntersVoreDownedPawns.value;
        public float HuntingAnimalsVoreChance => huntingAnimalsVoreChance.value / 100;
        public float FailedTameVoreChance => failedTameVoreChance.value / 100;
        public float RaiderVorenappingChance => raiderVorenappingChance.value / 100;
        public int MinVoreProposalCooldown => (int)minVoreProposalCooldown.value;
        public int MaxVoreProposalCooldown => (int)maxVoreProposalCooldown.value;
        public IntRange VoreProposalCooldownRange => new IntRange(MinVoreProposalCooldown, MaxVoreProposalCooldown);
        public int ProposalTimeTicks => (int)proposalTimeTicks.value;
        public float ForcedVoreChanceOnFailedProposal => forcedVoreChanceOnFailedProposal.value / 100;
        public bool PrisonersMustConsentToProposal => prisonersMustConsentToProposal.value;
        public bool SlavesMustConsentToProposal => slavesMustConsentToProposal.value;
        public bool AutoAcceptAnimal => autoAcceptAnimal.value;
        public int AutoAcceptAnimalSkill => Mathf.RoundToInt(autoAcceptAnimalSkill.value);
        public bool AutoAcceptSocial => autoAcceptSocial.value;
        public int AutoAcceptSocialSkillDifference => Mathf.RoundToInt(autoAcceptSocialSkillDifference.value);
        public IntRange FeederVorePreyCount => new IntRange((int)feederVoreMinPrey.value, (int)feederVoreMaxPrey.value);
        public bool UnwrapOnDisposalContainerRelease => unwrapOnDisposalContainerRelease.value;
        public bool MentalBreaksUseAutoVoreRules => mentalBreaksUseAutoVoreRules.value;
        public bool StrugglingCreatesInteractions => strugglingCreatesInteractions.value;
        public bool UseAlternativeMainTabIcon => useAlternativeMainTabIcon.value;
        public int EjectStunDuration => (int)ejectStunDuration.value;
        public bool BlockProposalInitiationDuringWorkTime => blockProposalInitiationDuringWorkTime.value;
        public bool AllowVisitorsAsAutoProposalTargets => allowVisitorsAsAutoProposalTargets.value;
        public bool AllowWildAnimalsAsAutoProposalTargets => allowWildAnimalsAsAutoProposalTargets.value;
        public bool PredatorAnimalsOnlyUseDigestGoal => predatorAnimalsOnlyUseDigestGoal.value;
        public int ProposalRange => Mathf.RoundToInt(proposalRange.value);

        public StruggleForceMode DefaultStruggleForceMode => defaultStruggleForceMode.value;
        public NotificationType ProposalDeniedNotification => proposalDeniedNotification.value;
        public NotificationType ProposalAcceptedNotification => proposalAcceptedNotification.value;
        public NotificationType FatalProposalAcceptedNotification => fatalProposalAcceptedNotification.value;
        public NotificationType StruggleBreakFreeNotification => struggleBreakFreeNotification.value;
        public NotificationType GoalSwitchNotification => goalSwitchNotification.value;
        public NotificationType FailedTameVoreNotification => failedTameVoreNotification.value;

        public override void EnsureSmartSettingDefinition()
        {
            if(showInvalidRMBOptions == null || showInvalidRMBOptions.IsInvalid())
                showInvalidRMBOptions = new BoolSmartSetting("RV2_Settings_General_DisplayUnavailableRMBOptions", true, true, "RV2_Settings_General_DisplayUnavailableRMBOptions_Tip");
            if(forbidDigestedCorpse == null || forbidDigestedCorpse.IsInvalid())
                forbidDigestedCorpse = new BoolSmartSetting("RV2_Settings_FineTuning_ForbidDigestedCorpse", false, false, "RV2_Settings_FineTuning_ForbidDigestedCorpse_Tip");
            if(defaultStripBehaviour == null || defaultStripBehaviour.IsInvalid())
                defaultStripBehaviour = new EnumSmartSetting<DefaultStripSetting>("RV2_Settings_FineTuning_DefaultStripBehaviour", DefaultStripSetting.Random, DefaultStripSetting.Random, "RV2_Settings_FineTuning_DefaultStripBehaviour_Tip");
            if(autoHaulVoreContainer == null || autoHaulVoreContainer.IsInvalid())
                autoHaulVoreContainer = new BoolSmartSetting("RV2_Settings_FineTuning_AutoHaulVoreContainer", true, true, "RV2_Settings_FineTuning_AutoHaulVoreContainer_Tip");
            if(autoOpenVoreContainer == null || autoOpenVoreContainer.IsInvalid())
                autoOpenVoreContainer = new BoolSmartSetting("RV2_Settings_FineTuning_AutoOpenVoreContainer", false, false, "RV2_Settings_FineTuning_AutoOpenVoreContainer_Tip");
            if(forbidNonFactionVoreContainer == null || forbidNonFactionVoreContainer.IsInvalid())
                forbidNonFactionVoreContainer = new BoolSmartSetting("RV2_Settings_FineTuning_ForbidNonFactionVoreContainer", false, false, "RV2_Settings_FineTuning_ForbidNonFactionVoreContainer_Tip");
            if(reformHumanoidsAsNewborn == null || reformHumanoidsAsNewborn.IsInvalid())
                reformHumanoidsAsNewborn = new BoolSmartSetting("RV2_Settings_FineTuning_ReformHumanoidsAsNewborn", true, true, "RV2_Settings_FineTuning_ReformHumanoidsAsNewborn_Tip");
            // Next added by ESEGN
            if(skipWarmupWhenAlreadyDigesting == null || skipWarmupWhenAlreadyDigesting.IsInvalid())
                skipWarmupWhenAlreadyDigesting = new BoolSmartSetting("RV2_Settings_FineTuning_SkipWarmupWhenAlreadyDigesting", false, false, "RV2_Settings_FineTuning_SkipWarmupWhenAlreadyDigesting_Tip");
            if(manhuntersVoreDownedPawns == null || manhuntersVoreDownedPawns.IsInvalid())
                manhuntersVoreDownedPawns = new BoolSmartSetting("RV2_Settings_FineTuning_ManhuntersVoreDownedPawns", true, true, "RV2_Settings_FineTuning_ManhuntersVoreDownedPawns_Tip");
            if(huntingAnimalsVoreChance == null || huntingAnimalsVoreChance.IsInvalid())
                huntingAnimalsVoreChance = new FloatSmartSetting("RV2_Settings_FineTuning_HuntingAnimalsVoreChance", 50f, 50f, 0f, 100f, "RV2_Settings_FineTuning_HuntingAnimalsVoreChance_Tip", "0", "%");
            if(failedTameVoreChance == null || failedTameVoreChance.IsInvalid())
                failedTameVoreChance = new FloatSmartSetting("RV2_Settings_FineTuning_FailedTameVoreChance", 30f, 30f, 0f, 100f, "RV2_Settings_FineTuning_FailedTameVoreChance_Tip", "0", "%");
            if(raiderVorenappingChance == null || raiderVorenappingChance.IsInvalid())
                raiderVorenappingChance = new FloatSmartSetting("RV2_Settings_FineTuning_RaiderVorenappingChance", 50f, 50f, 0f, 100f, "RV2_Settings_FineTuning_RaiderVorenappingChance_Tip", "0", "%");
            if(minVoreProposalCooldown == null || minVoreProposalCooldown.IsInvalid())
                minVoreProposalCooldown = new FloatSmartSetting("RV2_Settings_FineTuning_MinVoreProposalCooldown", 20000, 20000, 2500, 0, "RV2_Settings_FineTuning_VoreProposalCooldown_Tip", "0");   // maxvalue determined by other setting
            if(maxVoreProposalCooldown == null || maxVoreProposalCooldown.IsInvalid())
                maxVoreProposalCooldown = new FloatSmartSetting("RV2_Settings_FineTuning_MaxVoreProposalCooldown", 30000, 30000, 0, 1000000, "RV2_Settings_FineTuning_VoreProposalCooldown_Tip", "0"); // minvalue determined by other setting
            if(proposalTimeTicks == null || proposalTimeTicks.IsInvalid())
                proposalTimeTicks = new FloatSmartSetting("RV2_Settings_FineTuning_ProposalTimeTicks", 300f, 300f, 1f, 1200f, "RV2_Settings_FineTuning_ProposalTimeTicks_Tip", "0");
            if(forcedVoreChanceOnFailedProposal == null || forcedVoreChanceOnFailedProposal.IsInvalid())
                forcedVoreChanceOnFailedProposal = new FloatSmartSetting("RV2_Settings_FineTuning_ForcedVoreChanceOnFailedProposal", 10f, 10f, 0f, 100f, "RV2_Settings_FineTuning_ForcedVoreChanceOnFailedProposal", "0", "%");
            if(prisonersMustConsentToProposal == null || prisonersMustConsentToProposal.IsInvalid())
                prisonersMustConsentToProposal = new BoolSmartSetting("RV2_Settings_FineTuning_PrisonersMustConsentToProposal", false, false, "RV2_Settings_FineTuning_PrisonersMustConsentToProposal_Tip");
            if(slavesMustConsentToProposal == null || slavesMustConsentToProposal.IsInvalid())
                slavesMustConsentToProposal = new BoolSmartSetting("RV2_Settings_FineTuning_SlavesMustConsentToProposal", false, false, "RV2_Settings_FineTuning_SlavesMustConsentToProposal_Tip");
            if(autoAcceptAnimal == null || autoAcceptAnimal.IsInvalid())
                autoAcceptAnimal = new BoolSmartSetting("RV2_Settings_FineTuning_AutoAcceptAnimal", true, true, "RV2_Settings_FineTuning_AutoAcceptAnimal_Tip");
            if(autoAcceptAnimalSkill == null || autoAcceptAnimalSkill.IsInvalid())
                autoAcceptAnimalSkill = new FloatSmartSetting("RV2_Settings_FineTuning_AutoAcceptAnimal", 15f, 15f, 1f, 20f, "RV2_Settings_FineTuning_AutoAcceptAnimal_Tip", "0");
            if(autoAcceptSocial == null || autoAcceptSocial.IsInvalid())
                autoAcceptSocial = new BoolSmartSetting("RV2_Settings_FineTuning_AutoAcceptSocial", true, true, "RV2_Settings_FineTuning_AutoAcceptSocial_Tip");
            if(autoAcceptSocialSkillDifference == null || autoAcceptSocialSkillDifference.IsInvalid())
                autoAcceptSocialSkillDifference = new FloatSmartSetting("RV2_Settings_FineTuning_AutoAcceptSocialSkillDifference", 12f, 12f, 1f, 20f, "RV2_Settings_FineTuning_AutoAcceptSocialSkillDifference_Tip", "0");
            if(feederVoreMinPrey == null || feederVoreMinPrey.IsInvalid())
                feederVoreMinPrey = new FloatSmartSetting("RV2_Settings_FineTuning_FeederVoreMinPrey", 1f, 1f, 1f, 20f, "RV2_Settings_FineTuning_FeederVoreMinPrey_Tip", "0");
            if(feederVoreMaxPrey == null || feederVoreMaxPrey.IsInvalid())
                feederVoreMaxPrey = new FloatSmartSetting("RV2_Settings_FineTuning_FeederVoreMaxPrey", 3f, 3f, 1f, 20f, "RV2_Settings_FineTuning_FeederVoreMaxPrey_Tip", "0");
            if(unwrapOnDisposalContainerRelease == null || unwrapOnDisposalContainerRelease.IsInvalid())
                unwrapOnDisposalContainerRelease = new BoolSmartSetting("RV2_Settings_FineTuning_UnwrapOnDisposalContainerRelease", true, true, "RV2_Settings_FineTuning_UnwrapOnDisposalContainerRelease_Tip");
            if(mentalBreaksUseAutoVoreRules == null || mentalBreaksUseAutoVoreRules.IsInvalid())
                mentalBreaksUseAutoVoreRules = new BoolSmartSetting("RV2_Settings_FineTuning_MentalBreaksUseAutoVoreRules", true, true, "RV2_Settings_FineTuning_MentalBreaksUseAutoVoreRules_Tip");
            if(ejectStunDuration == null || ejectStunDuration.IsInvalid())
                ejectStunDuration = new FloatSmartSetting("RV2_Settings_FineTuning_EjectStunDuration", 500, 500, 0, 10000, "RV2_Settings_FineTuning_EjectStunDuration_Tip", "0");
            if(blockProposalInitiationDuringWorkTime == null || blockProposalInitiationDuringWorkTime.IsInvalid())
                blockProposalInitiationDuringWorkTime = new BoolSmartSetting("RV2_Settings_FineTuning_BlockProposalInitiationDuringWorkTime", false, false, "RV2_Settings_FineTuning_BlockProposalInitiationDuringWorkTime_Tip");
            if(allowVisitorsAsAutoProposalTargets == null || allowVisitorsAsAutoProposalTargets.IsInvalid())
                allowVisitorsAsAutoProposalTargets = new BoolSmartSetting("RV2_Settings_FineTuning_AllowVisitorsAsAutoProposalTargets", false, false, "RV2_Settings_FineTuning_AllowVisitorsAsAutoProposalTargets_Tip");
            if(allowWildAnimalsAsAutoProposalTargets == null || allowWildAnimalsAsAutoProposalTargets.IsInvalid())
                allowWildAnimalsAsAutoProposalTargets = new BoolSmartSetting("RV2_Settings_FineTuning_AllowWildAnimalAsAutoProposalTargets", false, false, "RV2_Settings_FineTuning_AllowWildAnimalAsAutoProposalTargets_Tip");
            if(predatorAnimalsOnlyUseDigestGoal == null || predatorAnimalsOnlyUseDigestGoal.IsInvalid())
                predatorAnimalsOnlyUseDigestGoal = new BoolSmartSetting("RV2_Settings_FineTuning_PredatorAnimalsOnlyUseDigestGoal", true, true, "RV2_Settings_FineTuning_PredatorAnimalsOnlyUseDigestGoal_Tip");
            if(proposalRange == null || proposalRange.IsInvalid())
                proposalRange = new FloatSmartSetting("RV2_Settings_FineTuning_ProposalRange", 50, 50, 5, 999, "RV2_Settings_FineTuning_ProposalRange_Tip", "0", "tiles");

            if(proposalDeniedNotification == null || proposalDeniedNotification.IsInvalid())
                proposalDeniedNotification = new EnumSmartSetting<NotificationType>("RV2_Settings_FineTuning_ProposalDeniedNotification", NotificationType.MessageThreatSmall, NotificationType.MessageThreatSmall);
            if(proposalAcceptedNotification == null || proposalAcceptedNotification.IsInvalid())
                proposalAcceptedNotification = new EnumSmartSetting<NotificationType>("RV2_Settings_FineTuning_ProposalAcceptedNotification", NotificationType.MessageThreatSmall, NotificationType.MessageThreatSmall);
            if(fatalProposalAcceptedNotification == null || fatalProposalAcceptedNotification.IsInvalid())
                fatalProposalAcceptedNotification = new EnumSmartSetting<NotificationType>("RV2_Settings_FineTuning_FatalProposalAcceptedNotification", NotificationType.MessageThreatSmall, NotificationType.MessageThreatSmall);
            if(defaultStruggleForceMode == null || defaultStruggleForceMode.IsInvalid())
                defaultStruggleForceMode = new EnumSmartSetting<StruggleForceMode>("RV2_Settings_FineTuning_DefaultStruggleForceMode", StruggleForceMode.WhenForced, StruggleForceMode.WhenForced, "RV2_Settings_FineTuning_DefaultStruggleForceMode_Tip");
            if(struggleBreakFreeNotification == null || struggleBreakFreeNotification.IsInvalid())
                struggleBreakFreeNotification = new EnumSmartSetting<NotificationType>("RV2_Settings_FineTuning_StruggleBreakFreeNotification", NotificationType.MessageNeutral, NotificationType.MessageNeutral);
            if(strugglingCreatesInteractions == null || strugglingCreatesInteractions.IsInvalid())
                strugglingCreatesInteractions = new BoolSmartSetting("RV2_Settings_FineTuning_StrugglingCreatesInteractions", true, true, "RV2_Settings_FineTuning_StrugglingCreatesInteractions_Tip");
            if(useAlternativeMainTabIcon == null || useAlternativeMainTabIcon.IsInvalid())
                useAlternativeMainTabIcon = new BoolSmartSetting("RV2_Settings_FineTuning_UseAlternativeMainVoreIcon", false, false, "RV2_Settings_FineTuning_UseAlternativeMainVoreIcon_Tip");
            if(goalSwitchNotification == null || goalSwitchNotification.IsInvalid())
                goalSwitchNotification = new EnumSmartSetting<NotificationType>("RV2_Settings_FineTuning_GoalSwitchNotification", NotificationType.MessageNeutral, NotificationType.MessageNeutral);
            if(failedTameVoreNotification == null || failedTameVoreNotification.IsInvalid())
                failedTameVoreNotification = new EnumSmartSetting<NotificationType>("RV2_Settings_FineTuning_FailedTameVoreNotification", NotificationType.MessageNeutral, NotificationType.MessageNeutral);
        }

        public override void Reset()
        {
            showInvalidRMBOptions = null;
            forbidDigestedCorpse = null;
            defaultStripBehaviour = null;
            autoHaulVoreContainer = null;
            autoOpenVoreContainer = null;
            forbidNonFactionVoreContainer = null;
            reformHumanoidsAsNewborn = null;
            skipWarmupWhenAlreadyDigesting = null; // ESEGN
            manhuntersVoreDownedPawns = null;
            huntingAnimalsVoreChance = null;
            failedTameVoreChance = null;
            raiderVorenappingChance = null;
            minVoreProposalCooldown = null;
            maxVoreProposalCooldown = null;
            proposalTimeTicks = null;
            forcedVoreChanceOnFailedProposal = null;
            prisonersMustConsentToProposal = null;
            slavesMustConsentToProposal = null;
            autoAcceptAnimal = null;
            autoAcceptAnimalSkill = null;
            autoAcceptSocial = null;
            autoAcceptSocialSkillDifference = null;
            feederVoreMinPrey = null;
            feederVoreMaxPrey = null;
            proposalDeniedNotification = null;
            proposalAcceptedNotification = null;
            fatalProposalAcceptedNotification = null;
            unwrapOnDisposalContainerRelease = null;
            mentalBreaksUseAutoVoreRules = null;
            defaultStruggleForceMode = null;
            struggleBreakFreeNotification = null;
            strugglingCreatesInteractions = null;
            useAlternativeMainTabIcon = null;
            goalSwitchNotification = null;
            failedTameVoreNotification = null;
            ejectStunDuration = null;
            blockProposalInitiationDuringWorkTime = null;
            allowVisitorsAsAutoProposalTargets = null;
            allowWildAnimalsAsAutoProposalTargets = null;
            predatorAnimalsOnlyUseDigestGoal = null;
            proposalRange = null;

            EnsureSmartSettingDefinition();
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

            list.Gap();

            list.HeaderLabel("RV2_Settings_FineTuning_Header_VoreProposals".Translate());
            minVoreProposalCooldown.OverrideMaxValue(MaxVoreProposalCooldown - 1);
            minVoreProposalCooldown.DoSetting(list);
            maxVoreProposalCooldown.OverrideMinValue(MinVoreProposalCooldown + 1);
            maxVoreProposalCooldown.DoSetting(list);
            ExplainVoreProposalCooldown(list);
            proposalTimeTicks.DoSetting(list);
            blockProposalInitiationDuringWorkTime.DoSetting(list);
            proposalDeniedNotification.DoSetting(list);
            proposalAcceptedNotification.DoSetting(list);
            fatalProposalAcceptedNotification.DoSetting(list);
            forcedVoreChanceOnFailedProposal.DoSetting(list);
            prisonersMustConsentToProposal.DoSetting(list);
            slavesMustConsentToProposal.DoSetting(list);
            autoAcceptAnimal.DoSetting(list);
            if(AutoAcceptAnimal)
                autoAcceptAnimalSkill.DoSetting(list);
            autoAcceptSocial.DoSetting(list);
            if(AutoAcceptSocial)
                autoAcceptSocialSkillDifference.DoSetting(list);

            list.Gap();

            list.HeaderLabel("RV2_Settings_FineTuning_Header_AutoVore".Translate());
            manhuntersVoreDownedPawns.DoSetting(list);
            huntingAnimalsVoreChance.DoSetting(list);
            predatorAnimalsOnlyUseDigestGoal.DoSetting(list);
            failedTameVoreChance.DoSetting(list);
            raiderVorenappingChance.DoSetting(list);
            mentalBreaksUseAutoVoreRules.DoSetting(list);
            if(FailedTameVoreChance > 0)
            {
                failedTameVoreNotification.DoSetting(list);
            }
            allowVisitorsAsAutoProposalTargets.DoSetting(list);
            allowWildAnimalsAsAutoProposalTargets.DoSetting(list);
            proposalRange.DoSetting(list);

            list.Gap();

            if(RV2Mod.Settings.features.FeederVoreEnabled)
            {
                list.HeaderLabel("RV2_Settings_FineTuning_Header_Feeding".Translate());
                feederVoreMinPrey.DoSetting(list);
                feederVoreMaxPrey.DoSetting(list);

                list.Gap();
            }

            list.HeaderLabel("RV2_Settings_FineTuning_Header_Misc".Translate());
            if(RV2Mod.Settings.features.ShowRMBVoreMenu)
            {
                showInvalidRMBOptions.DoSetting(list);
            }
            forbidDigestedCorpse.DoSetting(list);
            defaultStripBehaviour.DoSetting(list);
            autoHaulVoreContainer.DoSetting(list);
            autoOpenVoreContainer.DoSetting(list);
            forbidNonFactionVoreContainer.DoSetting(list);
            reformHumanoidsAsNewborn.DoSetting(list);
            skipWarmupWhenAlreadyDigesting.DoSetting(list); // ESEGN
            unwrapOnDisposalContainerRelease.DoSetting(list);
            ejectStunDuration.DoSetting(list);
            goalSwitchNotification.DoSetting(list);

            list.Gap();

            if(RV2Mod.Settings.features.StrugglingEnabled)
            {
                list.HeaderLabel("RV2_Settings_FineTuning_Header_Struggling".Translate());
                defaultStruggleForceMode.DoSetting(list);
                strugglingCreatesInteractions.DoSetting(list);
                // if the settings change, prompt the user to update underlying values
                StruggleForceMode currentStruggleForceMode = DefaultStruggleForceMode;
                if(previousStruggleForceMode == StruggleForceMode.Invalid)
                    previousStruggleForceMode = currentStruggleForceMode;
                if(previousStruggleForceMode != currentStruggleForceMode)
                {
                    PromptDefaultStruggleForceModeChanged(previousStruggleForceMode, currentStruggleForceMode);
                    previousStruggleForceMode = currentStruggleForceMode;
                }
                struggleBreakFreeNotification.DoSetting(list);

                list.Gap();
            }

            useAlternativeMainTabIcon.DoSetting(list);

            list.EndScrollView(ref height, ref heightStale);
        }

        private void ExplainVoreProposalCooldown(Listing_Standard list)
        {
            string label = "-> " + "RV2_Settings_FineTuning_ExplainedProposalCooldown".Translate(LogUtility.PresentIngameTime(MinVoreProposalCooldown), LogUtility.PresentIngameTime(MaxVoreProposalCooldown));
            string hint = "RV2_Settings_FineTuning_ExplainedProposalCooldown_Tip".Translate(LogUtility.PresentRealTime(MinVoreProposalCooldown), LogUtility.PresentRealTime(MaxVoreProposalCooldown));

            list.Label(new TaggedString(label), -1, hint);
        }

        private void PromptDefaultStruggleForceModeChanged(StruggleForceMode previousMode, StruggleForceMode newMode)
        {
            string text = "RV2_Settings_ChangePrompt_DefaultStruggleForceMode".Translate(UIUtility.EnumPresenter(previousMode.ToString()), UIUtility.EnumPresenter(newMode.ToString()));
            Action confirmAction = () =>
            {
                UpdateDefaultStruggleForceMode(previousMode, newMode);
            };
            Dialog_MessageBox dialog = new Dialog_MessageBox(text, "RV2_Settings_Rules_OK".Translate(), confirmAction, "RV2_Settings_Rules_Cancel".Translate(), () => { });
            Find.WindowStack.Add(dialog);
        }
        private void UpdateDefaultStruggleForceMode(StruggleForceMode previousMode, StruggleForceMode newMode)
        {
            RV2Mod.Settings.rules.Rules  // get all the rules
                .Select(entry => entry.Rule)
                .SelectMany(rule => rule.pathRules.Values)  // get all the vore path rules of all rules
                .ForEach(pathRule =>    // update each path rule
                {
                    pathRule.UpdateStruggleMode(previousMode, newMode);
                });
        }

        public override void DefsLoaded()
        {
            base.DefsLoaded();
            defaultStripBehaviour.valuePresenter = (DefaultStripSetting s) => s == DefaultStripSetting.AlwaysStrip ? "RV2_Settings_General_StrippingAlways".Translate() : s == DefaultStripSetting.NeverStrip ? "RV2_Settings_General_StrippingNever".Translate() : "RV2_Settings_General_StrippingRandom".Translate();

            // these are somewhat pointless now, but they allow translators to provide their own values. The new system of adding spaces before every upper case will force english strings
            proposalDeniedNotification.valuePresenter = RV2_Common.NotificationPresenter;
            proposalAcceptedNotification.valuePresenter = RV2_Common.NotificationPresenter;
            fatalProposalAcceptedNotification.valuePresenter = RV2_Common.NotificationPresenter;
            struggleBreakFreeNotification.valuePresenter = RV2_Common.NotificationPresenter;
            goalSwitchNotification.valuePresenter = RV2_Common.NotificationPresenter;
        }

        public override void ExposeData()
        {
            if(Scribe.mode == LoadSaveMode.Saving || Scribe.mode == LoadSaveMode.LoadingVars)
            {
                EnsureSmartSettingDefinition();
            }
            Scribe_Deep.Look(ref showInvalidRMBOptions, "showInvalidRMBOptions", new object[0]);
            Scribe_Deep.Look(ref forbidDigestedCorpse, "forbidDigestedCorpse", new object[0]);
            Scribe_Deep.Look(ref defaultStripBehaviour, "defaultStripBehaviour", new object[0]);
            Scribe_Deep.Look(ref autoHaulVoreContainer, "autoHaulVoreContainer", new object[0]);
            Scribe_Deep.Look(ref autoOpenVoreContainer, "autoOpenVoreContainer", new object[0]);
            Scribe_Deep.Look(ref forbidNonFactionVoreContainer, "forbidNonFactionVoreContainer", new object[0]);
            Scribe_Deep.Look(ref reformHumanoidsAsNewborn, "reformHumanoidsAsNewborn", new object[0]);
            Scribe_Deep.Look(ref skipWarmupWhenAlreadyDigesting, "skipWarmupWhenAlreadyDigesting", new object[0]); // ESEGN
            Scribe_Deep.Look(ref manhuntersVoreDownedPawns, "manhuntersVoreDownedPawns", new object[0]);
            Scribe_Deep.Look(ref huntingAnimalsVoreChance, "huntingAnimalsVoreChance", new object[0]);
            Scribe_Deep.Look(ref failedTameVoreChance, "failedTameVoreChance", new object[0]);
            Scribe_Deep.Look(ref raiderVorenappingChance, "raiderVorenappingChance", new object[0]);
            Scribe_Deep.Look(ref minVoreProposalCooldown, "minVoreProposalCooldown", new object[0]);
            Scribe_Deep.Look(ref maxVoreProposalCooldown, "maxVoreProposalCooldown", new object[0]);
            Scribe_Deep.Look(ref proposalTimeTicks, "proposalTimeTicks", new object[0]);
            Scribe_Deep.Look(ref forcedVoreChanceOnFailedProposal, "forcedVoreChanceOnFailedProposal", new object[0]);
            Scribe_Deep.Look(ref prisonersMustConsentToProposal, "prisonersMustConsentToProposal", new object[0]);
            Scribe_Deep.Look(ref slavesMustConsentToProposal, "slavesMustConsentToProposal", new object[0]);
            Scribe_Deep.Look(ref autoAcceptAnimal, "autoAcceptAnimal", new object[0]);
            Scribe_Deep.Look(ref autoAcceptAnimalSkill, "autoAcceptAnimalSkill", new object[0]);
            Scribe_Deep.Look(ref autoAcceptSocial, "autoAcceptSocial", new object[0]);
            Scribe_Deep.Look(ref autoAcceptSocialSkillDifference, "autoAcceptSocialSkillDifference", new object[0]);
            Scribe_Deep.Look(ref feederVoreMinPrey, "feederVoreMinPrey", new object[0]);
            Scribe_Deep.Look(ref feederVoreMaxPrey, "feederVoreMaxPrey", new object[0]);
            Scribe_Deep.Look(ref proposalDeniedNotification, "proposalDeniedNotification", new object[0]);
            Scribe_Deep.Look(ref proposalAcceptedNotification, "proposalAcceptedNotification", new object[0]);
            Scribe_Deep.Look(ref fatalProposalAcceptedNotification, "fatalProposalAcceptedNotification", new object[0]);
            Scribe_Deep.Look(ref unwrapOnDisposalContainerRelease, "unwrapOnDisposalContainerRelease", new object[0]);
            Scribe_Deep.Look(ref mentalBreaksUseAutoVoreRules, "mentalBreaksUseAutoVoreRules", new object[0]);
            Scribe_Deep.Look(ref defaultStruggleForceMode, "defaultStruggleForceMode", new object[0]);
            Scribe_Deep.Look(ref ejectStunDuration, "ejectStunDuration", new object[0]);
            Scribe_Deep.Look(ref allowVisitorsAsAutoProposalTargets, "allowVisitorsAsAutoProposalTargets", new object[0]);
            Scribe_Deep.Look(ref allowWildAnimalsAsAutoProposalTargets, "allowWildAnimalsAsAutoProposalTargets", new object[0]);
            Scribe_Deep.Look(ref predatorAnimalsOnlyUseDigestGoal, "predatorAnimalsOnlyUseDigestGoal", new object[0]);

            Scribe_Deep.Look(ref struggleBreakFreeNotification, "struggleBreakFreeNotification", new object[0]);
            Scribe_Deep.Look(ref strugglingCreatesInteractions, "strugglingCreatesInteractions", new object[0]);
            Scribe_Deep.Look(ref useAlternativeMainTabIcon, "useBetterMainVoreIcon", new object[0]);
            Scribe_Deep.Look(ref goalSwitchNotification, "goalSwitchNotification", new object[0]);
            Scribe_Deep.Look(ref failedTameVoreNotification, "failedTameVoreNotification", new object[0]);
            Scribe_Deep.Look(ref blockProposalInitiationDuringWorkTime, "blockProposalInitiationDuringWorkTime", new object[0]);

            PostExposeData();
        }
    }
}
