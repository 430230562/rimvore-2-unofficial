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
    public class StageFailTrigger_PawnNotVored : StageFailTrigger
    {
        public override bool Failed(LordJob_Ritual ritual, TargetInfo spot, TargetInfo focus)
        {
            if(ritual.Ritual.behavior.def.roles.FirstOrDefault((RitualRole r) => r.id == preyId) == null)
            {
                if(RV2Log.ShouldLog(false, "Rituals"))
                    RV2Log.Message("Failed stage due to missing prey id", "Rituals");
                return true;
            }
            if(ritual.Ritual.behavior.def.roles.FirstOrDefault((RitualRole r) => r.id == predatorId) == null)
            {
                if(RV2Log.ShouldLog(false, "Rituals"))
                    RV2Log.Message("Failed stage due to missing predator id", "Rituals");
                return true;
            }
            Pawn prey = ritual.assignments.FirstAssignedPawn(this.preyId);
            Pawn predator = ritual.assignments.FirstAssignedPawn(this.predatorId);

            if(prey == null || predator == null)
            {
                if(RV2Log.ShouldLog(false, "Rituals"))
                    RV2Log.Message("Failed stage due to null prey or predator", "Rituals");
                return true;
            }
            return !prey.IsPreyOf(predator);
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