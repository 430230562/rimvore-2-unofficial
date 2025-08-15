using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimVore2
{
    public static class DigestionUtility
    {
        public static void FinishDigestion(VoreTrackerRecord record, DamageDef appliedDamageDef)
        {
            if(RV2Log.ShouldLog(false, "OngoingVore"))
                RV2Log.Message("FinishDigestion called", "OngoingVore");
            Pawn prey = record.Prey;
            record.VoreContainer.FinishDigestion(prey, appliedDamageDef);
            DigestPawn(record, appliedDamageDef);
            record.VorePath.def.voreProduct?.AddVoreProducts(record);
            // add everything that was created (including the potential container) to the contained things again to excrete them
            //if(madeThings != null)
            //{
            //    record.VoreContainer.TryAddItems(madeThings);
            //}
        }

        private static void DigestPawn(VoreTrackerRecord record, DamageDef appliedDamageDef)
        {
            Pawn prey = record.Prey;
            // if the vore was done for a fatal vore ritual, do not cancel the ritual and flag the pawn as killed via ritual
            if(record.IsRitualRelated && record.VoreGoal.IsLethal)
            {
                (record.Predator.GetLord()?.LordJob as LordJob_Ritual)?.pawnsDeathIgnored?.Add(prey);
                prey.health.killedByRitual = true;
            }
            if(prey == null)
            {
                RV2Log.Warning("Can't finish prey, prey was null");
                return;
            }

            if(!prey.Dead)
            {
                KillPreyWithDamage(record, appliedDamageDef);
            }
            else
            {
                if(RV2Log.ShouldLog(false, "OngoingVore"))
                    RV2Log.Message("Prey already dead, could not kill", "OngoingVore");
            }

            Need_KillThirst killNeed = record.Predator?.needs?.TryGetNeed<Need_KillThirst>();
            if(killNeed != null)
            {
                killNeed.CurLevel = 1f;
            }

            VoreThoughtUtility.NotifyFatallyVored(record.Predator, record.Prey, record.VorePath.VoreGoal);

            // corpse creation has become a chore. The game doesn't register the pawn into the corpse, which leads to exceptions and bugged corpses in the end
            // this container shuffling should ensure proper encapsulation
            Corpse corpse = prey.Corpse;
            if(corpse == null)
            {
                RV2Log.Warning("Could not find corpse! Attempting to make a corpse out of the pawn");
                corpse = prey.MakeCorpse(prey.ownership?.AssignedGrave, prey.ownership?.OwnedBed);
                if(corpse == null)
                {
                    RV2Log.Warning($"Still couldn't make a corpse, skipping corpse processing");
                    return;
                }
            }
            corpse.GetDirectlyHeldThings().TryAddOrTransfer(prey);
            record.VoreContainer.TryAddOrTransfer(corpse);
            ProcessCorpse(record);
        }
        private static void ProcessCorpse(VoreTrackerRecord record)
        {
            Pawn prey = record.Prey;

            CorpseProcessingType type = RV2Mod.Settings.rules.GetCorpseProcessingType(record.Predator, record.VorePath.def);
            // mechanoids get special treatment
            if(prey.IsMechanoid())
            {
                DigestMechanoid(record);
                return;
            }
            if(RV2Mod.Settings.fineTuning.ForbidDigestedCorpse)
            {
                prey.Corpse.SetForbidden(true);
            }
            switch(type)
            {
                case CorpseProcessingType.Fresh:
                    return;
                case CorpseProcessingType.Destroy:
                    if(!prey.Corpse.Destroyed) prey.Corpse.Destroy();
                    break;
                case CorpseProcessingType.Rot:
                    DecomposePawn(prey, 0.5f);
                    break;
                case CorpseProcessingType.Dessicate:
                    DecomposePawn(prey);
                    break;
                default:
                    Log.Error("No CorpseProcessingType support for " + type);
                    break;
            }
        }

        private static void DigestMechanoid(VoreTrackerRecord record)
        {
            Pawn predator = record.Predator;
            Pawn prey = record.Prey;
            List<Thing> createdItems = new List<Thing>();
            int itemCount = (int)Mathf.Ceil(prey.BodySize);
            QuirkManager predatorQuirks = predator.QuirkManager();
            bool produceIntricateItems = predatorQuirks != null && predatorQuirks.HasSpecialFlag("NoTaintedApparel");
            // if we have a predator with the spotless digestion quirk, create steel and plasteel
            if(produceIntricateItems)
            {
                // base game returns  15 steel for size 1 (lancer, pikeman, scyther)
                // and 30 steel and 10 plasteel for size 1.8 (centipede)
                // to unify this system, just return 15 steel & 5 plasteel for each full size-point of the mech
                for(int i = 0; i < itemCount; i++)
                {
                    for(int j = 0; j < 15; j++)
                    {
                        createdItems.Add(ThingMaker.MakeThing(ThingDefOf.Steel));
                    }
                    for(int j = 0; j < 5; j++)
                    {
                        createdItems.Add(ThingMaker.MakeThing(ThingDefOf.Plasteel));
                    }
                }
            }
            // otherwise just produce metal slag based on body size
            else
            {
                for(int i = 0; i <= itemCount; i++)
                {
                    createdItems.Add(ThingMaker.MakeThing(ThingDefOf.ChunkSlagSteel));
                }
            }
            prey.Destroy();
            record.VoreContainer.TryAddOrTransfer(createdItems);
        }

        public static int GetQualityOrFallback(this Thing thing)
        {
            QualityCategory quality;
            bool success = thing.TryGetQuality(out quality);
            if(success)
            {
                return (int)quality;
            }
            // use reflection to retrieve the amount of values for the QualityCategory enum, then subtract by 1 cause 0 indexed
            int maxQuality = System.Enum.GetValues(typeof(QualityCategory)).EnumerableCount() - 1;
            int fallbackQuality = maxQuality / 3;
            if(RV2Log.ShouldLog(false, "OngoingVore"))
                RV2Log.Message($"Apparel had no QuirkQuality, returning {fallbackQuality} instead", "OngoingVore");
            return fallbackQuality;
        }

        private static void KillPreyWithDamage(VoreTrackerRecord record, DamageDef appliedDamageDef)
        {
            DoFactionImpact(record);
            Pawn predator = record.Predator;
            Pawn prey = record.Prey;
            DamageInfo dinfo = new DamageInfo(appliedDamageDef, 99999f, 99999f, -1, predator);
            dinfo.SetIgnoreInstantKillProtection(true);
            prey.Kill(dinfo);
            //DoFactionImpact(record);
            // would have loved to just call the deathActionWorker.PawnDied() but it requires the corpse, which is currently despawned and the predator can not be used in-place
            bool preyExplodesOnDeath = prey.RaceProps.DeathActionWorker is DeathActionWorker_SmallExplosion
                                                   || prey.RaceProps.DeathActionWorker is DeathActionWorker_BigExplosion;
            if(preyExplodesOnDeath)
            {
                // sadly base game explosions don't factor in pawn size at all and they are all either Big or Small
                float explosionSize = prey.BodySize * 2;
                GenExplosion.DoExplosion(predator.Position, predator.Map, explosionSize, DamageDefOf.Flame, predator);
            }
        }

        private static void DoFactionImpact(VoreTrackerRecord record)
        {
            if(RV2Mod.Settings.cheats.DisableFactionImpact)
            {
                if(RV2Log.ShouldLog(true, "FactionImpact"))
                    RV2Log.Message($"Not forcing faction impact, cheat enabled", "FactionImpact");
                return;
            }
            Pawn prey = record.Prey;
            Faction preyFaction = prey.Faction;
            if(preyFaction == null)
            {
                return;
            }
            // if vore was willing, no faction impact)
            if(record.ForcedBy == null)
            {
                if(RV2Log.ShouldLog(true, "FactionImpact"))
                    RV2Log.Message($"Not forcing faction impact, vore was willing", "FactionImpact");
                return;
            }
            // if predator is not controlled by player, no impact
            if(record.Predator.Faction != null && record.Predator.Faction == Faction.OfPlayer)
            {
                if(RV2Log.ShouldLog(true, "FactionImpact"))
                    RV2Log.Message($"Not forcing faction impact, predator is not of player faction", "FactionImpact");
                return;
            }
            // guilty pawns can be fatal vored without repercussions
            if(prey.guilt?.IsGuilty == true)
            {
                if(RV2Log.ShouldLog(true, "FactionImpact"))
                    RV2Log.Message($"Not forcing faction impact, prey {prey.LabelShort} was guity", "FactionImpact");
                return;
            }
            if(preyFaction != Faction.OfPlayer)
            {
                if(RV2Log.ShouldLog(false, "FactionImpact"))
                    RV2Log.Message($"Impacting faction goodwill with faction {preyFaction.Name}", "FactionImpact");
                Faction.OfPlayer.TryAffectGoodwillWith(preyFaction, Faction.OfPlayer.GoodwillToMakeHostile(preyFaction), true, !preyFaction.temporary, VoreHistoryDefOf.RV2_DigestedMember, null);
            }
        }

        private static void DecomposePawn(Pawn pawn, float rotModifier = 1f)
        {
            Corpse corpse = pawn.Corpse;
            CompRottable rotComp = corpse?.GetComp<CompRottable>();
            if(rotComp == null)
            {
                RV2Log.Warning("No CompRottable found for corpse. Skipping decomposition.");
                return;
            }
            rotComp.RotProgress = rotComp.PropsRot.TicksToDessicated * rotModifier;
        }

        public static void ApplyDigestionBookmark(VoreTrackerRecord record)
        {
            float progress = record.PassValues.TryGetValue("DigestionProgress");
            if(progress <= 0)
            {
                return;
            }
            Pawn prey = record.Prey;
            if(prey.health?.hediffSet?.hediffs == null)
            {
                return;
            }
            HediffDef bookmarkDef = RV2_Common.DigestionBookmarkHediff;
            // pawn might have hediff already
            Hediff existingHediff = prey.health.hediffSet.hediffs.Find(hediff => hediff.def == bookmarkDef);
            // if the pawn doesn't have the hediff yet, add it
            if(existingHediff == null)
            {
                existingHediff = prey.health.AddHediff(bookmarkDef);
            }
            if(RV2Log.ShouldLog(true, "OngoingVore"))
                RV2Log.Message($"Digestion recovery hediff was at value {existingHediff.Severity} -> adding {progress}", false, "OngoingVore");
            existingHediff.Severity += progress;
        }

        public static float GetPreviousDigestionProgress(Pawn pawn)
        {
            Hediff bookmarkHediff = pawn.health?.hediffSet?.hediffs?
                .Find(hediff => hediff.def == RV2_Common.DigestionBookmarkHediff);
            if(bookmarkHediff == null)
            {
                return 0f;
            }
            return bookmarkHediff.Severity;
        }
    }
}