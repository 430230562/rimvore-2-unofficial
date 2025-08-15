using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimVore2
{
    public class SettingsContainer_Ideology : SettingsContainer
    {
        public SettingsContainer_Ideology() { }

        private FloatSmartSetting autoVoreInterceptChance;
        private FloatSmartSetting voreFeastProposalAcceptanceModifier;

        public float AutoVoreInterceptionChance => autoVoreInterceptChance.value / 100;
        public float VoreFeastProposalAcceptanceModifier => voreFeastProposalAcceptanceModifier.value;

        public override void EnsureSmartSettingDefinition()
        {
            if(autoVoreInterceptChance == null || autoVoreInterceptChance.IsInvalid())
                autoVoreInterceptChance = new FloatSmartSetting("RV2_Settings_Ideology_AutoVoreInterceptChance", 50f, 50f, 0, 100, "RV2_Settings_Ideology_AutoVoreInterceptChance_Tip", "0", "%");
            if(voreFeastProposalAcceptanceModifier == null || voreFeastProposalAcceptanceModifier.IsInvalid())
                voreFeastProposalAcceptanceModifier = new FloatSmartSetting("RV2_Settings_Ideology_VoreFeastProposalAcceptanceModifier", 1.3f, 1.3f, 1f, 4f, "RV2_Settings_Ideology_VoreFeastProposalAcceptanceModifier_Tip", "0.00");
        }

        public override void Reset()
        {
            //autoVoreInterceptChance.Reset();
            //voreFeastProposalAcceptanceModifier.Reset();

            autoVoreInterceptChance = null;
            voreFeastProposalAcceptanceModifier = null;

            EnsureSmartSettingDefinition();
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

            autoVoreInterceptChance.DoSetting(list);
            voreFeastProposalAcceptanceModifier.DoSetting(list);

            list.EndScrollView(ref height, ref heightStale);
        }

        public override void ExposeData()
        {
            if(Scribe.mode == LoadSaveMode.Saving || Scribe.mode == LoadSaveMode.LoadingVars)
            {
                EnsureSmartSettingDefinition();
            }
            Scribe_Deep.Look(ref autoVoreInterceptChance, "autoVoreInterceptChance", new object[0]);
            Scribe_Deep.Look(ref voreFeastProposalAcceptanceModifier, "voreFeastProposalAcceptanceModifier", new object[0]);

            PostExposeData();
        }
    }
}