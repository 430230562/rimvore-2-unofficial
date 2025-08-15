using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimVore2
{
    public class MentalState_VoreTargeter : MentalState
    {
        public Pawn CurrentTarget;
        public VoreMentalStateDef VoreMentalStateDef => (VoreMentalStateDef)base.def;
        public virtual VoreTargetRequest Request => VoreMentalStateDef.request;
        protected string TargetChangeMessageKey => VoreMentalStateDef.targetChangeMessageKey;
        protected MentalStateDef FallbackMentalState => VoreMentalStateDef.fallbackMentalState;
        public VoreRole InitiatorRole => VoreMentalStateDef.initiatorRole;
        protected List<Pawn> voredTargets = new List<Pawn>();
        protected virtual int TargetsToVoreCount => VoreMentalStateDef.targetCountToVore;
        public bool HasVoredEnoughTargets => voredTargets.Count() >= TargetsToVoreCount;
        private bool hasTriggeredFallback = false;

        public void CallFallbackMentalState()
        {
            hasTriggeredFallback = true;
            base.pawn?.mindState?.mentalStateHandler?.TryStartMentalState(FallbackMentalState, "RV2_VoreMentalBreakNoPathAvailable".Translate());
        }

        public override void PreStart()
        {
            base.PreStart();
            //Log.Message("prestart hook, trying to determine target");
            if(!TryDetermineTarget())
            {
                //Log.Message("prestart target did not work, falling back to " + FallbackMentalState.defName);
                CallFallbackMentalState();
            }
        }

        public override TaggedString GetBeginLetterText()
        {
            string beginLetter = base.def.beginLetter;  //hasTriggeredFallback ? FallbackMentalState.beginLetter : 
            // we abuse base game behaviour here. If we are falling back to the fallback mental state, we don't want to display a letter
            // Base game does a check on the letter text that we can set to null to prevent the letter from being created altogether
            if(beginLetter.NullOrEmpty() || hasTriggeredFallback)
            {
                return null;
            }
            return beginLetter.Formatted(base.pawn.LabelShortCap, CurrentTarget?.LabelShort).AdjustedFor(base.pawn, "PAWN", true).CapitalizeFirst();
        }

#if v1_5
        public override void MentalStateTick()
        {
            base.MentalStateTick();
#else
        public override void MentalStateTick(int delta)
        {
            base.MentalStateTick(delta);
#endif
            if(ShouldCheckBasedOnPassedTime())
            {
                if(NeedToRecalculateTarget())
                {
                    if(!TryDetermineTarget())
                    {
                        if(RV2Log.ShouldLog(false, "MentalBreaks"))
                            RV2Log.Message("Need to recalculate target, but no target could be found, using fallback mental state", "MentalBreaks");
                        CallFallbackMentalState();
                        return;
                    }
                    if(TargetChangeMessageKey != null)
                    {
                        string message = TargetChangeMessageKey.Translate(base.pawn.LabelShortCap, CurrentTarget.LabelShortCap);
                        Messages.Message(message, base.pawn, MessageTypeDefOf.NeutralEvent);
                    }
                }
                lastCheckedTick = GenTicks.TicksGame;
            }
        }

        int lastCheckedTick = -1;
        bool ShouldCheckBasedOnPassedTime()
        {
            if(lastCheckedTick < 0)
            {
                return true;
            }
            if(GenTicks.TicksGame > lastCheckedTick + GenTicks.TickRareInterval)
            {
                return true;
            }
            return false;
        }

        private bool NeedToRecalculateTarget()
        {
            if(InitiatorRole == VoreRole.Predator)
            {
                bool currentTargetVored = CurrentTarget != null
                    && GlobalVoreTrackerUtility.IsPreyOf(CurrentTarget, base.pawn);

                if(currentTargetVored)
                {
                    voredTargets.AddDistinct(CurrentTarget);
                }
                bool targetStillValid = IsTargetStillValidAndReachable();
                return !(HasVoredEnoughTargets || targetStillValid);
            }
            else
            {
                if(GlobalVoreTrackerUtility.IsPreyOf(base.pawn, CurrentTarget))
                {
                    return false;
                }
                return !IsTargetStillValidAndReachable();
            }
        }

        private bool IsTargetStillValidAndReachable()
        {
            if(!base.pawn.CanReach(CurrentTarget, PathEndMode.ClosestTouch, Danger.Deadly))
            {
                return false;
            }
            if(!Request.IsValid(CurrentTarget, out _))
            {
                return false;
            }
            return true;
        }

        public override void PostEnd()
        {
            try
            {
                if(!base.def.recoveryMessage.NullOrEmpty() && PawnUtility.ShouldSendNotificationAbout(base.pawn))
                {
                    // sending the recovery message for dead pawns results in funny behaviour of a prey dying to digestion and then going "they changed their mind on being digested"
                    if(!base.pawn.Dead)
                    {
                        LookTargets lookTargets = new LookTargets(new List<Pawn>() { base.pawn, CurrentTarget });
                        TaggedString taggedString = base.def.recoveryMessage.Formatted(base.pawn.LabelShortCap, CurrentTarget.LabelShortCap);
                        Messages.Message(taggedString, lookTargets, MessageTypeDefOf.SituationResolved, true);
                    }
                }
            }
            catch(Exception e)
            {
                Log.Error("Exception whilst trying to end mental state: " + e);
            }

        }

        protected virtual bool TryDetermineTarget()
        {
            IEnumerable<Pawn> targets = TargetUtility.GetVorablePawns(base.pawn, Request, 1);
            if(targets.EnumerableNullOrEmpty())
            {
                return false;
            }
            CurrentTarget = targets.RandomElement();
            //Log.Message("Determined target " + CurrentTarget.LabelShort);
            return true;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref voredTargets, "targetsVored", LookMode.Reference);
            Scribe_References.Look(ref CurrentTarget, "CurrentTarget", false);
        }
    }

    public class MentalState_ForbiddenFruit : MentalState_VoreTargeter
    {
        readonly Func<Pawn, bool> validator = delegate (Pawn pawn)
        {
            DeathActionWorker deathAction = pawn.RaceProps?.DeathActionWorker;
            return deathAction is DeathActionWorker_SmallExplosion
                || deathAction is DeathActionWorker_BigExplosion;
        };

        public override VoreTargetRequest Request => new VoreTargetRequest
        (
            isAnimal: true,
            canBeFatalVored: true,
            validator: validator
        );
    }

    public class MentalState_VoreTargeter_Kill : MentalState_VoreTargeter
    {
#if v1_5
        public override void MentalStateTick()
        {
            base.MentalStateTick();
#else
        public override void MentalStateTick(int delta)
        {
            base.MentalStateTick(delta);
#endif
            if(ShouldCheckBasedOnPassedTime())
            {
                bool allTargetsDead = voredTargets.All(pawn => pawn.Dead);
                if(allTargetsDead && HasVoredEnoughTargets)
                {
                    pawn.MentalState.RecoverFromState();
                }
            }

        }
        int lastCheckedTick = -1;
        bool ShouldCheckBasedOnPassedTime()
        {
            if(lastCheckedTick < 0)
            {
                return true;
            }
            if(GenTicks.TicksGame > lastCheckedTick + GenTicks.TickRareInterval)
            {
                return true;
            }
            return false;
        }
    }
}
