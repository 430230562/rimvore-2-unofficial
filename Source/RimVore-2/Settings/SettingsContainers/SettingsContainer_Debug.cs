using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimVore2
{
    public class SettingsContainer_Debug : SettingsContainer
    {
        public bool EnableVerboseLoggingWarning = true;

        private BoolSmartSetting logging;
        private BoolSmartSetting verboseLogging;
        private BoolSmartSetting passPredatorTicks;
        private FloatSmartSetting hediffLabelRefreshInterval;
        private FloatSmartSetting maxCachedInteractions;
        public LoggingTagSettings loggingSettings = new LoggingTagSettings();

        public SettingsContainer_Debug() { }
        public bool Logging => logging.value;
        public bool VerboseLogging => verboseLogging.value;
        public bool PassPredatorTicks => passPredatorTicks.value;
        public int HediffLabelRefreshInterval => (int)hediffLabelRefreshInterval.value;
        public int MaxCachedInteractions => (int)maxCachedInteractions.value;

        public override bool DevModeOnly => true;

        public bool AllowedToLog(string tag)
        {
            return loggingSettings.AllowedToLog(tag);
        }

        public override void Reset()
        {
            logging = null;
            verboseLogging = null;
            passPredatorTicks = null;
            hediffLabelRefreshInterval = null;
            maxCachedInteractions = null;
            loggingSettings = null;

            EnsureSmartSettingDefinition();
        }

        public override void EnsureSmartSettingDefinition()
        {
            if(logging == null || logging.IsInvalid())
                logging = new BoolSmartSetting("RV2_Settings_Debug_Logging", false, false);
            if(verboseLogging == null || verboseLogging.IsInvalid())
                verboseLogging = new BoolSmartSetting("RV2_Settings_Debug_VerboseLogging", false, false);
            if(passPredatorTicks == null || passPredatorTicks.IsInvalid())
                passPredatorTicks = new BoolSmartSetting("RV2_Settings_Debug_PassPredatorTick", true, true);
            if(hediffLabelRefreshInterval == null || hediffLabelRefreshInterval.IsInvalid())
                hediffLabelRefreshInterval = new FloatSmartSetting("RV2_Settings_Debug_LabelRefresh", 150, 150, 1, 9999, null, "#");
            if(maxCachedInteractions == null || maxCachedInteractions.IsInvalid())
                maxCachedInteractions = new FloatSmartSetting("RV2_Settings_Debug_MaxCachedInteractions", 50, 50, 2, 200, "RV2_Settings_Debug_MaxCachedInteractions_Tip", "#");
            if(loggingSettings == null)
                loggingSettings = new LoggingTagSettings();
        }

        public bool HeightStale = true;
        private float height = 0f;
        private Vector2 scrollPosition;
        public void FillRect(Rect inRect)
        {
            Rect outerRect = inRect;
            UIUtility.MakeAndBeginScrollView(outerRect, height, ref scrollPosition, out Listing_Standard list);

            if(list.ButtonText("RV2_Settings_Reset".Translate()))
                Reset();

            logging.DoSetting(list);
            bool previousVerboseLogging = VerboseLogging;
            verboseLogging.DoSetting(list);
            // prompt the user to only enable verbose logging if they are sure about it.
            // I am so tired of getting flooded logs from first-time bug-reporters
            if(EnableVerboseLoggingWarning && previousVerboseLogging == false && VerboseLogging == true)
            {
                Dialog_MessageBox window = new Dialog_MessageBox("Please do not enable verbose mode unless you have been asked to or you *really* know what you are doing.", "OK, Enable Verbose", () => { }, "Nevermind, Disable Verbose", () => { verboseLogging.value = false; });
                Find.WindowStack.Add(window);
            }
            
            passPredatorTicks.DoSetting(list);
            hediffLabelRefreshInterval.DoSetting(list);
            maxCachedInteractions.DoSetting(list);
            if(loggingSettings.DoSetting(list))
            {
                HeightStale = true;
            }

            list.EndScrollView(ref height, ref HeightStale);
        }

        public override void ExposeData()
        {
            if(Scribe.mode == LoadSaveMode.Saving || Scribe.mode == LoadSaveMode.LoadingVars)
            {
                EnsureSmartSettingDefinition();
            }

            Scribe_Values.Look(ref EnableVerboseLoggingWarning, "EnableVerboseLoggingWarning");
            Scribe_Deep.Look(ref logging, "logging", new object[0]);
            Scribe_Deep.Look(ref verboseLogging, "verboseLogging", new object[0]);
            Scribe_Deep.Look(ref passPredatorTicks, "passPredatorTicks", new object[0]);
            Scribe_Deep.Look(ref hediffLabelRefreshInterval, "hediffLabelRefreshInterval", new object[0]);
            Scribe_Deep.Look(ref maxCachedInteractions, "maxCachedInteractions", new object[0]);
            Scribe_Deep.Look(ref loggingSettings, "loggingSettings", new object[0]);

            PostExposeData();
        }
    }
}
