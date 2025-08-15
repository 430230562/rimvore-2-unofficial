using rjw;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RV2_RJW
{
    public static class JobUtility
    {
        public static bool HasSexJob(Pawn pawn)
        {
            if(pawn?.jobs == null)
                return false;
            //Log.Message($"Pawn {pawn.LabelShort} has curDriver {pawn.jobs.curDriver}, which is JobDriver_Sex ? {(pawn.jobs.curDriver is JobDriver_Sex)}");
            return pawn.jobs.curDriver is JobDriver_Sex;
        }
    }
}
