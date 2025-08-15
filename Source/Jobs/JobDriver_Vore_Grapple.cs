using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimVore2
{
    public class JobDriver_Vore_Grapple : JobDriver
    {
        Pawn Target => TargetA.Pawn;
        const int lossGrappleState = 0;
        const int winGrappleState = 6;
        const int grappleMoveIntervalTicks = 100;
        /// <summary>
        /// 1 in-game hour
        /// </summary>
        const int grappleTimeoutTicks = 5000;

        int ticksPassed = 0;
        IntVec3 grapplePosition;
        float grappleStrengthenChance = 0.5f;
        int grappleState = 3;
        Effecter effecter;
        VoreJob voreJob;

        [TweakValue("RimVore-2", 0, 100)]
        static bool OverrideStrengthenChance = false;
        [TweakValue("RimVore-2", 0, 1)]
        static float ForcedStrengthenChance = 0.5f;

        float GrappleProgress()
        {
            return (float)grappleState / (float)winGrappleState;
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if(!base.pawn.CanVore(Target, out _))
            {
                return false;
            }
            if(!(Target is IAttackTarget attackTarget))
            {
                return false;
            }
            base.pawn.Map.attackTargetReservationManager.Reserve(base.pawn, base.job, attackTarget);
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            globalFinishActions.Add((JobCondition jobCondition) => FreeTarget());
            globalFailConditions.Add(TimeoutGrapple);


            if(RV2Log.ShouldLog(false, "VoreCombatGrapple"))
                RV2Log.Message($"Created Grapple job, pawn: {base.pawn.LabelShort} target: {Target.LabelShort}", "VoreCombatGrapple");
            CalculateGrappleStrengthenChance();

            grapplePosition = Target.Position;

            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOn(() => Target.Dead);

            yield return Toils_Misc.ThrowColonistAttackingMote(TargetIndex.A);
            Action grappleAction = () =>
            {
                ReadyForNextToil();
            };
            //yield return Toils_Combat.FollowAndMeleeAttack(TargetIndex.A, grappleAction);
            Toil grappleToil = Toils_General.Wait(int.MaxValue);
            grappleToil.AddPreInitAction(() =>
            {
                if(RV2Log.ShouldLog(false, "VoreCombatGrapple"))
                    RV2Log.Message($"Grapple toil initiated by {base.pawn.LabelShort}, target: {Target.LabelShort}", "VoreCombatGrapple");
                LockTarget();
            });
            grappleToil.AddPreTickAction(() =>
            {
                RotatePawns();
                if((GenTicks.TicksGame - base.startTick) % grappleMoveIntervalTicks != 0)
                {
                    return;
                }
                float chance = grappleStrengthenChance;
                if(OverrideStrengthenChance)
                {
                    chance = ForcedStrengthenChance;
                }

                if(RV2Log.ShouldLog(true, "VoreCombatGrapple"))
                    RV2Log.Message($"Current grapple state: {grappleState}, chance to strengthen grapple: {chance}", "VoreCombatGrapple");

                if(Rand.Chance(chance))
                {
                    grappleState++;
                }
                else
                {
                    grappleState--;
                }

                if(grappleState == winGrappleState)
                {
                    GrappleSuccess();
                }
                else if(grappleState == lossGrappleState)
                {
                    GrappleFailure();
                }
            });
            grappleToil.handlingFacing = true;
            grappleToil.defaultCompleteMode = ToilCompleteMode.Never;
            AddProgressBar(grappleToil);

            yield return grappleToil;
        }

        private JobCondition TimeoutGrapple()
        {
            if(ticksPassed++ >= grappleTimeoutTicks)
            {
                if(RV2Log.ShouldLog(false, "VoreCombatGrapple"))
                    RV2Log.Message($"Grapple exceeded timeout, forcing grapple state decrease", "VoreCombatGrapple");
                return JobCondition.Errored;
            }
            return JobCondition.Ongoing;
        }

        private void CalculateGrappleStrengthenChance()
        {
            float pawnGrappleStrength = CombatUtility.GetGrappleStrength(base.pawn, true);
            float targetGrappleStrength = CombatUtility.GetGrappleStrength(Target, false);
            float grappleDifference = pawnGrappleStrength - targetGrappleStrength;
            grappleStrengthenChance += grappleDifference / 50;
        }


        /// <summary>
        /// custom version of the ToilEffects.WithProgressBar, which allows hostile pawns to generate a progress bar
        /// </summary>
        /// <param name="toil"></param>
        private void AddProgressBar(Toil toil)
        {
            toil.AddPreTickAction(() =>
            {
                if(effecter == null)
                {
                    EffecterDef progressBar = EffecterDefOf.ProgressBar;
                    effecter = progressBar.Spawn();
                    return;
                }
                effecter.EffectTick(Target, toil.actor);
                MoteProgressBar mote = ((SubEffecter_ProgressBar)effecter.children[0]).mote;
                if(mote != null)
                {
                    mote.progress = Mathf.Clamp01(GrappleProgress());
                }
            });
            toil.AddFinishAction(() =>
            {
                if(effecter == null)
                    return;
                effecter.Cleanup();
                effecter = null;
            });
        }

        private void RotatePawns()
        {
            if(RV2Mod.Settings.combat.GrapplePawnsShareCell)
            {
                base.pawn.Position = grapplePosition;
                Target.Position = grapplePosition;
            }

            base.pawn.rotationTracker.Face(Target.DrawPos);
            Target.rotationTracker.Face(base.pawn.DrawPos);
        }

        private void GrappleFailure()
        {
            if(RV2Log.ShouldLog(false, "VoreCombatGrapple"))
                RV2Log.Message("Grapple failure", "VoreCombatGrapple");
            FreeTarget();
            base.pawn.stances?.stunner?.StunFor(RV2Mod.Settings.combat.GrappleFailureStunDuration, Target);
            base.EndJobWith(JobCondition.Incompletable);
        }

        private void GrappleSuccess()
        {
            RV2Log.Message("Grapple success", "VoreCombatGrapple");
            Hediff grappledHediff = HediffMaker.MakeHediff(RV2_Common.GrappledHediff, Target);
            if(Target.health == null)
            {
                RV2Log.Warning("Grapple succeeded, but target has no health for grapple hediff! This should have been caught by the IsUsableOn of the verb - falling back to grapple failure", "VoreCombatGrapple");
                GrappleFailure();
                return;
            }
            Target.health.AddHediff(grappledHediff);

            List<VorePathDef> vorePathWhiteList = VoreOptionUtility.ConditionalPathWhitelistForPredatorAnimals(pawn);
            VoreInteractionRequest request = new VoreInteractionRequest(base.pawn, Target, VoreRole.Predator, pathWhitelist: vorePathWhiteList);
            VoreInteraction interaction = VoreInteractionManager.Retrieve(request);
            if(interaction.PreferredPath == null)
            {
                RV2Log.Warning("Grapple succeeded, but there is no preferred path - this should have been caught by the IsUsableOn of the verb - falling back to grapple failure", "VoreCombatGrapple");
                GrappleFailure();
                return;
            }

            voreJob = VoreJobMaker.MakeJob(VoreJobDefOf.RV2_VoreInitAsPredator, pawn, TargetA);
            voreJob.VorePath = interaction.PreferredPath;
            voreJob.IsForced = true;

            if(RV2Log.ShouldLog(false, "VoreCombatGrapple"))
                RV2Log.Message($"Post-Grapple vore job created: {voreJob}", "VoreCombatGrapple");
            base.pawn.jobs.jobQueue.EnqueueFirst(voreJob);
            FreeTarget();
            base.EndJobWith(JobCondition.Succeeded);
        }

        private void LockTarget()
        {
            if(Target == null)
            {
                Log.Warning("Tried to lock grapple target, but the target was null, job executor: " + base.pawn?.LabelShort);
                base.EndJobWith(JobCondition.Errored);
                return;
            }
            Target.jobs.StartJob(JobMaker.MakeJob(JobDefOf.Wait_MaintainPosture, 9999), JobCondition.InterruptForced);
            Target.stances?.SetStance(new Stance_Grapple());

            if(!Target.Awake())
                RestUtility.WakeUp(Target);
        }
        private void FreeTarget()
        {
            Target.jobs?.EndCurrentJob(JobCondition.Succeeded);
            Target.stances?.CancelBusyStanceHard();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref grapplePosition, "grapplePosition");
            Scribe_Values.Look(ref grappleStrengthenChance, "grappleStrengthenChance");
            Scribe_Values.Look(ref grappleState, "grappleState");
            Scribe_Values.Look(ref ticksPassed, "ticksPassed");
            Scribe_Deep.Look(ref voreJob, "voreJob");
        }
    }
}
