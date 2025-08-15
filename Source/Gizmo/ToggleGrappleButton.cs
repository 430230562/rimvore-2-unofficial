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
    public class ToggleGrappleButton : SubGizmo
    {
        private PawnData pawnData;

        public ToggleGrappleButton(Pawn pawn) : base(pawn)
        {
            pawnData = pawn.PawnData();
        }

        public override bool IsVisible()
        {
            bool settingsAllow = RV2Mod.Settings.features.GrapplingEnabled && RV2Mod.Settings.combat.ToggleGrappleGizmoEnabled;
            if(!settingsAllow)
            {
                return false;
            }
            if(pawn.IsAnimal())
            {
                return pawn.Faction == Faction.OfPlayer;
            }

#if v1_3
            return pawn.IsColonistPlayerControlled;
#else
            return pawn.IsColonistPlayerControlled || pawn.IsColonyMechPlayerControlled;
#endif
        }

        private string CurrentTexturePath => pawnData.CanUseGrapple ? "Widget/grappleToggledOn" : "Widget/grappleToggledOff";
        protected override Texture2D CurrentTexture => ContentFinder<Texture2D>.Get(CurrentTexturePath);

        protected override string CurrentTip => pawnData.CanUseGrapple ? "RV2_Command_ToggleGrapple_DescOn".Translate() : "RV2_Command_ToggleGrapple_DescOff".Translate();

        public override void Action()
        {
            pawnData.CanUseGrapple = !pawnData.CanUseGrapple;
        }
    }
}
