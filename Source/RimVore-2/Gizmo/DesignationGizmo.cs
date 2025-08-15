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
    public class DesignationGizmo : SubGizmo
    {
        protected RV2Designation activeDesignation;

        public DesignationGizmo(Pawn pawn, RV2DesignationDef def) : base(pawn)
        {
            activeDesignation = pawn.PawnData()?.Designations?.TryGetValue(def);
            if(activeDesignation == null)
            {
                RV2Log.Error("SubIcon has activeDesignation NULL, can't draw gizmo");
                return;
            }
        }

        protected override Texture2D CurrentTexture => activeDesignation.CurrentIcon();
        protected override string CurrentTip => activeDesignation.CurrentTip();

        public override bool IsVisible()
        {
            if(activeDesignation.def == RV2DesignationDefOf.fatal && !RV2Mod.Settings.features.FatalVoreEnabled)
            {
                return false;
            }
            if(activeDesignation.def == RV2DesignationDefOf.endo && !RV2Mod.Settings.features.EndoVoreEnabled)
            {
                return false;
            }
            if(RV2Mod.Settings.features.AlwaysShowDesignationGizmos)
            {
                return true;
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

        public override void Action()
        {
            activeDesignation.Cycle();
        }
    }
}
