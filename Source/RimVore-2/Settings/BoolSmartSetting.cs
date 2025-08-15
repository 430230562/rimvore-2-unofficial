using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimVore2
{
    public class BoolSmartSetting : SmartSetting<bool>
    {
        public BoolSmartSetting() : base() { }

        public BoolSmartSetting(string labelKey, bool value, bool defaultValue, string tooltipKey = null) : base(labelKey, value, defaultValue, tooltipKey) { }

        protected override float RequiredHeight(Listing_Standard list)
        {
            return Text.CalcHeight(Label, list.ColumnWidth - ButtonSizeWithPadding * 2);
        }

        /// <summary>
        /// This is basically a re-implementation of base games checkbox - but with a reset button on the left
        /// </summary>
        public override void InternalDoSetting(Listing_Standard list)
        {
            base.DecorateSetting(list, out Rect rect);
            UIUtility.SplitRectVertically(rect, out Rect labelRect, out Rect checkboxRect, -1, ButtonSizeWithPadding);
            Widgets.Label(labelRect, Label);
            Texture2D checkboxTexture = value == true ? UITextures.CheckOnTexture : UITextures.CheckOffTexture;
            UIUtility.TextureInCenter(checkboxRect, checkboxTexture, out _);
            if(Widgets.ButtonInvisible(rect))
            {
                SoundDefOf.Tick_Low.PlayOneShotOnCamera(null);
                value = !value;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref value, "value");
            Scribe_Values.Look(ref defaultValue, "defaultValue");
        }
    }
}
