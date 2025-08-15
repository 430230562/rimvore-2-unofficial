//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using HarmonyLib;
//using Verse;

//namespace RimVore2
//{
//    /// <summary>
//    /// This class is temporarily disabled. The only need for it was during the "rapid development" phase of RV-2
//    /// 
//    /// Also this class was never a good idea to begin with, it uses HugsLib for version parsing from manifest, but never used it for the built-in updating mechanisms
//    /// 
//    /// I am hereby laying this class to rest, but will keep the code around in case it ever becomes necessary.
//    /// </summary>
//    public static class SettingsUpdater
//    {
//        public static void UpdateSettings()
//        {
//            return;
//            Version currentVersion = GetVersion();
//            if(RV2Mod.Settings.LastSavedVersion == null)
//            {
//                RV2Mod.Settings.LastSavedVersion = currentVersion.ToString();
//                RV2Mod.Settings.Write();
//                return;
//            }
//            Version lastVersion = new Version(RV2Mod.Settings.LastSavedVersion);
//            //Log.Message("Last saved version: " + lastVersion + " current version: " + currentVersion);
//            // settings refactor to SmartSetting classes
//            if(lastVersion < new Version("0.6.0"))
//                UpdateVersionZero();
//            if(lastVersion < new Version("1.3.1.0"))
//                UpdateOnePointThreeScribingChanges();
//            if(lastVersion < new Version("1.4.2.0"))
//                UpdateCachedInteractions();
//            if(lastVersion < new Version("1.6.0.0"))
//                UpdateRuleRefactor();
//            if(lastVersion < new Version("1.6.1.1"))
//                UpdateCorpseProcessingBug();
//            if(lastVersion >= new Version("1.6.1.1") && lastVersion < new Version("1.6.2.0"))
//                UpdateRulePresetBug();
//            RV2Mod.Settings.LastSavedVersion = currentVersion.ToString();
//            RV2Mod.Settings.Write();
//        }

//        [Obsolete("Unused for now, probably never want to use this")]
//        public static void PresentRestartDialog()
//        {
//            Action okAction = () => GenCommandLine.Restart();
//            Action denyAction = () => Find.WindowStack.Add(new Dialog_MessageBox("RV2_Dialog_RestartDeniedNotification".Translate()));
//            Dialog_MessageBox confirmDialog = new Dialog_MessageBox("RV2_Dialog_RestartNotification".Translate(), "RV2_Dialog_RestartOkay".Translate(), okAction, "RV2_Dialog_RestartNo".Translate(), denyAction);
//            Find.WindowStack.Add(confirmDialog);
//        }

//        private static void UpdateVersionZero()
//        {
//            GeneralUtility.NukeSettings();
//            Log.Warning("RV2 Settings have been wiped due to outdated version / fresh install. Easier than updating for both you and me, trust me. -Nabber");
//        }

//        private static void UpdateOnePointThreeScribingChanges()
//        {
//            GeneralUtility.NukeSettings();
//            Log.Warning("RV2 Settings had to be deleted - there was a critical issue with how each tab was scribed, which is now fixed, this *should* be the last time that I had to delete your settings, I am sorry. -Nabber");
//        }

//        private static void UpdateCachedInteractions()
//        {
//            Log.Warning("I reset your debug settings so that you use a bigger interaction cache by default. kthxbye -Nabber");
//            RV2Mod.Settings.debug.Reset();
//        }

//        private static void UpdateRuleRefactor()
//        {
//            Log.Warning("The rule settings have been refactored and are incompatible with your previously created rules. As a result they had to be reset. Sorry -Nabber");
//            RV2Mod.Settings.rules.Reset();
//            RV2Mod.Settings.rules.Presets.Clear();
//            DebugTools.Reset.AddNabbersPresetEntry();
//        }

//        private static void UpdateCorpseProcessingBug()
//        {
//            Log.Warning("There was a bug in how CorpseProcessing was saved for each path rule, which led to every single path rules always settings fresh corpse. I just reset all your corpse processing settings. Sorry fam -Nabber");
//            foreach(VorePathRule pathRule in RV2Mod.Settings.rules.Rules
//                .Where(rule => rule.Rule.pathRules != null)
//                .SelectMany(rule => rule.Rule.pathRules.Values))
//            {
//                pathRule.CorpseProcessingType = CorpseProcessingType.None;
//            }
//        }

//        /// <summary>
//        /// Reconstruct settings from existing settings, which may be double-deep saved due to not being cloned
//        /// </summary>
//        private static void UpdateRulePresetBug()
//        {
//            Log.Warning("This message is only for a few testers that ran between two testing versions. Love ya btw. And I show my love by removing all rule presets. Enjoy!");
//            RV2Mod.Settings.rules.Presets.Clear();
//            DebugTools.Reset.AddNabbersPresetEntry();
//        }

//        public static Version GetVersion()
//        {
//#if v1_3
//            //Log.Message("all package ids: " + String.Join(", ", LoadedModManager.RunningModsListForReading.Select(m => m.PackageId)));
//            ModContentPack pack = LoadedModManager.RunningModsListForReading.Find(mod => mod.PackageId == "nabber.rimvore2");
//            Version version = HugsLib.Core.ManifestFile.TryParse(pack)?.Version;
//            if(version == null)
//            {
//                Log.Warning("Version could not be parsed correctly, returning 0.0");
//                version = new Version(0, 0);
//            }
//            return version;
//#else
//            return new Version(0, 0);
//#endif
//        }
//    }
//}
