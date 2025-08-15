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
    public class SettingsTab_FineTuning : SettingsTab
    {
        public SettingsTab_FineTuning(string label, Action clickedAction, bool selected) : base(label, clickedAction, selected) { }
        public SettingsTab_FineTuning(string label, Action clickedAction, Func<bool> selected) : base(label, clickedAction, selected) { }

        private SettingsContainer_FineTuning FineTuning => RV2Mod.Settings.fineTuning;

        public override SettingsContainer AssociatedContainer => throw new NotImplementedException();

        public override void FillRect(Rect inRect)
        {
            FineTuning.FillRect(inRect);
        }
    }
}
