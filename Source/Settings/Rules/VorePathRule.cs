using System;
using System.Collections.Generic;
using Verse;

namespace RimVore2
{
    public class VorePathRule : IExposable
    {
        public bool Enabled = true;
        public bool AutoVoreEnabled = true;
        public string container = null;
        private string vorePath;
        private StruggleForceMode struggleForceMode;
        private FloatSmartSetting requiredStruggles;
        public VorePathDef VorePath => vorePath == null ? null : DefDatabase<VorePathDef>.GetNamed(vorePath);
        public ThingDef Container => container == null ? null : ThingDef.Named(container);
        private CorpseProcessingType corpseProcessingType = CorpseProcessingType.None;
        public CorpseProcessingType CorpseProcessingType
        {
            get
            {
                if(corpseProcessingType == CorpseProcessingType.None)
                {
                    corpseProcessingType = VorePath?.voreProduct?.destroyPrey == true ? CorpseProcessingType.Destroy : CorpseProcessingType.Dessicate;
                }
                return corpseProcessingType;
            }
            set
            {
                corpseProcessingType = value;
            }
        }
        public int RequiredStruggles => (int)requiredStruggles.value;

        public void SetContainer(ThingDef newContainer)
        {
            container = newContainer.defName;
        }
        public VorePathRule(string pathDefName)
        {
            vorePath = pathDefName;
            struggleForceMode = RV2Mod.Settings.fineTuning.DefaultStruggleForceMode;
            EnsureSmartSettingDefinition();
        }

        public VorePathRule() { }

        public void ValidateContainers()
        {
            if(Container?.GetModExtension<VoreContainerExtension>()?.IsValid() == false)
            {
                if(RV2Log.ShouldLog(false, "Settings"))
                    RV2Log.Message("Container with invalid ContainerExtension selected, setting to first valid or NULL", true, "Settings");
                SetContainer(VorePath.FirstValidContainer);
            }
        }

        public bool ShouldStruggle(bool isForced)
        {
            switch(struggleForceMode)
            {
                case StruggleForceMode.Always:
                    return true;
                case StruggleForceMode.Never:
                    return false;
                case StruggleForceMode.WhenForced:
                    return isForced;
                case StruggleForceMode.WhenNotForced:
                    return !isForced;
                default:
                    Log.Warning($"Unexpected {nameof(StruggleForceMode)}: {struggleForceMode}");
                    return false;
            }
        }
        public void UpdateStruggleMode(StruggleForceMode previousMode, StruggleForceMode newMode)
        {
            if(struggleForceMode == previousMode)
                struggleForceMode = newMode;
        }

        static Func<CorpseProcessingType, string> corpseProcessingTypePresenter = (CorpseProcessingType type) => $"RV2_CorpseProcessingType_{type}".Translate();
        static Func<StruggleForceMode, string> struggleForceModePresenter = (StruggleForceMode mode) => $"RV2_StruggleForceMode_{mode}".Translate();
        public void DoControls(Listing_Standard list, RuleState generalAutoVoreEnabled, ref bool heightStale)
        {
            Action<ThingDef> containerSelected = SetContainer;
            list.Label(new TaggedString(VorePath.label), -1, VorePath.description);
            list.Indent(true);
            bool oldEnabled = Enabled;
            list.CheckboxLabeled("RV2_Settings_Rules_Enabled".Translate(), ref Enabled, "RV2_Settings_Rules_Enabled_Tip".Translate());
            if(Enabled != oldEnabled)   // button has been pressed, this means we now hide elements and the height needs to be recalculated
            {
                heightStale = true;
            }
            if(!Enabled)
            {
                list.Outdent(true);
                return;
            }
            if(generalAutoVoreEnabled == RuleState.On)
            {
                list.CheckboxLabeled("RV2_Settings_Rules_AllowedInAutoVore".Translate(), ref AutoVoreEnabled, "RV2_Settings_Rules_AllowedInAutoVore_Tip".Translate());
            }
            List<ThingDef> validContainers = VorePath.ValidVoreProductContainers;
            if(!validContainers.NullOrEmpty())
            {
                list.CreateLabelledDropDown(validContainers, Container, UIUtility.DefLabelGetter<ThingDef>(), containerSelected, null, "RV2_Settings_Rules_Container".Translate(), "RV2_Settings_Rules_Container_Tip".Translate(), UIUtility.DefTooltipGetter<ThingDef>());
                Action<CorpseProcessingType> corpseSelection = (CorpseProcessingType newType) => CorpseProcessingType = newType;
                List<CorpseProcessingType> corpseBlacklist = new List<CorpseProcessingType>() { CorpseProcessingType.None };
                list.EnumLabeled("RV2_Settings_Rules_CorpseProcessing".Translate(), CorpseProcessingType, corpseSelection, valuePresenter: corpseProcessingTypePresenter, valueBlacklist: corpseBlacklist);
            }
            if(RV2Mod.Settings.features.StrugglingEnabled)
            {
                Action<StruggleForceMode> struggleModeSelection = (StruggleForceMode newMode) => struggleForceMode = newMode;
                List<StruggleForceMode> struggleBlacklist = new List<StruggleForceMode>() { StruggleForceMode.Invalid };
                list.EnumLabeled("RV2_Settings_Rules_StruggleForceMode".Translate(), struggleForceMode, struggleModeSelection, valuePresenter: struggleForceModePresenter, tooltip: "RV2_Settings_Rules_StruggleForceMode_Tip".Translate(), valueBlacklist: struggleBlacklist);
                if(struggleForceMode != StruggleForceMode.Never)
                    requiredStruggles.DoSetting(list);
            }
            list.Outdent(true);
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref Enabled, "Enabled");
            Scribe_Values.Look(ref AutoVoreEnabled, "AutoVoreEnabled");
            Scribe_Values.Look(ref container, "container");
            Scribe_Values.Look(ref vorePath, "vorePath");
            Scribe_Values.Look(ref corpseProcessingType, "CorpseProcessingType");
            Scribe_Values.Look(ref struggleForceMode, "struggleForceMode");
            Scribe_Deep.Look(ref requiredStruggles, "smartRequiredStruggles");
        }

        private void EnsureSmartSettingDefinition()
        {
            if(requiredStruggles == null || requiredStruggles.IsInvalid())
                requiredStruggles = new FloatSmartSetting("RV2_Settings_Rules_RequiredStruggles", VorePath.defaultRequiredStruggles, VorePath.defaultRequiredStruggles, 1, 999, "RV2_Settings_Rules_RequiredStruggles_Tip", "0");
        }

        public void DefsLoaded()
        {
            EnsureSmartSettingDefinition();
            if(RequiredStruggles == -1)
            {
                requiredStruggles.value = VorePath.defaultRequiredStruggles;
                if(RequiredStruggles == -1)
                {
                    requiredStruggles.value = RV2Mod.Settings.combat.DefaultRequiredStruggles;
                    if(RV2Log.ShouldLog(false, "Settings"))
                        RV2Log.Message($"VorePathDef {VorePath.defName} does not have \"defaultRequiredStruggles\" set, defauling to settings value {requiredStruggles}", "Settings");
                }
            }
            if(struggleForceMode == StruggleForceMode.Invalid)
                struggleForceMode = RV2Mod.Settings.fineTuning.DefaultStruggleForceMode;
        }
    }

    public enum CorpseProcessingType
    {
        Dessicate,
        Rot,
        Destroy,
        Fresh,
        [UpdateCompatibilityFix]
        None
    }
}
