#if !v1_3
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class Gene_ForcedHediff : Gene
    {
        GeneDefExtension_ForcedHediff Extension => def.GetModExtension<GeneDefExtension_ForcedHediff>();

        public override void PostAdd()
        {
            base.PostAdd();
            if(pawn?.health?.hediffSet == null)
            {
                return;
            }
            foreach (HediffDef def in Extension.hediffs)
            {
                if (!pawn.health.hediffSet.HasHediff(def))
                {
                    pawn.health.AddHediff(def);
                }
            }
        }

        public override void PostRemove()
        {
            base.PostRemove();
            if (!Extension.removeHediffsOnGeneRemoval)
            {
                return;
            }
            List<Hediff> hediffs = pawn?.health?.hediffSet?.hediffs;
            if (hediffs.NullOrEmpty())
            {
                return;
            }
            foreach (Hediff hediff in hediffs)
            {
                if (Extension.hediffs.Contains(hediff.def))
                {
                    pawn.health.hediffSet.hediffs.Remove(hediff);
                }
            }
        }
    }
}
#endif