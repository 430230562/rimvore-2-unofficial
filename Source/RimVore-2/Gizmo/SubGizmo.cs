using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimVore2
{
    public abstract class SubGizmo
    {
        protected Pawn pawn;

        protected abstract Texture2D CurrentTexture { get; }
        protected virtual string CurrentTip => null;
        public abstract void Action();

        public SubGizmo(Pawn pawn)
        {
            this.pawn = pawn;
        }

        public abstract bool IsVisible();

        public virtual bool Draw(Rect iconRect)
        {
            if(CurrentTip != null)
            {
                TooltipHandler.TipRegion(iconRect, CurrentTip);
            }
            GUI.DrawTexture(iconRect, CurrentTexture);

            if(Widgets.ButtonInvisible(iconRect, false))
            {
                Action();
                return true;
            }
            return false;
        }
    }
}
