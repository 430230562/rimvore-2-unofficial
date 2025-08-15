using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimVore2;

namespace LightGenitals
{
    public class SettingsTab_LG : SettingsTab
    {
        public SettingsTab_LG(string label, Action clickedAction, bool selected) : base(label, clickedAction, selected) { }
        public SettingsTab_LG(string label, Action clickedAction, Func<bool> selected) : base(label, clickedAction, selected) { }

        public override SettingsContainer AssociatedContainer => RV2_LG_Settings.lg;
        public SettingsContainer_LG LG => (SettingsContainer_LG)AssociatedContainer;

        public override void FillRect(Rect inRect)
        {
            LG.FillRect(inRect);
        }
    }
}
