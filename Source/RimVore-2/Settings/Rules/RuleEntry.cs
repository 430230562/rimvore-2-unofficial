using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class RuleEntry : IExposable, ICloneable
    {
        public RuleTarget Target;
        public VoreRule Rule;
        public bool isEnabled = true;

        public RuleEntry() { }

        public RuleEntry(RuleTarget target, VoreRule rule)
        {
            Target = target;
            Rule = rule;
        }

        public void ExposeData()
        {
            Scribe_Deep.Look(ref Target, "Target", new object[0]);
            Scribe_Deep.Look(ref Rule, "Rule", new object[0]);
            Scribe_Values.Look(ref isEnabled, nameof(isEnabled), true);
        }

        public object Clone()
        {
            return new RuleEntry()
            {
                Target = (RuleTarget)this.Target.Clone(),
                Rule = (VoreRule)this.Rule.Clone()
            };
        }
    }
}
