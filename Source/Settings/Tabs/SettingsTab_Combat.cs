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
    public class SettingsTab_Combat : SettingsTab
    {
        public SettingsTab_Combat(string label, Action clickedAction, bool selected) : base(label, clickedAction, selected) { }
        public SettingsTab_Combat(string label, Action clickedAction, Func<bool> selected) : base(label, clickedAction, selected) { }

        private SettingsContainer_Combat Combat => RV2Mod.Settings.combat;

        public override SettingsContainer AssociatedContainer => Combat;

        public override void FillRect(Rect inRect)
        {
            Combat.FillRect(inRect);
        }
    }
}