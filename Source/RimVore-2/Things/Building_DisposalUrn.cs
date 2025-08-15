using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RimVore2
{
    public class Building_DisposalUrn : Building_Casket
    {
        private Graphic cachedGraphic;
        public Building_DisposalUrn() : base()
        {
            contentsKnown = true;
        }

        public override int OpenTicks => 300;
        public override string GetInspectString()
        {
            if(!HasAnyContents)
            {
                return "RV2_DisposalUrn_Empty".Translate();
            }
            return LogUtility.QuantifyThings(innerContainer);
        }

        public override void Open()
        {
            base.Open();
            RecacheGraphic();
        }

        public override Graphic Graphic
        {
            get
            {
                if(cachedGraphic == null)
                {
                    RecacheGraphic();
                }
                return cachedGraphic;
            }
        }

        public void RecacheGraphic()
        {
            if(RV2Log.ShouldLog(false, "DisposalContainer"))
                RV2Log.Message("Recaching urn graphic", "DisposalContainer");
            if(GetDirectlyHeldThings().Count == 0)
            {
                cachedGraphic = base.Graphic;
            }
            else
            {
                FilledDisposalContainerGraphics disposalGraphics = def.GetModExtension<FilledDisposalContainerGraphics>();
                if(disposalGraphics != null)
                {
                    cachedGraphic = disposalGraphics.GetGraphic(GetDirectlyHeldThings().ToList());
                }
            }
        }
    }
}
