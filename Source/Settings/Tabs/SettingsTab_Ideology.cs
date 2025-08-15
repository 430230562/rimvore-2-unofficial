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
    public class SettingsTab_Ideology : SettingsTab
    {
        public SettingsTab_Ideology(string label, Action clickedAction, bool selected) : base(label, clickedAction, selected) { }
        public SettingsTab_Ideology(string label, Action clickedAction, Func<bool> selected) : base(label, clickedAction, selected) { }

        private SettingsContainer_Ideology Ideology => RV2Mod.Settings.ideology;

        public override SettingsContainer AssociatedContainer => Ideology;

        public override void FillRect(Rect inRect)
        {
            Ideology.FillRect(inRect);
        }
    }
}