using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimVore2
{
    public abstract class JobDriver_Vore_DownAndVoreOrBeVored : JobDriver
    {
        readonly TargetIndex targetIndex = TargetIndex.A;
        private bool notifiedPlayerAttacking = false;
        private bool notifiedPlayerAttacked = false;
        private bool firstHit = true;
        protected abstract void InformAttacked(Pawn initiator, Pawn target);
        protected abstract void InformTargeted(Pawn initiator, Pawn target);
        protected abstract bool InitAsPredator { get; }

        private Pawn TargetPawn => base.job.GetTarget(targetIndex).Pawn;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(targetIndex);

            // fail if prey is dead
            // wild animals hunting prey and accidentally killing them is fine, the game usually picks a "go to corpse and consume" job after this job fails with this condition
            this.FailOn(delegate ()
            {
                return TargetPawn.Dead;
            });

            base.AddFinishAction(delegate
            {
                base.Map.attackTargetsCache.UpdateTarget(base.pawn);
            });

            // for some reason the game does not remove the VoreJob from the pawns curJob, doing it manually this way
            globalFinishActions.Add((JobCondition jobCondition) =>
            {
                base.pawn.jobs.curJob = null;
            });

            Toil updateTargetToil = new Toil()
            {
                initAction = delegate ()
                {
                    base.Map.attackTargetsCache.UpdateTarget(base.pawn);
                }
            };
            Action hitAction = delegate ()
            {
                bool surpriseAttack = firstHit && !TargetPawn.IsColonist;
                if(base.pawn.meleeVerbs.TryMeleeAttack(TargetPawn, base.job.verbToUse, surpriseAttack))
                {
                    if(!notifiedPlayerAttacked && PawnUtility.ShouldSendNotificationAbout(TargetPawn))
                    {
                        notifiedPlayerAttacked = true;

                        // MOVE string message = "MessageAttackedByPredator".Translate(prey.LabelShort, base.pawn.LabelIndefinite(), prey.Named("PREY"), base.pawn.Named("PREDATOR"));
                        InformAttacked(base.pawn, TargetPawn);
                    }
                    base.Map.attackTargetsCache.UpdateTarget(base.pawn);
                    firstHit = false;
                }
            };

            Toil attackToil = Toils_Combat.FollowAndMeleeAttack(targetIndex, hitAction)
                .JumpIf(() => !TargetPawn.Downed, updateTargetToil)
                .FailOn(() => TargetPawn.Dead)
                .FailOn(() => Find.TickManager.TicksGame > base.startTick + 5000 && (float)(base.job.GetTarget(targetIndex).Cell - base.pawn.Position).LengthHorizontalSquared > 4f);
            attackToil.AddPreTickAction(new Action(CheckWarnPlayer));

            yield return updateTargetToil;
            yield return attackToil;
            Pawn predator = InitAsPredator ? base.pawn : TargetPawn;
            Pawn prey = InitAsPredator ? TargetPawn : base.pawn;
            yield return Toil_Vore.SwallowToil(base.job, predator, targetIndex);
            VoreJob voreJob = (VoreJob)base.job;
            voreJob.IsForced = true;
            yield return Toil_Vore.ExecutionToil_Direct(voreJob, base.pawn, predator, prey);
            yield break;
        }

        // Token: 0x06002BCB RID: 11211 RVA: 0x000FF3BC File Offset: 0x000FD5BC
        public override void Notify_DamageTaken(DamageInfo dinfo)
        {
            base.Notify_DamageTaken(dinfo);
            if(dinfo.Def.ExternalViolenceFor(base.pawn) && dinfo.Def.isRanged && dinfo.Instigator != null && dinfo.Instigator != TargetPawn && !base.pawn.InMentalState && !base.pawn.Downed)
            {
                base.pawn.mindState.StartFleeingBecauseOfPawnAction(dinfo.Instigator);
            }
        }

        // Token: 0x06002BCC RID: 11212 RVA: 0x000FF43C File Offset: 0x000FD63C
        private void CheckWarnPlayer()
        {
            if(notifiedPlayerAttacking)
            {
                return;
            }
            Pawn prey = TargetPawn;
            if(!prey.Spawned || prey.Faction != Faction.OfPlayer)
            {
                return;
            }
            if(Find.TickManager.TicksGame <= base.pawn.mindState.lastPredatorHuntingPlayerNotificationTick + 2500)
            {
                return;
            }
            if(!prey.Position.InHorDistOf(base.pawn.Position, 60f))
            {
                return;
            }
            InformAttacked(base.pawn, TargetPawn);
            base.pawn.mindState.Notify_PredatorHuntingPlayerNotification();
            notifiedPlayerAttacking = true;
        }

        // Token: 0x06002BC7 RID: 11207 RVA: 0x000FF35E File Offset: 0x000FD55E
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref firstHit, "firstHit", false, false);
            Scribe_Values.Look(ref notifiedPlayerAttacking, "notifiedPlayerAttacking", false, false);
        }

    }

    public class JobDriver_Vore_DownAndVore_AsAnimal : JobDriver_Vore_DownAndVoreOrBeVored
    {
        protected override bool InitAsPredator => true;

        protected override void InformAttacked(Pawn initiator, Pawn target)
        {
            Messages.Message("MessageAttackedByPredator".Translate(target.LabelShortCap, initiator.LabelIndefinite(),
                target.Named("PREY"), initiator.Named("PREDATOR")).CapitalizeFirst(),
                target,
                MessageTypeDefOf.ThreatSmall);
        }

        protected override void InformTargeted(Pawn initiator, Pawn target)
        {
            if(target.RaceProps.Humanlike)
            {
                Find.LetterStack.ReceiveLetter(
                    label: "LetterLabelPredatorHuntingColonist".Translate
                    (
                        initiator.LabelShortCap, target.LabelDefinite(),
                        initiator.Named("PREDATOR"),
                        target.Named("PREY")
                    ).CapitalizeFirst(),
                    text: "LetterPredatorHuntingColonist".Translate
                    (
                        initiator.LabelIndefinite(),
                        target.LabelDefinite(),
                        initiator.Named("PREDATOR"),
                        target.Named("PREY")
                    ).CapitalizeFirst(),
                    textLetterDef: LetterDefOf.ThreatBig,
                    lookTargets: initiator);
            }
            else
            {
                string key = target.Name.Numerical ? "LetterPredatorHuntingColonist" : "MessagePredatorHuntingPlayerAnimal";
                Messages.Message(
                    text: key.Translate(initiator.Named("PREDATOR"), target.Named("PREY")),
                    lookTargets: initiator,
                    def: MessageTypeDefOf.ThreatBig);
            }
        }
    }
}
