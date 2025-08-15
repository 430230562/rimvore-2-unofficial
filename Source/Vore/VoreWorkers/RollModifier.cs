using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace RimVore2
{
    public abstract class RollModifier : IExposable
    {
        public int priority = -1;

        public RollModifier() { }

        public ModifierOperation operation = ModifierOperation.Multiply;

        protected abstract bool TryGetModifier(VoreTrackerRecord record, out float modifier);

        /// <summary>
        /// Used to calculate the modifier impact without a VoreTrackerRecord, which is used to guess the duration a VorePath takes.
        /// </summary>
        public abstract float AbstractModifyValue(float value);

        public virtual float ModifyValue(float value, VoreTrackerRecord record)
        {
            if(TryGetModifier(record, out float modifier))
            {
                float newValue = operation.Aggregate(value, modifier);
                if(RV2Log.ShouldLog(true, "OngoingVore"))
                    RV2Log.Message(Explain(value, modifier, newValue), true, "OngoingVore");
                return newValue;
            }
            return value;
        }

        protected virtual string Explain(float oldValue, float modifier, float newValue)
        {
            return this.ToString() + " modified " + oldValue + " with modifier " + modifier + " for new value " + newValue + " operation: " + operation;
        }

        public virtual IEnumerable<string> ConfigErrors()
        {
            if(operation == ModifierOperation.Invalid)
            {
                yield return "Required field \"operation\" must be set";
            }
            if(operation == ModifierOperation.Set && priority == -1)
            {
                yield return "When using \"operation\" Set, the field \"priority\" is required";
            }
        }
        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref operation, "operation");
        }
    }

    public class RollModifier_Clamp : RollModifier
    {
        private float minValue = float.MinValue;
        private float maxValue = float.MaxValue;

        protected override bool TryGetModifier(VoreTrackerRecord record, out float modifier)
        {
            // clamp has no modifier
            modifier = 0;
            return true;
        }

        public override float ModifyValue(float value, VoreTrackerRecord record)
        {
            return value.LimitClamp(minValue, maxValue);
        }

        public override float AbstractModifyValue(float value)
        {
            return ModifyValue(value, null);
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if(minValue == float.MinValue && maxValue == float.MaxValue)
            {
                yield return "Clamp should set either \"minValue\" or \"maxValue\"!";
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref minValue, "minValue");
            Scribe_Values.Look(ref maxValue, "maxValue");
        }
    }

    public class RollModifier_Value : RollModifier
    {
        float value;

        protected override bool TryGetModifier(VoreTrackerRecord record, out float modifier)
        {
            modifier = value;
            return true;
        }

        public override float AbstractModifyValue(float value)
        {
            return ModifyValue(value, null);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref value, "value");
        }
    }

    public class RollModifier_RandomValue : RollModifier
    {
        FloatRange valueRange;

        protected override bool TryGetModifier(VoreTrackerRecord record, out float modifier)
        {
            modifier = valueRange.RandomInRange;
            return true;
        }

        public override float AbstractModifyValue(float value)
        {
            float assumedValue = valueRange.Average;
            return operation.Aggregate(value, assumedValue);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref valueRange, "valueRange");
        }
    }

    public class RollModifier_VoreSpeed : RollModifier
    {
        protected override bool TryGetModifier(VoreTrackerRecord record, out float modifier)
        {
            modifier = RV2Mod.Settings.cheats.VoreSpeedMultiplier;
            return true;
        }

        public override float AbstractModifyValue(float value)
        {
            return value;
        }
    }

    public class RollModifier_Size : RollModifier
    {
        VoreRole target;

        protected override bool TryGetModifier(VoreTrackerRecord record, out float modifier)
        {
            modifier = 1f;
            Pawn pawn = record.GetPawnByRole(target);
            if(pawn == null)
                return false;
            modifier = pawn.BodySize;
            return true;
        }

        public override float AbstractModifyValue(float value)
        {
            return value;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
                yield return error;
            if(target == VoreRole.Invalid)
                yield return $"Required field \"{nameof(target)}\" is not set";
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref target, "target");
        }
    }

    public class RollModifier_SizeDifference : RollModifier
    {
        VoreRole pawn;
        VoreRole relationTo;

        protected override bool TryGetModifier(VoreTrackerRecord record, out float modifier)
        {
            Pawn pawn1 = record.GetPawnByRole(pawn);
            Pawn pawn2 = record.GetPawnByRole(relationTo);
            modifier = pawn1.BodySize / pawn2.BodySize;
            return true;
        }

        public override float AbstractModifyValue(float value)
        {
            return value;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if(pawn == VoreRole.Invalid)
            {
                yield return "Required field \"pawn\" must be set";
            }
            if(relationTo == VoreRole.Invalid)
            {
                yield return "Required field \"relationTo\" must be set";
            }
            if(pawn == relationTo)
            {
                yield return "Setting \"pawn\" and \"relationTo\" to the same target will always result in 1";
            }
        }

        protected override string Explain(float oldValue, float modifier, float newValue)
        {
            return base.Explain(oldValue, modifier, newValue) + pawn + " / " + relationTo;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref pawn, "pawn");
            Scribe_Values.Look(ref relationTo, "relationTo");
        }
    }

    public abstract class TargetedRollModifier : RollModifier
    {
        VoreRole target;

        protected Pawn TargetPawn(VoreTrackerRecord record)
        {
            return record.GetPawnByRole(target);
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if(target == VoreRole.Invalid)
            {
                yield return "Required field \"target\" not set";
            }
        }

        protected override string Explain(float oldValue, float modifier, float newValue)
        {
            return base.Explain(oldValue, modifier, newValue) + " target: " + target;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref target, "target");
        }
    }

    public class RollModifier_FoodFallRate : TargetedRollModifier
    {
        protected override bool TryGetModifier(VoreTrackerRecord record, out float modifier)
        {
            Pawn pawn = TargetPawn(record);
            if(pawn.needs?.food == null)
            {
                modifier = 0;
                return false;
            }
            modifier = pawn.GetHungerRate() * 250;
            return true;
        }

        public override float AbstractModifyValue(float value)
        {
            const float baseFoodFallRate = 2.6666667E-05f;  // copied from base game, they have a constant somewhere, this merely needs to guess
            return operation.Aggregate(value, baseFoodFallRate);
        }
    }

    public class RollModifier_Capacity : TargetedRollModifier
    {
        PawnCapacityDef capacity;

        protected override bool TryGetModifier(VoreTrackerRecord record, out float modifier)
        {
            Pawn pawn = TargetPawn(record);
            if(pawn.health.capacities != null)
            {
                modifier = pawn.health.capacities.GetLevel(capacity);
                return true;
            }
            modifier = 0;
            return false;
        }

        protected override string Explain(float oldValue, float modifier, float newValue)
        {
            return base.Explain(oldValue, modifier, newValue) + " capacity: " + capacity.defName;
        }

        public override float AbstractModifyValue(float value)
        {
            return value;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if(capacity == null)
            {
                yield return "Required field \"capacity\" not set";
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref capacity, "capacity");
        }
    }

    public class RollModifier_Quirk : TargetedRollModifier
    {
        string modifierName;

        protected override bool TryGetModifier(VoreTrackerRecord record, out float modifier)
        {
            Pawn pawn = TargetPawn(record);
            QuirkManager pawnQuirks = pawn.QuirkManager(false);
            if(pawnQuirks == null)
            {
                modifier = operation.DefaultModifierValue();
                return false;
            }
            if(!pawnQuirks.HasValueModifier(modifierName))
            {
                if(RV2Log.ShouldLog(true, "OngoingVore"))
                    RV2Log.Message($"pawn {pawn.LabelShort} has no modifiers for {modifierName}", true, "OngoingVore");
                modifier = operation.DefaultModifierValue();
                return false;
            }
            return pawnQuirks.TryGetValueModifier(modifierName, operation, out modifier);
        }

        protected override string Explain(float oldValue, float modifier, float newValue)
        {
            return base.Explain(oldValue, modifier, newValue) + " modifierName: " + modifierName;
        }

        public override float AbstractModifyValue(float value)
        {
            return value;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if(modifierName == null)
            {
                yield return "Required field \"modifierName\" not set";
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref modifierName, "modifierName");
        }
    }

    public class RollModifier_Stat : TargetedRollModifier
    {
        StatDef stat;

        protected override bool TryGetModifier(VoreTrackerRecord record, out float modifier)
        {
            Pawn pawn = TargetPawn(record);
            modifier = pawn.GetStatValue(stat);
            return true;
        }

        protected override string Explain(float oldValue, float modifier, float newValue)
        {
            return base.Explain(oldValue, modifier, newValue) + " stat: " + stat.defName;
        }

        public override float AbstractModifyValue(float value)
        {
            return value;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if(stat == null)
            {
                yield return "Required field \"stat\" not set";
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref stat, "stat");
        }
    }
}
