using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public static class RV2ForbidUtility
    {
        public static bool CanForbid(this Thing thing)
        {
            return thing.TryGetComp<CompForbiddable>() != null;
        }
    }
}
