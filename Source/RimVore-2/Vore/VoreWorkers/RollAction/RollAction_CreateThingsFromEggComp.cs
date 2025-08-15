using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace RimVore2
{
    public class RollAction_CreateThingsFromEggComp : RollAction_CreateThings
    {
        protected override List<ThingDef> ThingDefsToCreate
        {
            get
            {
                return new List<ThingDef>() { GetEggDef(record.Predator) };
            }
        }

        /// <summary>
        /// - no egg comp -> fallback
        /// - no fertilized egg in comp -> unfertilized egg 
        /// - no unfertilized egg in comp -> fallback
        /// </summary>
        private ThingDef GetEggDef(Pawn pawn)
        {
            CompEggLayer eggComp = record.Predator.TryGetComp<CompEggLayer>();
            ThingDef fallbackDef = ThingDef.Named("EggChickenUnfertilized");
            if(eggComp == null)
            {
                if(RV2Log.ShouldLog(true, "OngoingVore"))
                    RV2Log.Message($"No CompEggLayer, using fallback: {fallbackDef.defName}", false, "OngoingVore");
                return fallbackDef;
            }
            ThingDef eggDef = eggComp.Props.eggUnfertilizedDef;
            if(eggDef != null)
            {
                if(RV2Log.ShouldLog(true, "OngoingVore"))
                    RV2Log.Message("Found CompEggLayer with unfertilized egg def", false, "OngoingVore");
                return eggDef;
            }
            eggDef = eggComp.Props.eggFertilizedDef;
            if(eggDef != null)
            {
                if(RV2Log.ShouldLog(true, "OngoingVore"))
                    RV2Log.Message($"Found CompEggLayer with fertilized egg def", false, "OngoingVore");
                return eggDef;
            }
            if(RV2Log.ShouldLog(true, "OngoingVore"))
                RV2Log.Message($"Found CompEggLayer with neither egg def, using fallback: {fallbackDef.defName}", false, "OngoingVore");
            return fallbackDef;
        }
    }
}
