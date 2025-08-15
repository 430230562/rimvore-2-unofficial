using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimVore2
{
    public class SettingsContainer_Features : SettingsContainer
    {
        public SettingsContainer_Features() { }

        private BoolSmartSetting voreQuirksEnabled;
        private BoolSmartSetting fatalVoreEnabled;
        private BoolSmartSetting endoVoreEnabled;
        private BoolSmartSetting feederVoreEnabled;
        private BoolSmartSetting animalsEnabled;
        private BoolSmartSetting showVoreTab;
        private BoolSmartSetting showRMBVoreMenu;
        private BoolSmartSetting showGizmo;
        private BoolSmartSetting scatEnabled;
        private BoolSmartSetting bonesEnabled;
        private BoolSmartSetting backstoryGenitalsEnabled;
        private BoolSmartSetting grapplingEnabled;
        private BoolSmartSetting strugglingEnabled;
        private BoolSmartSetting specificVoreScenariosIgnoreDesignations;
        private BoolSmartSetting ignoreDesignationsMentalState;
        private BoolSmartSetting ignoreDesignationsVoreHuntingAnimals;
        private BoolSmartSetting ignoreDesignationsKidnappingRaiders;
        private BoolSmartSetting ignoreDesignationsGoalSwitching;
        private BoolSmartSetting alwaysShowDesignationGizmos;
        private BoolSmartSetting rmbProposalsUseAutoVoreInterval;
        private BoolSmartSetting addVoredPawnsToColonistBar;

        public bool VoreQuirksEnabled => voreQuirksEnabled.value;
        public bool FatalVoreEnabled => fatalVoreEnabled.value;
        public bool EndoVoreEnabled => endoVoreEnabled.value;
        public bool FeederVoreEnabled => feederVoreEnabled.value;
        public bool AnimalsEnabled => animalsEnabled.value;
        public bool ShowVoreTab => showVoreTab.value;
        public bool ShowRMBVoreMenu => showRMBVoreMenu.value;
        public bool RmbProposalsUseAutoVoreInterval => rmbProposalsUseAutoVoreInterval.value;
        public bool ShowGizmo => showGizmo.value;
        public bool ScatEnabled => scatEnabled.value;
        public bool BonesEnabled => bonesEnabled.value;
        public bool BackstoryGenitalsEnabled => backstoryGenitalsEnabled.value;
        public bool GrapplingEnabled => grapplingEnabled.value;
        public bool StrugglingEnabled => strugglingEnabled.value;
        public bool SpecificVoreScenariosIgnoreDesignations => specificVoreScenariosIgnoreDesignations.value;
        public bool IgnoreDesignationsMentalState => SpecificVoreScenariosIgnoreDesignations && ignoreDesignationsMentalState.value;
        public bool IgnoreDesignationsVoreHuntingAnimals => SpecificVoreScenariosIgnoreDesignations && ignoreDesignationsVoreHuntingAnimals.value;
        public bool IgnoreDesignationsKidnappingRaiders => SpecificVoreScenariosIgnoreDesignations && ignoreDesignationsKidnappingRaiders.value;
        public bool IgnoreDesignationsGoalSwitching => SpecificVoreScenariosIgnoreDesignations && ignoreDesignationsGoalSwitching.value;
        public bool AlwaysShowDesignationGizmos => alwaysShowDesignationGizmos.value;
        public bool AddVoredPawnsToColonistBar => addVoredPawnsToColonistBar.value;


        public override void Reset()
        {
            voreQuirksEnabled = null;
            fatalVoreEnabled = null;
            endoVoreEnabled = null;
            feederVoreEnabled = null;
            animalsEnabled = null;
            showVoreTab = null;
            showRMBVoreMenu = null;
            rmbProposalsUseAutoVoreInterval = null;
            showGizmo = null;
            scatEnabled = null;
            bonesEnabled = null;
            backstoryGenitalsEnabled = null;
            grapplingEnabled = null;
            strugglingEnabled = null;
            specificVoreScenariosIgnoreDesignations = null;
            ignoreDesignationsMentalState = null;
            ignoreDesignationsVoreHuntingAnimals = null;
            ignoreDesignationsKidnappingRaiders = null;
            ignoreDesignationsGoalSwitching = null;
            alwaysShowDesignationGizmos = null;
            addVoredPawnsToColonistBar = null;

            EnsureSmartSettingDefinition();
        }

        public override void EnsureSmartSettingDefinition()
        {
            if(voreQuirksEnabled == null || voreQuirksEnabled.IsInvalid())
                voreQuirksEnabled = new BoolSmartSetting("RV2_Settings_Features_VoreQuirksEnabled", true, true, "RV2_Settings_Features_VoreQuirksEnabled_Tip");
            if(fatalVoreEnabled == null || fatalVoreEnabled.IsInvalid())
                fatalVoreEnabled = new BoolSmartSetting("RV2_Settings_Features_FatalVoreEnabled", true, true, "RV2_Settings_Features_FatalVoreEnabled_Tip");
            if(endoVoreEnabled == null || endoVoreEnabled.IsInvalid())
                endoVoreEnabled = new BoolSmartSetting("RV2_Settings_Features_EndoVoreEnabled", true, true, "RV2_Settings_Features_EndoVoreEnabled_Tip");
            if(feederVoreEnabled == null || feederVoreEnabled.IsInvalid())
                feederVoreEnabled = new BoolSmartSetting("RV2_Settings_Features_FeederVoreEnabled", true, true, "RV2_Settings_Features_FeederVoreEnabled_Tip");
            if(animalsEnabled == null || animalsEnabled.IsInvalid())
                animalsEnabled = new BoolSmartSetting("RV2_Settings_Features_AnimalsEnabled", true, true, "RV2_Settings_Features_AnimalsEnabled_Tip");
            if(showVoreTab == null || showVoreTab.IsInvalid())
                showVoreTab = new BoolSmartSetting("RV2_Settings_Features_ShowVoreTab", true, true, "RV2_Settings_Features_ShowVoreTab_Tip");
            if(showRMBVoreMenu == null || showRMBVoreMenu.IsInvalid())
                showRMBVoreMenu = new BoolSmartSetting("RV2_Settings_Features_ShowRMBVoreMenu", true, true, "RV2_Settings_Features_ShowRMBVoreMenu_Tip");
            if(rmbProposalsUseAutoVoreInterval == null || rmbProposalsUseAutoVoreInterval.IsInvalid())
                rmbProposalsUseAutoVoreInterval = new BoolSmartSetting("RV2_Settings_Features_RMBProposalsUseAutoVoreInterval", true, true, "RV2_Settings_Features_RMBProposalsUseAutoVoreInterval_Tip");
            if(showGizmo == null || showGizmo.IsInvalid())
                showGizmo = new BoolSmartSetting("RV2_Settings_Features_ShowGizmo", true, true, "RV2_Settings_Features_ShowGizmo_Tip");
            if(scatEnabled == null || scatEnabled.IsInvalid())
                scatEnabled = new BoolSmartSetting("RV2_Settings_Features_ScatEnabled", true, true, "RV2_Settings_Features_ScatEnabled_Tip");
            if(bonesEnabled == null || bonesEnabled.IsInvalid())
                bonesEnabled = new BoolSmartSetting("RV2_Settings_Features_BonesEnabled", true, true, "RV2_Settings_Features_BonesEnabled_Tip");
            if(backstoryGenitalsEnabled == null || backstoryGenitalsEnabled.IsInvalid())
                backstoryGenitalsEnabled = new BoolSmartSetting("RV2_Settings_Features_BackstoryGenitalsEnabled", true, true, "RV2_Settings_Features_BackstoryGenitalsEnabled_Tip");
            if(grapplingEnabled == null || grapplingEnabled.IsInvalid())
                grapplingEnabled = new BoolSmartSetting("RV2_Settings_Features_GrapplingEnabled", true, true, "RV2_Settings_Features_GrapplingEnabled_Tip");
            if(strugglingEnabled == null || strugglingEnabled.IsInvalid())
                strugglingEnabled = new BoolSmartSetting("RV2_Settings_Features_StrugglingEnabled", true, true, "RV2_Settings_Features_StrugglingEnabled_Tip");
            if(specificVoreScenariosIgnoreDesignations == null || specificVoreScenariosIgnoreDesignations.IsInvalid())
                specificVoreScenariosIgnoreDesignations = new BoolSmartSetting("RV2_Settings_Features_SpecificVoreScenariosIgnoreDesignations", false, false, "RV2_Settings_Features_SpecificVoreScenariosIgnoreDesignations_Tip");
            if(ignoreDesignationsMentalState == null || ignoreDesignationsMentalState.IsInvalid())
                ignoreDesignationsMentalState = new BoolSmartSetting("RV2_Settings_Features_IgnoreDesignations_MentalState", true, true, "RV2_Settings_Features_IgnoreDesignations_MentalState_Tip");
            if(ignoreDesignationsVoreHuntingAnimals == null || ignoreDesignationsVoreHuntingAnimals.IsInvalid())
                ignoreDesignationsVoreHuntingAnimals = new BoolSmartSetting("RV2_Settings_Features_IgnoreDesignations_VoreHuntingAnimals", true, true, "RV2_Settings_Features_IgnoreDesignations_VoreHuntingAnimals_Tip");
            if(ignoreDesignationsKidnappingRaiders == null || ignoreDesignationsKidnappingRaiders.IsInvalid())
                ignoreDesignationsKidnappingRaiders = new BoolSmartSetting("RV2_Settings_Features_IgnoreDesignations_KidnappingRaiders", true, true, "RV2_Settings_Features_IgnoreDesignations_KidnappingRaiders_Tip");
            if(ignoreDesignationsGoalSwitching == null || ignoreDesignationsGoalSwitching.IsInvalid())
                ignoreDesignationsGoalSwitching = new BoolSmartSetting("RV2_Settings_Features_IgnoreDesignations_IgnoreDesignationsGoalSwitching", true, true, "RV2_Settings_Features_IgnoreDesignations_IgnoreDesignationsGoalSwitching_Tip");
            if(alwaysShowDesignationGizmos == null || alwaysShowDesignationGizmos.IsInvalid())
                alwaysShowDesignationGizmos = new BoolSmartSetting("RV2_Settings_Features_AlwaysShowDesignationGizmos", false, false, "RV2_Settings_Features_AlwaysShowDesignationGizmos_Tip");
            if(addVoredPawnsToColonistBar == null || addVoredPawnsToColonistBar.IsInvalid())
                addVoredPawnsToColonistBar = new BoolSmartSetting("RV2_Settings_Features_AddVoredPawnsToColonistBar", true, true, "RV2_Settings_Features_AddVoredPawnsToColonistBar_Tip");

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

            if(list.ButtonText("RV2_Settings_ResetAll".Translate()))
                RV2Mod.Settings.Reset();

            voreQuirksEnabled.DoSetting(list);
            fatalVoreEnabled.DoSetting(list);
            endoVoreEnabled.DoSetting(list);
            feederVoreEnabled.DoSetting(list);
            animalsEnabled.DoSetting(list);
            showVoreTab.DoSetting(list);
            showRMBVoreMenu.DoSetting(list);
            if(ShowRMBVoreMenu)
            {
                rmbProposalsUseAutoVoreInterval.DoSetting(list);
            }
            showGizmo.DoSetting(list);
            bool needToUpdateBackstoryDescriptions = false; // if bones or scat is toggled, all descriptions must update
            bool oldScatEnabled = ScatEnabled;
            scatEnabled.DoSetting(list);
            needToUpdateBackstoryDescriptions |= oldScatEnabled != ScatEnabled; // scat flag changed, need to update backstories
            bool oldBonesEnabled = BonesEnabled;
            bonesEnabled.DoSetting(list);
            needToUpdateBackstoryDescriptions |= oldBonesEnabled != BonesEnabled; // bones flag changed, need to update backstories
            backstoryGenitalsEnabled.DoSetting(list);
            grapplingEnabled.DoSetting(list);
            strugglingEnabled.DoSetting(list);
            alwaysShowDesignationGizmos.DoSetting(list);
            specificVoreScenariosIgnoreDesignations.DoSetting(list);
            if(SpecificVoreScenariosIgnoreDesignations)
            {
                list.Indent(true);
                ignoreDesignationsMentalState.DoSetting(list);
                ignoreDesignationsVoreHuntingAnimals.DoSetting(list);
                ignoreDesignationsKidnappingRaiders.DoSetting(list);
                ignoreDesignationsGoalSwitching.DoSetting(list);
                list.Outdent(true);
            }

            list.EndScrollView(ref height, ref heightStale);

            if(needToUpdateBackstoryDescriptions)
            {
                if(RV2Log.ShouldLog(false, "Settings"))
                    RV2Log.Message("Need to update backstory descriptions due to change in scat / bones features.", "Settings");
                BackstoryUtility.UpdateAllBackstoryDescriptions();
            }
        }

        public override void ExposeData()
        {
            if(Scribe.mode == LoadSaveMode.Saving || Scribe.mode == LoadSaveMode.LoadingVars)
            {
                EnsureSmartSettingDefinition();
            }
            Scribe_Deep.Look(ref voreQuirksEnabled, "VoreQuirksEnabled", new object[0]);
            Scribe_Deep.Look(ref fatalVoreEnabled, "FatalVoreEnabled", new object[0]);
            Scribe_Deep.Look(ref endoVoreEnabled, "EndoVoreEnabled", new object[0]);
            Scribe_Deep.Look(ref feederVoreEnabled, "FeederVoreEnabled", new object[0]);
            Scribe_Deep.Look(ref animalsEnabled, "AnimalsEnabled", new object[0]);
            Scribe_Deep.Look(ref showVoreTab, "ShowVoreTab", new object[0]);
            Scribe_Deep.Look(ref showRMBVoreMenu, "ShowRMBVoreMenu", new object[0]);
            Scribe_Deep.Look(ref rmbProposalsUseAutoVoreInterval, "rmbProposalsUseAutoVoreInterval", new object[0]);
            Scribe_Deep.Look(ref showGizmo, "ShowGizmo", new object[0]);
            Scribe_Deep.Look(ref scatEnabled, "ScatEnabled", new object[0]);
            Scribe_Deep.Look(ref bonesEnabled, "BonesEnabled", new object[0]);
            Scribe_Deep.Look(ref backstoryGenitalsEnabled, "BackstoryGenitalsEnabled", new object[0]);
            Scribe_Deep.Look(ref grapplingEnabled, "grapplingEnabled", new object[0]);
            Scribe_Deep.Look(ref strugglingEnabled, "strugglingEnabled", new object[0]);
            Scribe_Deep.Look(ref specificVoreScenariosIgnoreDesignations, "specificVoreScenariosIgnoreDesignations", new object[0]);
            Scribe_Deep.Look(ref ignoreDesignationsMentalState, "ignoreDesignationsMentalState", new object[0]);
            Scribe_Deep.Look(ref ignoreDesignationsVoreHuntingAnimals, "ignoreDesignationsVoreHuntingAnimals", new object[0]);
            Scribe_Deep.Look(ref ignoreDesignationsKidnappingRaiders, "ignoreDesignationsKidnappingRaiders", new object[0]);
            Scribe_Deep.Look(ref ignoreDesignationsGoalSwitching, "ignoreDesignationsGoalSwitching", new object[0]);
            Scribe_Deep.Look(ref alwaysShowDesignationGizmos, "alwaysShowDesignationGizmos", new object[0]);

            PostExposeData();
        }
    }

}
