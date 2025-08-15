using System;
using System.Collections.Generic;
using Verse;

namespace RimVore2
{
    public class QuirkModifier : IExposable
    {
        public float modifierValue = float.MinValue;
        public string modifierName;

        public QuirkModifier() { }

        //public abstract void ApplyModifier(VoreTrackerRecord record);

        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref modifierValue, "modifierValue");
            Scribe_Values.Look(ref modifierName, "modifierName");
        }
    }
}
