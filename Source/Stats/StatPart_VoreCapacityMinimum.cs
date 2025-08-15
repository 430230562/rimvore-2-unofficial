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
    public class StatPart_VoreCapacityMinimum : StatPart
    {
        public override string ExplanationPart(StatRequest req)
        {
            float minValue = RV2Mod.Settings.cheats.MinimumVoreCapacity;
            if(minValue == default(float))
            {
                return null;
            }
            return $"{"RV2_StatsReport_VoreCapacityMinimum".Translate()}: {minValue}";
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            float minValue = RV2Mod.Settings.cheats.MinimumVoreCapacity;
            if(minValue == default(float))
            {
                return;
            }

            Mathf.Max(val, minValue);
        }
    }
}
