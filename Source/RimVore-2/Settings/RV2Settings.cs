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
    public class RV2Settings : ModSettings
    {
        public string LastSavedVersion;

        /// <summary>
        /// Originally I wanted to use this list to easily call the Reset() method on all containers, but for some reason the whole approach does not work
        /// Calling Reset in a ForEach() on the list does not actually reset the settings that are referenced via RV2Mod.settings.debug
        /// So we are going to have to call each field on its own and any mods patching into the settings have to Postfix the Reset, DefsLoaded and ExposeData
        /// </summary>
        //public List<SettingsContainer> SettingsContainers = new List<SettingsContainer>();
        public SettingsUniqueIDsManager SettingsUniqueIDsManager;

        public SettingsContainer_Debug debug;
        public SettingsContainer_Features features;
        public SettingsContainer_FineTuning fineTuning;
        public SettingsContainer_Cheats cheats;
        public SettingsContainer_Sounds sounds;
        public SettingsContainer_Quirks quirks;
        public SettingsContainer_Rules rules;
        public SettingsContainer_Combat combat;
        public SettingsContainer_Ideology ideology;

        public RV2Settings()
        {
            SettingsUniqueIDsManager = new SettingsUniqueIDsManager();
            debug = new SettingsContainer_Debug();
            features = new SettingsContainer_Features();
            fineTuning = new SettingsContainer_FineTuning();
            cheats = new SettingsContainer_Cheats();
            sounds = new SettingsContainer_Sounds();
            quirks = new SettingsContainer_Quirks();
            combat = new SettingsContainer_Combat();
            if(ModsConfig.IdeologyActive)
            {
                ideology = new SettingsContainer_Ideology();
            }
            rules = new SettingsContainer_Rules();
        }

        public void Reset()
        {
            debug?.Reset();
            features?.Reset();
            fineTuning?.Reset();
            cheats?.Reset();
            sounds?.Reset();
            quirks?.Reset();
            combat?.Reset();
            if(ModsConfig.IdeologyActive)
            {
                ideology?.Reset();
            }
            rules?.Reset();
        }

        public void DefsLoaded()
        {
            SettingsInsurance();
            //Log.Message($"Entering DefsLoaded debug: {debug != null} features: {features != null} fineTuning: {fineTuning != null} cheats: {cheats != null} sounds: {sounds != null} quirks: {quirks != null} rules: {rules != null}");

            debug.DefsLoaded();
            features.DefsLoaded();
            fineTuning.DefsLoaded();
            cheats.DefsLoaded();
            sounds.DefsLoaded();
            quirks.DefsLoaded();
            combat.DefsLoaded();
            if(ModsConfig.IdeologyActive)
            {
                ideology.DefsLoaded();
            }
            rules.DefsLoaded();
        }

        /// <summary>
        /// In case I ever have the brilliant idea of changing this again: 
        /// IF YOU USE A LIST, USE SCRIBE_COLLECTIONS - DON'T DO IT THO - THIS WORKS JUST FINE
        /// A list will just cause issues with settings from other assemblies always scribing themselves and then breaking when the other assemblies are removed
        /// Meaning you permanently break the entire scribe because one container can not be loaded anymore - do you want that?
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref SettingsUniqueIDsManager, "SettingsUniqueIDsManager", new object[0]);

            Scribe_Values.Look(ref LastSavedVersion, "LastSavedVersion");
            Scribe_Deep.Look(ref debug, "debug", new object[0]);
            Scribe_Deep.Look(ref features, "features", new object[0]);
            Scribe_Deep.Look(ref fineTuning, "fineTuning", new object[0]);
            Scribe_Deep.Look(ref cheats, "cheats", new object[0]);
            Scribe_Deep.Look(ref sounds, "sounds", new object[0]);
            Scribe_Deep.Look(ref quirks, "quirks", new object[0]);
            Scribe_Deep.Look(ref rules, "rules", new object[0]);
            Scribe_Deep.Look(ref combat, "combat", new object[0]);
            if(ModsConfig.IdeologyActive)
            {
                Scribe_Deep.Look(ref ideology, "ideology", new object[0]);
            }

            if(Scribe.mode == LoadSaveMode.LoadingVars)
            {
                SettingsInsurance();
            }
        }

        /// <summary>
        /// A mistake in the scribing mechanism led to all settings being scribed in a single container, this makes it possible to restore from the erroreous state
        /// </summary>
        private void SettingsInsurance()
        {
            if(SettingsUniqueIDsManager == null)
                SettingsUniqueIDsManager = new SettingsUniqueIDsManager();
            if(debug == null)
                debug = new SettingsContainer_Debug();
            if(features == null)
                features = new SettingsContainer_Features();
            if(fineTuning == null)
                fineTuning = new SettingsContainer_FineTuning();
            if(cheats == null)
                cheats = new SettingsContainer_Cheats();
            if(sounds == null)
                sounds = new SettingsContainer_Sounds();
            if(quirks == null)
                quirks = new SettingsContainer_Quirks();
            if(rules == null)
                rules = new SettingsContainer_Rules();
            if(combat == null)
                combat = new SettingsContainer_Combat();
            if(ModsConfig.IdeologyActive && ideology == null)
                ideology = new SettingsContainer_Ideology();
        }
    }
}

