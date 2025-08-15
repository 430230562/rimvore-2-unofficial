using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace RimVore2
{
    public abstract class RollAction_PassValue : RollAction
    {
        public string name;
        protected float minValue = float.MinValue;
        protected float maxValue = float.MaxValue;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref name, "name");
            Scribe_Values.Look(ref minValue, "minValue");
            Scribe_Values.Look(ref maxValue, "maxValue");
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if(name == null)
            {
                yield return "Required field \"name\" not provided";
            }
            if(minValue > maxValue)
            {
                yield return "field \"minValue\" is larger than field \"maxValue\"";
            }
        }
    }
}
