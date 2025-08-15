using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public abstract class RuleTargetStaleTrigger : IExposable
    {
        public abstract bool ShouldRemove();
        public abstract void ExposeData();
    }

    public class RuleTargetStaleTrigger_Never : RuleTargetStaleTrigger
    {
        public override bool ShouldRemove()
        {
            return false;
        }
        public override void ExposeData()
        {
            // nothing to scribe
        }
    }

    public class RuleTargetStaleTrigger_Timed_Rare : RuleTargetStaleTrigger
    {
        public int staleOnTick;
        public RuleTargetStaleTrigger_Timed_Rare(int rareTicks)
        {
            int tickLifeTime = rareTicks * GenTicks.TickRareInterval;
            staleOnTick = GenTicks.TicksAbs + tickLifeTime;
        }


        public override bool ShouldRemove()
        {
            return GenTicks.TicksAbs >= staleOnTick;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref staleOnTick, "staleOnTick");
        }
    }
}
