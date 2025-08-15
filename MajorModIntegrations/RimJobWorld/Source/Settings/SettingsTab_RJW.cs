using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimVore2;

namespace RV2_RJW
{
    public class SettingsTab_RJW : SettingsTab
    {
        public SettingsTab_RJW(string label, Action clickedAction, bool selected) : base(label, clickedAction, selected) { }
        public SettingsTab_RJW(string label, Action clickedAction, Func<bool> selected) : base(label, clickedAction, selected) { }

        public override SettingsContainer AssociatedContainer => RV2_RJW_Settings.rjw;
        public SettingsContainer_RJW RJW => (SettingsContainer_RJW)AssociatedContainer;

        public override void FillRect(Rect inRect)
        {
            RJW.FillRect(inRect);
        }
    }
}
