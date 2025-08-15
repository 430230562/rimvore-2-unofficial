using HarmonyLib;
using RimVore2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace LightGenitals
{
    /// <summary>
    /// Subclass Mod to receive a settings file that we can scribe into
    /// </summary>
    public class RV2_LG_Mod : Mod
    {
        public static RV2_LG_Mod mod;
        public RV2_LG_Mod(ModContentPack content) : base(content)
        {
            mod = this;
            GetSettings<RV2_LG_Settings>();  // create !static! settings
            WriteSettings();
        }
    }

    /// <summary>
    /// Serves as the settings to be retrieved for RV2_LG_Mod and as the static reference to the SettingsContainer_LG
    /// </summary>
    public class RV2_LG_Settings : ModSettings
    {
        public static SettingsContainer_LG lg;

        public RV2_LG_Settings()
        {
            lg = new SettingsContainer_LG();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref lg, "lg", new object[0]);
        }
    }

    /// <summary>
    /// Ensure definition
    /// </summary>
    [HarmonyPatch(typeof(RV2Settings), "DefsLoaded")]
    public static class Patch_RV2Settings_DefsLoaded
    {
        [HarmonyPostfix]
        private static void AddLGSettings()
        {
            RV2_LG_Settings.lg.DefsLoaded();
        }
    }

    /// <summary>
    /// Reset alongside other settings on "ResetAll"
    /// </summary>
    [HarmonyPatch(typeof(RV2Settings), "Reset")]
    public static class Patch_RV2Settings_Reset
    {
        [HarmonyPostfix]
        private static void AddLGSettings()
        {
            RV2_LG_Settings.lg.Reset();
        }
    }

    /// <summary>
    /// This SCRIBES the settings, because the settings appear in the RV2Mod - which means the settings for RV2_LG don't get scribed on their own
    /// </summary>
    [HarmonyPatch(typeof(RV2Mod), "WriteSettings")]
    public static class Patch_RV2Mod_WriteSettings
    {
        [HarmonyPostfix]
        private static void AddLGSettings()
        {
            RV2_LG_Mod.mod.WriteSettings();
        }
    }

    /// <summary>
    /// This will display the LG settings in the settings UI
    /// </summary>
    [HarmonyPatch(typeof(Window_Settings), "InitializeTabs")]
    public static class Patch_RV2Mod
    {
        [HarmonyPostfix]
        private static void AddLGSettings()
        {
            Window_Settings.AddTab(new SettingsTab_LG("LightGenitals_tabName".Translate(), null, null));
        }
    }
}
