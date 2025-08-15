using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using rjw;
using Verse.AI;
using RimVore2;
using rjw.Modules.Interactions.Enums;
using static rjw.xxx;

namespace RV2_RJW
{
    [HarmonyPatch(typeof(JobDriver_Sex), "Orgasm")]
    public class Patch_JobDriver_Sex_Orgasm
    {
        [HarmonyPrefix]
        private static void EjectPreyIntoPartner(JobDriver_Sex __instance)
        {
            try
            {
                if(__instance.sex_ticks > __instance.orgasmstick)
                {
                    // orgasm is called, but needs to actually check if it is an orgasm on its own - bad code in RJW
                    return;
                }
                new SexEjectOrTransferHandler(__instance);
            }
            catch (Exception e)
            {
                RV2Log.Error("Something went wrong during orgasm inject: " + e);
            }
        }

        public class SexEjectOrTransferHandler
        {
            readonly SexProps sexProps;
            readonly Pawn initiator;
            readonly Pawn target;
            readonly JobDriver_Sex jobDriver;

            Pawn giver = null;
            Pawn receiver = null;
            VoreTrackerRecord oldRecord = null;
            VoreTrackerRecord newRecord = null;

            rjwSextype SexType => sexProps.sexType;
            // arguments to be used for user-notifications, these are not null-checked and must be called in the correct context
            NamedArgument ArgGiver => giver.LabelShort.Named("GIVER");
            NamedArgument ArgOldVoreType => oldRecord.VoreType.label.Named("OLDVORETYPE");
            NamedArgument ArgPrey => oldRecord.Prey.LabelShort.Named("PREY");
            NamedArgument ArgSex => SexType.Named("SEX");
            NamedArgument ArgNewVoreType => newRecord.VoreType.label.Named("NEWVORETYPE");
            NamedArgument ArgNewVoreGoal => newRecord.VoreGoal.label.Named("NEWVOREGOAL");
            NamedArgument ArgReceiver => receiver.LabelShort.Named("RECEIVER");

            public SexEjectOrTransferHandler(JobDriver_Sex jobDriver)
            {
                this.jobDriver = jobDriver;
                sexProps = jobDriver.Sexprops;
                initiator = jobDriver.pawn;
                target = jobDriver.Partner;
                if(RV2Log.ShouldLog(false, "RJW"))
                    RV2Log.Message("Calling patch for " + StringifySexProps(), "RJW");
                Calculate();
            }

            private void Calculate()
            {
                if(jobDriver is JobDriver_SexBaseReciever)
                {
                    // do not apply to recipients of sex
                    return;
                }
                if(sexProps == null)
                {
                    RV2Log.Warning("No sex props", "RJW");
                    return;
                }
                if(sexProps.usedCondom)
                {
                    RV2Log.Warning("Condom was used, currently there is no eject-into-condom functionality, but it prevents prey ejection / transfer :)", "RJW");
                    return;
                }
                Common.SexTypeGiverDictionary.DetermineGiver(sexProps, initiator, target, out giver, out receiver);
                if(giver == null)
                {
                    if(RV2Log.ShouldLog(false, "RJW"))
                        RV2Log.Message("No giver", "RJW");
                    return;
                }
                else
                {
                    if(RV2Log.ShouldLog(false, "RJW"))
                        RV2Log.Message($"giver: {giver.LabelShort} , receiver: {(receiver == null ? "None" : receiver.LabelShort)}", "RJW");
                }
                QuirkManager quirks = giver.QuirkManager(false);
                float baseSexEjectChance = RV2_RJW_Settings.rjw.BaseSexEjectChance;
                float chanceToEject = quirks != null ? quirks.ModifyValue("SexEjectChance", baseSexEjectChance) : baseSexEjectChance;
                if(!Rand.Chance(chanceToEject))
                {
                    if(RV2Log.ShouldLog(false, "RJW"))
                        RV2Log.Message("Roll to eject failed, chance: " + UIUtility.PercentagePresenter(chanceToEject), "RJW");
                    return;
                }
                if(!giver.PawnData().VoreTracker.IsTrackingVore)
                {
                    if(RV2Log.ShouldLog(false, "RJW"))
                        RV2Log.Message("No prey", "RJW");
                    return;
                }
                IEnumerable<VoreTrackerRecord> validRecords = ValidRecordsForSexEjectSource();
                if(validRecords.EnumerableNullOrEmpty())
                {
                    if(RV2Log.ShouldLog(false, "RJW"))
                        RV2Log.Message("No valid records", "RJW");
                    return;
                }
                VoreTrackerRecord targetRecord = validRecords.RandomElement();
                TransferOrEject(targetRecord);
            }

            private void TransferOrEject(VoreTrackerRecord record)
            {
                oldRecord = record;
                RJW_SexToVoreLookupDef lookupDef = Common.SexToVoreLookups
                    .Find(lookup => lookup.AppliesTo(record.VorePath.def));
                if(lookupDef == null)
                {
                    if(RV2Log.ShouldLog(false, "RJW"))
                        RV2Log.Message("No SexToVoreLookupDef found", "RJW");
                    return;
                }
                if(lookupDef.ShouldEject(sexProps))
                {
                    if(RV2Log.ShouldLog(false, "RJW"))
                        RV2Log.Message($"Ejecting {record} due to transferDef targeting Vore: {record.VorePath.def.defName} + sex: {sexProps.sexType}", "RJW");
                    Eject(record);
                }
                else if(target != null && lookupDef.ShouldTransfer(sexProps, out VoreTypeDef targetType))
                {
                    if(RV2Log.ShouldLog(false, "RJW"))
                        RV2Log.Message($"Transferring {record.Prey.LabelShort} from {record.Predator.LabelShort} to {target.LabelShort} due to TransferDef targeting vore: {record.VorePath.def.defName} + sex: {sexProps.sexType}", "RJW");
                    Transfer(targetType);
                }
                else
                {
                    if(RV2Log.ShouldLog(false, "RJW"))
                        RV2Log.Message("Nothing to do for vore: " + record.VorePath.def.defName + " + sex : " + sexProps.sexType, "RJW");
                }
            }
            private IEnumerable<VoreTrackerRecord> ValidRecordsForSexEjectSource()
            {
                return giver.PawnData().VoreTracker.VoreTrackerRecords
                    .Where(record => Common.SexToVoreLookups    // get all the lookups from the Defs
                        .Any(lookupDef => lookupDef.AppliesTo(record.VorePath.def)) // check which lookups apply to the current vore
                        && !record.Prey.Dead);  // we can only transfer living prey
            }
            private void Eject(VoreTrackerRecord record)
            {
                oldRecord = record;
                record.IsInterrupted = true;
                record.Predator.PawnData().VoreTracker.Eject(record, record.Predator);
                NotifyEjectHappened();
            }

            private void Transfer(VoreTypeDef voreTypeDef)
            {
                if(receiver == null)
                {
                    RV2Log.Warning("Tried to transfer prey during sex, but target is null", "RJW");
                    return;
                }
                List<VoreTypeDef> typeWhitelist = new List<VoreTypeDef>() { voreTypeDef };
                VoreInteractionRequest request = new VoreInteractionRequest(receiver, oldRecord.Prey, VoreRole.Predator, true, typeWhitelist: typeWhitelist);
                VoreInteraction interaction = VoreInteractionManager.Retrieve(request);
                VorePathDef pathDef = interaction.PreferredPath;
                if(pathDef == null)
                {
                    RV2Log.Warning("No preferred path determinable for sex-transfered predator " + receiver.LabelShort, "RJW");
                    return;
                }
                int targetIndex = GetValidInsertionIndex(pathDef);
                if(targetIndex == -1)
                {
                    RV2Log.Warning("No proper path index found for path " + pathDef, "RJW");
                    return;
                }
                newRecord = new VoreTrackerRecord(oldRecord)
                {
                    Predator = receiver,
                    Prey = oldRecord.Prey,
                    VorePath = new VorePath(pathDef),
                    Initiator = oldRecord.Initiator,
                    VorePathIndex = targetIndex
                };
                PreVoreUtility.PopulateRecord(ref newRecord, true);
                giver.PawnData().VoreTracker.UntrackVore(oldRecord);
                receiver.PawnData().VoreTracker.TrackVore(newRecord);

                IncreaseRecords(giver, receiver);
                NotifyTransferHappened();
            }

            private void IncreaseRecords(Pawn giver, Pawn receiver)
            {
                giver.records?.AddTo(Common.voreTransferGiverRecord, 1);
                receiver.records?.AddTo(Common.voreTransferReceiverRecord, 1);
            }

            private void NotifyTransferHappened()
            {
                string description = "RV2_RJW_PreySexTransferDescription".Translate(ArgGiver, ArgReceiver, ArgSex, ArgOldVoreType, ArgNewVoreType, ArgNewVoreGoal, ArgPrey);
                NotificationType notificationType = RV2_RJW_Settings.rjw.SexTransferPreyNotification;
                NotificationUtility.DoNotification(notificationType, description, "RV2_RJW_PreySexTransferLabel".Translate());
            }

            private void NotifyEjectHappened()
            {
                string description = "RV2_RJW_PreySexEjectDescription".Translate(ArgGiver, ArgReceiver, ArgSex, ArgOldVoreType, ArgPrey);
                NotificationType notificationType = RV2_RJW_Settings.rjw.SexEjectPreyNotification;
                NotificationUtility.DoNotification(notificationType, description, "RV2_RJW_PreySexEjectLabel".Translate());
            }

            /// <summary>
            /// Modders can patch this method and check for their custom paths to use a different initial index
            /// </summary>
            private int GetValidInsertionIndex(VorePathDef path)
            {
                return 0;
            }

            private string StringifySexProps()
            {
                if(sexProps == null)
                {
                    return "NULL";
                }
                bool isReverse = rjw.Modules.Interactions.Helpers.InteractionHelper
                    .GetWithExtension(sexProps.dictionaryKey)?   // get extension addons, which contains a flag for reverse sex types, dictionaryKey is the literal InteractionDef
                    .HasInteractionTag(InteractionTag.Reverse) // check for the reverse
                    == true;
                return $"pawn: {sexProps.pawn}({sexProps?.pawn?.jobs?.curDriver?.GetType()}) - partner: {sexProps.partner}({sexProps.partner?.jobs?.curDriver?.GetType()})\n" +
                    $"sexType: {sexProps.sexType} - interaction: {sexProps.dictionaryKey} - reverse ? {isReverse}";
            }
        }
    }
}