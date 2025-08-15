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
    public class SettingsTab_Cheats : SettingsTab
    {
        public SettingsTab_Cheats(string label, Action clickedAction, bool selected) : base(label, clickedAction, selected) { }
        public SettingsTab_Cheats(string label, Action clickedAction, Func<bool> selected) : base(label, clickedAction, selected) { }

        public override SettingsContainer AssociatedContainer => RV2Mod.Settings.cheats;

        private SettingsContainer_Cheats Cheats => (SettingsContainer_Cheats)AssociatedContainer;
        public override void FillRect(Rect inRect)
        {
            Cheats.FillRect(inRect);
        }
    }
}
