using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimVore2;
using Verse;

namespace RV2_RJW
{
    public class SettingsContainer_RJW : SettingsContainer
    {
        public SettingsContainer_RJW() { }

        private FloatSmartSetting baseSexEjectChance;
        private BoolSmartSetting vaginalVoreAbortsPregnancies;
        private FloatSmartSetting postSexProposalChance;
        private BoolSmartSetting postSexProposalsAreForced;
        private FloatSmartSetting postRapeProposalChance;
        private BoolSmartSetting postRapeProposalsAreForced;
        private EnumSmartSetting<NotificationType> sexEjectPreyNotification;
        private EnumSmartSetting<NotificationType> sexTransferPreyNotification;
        private BoolSmartSetting bestialityIsRape;
        private BoolSmartSetting disableVoreGrappleDuringSex;

        public float BaseSexEjectChance => baseSexEjectChance.value / 100;
        public float PostSexProposalChance => postSexProposalChance.value / 100;
        public bool PostSexProposalsAreForced => postSexProposalsAreForced.value;
        public float PostRapeProposalChance => postRapeProposalChance.value / 100;
        public bool PostRapeProposalsAreForced => postRapeProposalsAreForced.value;
        public bool VaginalVoreAbortsPregnancies => vaginalVoreAbortsPregnancies.value;
        public NotificationType SexEjectPreyNotification => sexEjectPreyNotification.value;
        public NotificationType SexTransferPreyNotification => sexTransferPreyNotification.value;
        public bool BestialityIsRape => bestialityIsRape.value;
        public bool DisableVoreGrappleDuringSex => disableVoreGrappleDuringSex.value;


        public override void Reset()
        {
            baseSexEjectChance = null;
            vaginalVoreAbortsPregnancies = null;
            postSexProposalChance = null;
            postSexProposalsAreForced = null;
            postRapeProposalChance = null;
            postRapeProposalsAreForced = null;
            sexEjectPreyNotification = null;
            sexTransferPreyNotification = null;
            bestialityIsRape = null;
            disableVoreGrappleDuringSex = null;

            EnsureSmartSettingDefinition();

        }

        public override void EnsureSmartSettingDefinition()
        {
            if(baseSexEjectChance == null || baseSexEjectChance.IsInvalid())
                baseSexEjectChance = new FloatSmartSetting("RV2_RJW_Settings_BaseSexEjectChance", 50, 50, 0, 100, "RV2_RJW_Settings_BaseSexEjectChance_Tip", "0", "%");
            if(vaginalVoreAbortsPregnancies == null || vaginalVoreAbortsPregnancies.IsInvalid())
                vaginalVoreAbortsPregnancies = new BoolSmartSetting("RV2_RJW_Settings_VaginalVoreAbortsPregnancies", false, false, "RV2_RJW_Settings_VaginalVoreAbortsPregnancies_Tip");
            if(postSexProposalChance == null || postSexProposalChance.IsInvalid())
                postSexProposalChance = new FloatSmartSetting("RV2_RJW_Settings_PostSexProposalChance", 10f, 10f, 0, 100, "RV2_RJW_Settings_PostSexProposalChance_Tip", "0", "%");
            if(postSexProposalsAreForced == null || postSexProposalsAreForced.IsInvalid())
                postSexProposalsAreForced = new BoolSmartSetting("RV2_RJW_Settings_PostSexProposalsAreForced", false, false, "RV2_RJW_Settings_PostSexProposalsAreForced_Tip");
            if(postRapeProposalChance == null || postRapeProposalChance.IsInvalid())
                postRapeProposalChance = new FloatSmartSetting("RV2_RJW_Settings_PostRapeProposalChance", 10f, 10f, 0, 100, "RV2_RJW_Settings_PostRapeProposalChance_Tip", "0", "%");
            if(postRapeProposalsAreForced == null || postRapeProposalsAreForced.IsInvalid())
                postRapeProposalsAreForced = new BoolSmartSetting("RV2_RJW_Settings_PostRapeProposalsAreForced", true, true, "RV2_RJW_Settings_PostRapeProposalsAreForced_Tip");
            if(sexEjectPreyNotification == null || sexEjectPreyNotification.IsInvalid())
                sexEjectPreyNotification = new EnumSmartSetting<NotificationType>("RV2_RJW_Settings_EjectNotificationType", NotificationType.MessageNeutral, NotificationType.MessageNeutral);
            if(sexTransferPreyNotification == null || sexTransferPreyNotification.IsInvalid())
                sexTransferPreyNotification = new EnumSmartSetting<NotificationType>("RV2_RJW_Settings_TransferNotificationType", NotificationType.MessageNeutral, NotificationType.MessageNeutral);
            if(bestialityIsRape == null || bestialityIsRape.IsInvalid())
                bestialityIsRape = new BoolSmartSetting("RV2_RJW_Settings_BestialityIsRape", false, false, "RV2_RJW_Settings_BestialityIsRape_Tip");
            if(disableVoreGrappleDuringSex == null || disableVoreGrappleDuringSex.IsInvalid())
                disableVoreGrappleDuringSex = new BoolSmartSetting("RV2_RJW_Settings_DisableVoreGrappleDuringSex", true, true, "RV2_RJW_Settings_DisableVoreGrappleDuringSex_Tip");
        }

        private bool heightStale = true;
        private float height = 0f;
        private Vector2 scrollPosition;
        public void FillRect(Rect inRect)
        {
            Rect outerRect = inRect;
            UIUtility.MakeAndBeginScrollView(outerRect, height, ref scrollPosition, out Listing_Standard list);

            if(list.ButtonText("RV2_Settings_Reset".Translate()))
                Reset();

            baseSexEjectChance.DoSetting(list);
            vaginalVoreAbortsPregnancies.DoSetting(list);
            postSexProposalChance.DoSetting(list);
            postSexProposalsAreForced.DoSetting(list);
            postRapeProposalChance.DoSetting(list);
            postRapeProposalsAreForced.DoSetting(list);
            bestialityIsRape.DoSetting(list);
            disableVoreGrappleDuringSex.DoSetting(list);

            sexEjectPreyNotification.DoSetting(list);
            sexTransferPreyNotification.DoSetting(list);

            list.EndScrollView(ref height, ref heightStale);
        }

        public override void DefsLoaded()
        {
            base.DefsLoaded();
            sexEjectPreyNotification.valuePresenter = RV2_Common.NotificationPresenter;
            sexTransferPreyNotification.valuePresenter = RV2_Common.NotificationPresenter;
        }

        public override void ExposeData()
        {
            if(Scribe.mode == LoadSaveMode.Saving || Scribe.mode == LoadSaveMode.LoadingVars)
            {
                EnsureSmartSettingDefinition();
            }

            Scribe_Deep.Look(ref baseSexEjectChance, "baseSexEjectChance", new object[0]);
            Scribe_Deep.Look(ref vaginalVoreAbortsPregnancies, "vaginalVoreAbortsPregnancies", new object[0]);
            Scribe_Deep.Look(ref postSexProposalChance, "postSexProposalChance", new object[0]);
            Scribe_Deep.Look(ref postSexProposalsAreForced, "postSexProposalsAreForced", new object[0]);
            Scribe_Deep.Look(ref postRapeProposalChance, "postRapeProposalChance", new object[0]);
            Scribe_Deep.Look(ref postRapeProposalsAreForced, "postRapeProposalsAreForced", new object[0]);
            Scribe_Deep.Look(ref sexEjectPreyNotification, "sexEjectPreyNotification", new object[0]);
            Scribe_Deep.Look(ref sexTransferPreyNotification, "sexTransferPreyNotification", new object[0]);
            Scribe_Deep.Look(ref bestialityIsRape, "bestialityIsRape", new object[0]);
            Scribe_Deep.Look(ref disableVoreGrappleDuringSex, "disableVoreGrappleDuringSex", new object[0]);

            PostExposeData();
        }
    }

}
