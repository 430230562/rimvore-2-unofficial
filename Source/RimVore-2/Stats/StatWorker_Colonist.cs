using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class StatWorker_Colonist : StatWorker
    {
        public override bool ShouldShowFor(StatRequest req)
        {
            if(!base.ShouldShowFor(req))
            {
                return false;
            }
            if(!(req.Thing is Pawn pawn))
            {
                return false;
            }
            return pawn.IsColonist;
        }
    }
}
