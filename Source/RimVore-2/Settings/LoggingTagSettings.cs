using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimVore2
{
    public class LoggingTagSettings : IExposable
    {
        public SortedDictionary<string, bool> loggingTags = new SortedDictionary<string, bool>();
        private string inputTag = string.Empty;

        public void Reset()
        {
            loggingTags.Clear();
            inputTag = string.Empty;
        }

        public bool DoSetting(Listing_Standard list)
        {
            list.GapLine();
            list.ColumnWidth /= 2;
            list.Label("Allowed log tags (re-enter tag to remove)");
            bool modified = DoTagInput(list);
            DoTagList(list);
            list.ColumnWidth *= 2;
            return modified;
        }

        /// <returns>True if list was modified, otherwise false</returns>
        private bool DoTagInput(Listing_Standard list)
        {
            inputTag = list.TextEntry(inputTag);
            Rect rowRect = list.GetRect(Text.LineHeight);
            float buttonWidth = rowRect.width / 3;
            Rect addRemoveRect = new Rect(rowRect.xMin, rowRect.yMin, buttonWidth, rowRect.height);
            Rect enableAllRect = new Rect(addRemoveRect.xMax, addRemoveRect.yMin, buttonWidth, rowRect.height);
            Rect disableAllRect = new Rect(enableAllRect.xMax, enableAllRect.yMin, buttonWidth, rowRect.height);
            if(Widgets.ButtonText(addRemoveRect, "Add/Remove") && inputTag != string.Empty)
            {
                if(loggingTags.ContainsKey(inputTag))
                {
                    loggingTags.Remove(inputTag);
                }
                else
                {
                    loggingTags.SetOrAdd(inputTag.ToLower(), true);
                }
                inputTag = string.Empty;
                return true;
            }
            if(Widgets.ButtonText(enableAllRect, "Enable all"))
            {
                foreach(string key in loggingTags.Keys.ToList())
                {
                    loggingTags[key] = true;
                }
            }
            if(Widgets.ButtonText(disableAllRect, "Disable all"))
            {
                foreach(string key in loggingTags.Keys.ToList())
                {
                    loggingTags[key] = false;
                }
            }
            return false;
        }

        private void DoTagList(Listing_Standard list)
        {
            foreach(KeyValuePair<string, bool> kvp in new Dictionary<string, bool>(loggingTags))
            {
                string tag = kvp.Key;
                bool enabled = kvp.Value;
                list.CheckboxLabeled(tag, ref enabled, tag);
                if(kvp.Value != enabled)
                {
                    loggingTags.SetOrAdd(tag, enabled);
                }
            }
        }

        public bool AllowedToLog(string tag)
        {
            tag = tag.ToLower();
            if(!loggingTags.ContainsKey(tag))
            {
                loggingTags.Add(tag, true);
                RV2Log.Message($"Added \"{tag}\" to categories via self-discovery", "Debug");
                RV2Mod.Settings.debug.HeightStale = true;
            }
            return loggingTags[tag];
        }

        public void ExposeData()
        {
            Dictionary<string, bool> scribingDict = loggingTags?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            Scribe_Collections.Look<string, bool>(ref scribingDict, "scribingDict", LookMode.Value);
            if(scribingDict != null)
                loggingTags = new SortedDictionary<string, bool>(scribingDict);

            if(Scribe.mode == LoadSaveMode.LoadingVars && loggingTags == null)
                loggingTags = new SortedDictionary<string, bool>();
        }
    }
}
