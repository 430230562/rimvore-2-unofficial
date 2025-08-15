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
    public class SettingsTab_Debug : SettingsTab
    {
        public SettingsTab_Debug(string label, Action clickedAction, bool selected) : base(label, clickedAction, selected) { }
        public SettingsTab_Debug(string label, Action clickedAction, Func<bool> selected) : base(label, clickedAction, selected) { }

        public override SettingsContainer AssociatedContainer => RV2Mod.Settings.debug;
        public SettingsContainer_Debug Debug => (SettingsContainer_Debug)AssociatedContainer;

        public override void FillRect(Rect inRect)
        {
            Debug.FillRect(inRect);
        }
    }
}
