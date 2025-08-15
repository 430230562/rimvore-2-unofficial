using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimVore2
{
    class JobDriver_Vore_EjectPrey_Force : JobDriver
    {
        readonly TargetIndex targetIndex = TargetIndex.A;

        private Pawn ejectPawn;
        private Pawn EjectPawn
        {
            get
            {
                if(ejectPawn == null)
                {
                    ejectPawn = TargetB.Pawn;
                }
                if(ejectPawn == null)
                {
                    this.FailOn(() => true);
                }
                return ejectPawn;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return base.pawn.Reserve(base.job.GetTarget(targetIndex), base.job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(targetIndex);

            Pawn initiatorPawn = base.pawn;
            Pawn targetPawn = (Pawn)TargetA;    // TargetA will be scribed correctly, because they are spawned normally - if not, the FailOnDespawnedOrNull will take care of it

            if(RV2Log.ShouldLog(false, "Jobs"))
                RV2Log.Message($"Job started with initiator: {initiatorPawn.LabelShort} and target {targetPawn.LabelShort}", "Jobs");


            yield return Toils_Goto.GotoThing(targetIndex, PathEndMode.Touch);

            // toil is named swallow, but fulfills the same purpose as un-swallowing - wait for default duration with target pawn
            yield return Toil_Vore.SwallowToil(base.job, targetPawn, targetIndex);
            float ejectChance;
            if(RV2Mod.Settings.cheats.ExternalEjectAlwaysSucceeds)
            {
                ejectChance = 1;
            }
            else
            {
                ejectChance = initiatorPawn.GetStatValue(VoreStatDefOf.RV2_ExternalEjectChance);
                float sizeRatio = initiatorPawn.BodySize / targetPawn.BodySize;
                ejectChance *= sizeRatio;
            }
            if(RV2Log.ShouldLog(false, "ExternalEject"))
                RV2Log.Message($"Final ejection chance: {ejectChance}", "ExternalEject");
            if(Rand.Chance(ejectChance))
            {
                yield return Toil_Vore.EjectToil(initiatorPawn, targetPawn, EjectPawn, true);
            }
            else
            {
                yield return new Toil()
                {
                    initAction = () =>
                    {
                        VoreTrackerRecord record = GlobalVoreTrackerUtility.GetVoreRecord(EjectPawn);
                        if(record == null)
                        {
                            Log.Error($"Could not retrieve vore tracker record for {EjectPawn.ToStringSafe()}");
                            return;
                        }
                        record.WasExternalEjectAttempted = true;
                        string message = "RV2_Message_ExternalEjectFailed".Translate(initiatorPawn.Named("INITIATOR"), targetPawn.Named("TARGET"), EjectPawn.Named("PREY"));
                        NotificationUtility.DoNotification(NotificationType.MessageNeutral, message);
                    }
                };
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref ejectPawn, "ejectPawn", true);
        }
    }
}
