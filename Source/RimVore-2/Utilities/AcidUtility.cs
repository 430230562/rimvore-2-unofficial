using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimVore2
{
    public static class AcidUtility
    {
        public static DamageDef GetDigestDamageDef(VoreTrackerRecord record)
        {
            List<RollAction> actions = RollUtility.GetAllRollActions(record.VorePath.def, (VoreStageDef stage) => stage.onEnd);
            return ((RollAction_Digest)actions
                .Find(a => a is RollAction_Digest))
                ?.damageDef;
        }

        public static DamageDef GetDigestDamageDef(VoreStageDef stage)
        {
            List<RollAction> actions = RollUtility.GetAllRollActions(stage, (VoreStageDef s) => s.onEnd);
            return ((RollAction_Digest)actions
                .Find(a => a is RollAction_Digest))
                ?.damageDef;
        }

        public static void ApplyAcidByDigestionProgress(VoreTrackerRecord record, float progress, DamageDef appliedDamageDef)
        {
            DestroyBodyPartsByDigestionProgress(record, progress, appliedDamageDef);
            SpreadAcidAcrossBodyByDigestionProgress(record, progress, appliedDamageDef);
            bool isDigestionFinished = progress == 1f;
            if(isDigestionFinished)
                DigestionUtility.FinishDigestion(record, appliedDamageDef);
        }

        private static void DestroyBodyPartsByDigestionProgress(VoreTrackerRecord record, float progress, DamageDef appliedDamageDef)
        {
            Pawn prey = record.Prey;
            // 0 = 6, 0.2 = 5, 0.4 = 4, 0.6 = 3, 0.8 = 2, 1 = 1; 
            int depth = 6 - (int)Math.Round(progress / 0.2f);
            if(depth >= 6)
            {
                return;
            }
            // if digestion is not finished, protect vital parts from being destroyed
            //bool shouldProtectVitals = progress != 1f;
            // destroying vital body parts sadly leads to death messages reporting death due to missing parts rather than acid damage, which is what we actually want, not destroying the vital parts is a band-aid fix
            bool shouldProtectVitals = true;
            if(RV2Log.ShouldLog(false, "Bodyparts"))
                RV2Log.Message($"Calculated destruction depth of {depth}", "BodyParts");
            DestroyBodyPartsByDepth(prey, depth, appliedDamageDef, shouldProtectVitals);
        }

        private static void SpreadAcidAcrossBodyByDigestionProgress(VoreTrackerRecord record, float progress, DamageDef appliedDamageDef)
        {
            Pawn prey = record.Prey;
            float applicableAcidDamage = DamageUtility.GetAvailableDamageUntilLethal(prey) * progress;
            // leave at least 20% to lethal damage so the game has some room to do dumb stuff to the damage
            applicableAcidDamage -= 0.2f * prey.health.LethalDamageThreshold;
            if(applicableAcidDamage <= 0f)
            {
                return;
            }
            if(RV2Log.ShouldLog(false, "Bodyparts"))
                RV2Log.Message($"Applying {applicableAcidDamage} damage to rest of body", "BodyParts");
            SpreadAcidAcrossBody(record, applicableAcidDamage, appliedDamageDef);
        }

        private static void SpreadAcidAcrossBody(VoreTrackerRecord record, float damage, DamageDef appliedDamageDef)
        {
            Pawn prey = record.Prey;
            Pawn predator = record.Predator;
            List<BodyPartRecord> bodyParts = BodyPartUtility.GetAllAcidVulnerableBodyParts(prey);
            float bodyPartDamage = damage / bodyParts.Count;
            DamageInfo dinfo = new DamageInfo(appliedDamageDef, bodyPartDamage, appliedDamageDef.defaultArmorPenetration, -1, predator);
            foreach(BodyPartRecord bodyPart in bodyParts)
            {
                dinfo.SetAmount(CalculateNonDestructiveDamageToPart(prey, bodyPart, bodyPartDamage));
                ApplyAcidToBodyPart(prey, dinfo, bodyPart);
            }
        }

        private static float CalculateNonDestructiveDamageToPart(Pawn pawn, BodyPartRecord bodyPart, float plannedDamage)
        {
            float bodyPartHealth = pawn.health.hediffSet.GetPartHealth(bodyPart);
            float bodyPartPityHealth = bodyPart.def.hitPoints * RV2Mod.Settings.cheats.AcidPityHealthFactor;
            float damageWithoutDestroying = Math.Min(plannedDamage, bodyPartHealth - bodyPartPityHealth);
            return Mathf.Clamp(damageWithoutDestroying, 0f, bodyPartHealth);
        }

        private static void ApplyAcidToBodyPart(Pawn pawn, DamageInfo dinfo, BodyPartRecord bodyPart)
        {
            dinfo.SetHitPart(bodyPart);
            pawn.TakeDamage(dinfo);
        }

        private static void DestroyBodyPartsByDepth(Pawn pawn, int depth, DamageDef appliedDamageDef = null, bool shouldProtectVitals = true, bool canRollForSave = true)
        {
            List<BodyPartRecord> bodyParts = BodyPartUtility.GetAllAcidVulnerableBodyParts(pawn);
            foreach(BodyPartRecord bodyPart in bodyParts)
            {
                // a parent body part might already be missing while we loop through the parts, skip if the body part is already gone
                if(pawn.health.hediffSet.PartIsMissing(bodyPart))
                {
                    continue;
                }
                int bodyPartDepth = BodyPartUtility.GetBodyPartDepth(bodyPart);
                if(bodyPartDepth >= depth)
                {
                    if(canRollForSave)
                    {
                        if(Rand.Chance(RV2Mod.Settings.cheats.AcidSavingThrowChance))
                        {
                            if(RV2Log.ShouldLog(false, "Bodyparts"))
                                RV2Log.Message($"Body part {bodyPart.Label} had a successful saving throw and was not destroyed", "BodyParts");
                            continue;
                        }
                    }
                    bool isVital = BodyPartUtility.IsVitalOrHasVitalChildren(bodyPart);
                    if(RV2Log.ShouldLog(false, "Bodyparts"))
                        RV2Log.Message($"part {bodyPart.Label} is vital or has vital children ? {isVital}", "BodyParts");
                    if(isVital && shouldProtectVitals)
                    {
                        if(RV2Log.ShouldLog(false, "Bodyparts"))
                            RV2Log.Message($"Preventing vital body part {bodyPart.Label} from being destroyed", "BodyParts");
                        float damage = CalculateNonDestructiveDamageToPart(pawn, bodyPart, 999f);
                        pawn.TakeDamage(new DamageInfo(appliedDamageDef, damage, 999f, -1, null, bodyPart));
                    }
                    else
                    {
                        Hediff_MissingPart missingPartHediff = (Hediff_MissingPart)HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, pawn, bodyPart);
                        missingPartHediff.lastInjury = appliedDamageDef.hediff;
                        // don't want bleeding, tend it immediately
                        missingPartHediff.Tended(0.2f, 0.2f);
                        pawn.health.AddHediff(missingPartHediff);
                    }
                }
            }
        }
    }
}
