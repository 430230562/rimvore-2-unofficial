using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace RimVore2
{
    public class RollAction_CreateThingsFromDef : RollAction_CreateThings
    {
        List<ThingDef> things;
        protected override List<ThingDef> ThingDefsToCreate => things;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if(things.NullOrEmpty())
            {
                yield return "required list \"things\" is not set or empty";
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref things, "things", LookMode.Def);
        }
    }
}
