using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace RimVore2
{
    public class RollAction_CreateThingsFromContainer : RollAction_CreateThings
    {
        protected override List<ThingDef> ThingDefsToCreate => record.VoreContainer?.VoreProductContainer?.ProvideItems();
    }
}
