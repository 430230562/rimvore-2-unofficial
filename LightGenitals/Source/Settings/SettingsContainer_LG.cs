using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimVore2;
using Verse;

namespace LightGenitals
{
    public class SettingsContainer_LG : SettingsContainer
    {
        public SettingsContainer_LG() { }

        private FloatSmartSetting maleChanceToSpawnWithBreasts;
        private FloatSmartSetting maleChanceToSpawnWithVagina;
        private FloatSmartSetting maleChanceToSpawnWithPenis;
        private BoolSmartSetting maleCanSpawnWithNoGenitals;

        private FloatSmartSetting femaleChanceToSpawnWithBreasts;
        private FloatSmartSetting femaleChanceToSpawnWithVagina;
        private FloatSmartSetting femaleChanceToSpawnWithPenis;
        private BoolSmartSetting femaleCanSpawnWithNoGenitals;

        public float MaleChanceToSpawnWithBreasts => maleChanceToSpawnWithBreasts.value / 100;
        public float MaleChanceToSpawnWithVagina => maleChanceToSpawnWithVagina.value / 100;
        public float MaleChanceToSpawnWithPenis => maleChanceToSpawnWithPenis.value / 100;
        public bool MaleCanSpawnWithNoGenitals => maleCanSpawnWithNoGenitals.value;
        public float FemaleChanceToSpawnWithBreasts => femaleChanceToSpawnWithBreasts.value / 100;
        public float FemaleChanceToSpawnWithVagina => femaleChanceToSpawnWithVagina.value / 100;
        public float FemaleChanceToSpawnWithPenis => femaleChanceToSpawnWithPenis.value / 100;
        public bool FemaleCanSpawnWithNoGenitals => femaleCanSpawnWithNoGenitals.value;


        public override void Reset()
        {
            maleChanceToSpawnWithBreasts = null;
            maleChanceToSpawnWithVagina = null;
            maleChanceToSpawnWithPenis = null;
            maleCanSpawnWithNoGenitals = null;
            femaleChanceToSpawnWithBreasts = null;
            femaleChanceToSpawnWithVagina = null;
            femaleChanceToSpawnWithPenis = null;
            femaleCanSpawnWithNoGenitals = null;

            EnsureSmartSettingDefinition();
        }

        public override void EnsureSmartSettingDefinition()
        {
            if(maleChanceToSpawnWithBreasts == null || maleChanceToSpawnWithBreasts.IsInvalid())
                maleChanceToSpawnWithBreasts = new FloatSmartSetting("LightGenitals_chanceToSpawnWithBreasts", 0, 0, 0, 100, null, "0", "%");
            if(maleChanceToSpawnWithVagina == null || maleChanceToSpawnWithVagina.IsInvalid())
                maleChanceToSpawnWithVagina = new FloatSmartSetting("LightGenitals_chanceToSpawnWithVagina", 0, 0, 0, 100, null, "0", "%");
            if(maleChanceToSpawnWithPenis == null || maleChanceToSpawnWithPenis.IsInvalid())
                maleChanceToSpawnWithPenis = new FloatSmartSetting("LightGenitals_chanceToSpawnWithPenis", 100, 100, 0, 100, null, "0", "%");
            if(maleCanSpawnWithNoGenitals == null || maleCanSpawnWithNoGenitals.IsInvalid())
                maleCanSpawnWithNoGenitals = new BoolSmartSetting("LightGenitals_canSpawnWithNoGenitals", true, true, "LightGenitals_canSpawnWithNoGenitals_Tip");
            if(femaleChanceToSpawnWithBreasts == null || femaleChanceToSpawnWithBreasts.IsInvalid())
                femaleChanceToSpawnWithBreasts = new FloatSmartSetting("LightGenitals_chanceToSpawnWithBreasts", 100, 100, 0, 100, null, "0", "%");
            if(femaleChanceToSpawnWithVagina == null || femaleChanceToSpawnWithVagina.IsInvalid())
                femaleChanceToSpawnWithVagina = new FloatSmartSetting("LightGenitals_chanceToSpawnWithVagina", 100, 100, 0, 100, null, "0", "%");
            if(femaleChanceToSpawnWithPenis == null || femaleChanceToSpawnWithPenis.IsInvalid())
                femaleChanceToSpawnWithPenis = new FloatSmartSetting("LightGenitals_chanceToSpawnWithPenis", 0, 0, 0, 100, null, "0", "%");
            if(femaleCanSpawnWithNoGenitals == null || femaleCanSpawnWithNoGenitals.IsInvalid())
                femaleCanSpawnWithNoGenitals = new BoolSmartSetting("LightGenitals_canSpawnWithNoGenitals", true, true, "LightGenitals_canSpawnWithNoGenitals_Tip");
        }

        private bool maleHeightStale = true;
        private float maleHeight = 0f;
        private Vector2 maleScrollPosition;
        private bool femaleHeightStale = true;
        private float femaleHeight = 0f;
        private Vector2 femaleScrollPosition;

        public void FillRect(Rect inRect)
        {
            UIUtility.SplitRectVertically(inRect, out Rect maleRect, out Rect femaleRect);

            // male section
            UIUtility.MakeAndBeginScrollView(maleRect, maleHeight, ref maleScrollPosition, out Listing_Standard maleList);
            maleList.HeaderLabel("LightGenitals_Male".Translate(), true);
            maleChanceToSpawnWithBreasts.DoSetting(maleList);
            maleChanceToSpawnWithVagina.DoSetting(maleList);
            maleChanceToSpawnWithPenis.DoSetting(maleList);
            maleCanSpawnWithNoGenitals.DoSetting(maleList);
            UIUtility.EndScrollView(maleList, ref maleHeight, ref maleHeightStale);

            // female section
            UIUtility.MakeAndBeginScrollView(femaleRect, femaleHeight, ref femaleScrollPosition, out Listing_Standard femaleList);
            femaleList.HeaderLabel("LightGenitals_Female".Translate(), true);
            femaleChanceToSpawnWithBreasts.DoSetting(femaleList);
            femaleChanceToSpawnWithVagina.DoSetting(femaleList);
            femaleChanceToSpawnWithPenis.DoSetting(femaleList);
            femaleCanSpawnWithNoGenitals.DoSetting(femaleList);
            UIUtility.EndScrollView(femaleList, ref femaleHeight, ref femaleHeightStale);
        }

        public override void ExposeData()
        {
            Scribe_Deep.Look(ref maleChanceToSpawnWithBreasts, "maleChanceToSpawnWithBreasts", new object[0]);
            Scribe_Deep.Look(ref maleChanceToSpawnWithVagina, "maleChanceToSpawnWithVagina", new object[0]);
            Scribe_Deep.Look(ref maleChanceToSpawnWithPenis, "maleChanceToSpawnWithPenis", new object[0]);
            Scribe_Deep.Look(ref maleCanSpawnWithNoGenitals, "maleCanSpawnWithNoGenitals", new object[0]);
            Scribe_Deep.Look(ref femaleChanceToSpawnWithBreasts, "femaleChanceToSpawnWithBreasts", new object[0]);
            Scribe_Deep.Look(ref femaleChanceToSpawnWithVagina, "femaleChanceToSpawnWithVagina", new object[0]);
            Scribe_Deep.Look(ref femaleChanceToSpawnWithPenis, "femaleChanceToSpawnWithPenis", new object[0]);
            Scribe_Deep.Look(ref femaleCanSpawnWithNoGenitals, "femaleCanSpawnWithNoGenitals", new object[0]);

            PostExposeData();
        }
    }

}
