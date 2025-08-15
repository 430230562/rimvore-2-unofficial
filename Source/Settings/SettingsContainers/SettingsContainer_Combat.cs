using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimVore2
{
    public class SettingsContainer_Combat : SettingsContainer
    {
        public SettingsContainer_Combat() { }

        private FloatSmartSetting grappleVerbSelectionBaseChance;
        private FloatSmartSetting grappleVerbSelectionMaxChance;
        private FloatSmartSetting grappleStrengthMeleeSkillDivider;
        private BoolSmartSetting useAutoRules;
        private FloatSmartSetting defaultRequiredStruggles;
        private FloatSmartSetting defaultStruggleChance;
        private BoolSmartSetting grapplePawnsShareCell;
        private FloatSmartSetting grappleFailureStunDuration;
        private BoolSmartSetting useGrappleGizmoEnabled;
        private BoolSmartSetting toggleGrappleGizmoEnabled;
        private BoolSmartSetting grappleCanTimeout;
        private FloatSmartSetting grappleTimeoutTicks;

        public float GrappleVerbSelectionBaseChance => grappleVerbSelectionBaseChance.value / 100;
        public float GrappleVerbSelectionMaxChance => grappleVerbSelectionMaxChance.value / 100;
        public int GrappleStrengthMeleeSkillDivider => (int)grappleStrengthMeleeSkillDivider.value;
        public bool UseAutoRules => useAutoRules.value;
        public int DefaultRequiredStruggles => (int)defaultRequiredStruggles.value;
        public float DefaultStruggleChance => defaultStruggleChance.value / 100;
        public bool GrapplePawnsShareCell => grapplePawnsShareCell.value;
        public int GrappleFailureStunDuration => (int)grappleFailureStunDuration.value;
        public bool UseGrappleGizmoEnabled => useGrappleGizmoEnabled.value;
        public bool ToggleGrappleGizmoEnabled => toggleGrappleGizmoEnabled.value;
        public bool GrappleCanTimeout => grappleCanTimeout.value;
        public int GrappleTimeoutTicks => (int)grappleTimeoutTicks.value;

        private const int maxGrappleTimeoutTicks = 60000;
        public override void EnsureSmartSettingDefinition()
        {
            if(grappleVerbSelectionBaseChance == null || grappleVerbSelectionBaseChance.IsInvalid())
                grappleVerbSelectionBaseChance = new FloatSmartSetting("RV2_Settings_Combat_GrappleVerbSelectionBaseChance", 15f, 15f, 0, 100, "RV2_Settings_Combat_GrappleVerbSelectionBaseChance_Tip", "0", "%");
            if(grappleVerbSelectionMaxChance == null || grappleVerbSelectionMaxChance.IsInvalid())
                grappleVerbSelectionMaxChance = new FloatSmartSetting("RV2_Settings_Combat_GrappleVerbSelectionMaxChance", 50f, 50f, 0, 100, "RV2_Settings_Combat_GrappleVerbSelectionMaxChance_Tip", "0", "%");
            if(grappleStrengthMeleeSkillDivider == null || grappleStrengthMeleeSkillDivider.IsInvalid())
                grappleStrengthMeleeSkillDivider = new FloatSmartSetting("RV2_Settings_Combat_GrappleStrengthMeleeSkillDivider", 5f, 5f, 1, 20, "RV2_Settings_Combat_GrappleStrengthMeleeSkillDivider_Tip", "0");
            if(useAutoRules == null || useAutoRules.IsInvalid())
                useAutoRules = new BoolSmartSetting("RV2_Settings_Combat_UseAutoRules", true, true, "RV2_Settings_Combat_UseAutoRules_Tip");
            if(defaultRequiredStruggles == null || defaultRequiredStruggles.IsInvalid())
                defaultRequiredStruggles = new FloatSmartSetting("RV2_Settings_Combat_DefaultRequiredStruggles", 60f, 60f, 1, 500, "RV2_Settings_Combat_DefaultRequiredStruggles_Tip", "0");
            if(defaultStruggleChance == null || defaultStruggleChance.IsInvalid())
                defaultStruggleChance = new FloatSmartSetting("RV2_Settings_Combat_DefaultStruggleChance", 35f, 35f, 0, 100, "RV2_Settings_Combat_DefaultStruggleChance_Tip", "0", "%");
            if(grapplePawnsShareCell == null || grapplePawnsShareCell.IsInvalid())
                grapplePawnsShareCell = new BoolSmartSetting("RV2_Settings_Combat_GrapplePawnsShareCell", true, true, "RV2_Settings_Combat_GrapplePawnsShareCell_Tip");
            if(grappleFailureStunDuration == null || grappleFailureStunDuration.IsInvalid())
                grappleFailureStunDuration = new FloatSmartSetting("RV2_Settings_Combat_GrappleFailureStunDuration", 500f, 500f, 1, 1000, "RV2_Settings_Combat_GrappleFailureStunDuration_Tip");
            if(useGrappleGizmoEnabled == null || useGrappleGizmoEnabled.IsInvalid())
                useGrappleGizmoEnabled = new BoolSmartSetting("RV2_Settings_Combat_UseGrappleGizmoEnabled", true, true);
            if(toggleGrappleGizmoEnabled == null || toggleGrappleGizmoEnabled.IsInvalid())
                toggleGrappleGizmoEnabled = new BoolSmartSetting("RV2_Settings_Combat_ToggleGrappleGizmoEnabled", true, true);
            if(grappleCanTimeout == null || grappleCanTimeout.IsInvalid())
                grappleCanTimeout = new BoolSmartSetting("RV2_Settings_Combat_GrappleCanTimeout", true, true, "RV2_Settings_Combat_GrappleCanTimeout_Tip");
            if(grappleTimeoutTicks == null || toggleGrappleGizmoEnabled.IsInvalid())
                grappleTimeoutTicks = new FloatSmartSetting("RV2_Settings_Combat_GrappleTimeoutTicks", 2500, 2500, 600, maxGrappleTimeoutTicks, format: "0");
        }

        public override void Reset()
        {
            grappleVerbSelectionBaseChance = null;
            grappleVerbSelectionMaxChance = null;
            grappleStrengthMeleeSkillDivider = null;
            useAutoRules = null;
            defaultRequiredStruggles = null;
            defaultStruggleChance = null;
            grapplePawnsShareCell = null;
            grappleFailureStunDuration = null;
            useGrappleGizmoEnabled = null;
            toggleGrappleGizmoEnabled = null;
            grappleCanTimeout = null;
            grappleTimeoutTicks = null;

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

            useAutoRules.DoSetting(list);

            if(RV2Mod.Settings.features.GrapplingEnabled)
            {
                grappleVerbSelectionBaseChance.DoSetting(list);
                if(grappleVerbSelectionBaseChance.value > grappleVerbSelectionMaxChance.value) // make sure max is always at least equal to base
                    grappleVerbSelectionMaxChance.value = grappleVerbSelectionBaseChance.value;
                grappleVerbSelectionMaxChance.DoSetting(list);
                grappleStrengthMeleeSkillDivider.DoSetting(list);
                grapplePawnsShareCell.DoSetting(list);
                grappleFailureStunDuration.DoSetting(list);
                useGrappleGizmoEnabled.DoSetting(list);
                toggleGrappleGizmoEnabled.DoSetting(list);
                grappleCanTimeout.DoSetting(list);
                if(GrappleCanTimeout)
                {
                    grappleTimeoutTicks.DoSetting(list);
                    ExplainGrappleTimeout(list);
                }
            }
            if(RV2Mod.Settings.features.StrugglingEnabled)
            {
                defaultRequiredStruggles.DoSetting(list);
                defaultStruggleChance.DoSetting(list);
            }

            list.EndScrollView(ref height, ref heightStale);
        }

        private void ExplainGrappleTimeout(Listing_Standard list)
        {
            string label = "RV2_Settings_Combat_GrappleTimeoutExplanation".Translate(GrappleTimeoutTicks.ToStringTicksToPeriodVerbose());
            list.Label(label);
        }

        public override void ExposeData()
        {
            if(Scribe.mode == LoadSaveMode.Saving || Scribe.mode == LoadSaveMode.LoadingVars)
            {
                EnsureSmartSettingDefinition();
            }
            Scribe_Deep.Look(ref grappleVerbSelectionBaseChance, "grappleVerbSelectionBaseChance", new object[0]);
            Scribe_Deep.Look(ref grappleVerbSelectionMaxChance, "grappleVerbSelectionMaxChance", new object[0]);
            Scribe_Deep.Look(ref grappleStrengthMeleeSkillDivider, "grappleStrengthMeleeSkillDivider", new object[0]);
            Scribe_Deep.Look(ref useAutoRules, "useAutoRules", new object[0]);
            Scribe_Deep.Look(ref defaultRequiredStruggles, "defaultRequiredStruggles", new object[0]);
            Scribe_Deep.Look(ref defaultStruggleChance, "defaultStruggleChance", new object[0]);
            Scribe_Deep.Look(ref grapplePawnsShareCell, "grapplePawnsShareCell", new object[0]);
            Scribe_Deep.Look(ref grappleFailureStunDuration, "grappleFailureStunDuration", new object[0]);
            Scribe_Deep.Look(ref useGrappleGizmoEnabled, "useGrappleGizmoEnabled", new object[0]);
            Scribe_Deep.Look(ref toggleGrappleGizmoEnabled, "toggleGrappleGizmoEnabled", new object[0]);

            PostExposeData();
        }
    }
}