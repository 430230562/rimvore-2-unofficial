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
    public class StatPart_MaxGrappleChance : StatPart
    {
        public override string ExplanationPart(StatRequest req)
        {
            return $"{"RV2_StatsReport_MaxGrappleChance".Translate()}: {RV2Mod.Settings.combat.GrappleVerbSelectionMaxChance}";
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            Mathf.Min(val, RV2Mod.Settings.combat.GrappleVerbSelectionBaseChance);
        }
    }
}
