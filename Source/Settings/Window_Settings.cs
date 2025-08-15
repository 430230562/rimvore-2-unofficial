using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimVore2
{
    public class Window_Settings : Window
    {
        private static int currentTabIndex;
        private static List<TabRecord> tabs;
        private SettingsTab CurrentTab => tabs.Count > currentTabIndex ? (SettingsTab)tabs[currentTabIndex] : null;

        private const float TabHeight = 32f;  // I think base game has 31 as the tab height, hard to tell with the compiler optimized constants 
        private Color SelectedColor => Color.green;
        private static TabRecord selectedTab;
        private static bool showOnlySettingsMatchingSearch = false;
        private static string currentSearchText = "";

        public Window_Settings()
        {
            doCloseX = true;
            absorbInputAroundWindow = true;
            focusWhenOpened = true;
            forcePause = true;
            draggable = true;
            resizeable = true;
            onlyOneOfTypeAllowed = true;
            UIUtility.OverwriteResizer(this);
        }

        public override void PreClose()
        {
            base.PreClose();
            if(RV2Mod.Mod != null)
            {
                RV2Mod.Mod.WriteSettings();
            }
            VoreInteractionManager.ClearCachedInteractions();
        }

        public override void PostClose()
        {
            base.PostClose();
            RV2Mod.Settings.rules.RecacheAllDesignations();
        }

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(UI.screenWidth * 0.4f, UI.screenHeight * 0.6f);
            }
        }

        const float searchToolHeight = 24f;
        const int tabsPerRow = 7;
        public override void DoWindowContents(Rect inRect)
        {
            inRect = new Rect(
                inRect.x,
                inRect.y,
                inRect.width,
                inRect.height);
            InitializeTabs();

            Rect searchToolRect = inRect.BottomPartPixels(searchToolHeight);
            DrawSearchTool(searchToolRect);

            Rect tabContentRect = inRect.TopPartPixels(inRect.height - searchToolRect.height);
            int tabRows = (int)Math.Ceiling((float)tabs.Count / tabsPerRow);
            // the content rect must be moved down because tab drawing behavior draws ABOVE the content rect provided to it
            tabContentRect = tabContentRect.ContractedBy(tabRows * TabHeight);
            DrawTabsWithContent(tabContentRect, tabRows);
        }

        private static void InitializeTabs()
        {
            if(tabs == null)
            {
                tabs = new List<TabRecord>();
            }
            if(Prefs.DevMode)
            {
                AddTab(new SettingsTab_Debug("RV2_Settings_TabNames_Debug".Translate(), null, null));
            }
            AddTab(new SettingsTab_Features("RV2_Settings_TabNames_Features".Translate(), null, null));
            AddTab(new SettingsTab_FineTuning("RV2_Settings_TabNames_FineTuning".Translate(), null, null));
            AddTab(new SettingsTab_Sounds("RV2_Settings_TabNames_Sounds".Translate(), null, null));
            AddTab(new SettingsTab_Cheats("RV2_Settings_TabNames_Cheats".Translate(), null, null));
            AddTab(new SettingsTab_Rules("RV2_Settings_TabNames_Rules".Translate(), null, null));
            AddTab(new SettingsTab_Quirks("RV2_Settings_TabNames_Quirks".Translate(), null, null));
            AddTab(new SettingsTab_Combat("RV2_Settings_TabNames_Combat".Translate(), null, null));
            if(ModsConfig.IdeologyActive)
            {
                AddTab(new SettingsTab_Ideology("RV2_Settings_TabNames_Ideology".Translate(), null, null));
            }
        }
        static Func<SettingsTab, Action> clickedAction = (SettingsTab tab) => () => currentTabIndex = tabs.IndexOf(tab);
        static Func<SettingsTab, Func<bool>> selectedGetter = (SettingsTab tab) => () => currentTabIndex == tabs.IndexOf(tab);
        // the RecordTab constructor is bad, so we call it with NULL clickedAction and selectedGetter in order to add them afterwards with reference to the created tab
        public static void AddTab(SettingsTab tab)
        {
            // each tab type can only exist once
            if(tabs.Any(t => t.GetType() == tab.GetType()))
            {
                // Log.Message("tabs already contains tab of type " + tab.GetType().ToString());
                return;
            }
            tab.clickedAction = clickedAction(tab);
            tab.selectedGetter = selectedGetter(tab);
            tabs.Add(tab);
        }

        private void DrawTabsWithContent(Rect inRect, int rows)
        {
            // okay, the tab drawer is fucking weird, you give it a Rectangle and it will do its best to draw ABOVE the rectangle you gave it rather than inside the rectangle
            TabRecord record = TabDrawer.DrawTabs(inRect, tabs, rows: rows, null);
            if(record != null)
            {
                if(selectedTab == null)
                {
                    selectedTab = record;
                    record.labelColor = SelectedColor;
                }
                else if(selectedTab != record)
                {
                    selectedTab.labelColor = record.labelColor;
                    selectedTab = record;
                    record.labelColor = SelectedColor;
                }
            }
            // lower the box a bit because the tab drawer leaves no space
            inRect.y += 3;
            inRect.height -= 3;
            CurrentTab?.FillRect(inRect);
        }

        private void DrawSearchTool(Rect inRect)
        {
            string checkboxLabel = "RV2_Settings_SearchToolInfo".Translate();
            float checkboxWidth = Text.CalcSize(checkboxLabel).x + Widgets.CheckboxSize;
            Rect checkboxRect = inRect.LeftPartPixels(checkboxWidth);
            Widgets.CheckboxLabeled(checkboxRect, checkboxLabel, ref showOnlySettingsMatchingSearch);
            TooltipHandler.TipRegion(checkboxRect, "RV2_Settings_SearchToolInfo_Tip".Translate());
            Rect searchTextAreaRect = inRect.RightPartPixels(inRect.width - checkboxRect.width);
            currentSearchText = Widgets.TextArea(searchTextAreaRect, currentSearchText);
        }

        public static bool AllowsDrawing<T>(SmartSetting<T> setting)
        {
            if(!showOnlySettingsMatchingSearch)
            {
                return true;
            }
            if(currentSearchText == "")
            {
                return true;
            }
            if(setting.Label?.Contains(currentSearchText) == true)
            {
                return true;
            }
            if(setting.Tooltip?.Contains(currentSearchText) == true)
            {
                return true;
            }
            // allows enum values to be picked up by search
            if(setting.value.ToStringSafe().Contains(currentSearchText) == true)
            {
                return true;
            }
            return false;
        }
    }
}
