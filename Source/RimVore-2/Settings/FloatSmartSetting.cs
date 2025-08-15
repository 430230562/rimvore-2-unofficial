using UnityEngine;
using Verse;

namespace RimVore2
{
    public class FloatSmartSetting : SmartSetting<float>
    {
        const float textBoxWidth = 50f;
        float minValue;
        float maxValue;
        string format = "0.00";
        string extraLabel = "";

        public FloatSmartSetting() : base() { }

        public FloatSmartSetting(string labelKey, float value, float defaultValue, float minValue = 0, float maxValue = 999, string tooltipKey = null, string format = "0.00", string extraLabel = "") : base(labelKey, value, defaultValue, tooltipKey)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.format = format;
            this.extraLabel = extraLabel;
        }

        public void OverrideMinValue(float value)
        {
            minValue = value;
        }
        public void OverrideMaxValue(float value)
        {
            maxValue = value;
        }

        protected override float RequiredHeight(Listing_Standard list)
        {
            float labelWidth = list.ColumnWidth - ButtonSizeWithPadding - textBoxWidth;
            return Text.CalcHeight(Label, labelWidth) + Text.CalcHeight(" ", labelWidth);
        }

        public override void InternalDoSetting(Listing_Standard list)
        {
            base.DecorateSetting(list, out Rect rect);
            UIUtility.FancySlider(rect, Label, ref value, minValue, maxValue, format, extraLabel);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref value, "value");
            Scribe_Values.Look(ref defaultValue, "defaultValue");
            Scribe_Values.Look(ref minValue, "minValue");
            Scribe_Values.Look(ref maxValue, "maxValue");
            Scribe_Values.Look(ref format, "format", "0.00");
            Scribe_Values.Look(ref extraLabel, "extraLabel", "");
        }
    }
}
