using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimVore2
{
    public static class Toil_Vore
    {
        public const int baseTraversalDuration = 600;

        public static Toil SwallowToil(Job job, Pawn predator, TargetIndex targetIndex, int swallowDuration = baseTraversalDuration, bool considerSizeDifference = true)
        {
            Pawn prey = job.GetTarget(targetIndex).Pawn;
            ModifyTraversalDuration(ref swallowDuration, predator, prey);

            Toil swallowToil = Toils_General.WaitWith(targetIndex, swallowDuration, true, true);
            swallowToil.socialMode = RandomSocialMode.Off;

            return swallowToil;
        }

        public static Toil EjectionToil(Pawn predator, Pawn prey, int ejectionDuration = baseTraversalDuration)
        {
            ModifyTraversalDuration(ref ejectionDuration, predator, prey);

            Toil vomitToil = Toils_General.Wait(ejectionDuration);
            vomitToil.socialMode = RandomSocialMode.Off;

            return vomitToil;
        }

        private static int ModifyTraversalDuration(ref int durationInt, Pawn predator, Pawn prey, bool considerSizeDifference = true)
        {
            float duration = durationInt;
            string explanation = $"Base: {duration}";
            // disabled due to being used in vore paths unrelated to oral vore, would require a path-provided influence capacity
            //float eatingCapacity = predator.health?.capacities?.GetLevel(PawnCapacityDefOf.Eating) ?? 0;
            //if(eatingCapacity != 0)
            //{
            //    duration /= eatingCapacity;
            //    explanation += $"\n / eating capacity ({eatingCapacity}) -> {duration}";
            //}
            float voreSpeedMultiplier = RV2Mod.Settings.cheats.VoreSpeedMultiplier;
            if(voreSpeedMultiplier != 0)
            {
                duration /= voreSpeedMultiplier;
                explanation += $"\n / vore speed multiplier ({voreSpeedMultiplier}) -> {duration}";
            }
            // size difference is disregarded for feeder vore, because the toil provides the wait time for *all* prey at toil creation time
            if(considerSizeDifference)
            {
                float sizeDifference = predator.GetSizeInRelationTo(prey);
                duration /= sizeDifference;
                explanation += $"\n / size difference ({sizeDifference}) -> {duration}";
            }
            QuirkManager predatorQuirks = predator.QuirkManager();
            if(predatorQuirks != null && predatorQuirks.HasValueModifier("SwallowSpeed"))
            {
                if(predatorQuirks.TryGetValueModifier("SwallowSpeed", ModifierOperation.Multiply, out float quirkMultiplier))
                {
                    duration /= quirkMultiplier;
                    explanation += $"\n / quirk ({quirkMultiplier}) -> {duration}";
                }
            }
            durationInt = Math.Max(1, (int)duration);

            if(RV2Log.ShouldLog(false, "Jobs"))
                RV2Log.Message($"Calculated traversal time {duration}: {explanation}", true, "Jobs");
            // waiting for 0 ticks would produce an error, so wait for at least 1 tick
            return durationInt;
        }

        public static Toil ExecutionToil_Direct(VoreJob voreJob, Pawn initiator, Pawn predator, Pawn prey)
        {
            return new Toil()
            {
                initAction = delegate ()
                {

                    if(RV2Log.ShouldLog(false, "Jobs"))
                        RV2Log.Message("Execution Toil finish action called", "Jobs");
                    VorePath vorePath = new VorePath(voreJob.VorePath);
                    VoreTrackerRecord record = new VoreTrackerRecord(predator, prey, voreJob.IsForced, initiator, vorePath, 0, voreJob.IsRitualRelated);
                    record.IsPlayerForced = voreJob.playerForced;
                    PreVoreUtility.PopulateRecord(ref record);
                    predator.PawnData().VoreTracker.TrackVore(record);

                    ForcedState forcedState = voreJob.GetForcedState(predator, prey, voreJob.IsForced);

                    if(VoreValidator.StripBeforeVore(predator, prey, forcedState))
                    {
                        prey.Strip();
                        record.PreyStartedNaked = true;
                    }
                    RV2RitualUtility.RemoveVoreFeastDuty(prey);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }
        public static Toil ExecutionToil_Feeder(VoreJob voreJob, Pawn initiator, TargetIndex predatorIndex, TargetIndex preyIndex)
        {
            return new Toil()
            {
                initAction = delegate ()
                {
                    Pawn predator = voreJob.GetTarget(predatorIndex).Pawn;
                    Pawn prey = voreJob.GetTarget(preyIndex).Pawn;

                    if(RV2Log.ShouldLog(false, "Jobs"))
                        RV2Log.Message("Feeder execution Toil finish action called", "Jobs");
                    VorePath vorePath = new VorePath(voreJob.VorePath);
                    VoreTrackerRecord record = new VoreTrackerRecord(predator, prey, voreJob.IsForced, initiator, vorePath, 0, voreJob.IsRitualRelated);
                    PreVoreUtility.PopulateRecord(ref record);
                    predator.PawnData().VoreTracker.TrackVore(record);

                    ForcedState forcedState = voreJob.GetForcedState(predator, prey, voreJob.IsForced);

                    if(VoreValidator.StripBeforeVore(predator, prey, forcedState))
                    {
                        prey.Strip();
                        record.PreyStartedNaked = true;
                    }
                    RV2RitualUtility.RemoveVoreFeastDuty(prey);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }

        
        public static Toil EjectToil(Pawn interruptingPawn, Pawn predatorPawn, Pawn ejectPawn, bool isInterrupted = false)
        {
            return new Toil()
            {
                initAction = delegate ()
                {
                    VoreTracker voreTracker = predatorPawn.PawnData().VoreTracker;
                    VoreTrackerRecord record = ejectPawn.GetVoreRecord();
                    if(record != null)
                    {
                        record.IsInterrupted = isInterrupted;
                        voreTracker.Eject(record, interruptingPawn);

                        record.Prey.stances?.stunner?.StunFor(RV2Mod.Settings.fineTuning.EjectStunDuration, record.Predator, false);
                    }
                    else
                    {
                        Log.Warning("RimVore-2: Tried to eject pawn, but could not find VoreTrackerRecord: " + ejectPawn.ToStringSafe());
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }
    }
}