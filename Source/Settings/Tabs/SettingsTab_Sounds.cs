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
    public class SettingsTab_Sounds : SettingsTab
    {
        public SettingsTab_Sounds(string label, Action clickedAction, bool selected) : base(label, clickedAction, selected) { }
        public SettingsTab_Sounds(string label, Action clickedAction, Func<bool> selected) : base(label, clickedAction, selected) { }

        public override SettingsContainer AssociatedContainer => RV2Mod.Settings.sounds;
        public SettingsContainer_Sounds Sounds => (SettingsContainer_Sounds)AssociatedContainer;

        public override void FillRect(Rect inRect)
        {
            Sounds.FillRect(inRect);
        }
    }
}
