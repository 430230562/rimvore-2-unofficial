using RimWorld;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimVore2
{
    public interface ISmartSetting : IExposable
    {
        void Reset();
        void DoSetting(Listing_Standard list);
        void DefsLoaded();
    }

    public abstract class SmartSetting<T> : ISmartSetting
    {
        protected const float buttonSize = 16f;
        protected const float buttonPadding = 4f;
        protected string labelKey;
        protected string tooltipKey;
        public T value;
        protected T defaultValue;

        protected float ButtonSizeWithPadding => buttonSize + buttonPadding * 2;
        public string Label => labelKey.Translate();
        public string Tooltip => tooltipKey?.Translate();


        public SmartSetting() { }

        public SmartSetting(string labelKey, T value, T defaultValue, string tooltipKey = null)
        {
            this.labelKey = labelKey;
            this.value = value;
            this.tooltipKey = tooltipKey;
            this.defaultValue = defaultValue;
        }
        protected abstract float RequiredHeight(Listing_Standard list);

        public void DoSetting(Listing_Standard list)
        {
            if(!ShouldDrawSetting())
            {
                return;
            }
            InternalDoSetting(list);
        }

        protected virtual bool ShouldDrawSetting()
        {
            return Window_Settings.AllowsDrawing(this);
        }

        public abstract void InternalDoSetting(Listing_Standard list);

        public bool IsInvalid()
        {
            return labelKey == null;
        }

        public void DecorateSetting(Listing_Standard list, out Rect settingsRect)
        {
            Rect rect = list.GetRect(RequiredHeight(list));
            if(Mouse.IsOver(rect))
            {
                Widgets.DrawHighlight(rect);
            }
            if(Tooltip != null)
            {
                TooltipHandler.TipRegion(rect, Tooltip);
            }
            ResetButton(rect, out settingsRect);
        }

        protected void ResetButton(Rect rect, out Rect settingsRect)
        {
            UIUtility.SplitRectVertically(rect, out Rect outerButtonRect, out settingsRect, ButtonSizeWithPadding);
            bool isDefault = value.GetHashCode() == defaultValue.GetHashCode();
            // for some reason this causes odd behaviour when moving the slider through the default value, so we draw a phantom button instead
            // might have something to do with the additional potential event created by ButtonInvisible, so this is a bit of a mess
            // try it out if you don't believe me :^)
            //if(isDefault)
            //{
            //    return;
            //}
            Texture2D tex = isDefault ? UITextures.BlankButtonTexture : UITextures.ResetButtonTexture;
            UIUtility.TextureInCenter(outerButtonRect, tex, out Rect buttonRect, buttonSize);
            if(Widgets.ButtonInvisible(buttonRect, !isDefault))
            {
                if(!isDefault)
                {
                    SoundDefOf.Tick_Low.PlayOneShotOnCamera(null);
                    Reset();
                }
            }
        }

        public void Reset()
        {
            value = defaultValue;
        }

        public virtual void DefsLoaded() { }

        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref labelKey, "labelKey");
            Scribe_Values.Look(ref tooltipKey, "tooltipKey");
        }
    }
}
