using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class RitualRoleColonyAnimal : RitualRole
    {
        public override bool Animal => true;
        public override bool AppliesToPawn(Pawn p, out string reason, TargetInfo selectedTarget, LordJob_Ritual ritual = null, RitualRoleAssignments assignments = null, Precept_Ritual precept = null, bool skipReason = false)
        {
            reason = null;
            bool isColonyAnimal = p.IsAnimal() && p.Faction.IsPlayerSafe();
            if(!isColonyAnimal)
            {
                if(!skipReason)
                    reason = "MessageRitualRoleMustBeColonistOrColonyAnimal".Translate(base.Label);
                return false;
            }

            return true;
        }

        public override bool AppliesToRole(Precept_Role role, out string reason, Precept_Ritual ritual = null, Pawn pawn = null, bool skipReason = false)
        {
            reason = null;
            return false;
        }
    }
}
