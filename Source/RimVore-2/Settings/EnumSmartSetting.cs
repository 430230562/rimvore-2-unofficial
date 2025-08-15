using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimVore2
{
    public class EnumSmartSetting<T> : SmartSetting<T> where T : struct, IConvertible
    {
        List<T> whiteList;
        List<T> blackList;
        public Func<T, string> valuePresenter = (T enumValue) => UIUtility.EnumPresenter(enumValue.ToString());
        FloatMenu buttonMenu;
        FloatMenu ButtonMenu
        {
            get
            {
                if(buttonMenu == null)
                {
                    List<FloatMenuOption> options = ValidOptions()   // conver each valid enum value into a selectable float menu option
                        .Select(enumValue =>
                        {
                            string valueLabel = valuePresenter(enumValue).ToString();
                            Action valueAction = () => value = enumValue;
                            return new FloatMenuOption(valueLabel, valueAction);
                        })
                        .ToList();
                    buttonMenu = new FloatMenu(options);
                }
                return buttonMenu;
            }
        }

        public EnumSmartSetting() : base() { }

        public EnumSmartSetting(string labelKey, T value, T defaultValue, string tooltipKey = null, List<T> whiteList = null, List<T> blackList = null) : base(labelKey, value, defaultValue, tooltipKey)
        {
            this.whiteList = whiteList;
            this.blackList = blackList;
        }

        private string ButtonLabel => valuePresenter(value);
        private float ButtonWidth => Text.CalcSize(" " + ButtonLabel.ToString() + " ").x;

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

            if(Widgets.ButtonText(buttonRect, ButtonLabel))
            {
                Find.WindowStack.Add(ButtonMenu);
            }
        }

        public IEnumerable<T> ValidOptions()
        {
            foreach(T value in typeof(T).GetEnumValues())
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
            Scribe_Values.Look(ref value, "value");
            Scribe_Values.Look(ref defaultValue, "defaultValue");
            Scribe_Collections.Look(ref whiteList, "whiteList", LookMode.Value);
            Scribe_Collections.Look(ref blackList, "blackList", LookMode.Value);
        }
    }
}
