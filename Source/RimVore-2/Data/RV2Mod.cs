using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimVore2
{
    public class RV2Mod : Mod
    {
        public static RV2Component RV2Component;
        public static RV2Settings Settings;
        public static RV2Mod Mod;
        public RV2Mod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<RV2Settings>();  // create !static! settings
            Mod = this;
            // initialize singletons
            new Targeter_ForcePause();

            InitializeEarlyPatches();
            //SettingsUpdater.UpdateSettings();
        }

        static bool earlyPatchesInitialized = false;
        public static void InitializeEarlyPatches()
        {
            if(earlyPatchesInitialized)
                return;
            earlyPatchesInitialized = true;

            RV2BackCompatibility.AddRV2BackCompatibilities();

            Harmony harmony = new Harmony("RV2_EARLY");

            // early patch to actually influence XML loading
            harmony.Patch(AccessTools.Method(typeof(DirectXmlLoader), "DefFromNode"), new HarmonyMethod(typeof(Patch_DirectXmlLoader), "InterruptDateTaggedNodes"));
            harmony.Patch(AccessTools.PropertyGetter(typeof(BackstoryDef), nameof(BackstoryDef.DisabledWorkTypes)),
                prefix: new HarmonyMethod(typeof(Patch_BackstoryDef), nameof(Patch_BackstoryDef.LockDefDatabaseIfUninitialized)),
                postfix: new HarmonyMethod(typeof(Patch_BackstoryDef), nameof(Patch_BackstoryDef.UnlockDefDatabase)));

            // This will enqueue an event to call the DefsLoaded hook at the end of the games startup sequence
            LongEventHandler.QueueLongEvent(() => Mod.DefsLoaded(), "RV2_DefsLoaded", false, null);
        }


        public void DefsLoaded()
        {
            /// <remarks>
            /// We must call WriteSettings() to properly initialize the settings. The settings must be written to files to be accessible on first-time settings creation
            /// I sadly can't explain why this happens, but all the settings values are null if the settings files don't exist
            /// By calling it immediately once the game is ready, we ensure that future calls to the settings actually work
            /// </remarks>
            WriteSettings();

            // poke
            Settings.DefsLoaded();
            // additional configuration issues
            ConfigUtility.PresentAdditionalConfigErrors();
            ConfigUtility.PresentAdditionalConfigMessages();

            // backstories are enabled / disabled based on scat / bones settings
            BackstoryUtility.UpdateAllBackstoryDescriptions();
        }

        public override string SettingsCategory()
        {
            return "RV2_Settings_Category".Translate();
        }

        /// <summary>
        /// Exists purely to be patched into by RJW integration
        /// </summary>
        public override void WriteSettings()
        {
            base.WriteSettings();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            // the base games settings are extremely static, they have a fixed size and are not draggable
            CloseNativeSettings();
            // custom window "fixes" those issues
            Find.WindowStack.Add(new Window_Settings());
        }

        private static List<Type> windowTypesToClose;
        private void CloseNativeSettings()
        {
            if (windowTypesToClose == null)
            {
                windowTypesToClose = new List<Type>();
                windowTypesToClose.Add(typeof(RimWorld.Dialog_ModSettings));
                // using reflection is a better choice than adding HugsLib as a hard dependency
            }
            foreach(Type type in windowTypesToClose)
            {
                Find.WindowStack.TryRemoveAssignableFromType(type, false);
            }

            //Find.WindowStack.TryRemoveAssignableFromType(typeof(HugsLib.Settings.Dialog_ModSettings), false);
            //Find.WindowStack.TryRemoveAssignableFromType(typeof(Dialog_VanillaModSettings), false);
        }
    }
}
