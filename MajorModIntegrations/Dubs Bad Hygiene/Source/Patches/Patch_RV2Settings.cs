using HarmonyLib;
using RimVore2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RV2_DBH
{
    /// <summary>
    /// Subclass Mod to receive a settings file that we can scribe into
    /// </summary>
    public class RV2_DBH_Mod : Mod
    {
        public static RV2_DBH_Mod mod;
        public RV2_DBH_Mod(ModContentPack content) : base(content)
        {
            mod = this;
            GetSettings<RV2_DBH_Settings>();  // create !static! settings
            WriteSettings();
        }
    }

    /// <summary>
    /// Serves as the settings to be retrieved for RV2_DBH_Mod and as the static reference to the SettingsContainer_DBH
    /// </summary>
    public class RV2_DBH_Settings : ModSettings
    {
        public static SettingsContainer_DBH dbh;

        public RV2_DBH_Settings()
        {
            dbh = new SettingsContainer_DBH();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref dbh, "dbh", new object[0]);
        }
    }

    /// <summary>
    /// Ensure definition
    /// </summary>
    [HarmonyPatch(typeof(RV2Settings), "DefsLoaded")]
    public static class Patch_RV2Settings_DefsLoaded
    {
        [HarmonyPostfix]
        private static void AddDBHSettings()
        {
            RV2_DBH_Settings.dbh.DefsLoaded();
        }
    }

    /// <summary>
    /// Reset alongside other settings on "ResetAll"
    /// </summary>
    [HarmonyPatch(typeof(RV2Settings), "Reset")]
    public static class Patch_RV2Settings_Reset
    {
        [HarmonyPostfix]
        private static void AddDBHSettings()
        {
            RV2_DBH_Settings.dbh.Reset();
        }
    }

    /// <summary>
    /// This SCRIBES the settings, because the settings appear in the RV2Mod - which means the settings for RV2_DBH don't get scribed on their own
    /// </summary>
    [HarmonyPatch(typeof(RV2Mod), "WriteSettings")]
    public static class Patch_RV2Mod_WriteSettings
    {
        [HarmonyPostfix]
        private static void AddDBHSettings()
        {
            RV2_DBH_Mod.mod.WriteSettings();
        }
    }

    /// <summary>
    /// This will display the DBH settings in the settings UI
    /// </summary>
    [HarmonyPatch(typeof(Window_Settings), "InitializeTabs")]
    public static class Patch_RV2Mod
    {
        [HarmonyPostfix]
        private static void AddDBHSettings()
        {
            Window_Settings.AddTab(new SettingsTab_DBH("RV2_Settings_TabNames_DBH".Translate(), null, null));
        }
    }
}
