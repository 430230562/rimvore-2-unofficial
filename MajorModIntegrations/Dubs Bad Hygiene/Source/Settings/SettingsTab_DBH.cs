using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimVore2;

namespace RV2_DBH
{
    public class SettingsTab_DBH : SettingsTab
    {
        public SettingsTab_DBH(string label, Action clickedAction, bool selected) : base(label, clickedAction, selected) { }
        public SettingsTab_DBH(string label, Action clickedAction, Func<bool> selected) : base(label, clickedAction, selected) { }

        public override SettingsContainer AssociatedContainer => RV2_DBH_Settings.dbh;
        public SettingsContainer_DBH DBH => (SettingsContainer_DBH)AssociatedContainer;

        public override void FillRect(Rect inRect)
        {
            DBH.FillRect(inRect);
        }
    }
}
