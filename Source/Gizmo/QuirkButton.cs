using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimVore2
{
    public class QuirkButton : SubGizmo
    {
        public QuirkButton(Pawn pawn) : base(pawn) { }

        protected override Texture2D CurrentTexture => ContentFinder<Texture2D>.Get("Widget/quirk_menu");

        protected override string CurrentTip => "RV2_QuirkWindow_Header".Translate();

        public override bool IsVisible()
        {
            return VoreValidator.CanHaveQuirks(pawn, out _);
        }

        public override void Action()
        {
            Find.WindowStack.Add(new Window_Quirks(pawn));
        }
    }
}
