using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class StatPart_GrappleMeleeSkill : StatPart
    {
        private static int MeleeSkillDivider => RV2Mod.Settings.combat.GrappleStrengthMeleeSkillDivider;

        public override string ExplanationPart(StatRequest req)
        {
            Pawn pawn = req.Thing as Pawn;
            if(pawn == null)
            {
                return null;
            }
            SkillRecord meleeSkill = pawn.skills?.GetSkill(SkillDefOf.Melee);
            if(meleeSkill == null)
            {
                return null;
            }
            return $"{"RV2_StatsReport_InfluencedMeleeImpact".Translate()}: ({meleeSkill.Level}[Skill] / {MeleeSkillDivider}[Global Modifier]) -> x {((float)meleeSkill.Level / (float)MeleeSkillDivider).ToStringByStyle(ToStringStyle.PercentZero)}";
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            Pawn pawn = req.Thing as Pawn;
            if(pawn == null)
            {
                return;
            }
            SkillRecord meleeSkill = pawn.skills?.GetSkill(SkillDefOf.Melee);
            if(meleeSkill == null)
            {
                return;
            }
            float meleeSkillMultiplier = (float)meleeSkill.Level / (float)MeleeSkillDivider;
            val *= meleeSkillMultiplier;
        }
    }
}
