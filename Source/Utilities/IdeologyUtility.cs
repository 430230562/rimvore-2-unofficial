using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public static class IdeologyUtility
    {
        public static bool IsIdeoPredator(Pawn pawn)
        {
            if(!ModsConfig.IdeologyActive)
            {
                return false;
            }
            return pawn.Ideo?.GetRole(pawn)?.def == RV2_Common.IdeoRolePredator;
        }
        public static bool IsIdeoPrey(Pawn pawn)
        {
            if(!ModsConfig.IdeologyActive)
            {
                return false;
            }
            return pawn.Ideo?.GetRole(pawn)?.def == RV2_Common.IdeoRolePrey;
        }
        public static bool IsIdeoFeeder(Pawn pawn)
        {
            if(!ModsConfig.IdeologyActive)
            {
                return false;
            }
            return pawn.Ideo?.GetRole(pawn)?.def == RV2_Common.IdeoRoleFeeder;
        }
    }
}