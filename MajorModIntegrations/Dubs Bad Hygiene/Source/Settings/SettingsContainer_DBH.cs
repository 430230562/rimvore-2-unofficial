using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimVore2;
using Verse;

namespace RV2_DBH
{
    public class SettingsContainer_DBH : SettingsContainer
    {
        public SettingsContainer_DBH() { }

        private BoolSmartSetting limitToiletDisposalToContainers;

        public bool LimitToiletDisposalToContainers => limitToiletDisposalToContainers.value;


        public override void Reset()
        {
            limitToiletDisposalToContainers = null;

            EnsureSmartSettingDefinition();

        }

        public override void EnsureSmartSettingDefinition()
        {
            if(limitToiletDisposalToContainers == null || limitToiletDisposalToContainers.IsInvalid())
                limitToiletDisposalToContainers = new BoolSmartSetting("RV2_Settings_DBH_LimitToiletDisposalToContainers", true, true, "RV2_Settings_DBH_LimitToiletDisposalToContainers_Tip");
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

            limitToiletDisposalToContainers.DoSetting(list);

            list.EndScrollView(ref height, ref heightStale);
        }

        public override void ExposeData()
        {
            Scribe_Deep.Look(ref limitToiletDisposalToContainers, "limitToiletDisposalToContainers", new object[0]);

            PostExposeData();
        }
    }

}
