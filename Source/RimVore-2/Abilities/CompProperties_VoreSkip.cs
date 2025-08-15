using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class AbilityExtension_MultiTarget : DefModExtension
    {
        public int requiredTargets;
        public bool canPickSameTargetMultipleTimes;
    }
    public class AbilityExtension_VoreSkip : AbilityExtension_MultiTarget
    {
        public bool allowChoosingBodyPart = false;
        public bool allowChoosingVoreGoal = false;
    }

    /// <summary>
    /// I just want to store data in a comp, thanks.
    /// </summary>
    public class CompAbilityEffect_None : CompAbilityEffect
    {

    }
}
