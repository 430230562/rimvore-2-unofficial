using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimVore2
{
    public class Verb_VoreGrapple : Verb_MeleeAttack
    {
        protected override DamageWorker.DamageResult ApplyMeleeDamageToTarget(LocalTargetInfo target)
        {
            Job grappleJob = JobMaker.MakeJob(VoreJobDefOf.RV2_VoreGrapple, target);
            grappleJob.expiryInterval = int.MaxValue;
            if(RV2Log.ShouldLog(true, "VoreCombatGrapple"))
                RV2Log.Message($"{CasterPawn.LabelShort} started grapple job", "VoreCombatGrapple");
            CasterPawn.jobs.StartJob(grappleJob, JobCondition.Succeeded);
            return new DamageWorker.DamageResult();
        }

        // TODO: make a ModExtension and cache all jobs tagged with it for this list rather than hard-coding it
        public static List<JobDef> InvalidJobs = new List<JobDef>()
        {
            JobDefOf.SocialFight,
            JobDefOf.BeatFire,
            JobDefOf.PrisonerExecution
        };

        public override bool Available()
        {
            if(!base.Available())
            {
                return false;
            }
            if(!RV2Mod.Settings.features.GrapplingEnabled)
            {
                return false;
            }
            PawnData pawnData = CasterPawn.PawnData();
            if(!pawnData.CanUseGrapple)
            {
                return false;
            }
            if(!base.CasterPawn.CanParticipateInVore(out _))
            {
                return false;
            }
            if(InvalidJobs.Contains(CasterPawn.CurJobDef))
            {
                return false;
            }
            return true;
        }

        public override bool IsUsableOn(Thing target)
        {
            if(!base.IsUsableOn(target))
            {
                if(RV2Log.ShouldLog(true, "VoreCombatGrapple"))
                    RV2Log.Message($"{CasterPawn.LabelShort} - Grapple not usable, base check returned false", false, "VoreCombatGrapple");
                return false;
            }
            if(!(target is Pawn targetPawn))
            {
                if(RV2Log.ShouldLog(true, "VoreCombatGrapple"))
                    RV2Log.Message($"{CasterPawn.LabelShort} - Grapple not usable, target is not a pawn, target is {(target == null ? "NULL" : target.GetType().ToString())}", false, "VoreCombatGrapple");
                return false;
            }
            if(CombatUtility.IsInvolvedInGrapple(targetPawn) || CombatUtility.IsInvolvedInGrapple(base.CasterPawn))
            {
                if(RV2Log.ShouldLog(true, "VoreCombatGrapple"))
                    RV2Log.Message($"{CasterPawn.LabelShort} - Grapple not usable, either pawn is already involved in a grapple", false, "VoreCombatGrapple");
                return false;
            }
            // the pawn must be able to receive the "grappled" hediff
            if(targetPawn.health == null)
            {
                if(RV2Log.ShouldLog(true, "VoreCombatGrapple"))
                    RV2Log.Message($"{CasterPawn.LabelShort} - Grapple not usable, target has no health", false, "VoreCombatGrapple");
                return false;
            }
            float grappleStrength = CombatUtility.GetGrappleStrength(base.CasterPawn, true);
            if(grappleStrength <= 0)
            {
                if(RV2Log.ShouldLog(true, "VoreCombatGrapple"))
                    RV2Log.Message($"{CasterPawn.LabelShort} - Grapple not usable, grapple strength is too low: {grappleStrength}", false, "VoreCombatGrapple");
                return false;
            }
            if(!base.CasterPawn.CanVore(targetPawn, out _))
            {
                if(RV2Log.ShouldLog(true, "VoreCombatGrapple"))
                    RV2Log.Message($"{CasterPawn.LabelShort} - Grapple not usable, can't vore target", false, "VoreCombatGrapple");
                return false;
            }
            bool useAutoRules = RV2Mod.Settings.combat.UseAutoRules;
            List<VorePathDef> vorePathWhitelist = VoreOptionUtility.ConditionalPathWhitelistForPredatorAnimals(CasterPawn);
            VoreInteractionRequest request = new VoreInteractionRequest(CasterPawn, targetPawn, VoreRole.Predator, isForAuto: useAutoRules, pathWhitelist: vorePathWhitelist);
            VoreInteraction interaction = VoreInteractionManager.Retrieve(request);
            if(interaction.PreferredPath == null)
            {
                if(RV2Log.ShouldLog(true, "VoreCombatGrapple"))
                    RV2Log.Message($"{CasterPawn.LabelShort} - Grapple not usable, no preferred path", false, "VoreCombatGrapple");
                return false;
            }

            return true;
        }

        public override void OrderForceTarget(LocalTargetInfo target)
        {
            Job job = JobMaker.MakeJob(VoreJobDefOf.RV2_VoreGrapple, target);
            job.playerForced = true;
            base.CasterPawn.jobs.TryTakeOrderedJob(job);
        }
    }
}
