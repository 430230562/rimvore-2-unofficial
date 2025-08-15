using System;
using RimVore2;
using UnityEngine;

namespace RV2_PawnMorpher
{
    public class SettingsTab_PM : SettingsTab
    {
        public SettingsTab_PM(string label, Action clickedAction, bool selected) : base(label, clickedAction, selected) { }
        public SettingsTab_PM(string label, Action clickedAction, Func<bool> selected) : base(label, clickedAction, selected) { }

        public override SettingsContainer AssociatedContainer => RV2_PM_Settings.pm;
        public SettingsContainer_PM PM => (SettingsContainer_PM)AssociatedContainer;

        public override void FillRect(Rect inRect)
        {
            PM.FillRect(inRect);
        }
    }
}
