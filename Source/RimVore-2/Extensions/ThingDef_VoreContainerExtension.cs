using System;
using System.Collections.Generic;
using Verse;

namespace RimVore2
{
    public class VoreContainerExtension : DefModExtension
    {
        public List<ThingDef> forcedProducedThings;
        public bool isBones = false;
        public bool isScat = false;

        public bool IsValid()
        {
            if(!RV2Mod.Settings.features.BonesEnabled && isBones)
            {
                return false;
            }
            if(!RV2Mod.Settings.features.ScatEnabled && isScat)
            {
                return false;
            }
            return true;
        }

        public bool ProvidesForcedResource() => forcedProducedThings?.Count > 0 == true;
    }
}
