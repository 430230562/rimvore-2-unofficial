#if v1_5
using LudeonTK;
#endif
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
    public static class CombatUtility
    {
        static SimpleCurve SizeToGrappleStrengthCurve = new SimpleCurve
        {
            Points =
            {
                new CurvePoint(0f, 0.5f),
                new CurvePoint(1f, 4f),
                new CurvePoint(4f, 16f)
            }
        };
        private static int MeleeSkillDivider => RV2Mod.Settings.combat.GrappleStrengthMeleeSkillDivider;

        public static float GetGrappleStrength(Pawn pawn, bool isAttacker)
        {
            // initialize quirks if unitialized to allow stat to work properly
            pawn.QuirkManager();

            StatDef grappleStat = isAttacker ? VoreStatDefOf.RV2_GrappleStrength_Attacker : VoreStatDefOf.RV2_GrappleStrength_Defender;
            return pawn.GetStatValue(grappleStat);

            //float value = SizeToGrappleStrengthCurve.Evaluate(pawn.BodySize);
            //string reportString = $"Base body size value: {value} ({pawn.BodySize})";
            //SkillRecord meleeSkill = pawn.skills?.GetSkill(SkillDefOf.Melee);
            //if(meleeSkill != null)
            //{
            //    float meleeSkillMultiplier = (float)meleeSkill.Level / (float)MeleeSkillDivider;
            //    value *= meleeSkillMultiplier;
            //    reportString += $" x MeleeSkillMultiplier {meleeSkillMultiplier}({meleeSkill.Level} / {MeleeSkillDivider})";
            //}
            //PawnCapacitiesHandler capacities = pawn.health?.capacities;
            //if(capacities != null)
            //{
            //    float manipulationCapacityMultiplier = capacities.GetLevel(PawnCapacityDefOf.Manipulation);
            //    value *= manipulationCapacityMultiplier;
            //    reportString += $" x ManipulationMultiplier {manipulationCapacityMultiplier}";
            //}
            //GrappleRole role = GeneralUtility.BoolToGrappleRole(isAttacker);
            //if(TryGetTraitStrengthMultiplier(pawn, role, ref reportString, out float traitMultiplier))
            //    value *= traitMultiplier;

            //if(TryGetRaceStrengthMultiplier(pawn, role, ref reportString, out float raceMultiplier))
            //    value *= raceMultiplier;

            //if(TryGetQuirkStrengthMultiplier(pawn, isAttacker, ref reportString, out float quirkMultiplier))
            //    value *= quirkMultiplier;

            //reportString += $" = {value}";
            //if(RV2Log.ShouldLog(false, "VoreCombatGrapple"))
            //    RV2Log.Message(reportString, true, "VoreCombatGrapple");
            //return value;
        }

        //private static bool TryGetTraitStrengthMultiplier(Pawn pawn, GrappleRole role, ref string reportString, out float multiplier)
        //{
        //    multiplier = 1f;
        //    IEnumerable<Trait> traits = pawn.story?.traits?.allTraits;
        //    if(traits.EnumerableNullOrEmpty())
        //        return false;
        //    foreach(Trait trait in traits)
        //    {
        //        Extension_GrappleInfluence grappleExtension = trait.def.GetModExtension<Extension_GrappleInfluence>();
        //        if(grappleExtension == null)
        //            continue;
        //        float extensionMultiplier = grappleExtension.StrengthMultiplierForTraits(role, trait.Degree);
        //        reportString += $" x Trait {extensionMultiplier}({trait.Label})";
        //        multiplier *= extensionMultiplier;
        //    }
        //    return true;
        //}

        //private static bool TryGetQuirkStrengthMultiplier(Pawn pawn, bool isAttacker, ref string reportString, out float multiplier)
        //{
        //    multiplier = 1f;
        //    QuirkManager quirks = pawn.QuirkManager(false);
        //    if(quirks == null)
        //        return false;
        //    // get the multiplier that applies no matter what role in the grapple the pawn plays
        //    if(quirks.TryGetValueModifier("GrappleStrength", ModifierOperation.Multiply, out float strengthMultiplier))
        //    {
        //        reportString += $" x Quirks (GrappleStrength) {strengthMultiplier}";
        //        multiplier *= strengthMultiplier;
        //    }
        //    // then get the multipliers for attacker / defender and apply it to the base multiplier
        //    if(isAttacker && quirks.TryGetValueModifier("GrappleStrengthAsAttacker", ModifierOperation.Multiply, out float attackerMultiplier))
        //    {
        //        reportString += $" x Quirks (GrappleStrengthAsAttacker) {attackerMultiplier}";
        //        multiplier *= attackerMultiplier;
        //    }
        //    else if(!isAttacker && quirks.TryGetValueModifier("GrappleStrengthAsDefender", ModifierOperation.Multiply, out float defenderMultiplier))
        //    {
        //        reportString += $" x Quirks (GrappleStrengthAsDefender) {defenderMultiplier}";
        //        multiplier *= defenderMultiplier;
        //    }
        //    return true;
        //}

        //private static bool TryGetRaceStrengthMultiplier(Pawn pawn, GrappleRole role, ref string reportString, out float multiplier)
        //{
        //    multiplier = 1f;
        //    Extension_GrappleInfluence grappleExtension = pawn.def.GetModExtension<Extension_GrappleInfluence>();
        //    if(grappleExtension == null)
        //        return false;
        //    multiplier = grappleExtension.StrengthMultiplierForRace(role);
        //    reportString += $" x Race {multiplier}";
        //    return true;
        //}

        // not as complicated as strength calculations, so rolling all in one method
        public static float GetGrappleChance(Pawn pawn, bool isAttacker)
        {
            // initialize quirks if unitialized to allow stat to work properly
            pawn.QuirkManager();

            return pawn.GetStatValue(VoreStatDefOf.RV2_GrappleChance);

            //PawnData pawnData = pawn.PawnData();
            //if(pawnData == null || !pawnData.CanUseGrapple)
            //{
            //    return 0;
            //}
            //float chance = RV2Mod.Settings.combat.GrappleVerbSelectionBaseChance;
            //QuirkManager quirks = pawn.QuirkManager(false);
            //if(quirks != null && quirks.TryGetValueModifier("GrappleChance", ModifierOperation.Multiply, out float quirkMultiplier))
            //{
            //    chance *= quirkMultiplier;
            //}
            //Extension_GrappleInfluence raceExtension = pawn.def.GetModExtension<Extension_GrappleInfluence>();
            //GrappleRole role = GeneralUtility.BoolToGrappleRole(isAttacker);
            //if(raceExtension != null)
            //{
            //    chance *= raceExtension.ChanceMultiplierForRace(role);
            //}
            //List<Trait> traits = pawn.story?.traits?.allTraits;
            //if(!traits.NullOrEmpty())
            //{
            //    foreach(Trait trait in traits)
            //    {
            //        Extension_GrappleInfluence traitExtension = trait.def.GetModExtension<Extension_GrappleInfluence>();
            //        if(traitExtension == null)
            //            continue;
            //        chance *= traitExtension.ChanceMultiplierForTraits(role, trait.Degree);
            //    }
            //}

            //// cap the chance to prevent odd game behaviour
            //chance = Math.Min(chance, RV2Mod.Settings.combat.GrappleVerbSelectionMaxChance);

            //return chance;
        }

        public static bool IsInvolvedInGrapple(Pawn pawn)
        {
            return pawn.CurJobDef == VoreJobDefOf.RV2_VoreGrapple
                || pawn.stances?.curStance is Stance_Grapple;
        }

        /// <summary>
        /// General eligibility check, target may be null for pawn-only validation
        /// </summary>
        public static bool CanGrapple(Pawn pawn, out string reason, Pawn target = null)
        {
            if(!pawn.Tools.Any(tool => tool.capacities.Contains(RV2_Common.VoreGrappleToolCapacity)))
            {
                RV2Log.Warning($"Race {pawn.def.LabelCap} does not have grapple tool, this is most likely caused by the race mod creator not inheriting tools from BasePawn.", true);
                reason = "RV2_GrappleInvalid_RaceUnable".Translate(pawn.def.LabelCap);
                return false;
            }
            if(IsInvolvedInGrapple(pawn))
            {
                reason = "RV2_GrappleInvalid_AlreadyGrappling".Translate(pawn.LabelShortCap);
                return false;
            }
            if(target != null && IsInvolvedInGrapple(target))
            {
                reason = "RV2_GrappleInvalid_AlreadyGrappling".Translate(target.LabelShortCap);
                return false;
            }
            if(GetGrappleChance(pawn, true) <= 0)
            {
                reason = "RV2_GrappleInvalid_ChanceIsZero".Translate(pawn.LabelShortCap);
                return false;
            }
            reason = null;
            return true;
        }

        [DebugAction("RimVore-2", "Log grapple strength", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void CallGrappleStrength(Pawn p)
        {
            GetGrappleStrength(p, true);
        }
        [DebugAction("RimVore-2", "Log grapple defense", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void CallGrappleDefense(Pawn p)
        {
            GetGrappleStrength(p, false);
        }
    }

}
