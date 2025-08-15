using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimVore2;
using Verse;

namespace RV2_PawnMorpher
{
    public class SettingsContainer_PM : SettingsContainer
    {
        public SettingsContainer_PM() { }

        private BoolSmartSetting pawnmorpherAnimalReformation;

        public bool PawnmorpherAnimalReformation => pawnmorpherAnimalReformation.value;

        public override void Reset()
        {
            pawnmorpherAnimalReformation = null;

            EnsureSmartSettingDefinition();
        }

        public override void EnsureSmartSettingDefinition()
        {
            if (pawnmorpherAnimalReformation == null || pawnmorpherAnimalReformation.IsInvalid())
                pawnmorpherAnimalReformation = new BoolSmartSetting("RV2_PM_Settings_PawnmorpherAnimalReformation", false, false, "RV2_PM_Settings_PawnmorpherAnimalReformation_Tip");
        }

        private bool heightStale = true;
        private float height = 0f;
        private Vector2 scrollPosition;
        public void FillRect(Rect inRect)
        {
            Rect outerRect = inRect;
            UIUtility.MakeAndBeginScrollView(outerRect, height, ref scrollPosition, out Listing_Standard list);

            if (list.ButtonText("RV2_Settings_Reset".Translate()))
                Reset();

            pawnmorpherAnimalReformation.DoSetting(list);

            list.EndScrollView(ref height, ref heightStale);
        }

        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving || Scribe.mode == LoadSaveMode.LoadingVars)
            {
                EnsureSmartSettingDefinition();
            }

            Scribe_Deep.Look(ref pawnmorpherAnimalReformation, "PawnmorpherAnimalReformation", new object[0]);

            PostExposeData();
        }
    }
}
