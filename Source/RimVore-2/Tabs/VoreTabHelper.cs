using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimVore2.Tabs
{
    public static class VoreTabHelper
    {
        public static void ResetGUI()
        {
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
        }

        //Returns the distance that x should be offset after adding for additional icons
        public static float DrawIconConditionally(float totalXOffset, float y, float IconSize, Texture2D icon, Action OnClick = null, string tooltip = null, Func<bool> conditionalDraw = null)
        {
            if(conditionalDraw != null && !conditionalDraw()) return 0;

            DrawIcon(totalXOffset, y, IconSize, icon, OnClick, tooltip);

            return IconSize;
        }
        public static void DrawIcon(float x, float y, float size, Texture2D iconTexture, Action OnClick = null, string tooltip = null)
        {
            Rect rect = new Rect(x, y, size, size);
            if(tooltip != null && Mouse.IsOver(rect))
            {
                TooltipHandler.TipRegion(rect, tooltip);
            }
            if(Widgets.ButtonImage(rect, iconTexture))
            {
                OnClick?.Invoke();
            }
        }
    }

}
