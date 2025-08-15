using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace RimVore2
{
    public class RollAction_ShowMote : RollAction
    {
        FleckDef mote;

        public RollAction_ShowMote()
        {
            target = VoreRole.Predator;
        }

        public override bool TryAction(VoreTrackerRecord record, float rollStrength)
        {
            base.TryAction(record, rollStrength);
            if(!TargetPawn.Spawned || mote == null)
            {
                return false;
            }
            FleckMaker.ThrowMetaIcon(TargetPawn.Position, TargetPawn.Map, mote);
            return true;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if(mote == null)
            {
                yield return "Required field \"mote\" not set";
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref mote, "mote");
        }
    }
}
