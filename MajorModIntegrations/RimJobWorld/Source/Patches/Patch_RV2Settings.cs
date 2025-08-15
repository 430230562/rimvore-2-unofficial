using HarmonyLib;
using RimVore2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RV2_RJW
{
    /// <summary>
    /// Subclass Mod to receive a settings file that we can scribe into
    /// </summary>
    public class RV2_RJW_Mod : Mod
    {
        public static RV2_RJW_Mod mod;
        public RV2_RJW_Mod(ModContentPack content) : base(content)
        {
            mod = this;
            GetSettings<RV2_RJW_Settings>();  // create !static! settings
            WriteSettings();
        }
    }

    /// <summary>
    /// Serves as the settings to be retrieved for RV2_RJW_Mod and as the static reference to the SettingsContainer_RJW
    /// </summary>
    public class RV2_RJW_Settings : ModSettings
    {
        public static SettingsContainer_RJW rjw;

        public RV2_RJW_Settings()
        {
            rjw = new SettingsContainer_RJW();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref rjw, "rjw", new object[0]);
        }
    }

    /// <summary>
    /// Ensure definition
    /// </summary>
    [HarmonyPatch(typeof(RV2Settings), "DefsLoaded")]
    public static class Patch_RV2Settings_DefsLoaded
    {
        [HarmonyPostfix]
        private static void AddRJWSettings()
        {
            RV2_RJW_Settings.rjw.DefsLoaded();
        }
    }

    /// <summary>
    /// Reset alongside other settings on "ResetAll"
    /// </summary>
    [HarmonyPatch(typeof(RV2Settings), "Reset")]
    public static class Patch_RV2Settings_Reset
    {
        [HarmonyPostfix]
        private static void AddRJWSettings()
        {
            RV2_RJW_Settings.rjw.Reset();
        }
    }

    /// <summary>
    /// This SCRIBES the settings, because the settings appear in the RV2Mod - which means the settings for RV2_RJW don't get scribed on their own
    /// </summary>
    [HarmonyPatch(typeof(RV2Mod), "WriteSettings")]
    public static class Patch_RV2Mod_WriteSettings
    {
        [HarmonyPostfix]
        private static void AddRJWSettings()
        {
            RV2_RJW_Mod.mod.WriteSettings();
        }
    }

    /// <summary>
    /// This will display the RJW settings in the settings UI
    /// </summary>
    [HarmonyPatch(typeof(Window_Settings), "InitializeTabs")]
    public static class Patch_Window_Settings
    {
        [HarmonyPostfix]
        private static void AddRJWSettings()
        {
             Window_Settings.AddTab(new SettingsTab_RJW("RV2_Settings_TabNames_RJW".Translate(), null, null));
        }
    }
}
