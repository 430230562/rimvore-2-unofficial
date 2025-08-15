using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimVore2
{
    public abstract class SettingsTab : TabRecord
    {
        public SettingsTab(string label, Action clickedAction, bool selected) : base(label, clickedAction, selected) { }
        public SettingsTab(string label, Action clickedAction, Func<bool> selected) : base(label, clickedAction, selected) { }
        public abstract void FillRect(Rect inRect);

        public abstract SettingsContainer AssociatedContainer { get; }

        public void DoResetButton(Listing_Standard list)
        {
            if(list.ButtonText("RV2_Settings_ResetAll"))
            {
                AssociatedContainer.Reset();
            }
        }
    }
}
