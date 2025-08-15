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
    public class SettingsTab_Features : SettingsTab
    {
        public SettingsTab_Features(string label, Action clickedAction, bool selected) : base(label, clickedAction, selected) { }
        public SettingsTab_Features(string label, Action clickedAction, Func<bool> selected) : base(label, clickedAction, selected) { }

        public override SettingsContainer AssociatedContainer => RV2Mod.Settings.features;
        public SettingsContainer_Features Features => (SettingsContainer_Features)AssociatedContainer;

        public override void FillRect(Rect inRect)
        {
            Features.FillRect(inRect);
        }
    }
}
