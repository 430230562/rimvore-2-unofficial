using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace RimVore2
{
    public abstract class RollAction_CreateThings : RollAction_PassValue_Subtract
    {
        protected abstract List<ThingDef> ThingDefsToCreate { get; }

        float requiredNutrition = 1f;
        int quantityPerRequiredNutrition = 1;
        bool reducePreyNutrition = true;

        float nutritionReserve = 0;

        public RollAction_CreateThings() : base() { }

        const string PreyNutritionPassValueName = "PreyNutrition";
        public override bool TryAction(VoreTrackerRecord record, float rollStrength)
        {
            //base.TryAction(record, rollStrength);
            float currentPreyNutrition = record.PassValues.TryGetValue(PreyNutritionPassValueName, 0f);
            int producedThingCount;
            // reduce rollstrength if strength is higher than leftover nutrition
            rollStrength = Math.Min(currentPreyNutrition, rollStrength);
            base.TryAction(record, rollStrength);
            nutritionReserve += rollStrength;
            if(requiredNutrition > 0f)
            {
                producedThingCount = (int)Math.Floor(nutritionReserve / requiredNutrition);
            }
            else
            {
                producedThingCount = 1;
            }
            if(RV2Log.ShouldLog(true, "OngoingVore"))
                RV2Log.Message($"Created {producedThingCount} items, current nutrition reserves: {nutritionReserve}", false, "OngoingVore");
            producedThingCount *= quantityPerRequiredNutrition;
            nutritionReserve -= producedThingCount * requiredNutrition;
            if(producedThingCount <= 0)
            {
                return false;
            }
            if(reducePreyNutrition)
            {
                base.TryAction(record, rollStrength);
            }
            if(ThingDefsToCreate.NullOrEmpty())
            {
                RV2Log.Warning("Tried to create items, but list of items to create is empty", "OngoingVore");
                return false;
            }
            producedThingCount = Mathf.CeilToInt(producedThingCount / ThingDefsToCreate.Count());
            foreach(ThingDef thingDef in ThingDefsToCreate)
            {
                Thing createdThing = CreateThing(thingDef, producedThingCount);
                if(RV2Log.ShouldLog(true, "OngoingVore"))
                    RV2Log.Message($"Created {createdThing.def.label}, stackCount: {createdThing.stackCount}", false, "OngoingVore");
                record.VoreContainer.TryAddOrTransfer(createdThing);
            }
            return true;
        }

        protected Thing CreateThing(ThingDef thingDef, int stackCount)
        {
            Thing thing = ThingMaker.MakeThing(thingDef);
            thing.stackCount = stackCount;
            return thing;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if(invert)
            {
                yield return "Cannot use \"invert\" for thing creation";
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref requiredNutrition, "requiredNutrition");
            Scribe_Values.Look(ref nutritionReserve, "nutritionReserve");
            Scribe_Values.Look(ref quantityPerRequiredNutrition, "quantityPerRequiredNutrition");
            Scribe_Values.Look(ref reducePreyNutrition, "reducePreyNutrition");
        }
    }
}
