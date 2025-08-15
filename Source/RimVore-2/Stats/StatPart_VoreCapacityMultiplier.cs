using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimVore2
{
    public class StatPart_VoreCapacityMultiplier : StatPart
    {
        public override string ExplanationPart(StatRequest req)
        {
            return $"{"RV2_StatsReport_VoreCapacityMultiplier".Translate()}: {RV2Mod.Settings.cheats.BodySizeToVoreCapacity.ToStringPercentEmptyZero()}";
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            val *= RV2Mod.Settings.cheats.BodySizeToVoreCapacity;
        }
    }
}
