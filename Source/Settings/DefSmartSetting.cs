using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimVore2
{
    public class DefSmartSetting<T> : SmartSetting<T> where T : Def, new()
    {
        string unresolvedValue;
        string unresolvedDefaultValue;
        List<T> whiteList;
        List<T> blackList;
        FloatMenu buttonMenu;
        FloatMenu ButtonMenu
        {
            get
            {
                if(buttonMenu == null)
                {
                    List<FloatMenuOption> options = ValidOptions()   // conver each valid enum value into a selectable float menu option
                        .Select(def =>
                        {
                            Action valueAction = () => value = def;
                            return new FloatMenuOption(def.label, valueAction);
                        })
                        .ToList();
                    buttonMenu = new FloatMenu(options);
                }
                return buttonMenu;
            }
        }

        public DefSmartSetting() : base() { }

        public DefSmartSetting(string labelKey, T value, T defaultValue, string tooltipKey = null, List<T> whiteList = null, List<T> blackList = null) : base(labelKey, value, defaultValue, tooltipKey)
        {
            this.whiteList = whiteList;
            this.blackList = blackList;
        }

        private float ButtonWidth => Text.CalcSize(value.ToString() + "    ").x;

        protected override float RequiredHeight(Listing_Standard list)
        {
            float labelWidth = list.ColumnWidth - ButtonWidth;
            return Text.CalcHeight(Label, labelWidth);
        }

        public override void InternalDoSetting(Listing_Standard list)
        {
            base.DecorateSetting(list, out Rect rect);
            UIUtility.SplitRectVertically(rect, out Rect labelRect, out Rect buttonRect, -1, ButtonWidth);
            Widgets.Label(labelRect, Label);

            if(Widgets.ButtonText(buttonRect, value.label))
            {
                Find.WindowStack.Add(ButtonMenu);
            }
        }

        public IEnumerable<T> ValidOptions()
        {
            foreach(T value in DefDatabase<T>.AllDefsListForReading)
            {
                if(blackList?.Contains(value) == true)
                {
                    continue;
                }
                if(whiteList?.Contains(value) == false)
                {
                    continue;
                }
                yield return value;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if(Scribe.mode == LoadSaveMode.Saving)
            {
                unresolvedValue = value.defName;
                Scribe_Values.Look(ref unresolvedValue, "unresolvedValue");
                unresolvedDefaultValue = defaultValue.defName;
                Scribe_Values.Look(ref unresolvedDefaultValue, "unresolvedDefaultValue");
            }
            else if(Scribe.mode == LoadSaveMode.LoadingVars)
            {
                Scribe_Values.Look(ref unresolvedValue, "unresolvedValue");
                Scribe_Values.Look(ref unresolvedDefaultValue, "unresolvedDefaultValue");
            }
            Scribe_Collections.Look(ref whiteList, "whiteList", LookMode.Value);
            Scribe_Collections.Look(ref blackList, "blackList", LookMode.Value);
        }

        public override void DefsLoaded()
        {
            if(unresolvedValue != null)
            {
                value = DefDatabase<T>.GetNamed(unresolvedValue);
            }
            if(unresolvedDefaultValue != null)
            {
                defaultValue = DefDatabase<T>.GetNamed(unresolvedDefaultValue);
            }
        }
    }
}
