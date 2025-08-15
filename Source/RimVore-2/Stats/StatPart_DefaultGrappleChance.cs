using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class StatPart_DefaultGrappleChance : StatPart
    {
        public override string ExplanationPart(StatRequest req)
        {
            return $"{"RV2_StatsReport_DefaultGrappleChance".Translate()}: {RV2Mod.Settings.combat.GrappleVerbSelectionBaseChance}";
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            val *= RV2Mod.Settings.combat.GrappleVerbSelectionBaseChance;
        }
    }
}
