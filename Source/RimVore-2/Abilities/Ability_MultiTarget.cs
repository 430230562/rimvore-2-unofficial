using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public abstract class Ability_MultiTarget : Ability
    {
        public Ability_MultiTarget() : base() { }
        public Ability_MultiTarget(Pawn pawn, Precept sourcePrecept) : base(pawn, sourcePrecept) { }
        public Ability_MultiTarget(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(pawn, sourcePrecept, def) { }
        public Ability_MultiTarget(Pawn pawn) : base(pawn) { }
        public Ability_MultiTarget(Pawn pawn, AbilityDef def) : base(pawn, def) { }

        public LocalTargetInfo initialTarget;
        public List<LocalTargetInfo> targets = new List<LocalTargetInfo>();

        public virtual Predicate<TargetInfo> AdditionalTargetValidator { get; }
        public virtual void Notify_AllTargetsPicked()
        {
            foreach(LocalTargetInfo target in targets)
            {
                // initial target is "activated" by base ability code
                if(target == initialTarget)
                    continue;
                Activate(target, verb.CurrentDestination);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_TargetInfo.Look(ref initialTarget, "initialTarget");
            Scribe_Collections.Look(ref targets, "targets", LookMode.LocalTargetInfo);
        }
    }
}
