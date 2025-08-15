using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace RimVore2
{
    public class RollAction_PassValue_Set : RollAction_PassValue
    {
        private enum VorePassValueType
        {
            Value,
            PreyNutrition,
            DigestionProgress
        }
        protected float value = 0;

#pragma warning disable IDE0044 // Add readonly modifier
        private VorePassValueType type = VorePassValueType.Value;
#pragma warning restore IDE0044 // Add readonly modifier

        public override bool TryAction(VoreTrackerRecord record, float rollStrength)
        {
            base.TryAction(record, rollStrength);
            switch(type)
            {
                case VorePassValueType.Value:
                    record.SetPassValue(name, value);
                    return true;
                case VorePassValueType.PreyNutrition:
                    float preyNutritionValue = VoreCalculationUtility.CalculatePreyNutrition(record.Prey, record.Predator);
                    record.SetPassValue(name, preyNutritionValue);
                    return true;
                case VorePassValueType.DigestionProgress:
                    float digestionProgressValue = DigestionUtility.GetPreviousDigestionProgress(record.Prey);
                    if(digestionProgressValue > 0 && RV2Log.ShouldLog(false, "OngoingVore"))
                    {
                        RV2Log.Message($"Resuming digestion progress at {digestionProgressValue}", "OngoingVore");
                    }
                    record.SetPassValue(name, digestionProgressValue);
                    return true;
                default:
                    RV2Log.Error("Unknown VorePassValue type: " + type, "OngoingVore");
                    return false;
            }
        }

        public float AbstractValue()
        {
            switch(type)
            {
                case VorePassValueType.Value:
                    return value;
                case VorePassValueType.PreyNutrition:
                    return 5.2f;   // the value that base game values have
                case VorePassValueType.DigestionProgress:
                    return 0;
                default:
                    return 0;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref value, "value");
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if(type != VorePassValueType.Value && value != 0)
            {
                yield return "WARNING: Field \"value\" is set when \"type\" is not \"Value\". The currently set type will overwrite whichever \"value\" you set, consider removing \"value\" or using \"type\"=\"Value\" instead.";
            }
        }
    }
}
