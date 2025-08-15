using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimVore2
{
    public class RaceDef_AlternativeVoreProductContainer : DefModExtension
    {
        Dictionary<VorePathDef, ThingDef> alternativeContainersForPaths;

        public ThingDef AlternativeContainerFor(VorePathDef path)
        {
            return alternativeContainersForPaths.TryGetValue(path, null);
        }
    }
}
