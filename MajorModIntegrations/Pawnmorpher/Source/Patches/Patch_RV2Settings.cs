using Verse;
using RimVore2;
using HarmonyLib;

namespace RV2_PawnMorpher
{
    public class RV2_PM_Mod : Mod
    {
        public static RV2_PM_Mod mod;

        public RV2_PM_Mod(ModContentPack content) : base(content)
        {
            mod = this;
            GetSettings<RV2_PM_Settings>();
            WriteSettings();
        }
    }
    public class RV2_PM_Settings : ModSettings
    {
        public static SettingsContainer_PM pm;

        public RV2_PM_Settings()
        {
            pm = new SettingsContainer_PM();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref pm, "pm", new object[0]);
        }
    }

    [HarmonyPatch(typeof(RV2Settings), "DefsLoaded")]
    public static class Patch_RV2Settings_DefsLoaded
    {
        [HarmonyPostfix]
        private static void AddPMSettings()
        {
            RV2_PM_Settings.pm.DefsLoaded();
        }
    }

    [HarmonyPatch(typeof(RV2Settings), "Reset")]
    public static class Patch_RV2Settings_Reset
    {
        [HarmonyPostfix]
        private static void AddPMSettings()
        {
            RV2_PM_Settings.pm.Reset();
        }
    }

    [HarmonyPatch(typeof(RV2Mod), "WriteSettings")]
    public static class Patch_RV2Mod_WriteSettings
    {
        [HarmonyPostfix]
        private static void AddPMSettings()
        {
            RV2_PM_Mod.mod.WriteSettings();
        }
    }

    [HarmonyPatch(typeof(Window_Settings), "InitializeTabs")]
    public static class Patch_Window_Settings
    {
        [HarmonyPostfix]
        private static void AddPMSettings()
        {
            Window_Settings.AddTab(new SettingsTab_PM("RV2_Settings_TabNames_PM".Translate(), null, null));
        }
    }
}
