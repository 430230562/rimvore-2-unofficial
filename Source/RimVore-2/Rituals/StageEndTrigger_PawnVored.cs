using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;

namespace RimVore2
{
    public class StageEndTrigger_PawnVored : StageEndTrigger
    {
        public override Trigger MakeTrigger(LordJob_Ritual ritual, TargetInfo spot, IEnumerable<TargetInfo> foci, RitualStage stage)
        {
            if(ritual.Ritual.behavior.def.roles.FirstOrDefault((RitualRole r) => r.id == preyId) == null)
            {
                return null;
            }
            if(ritual.Ritual.behavior.def.roles.FirstOrDefault((RitualRole r) => r.id == predatorId) == null)
            {
                return null;
            }
            Pawn prey = ritual.assignments.FirstAssignedPawn(this.preyId);
            Pawn predator = ritual.assignments.FirstAssignedPawn(this.predatorId);

            if(prey == null || predator == null)
            {
                return null;
            }
            return new Trigger_TickCondition(() => prey.IsPreyOf(predator), 1);
        }

        public override void ExposeData()
        {
            Scribe_Values.Look<string>(ref predatorId, "predatorId", null, false);
            Scribe_Values.Look<string>(ref preyId, "preyId", null, false);
        }

        [NoTranslate]
        public string predatorId;
        [NoTranslate]
        public string preyId;

    }
}