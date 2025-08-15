using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimVore2
{
    public class SettingsContainer_Sounds : SettingsContainer
    {
        public SettingsContainer_Sounds() { }

        private BoolSmartSetting soundsEnabled;
        private FloatSmartSetting soundVolumeModifier;
        public StringResolvable<SoundDef, bool> EnabledSounds = new StringResolvable<SoundDef, bool>(LookMode.Value);

        public bool SoundsEnabled => soundsEnabled.value;
        public float SoundVolumeModifier => soundVolumeModifier.value;

        public bool IsEnabled(SoundDef sound)
        {
            if (!EnabledSounds.ContainsKey(sound))
            {
                return false;
            }
            return EnabledSounds[sound];
        }

        public override void DefsLoaded()
        {
            base.DefsLoaded();
            if(EnabledSounds == null)
            {
                EnabledSounds = new StringResolvable<SoundDef, bool>(LookMode.Value);
            }
            EnabledSounds.Sync(RV2_Common.VoreSounds, true);
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

            soundsEnabled.DoSetting(list);
            if(SoundsEnabled)
            {
                soundVolumeModifier.DoSetting(list);
                foreach(SoundDef sound in RV2_Common.VoreSounds)
                {
                    bool state = IsEnabled(sound);
                    list.CheckboxLabeled(sound.defName, ref state, sound.defName); // defName as tooltip so the game draws the mouse-over highlight
                    EnabledSounds.SetOrAdd(sound, state);
                }
            }

            list.EndScrollView(ref height, ref heightStale);
        }

        public override void EnsureSmartSettingDefinition()
        {
            if(soundsEnabled == null || soundsEnabled.IsInvalid())
                soundsEnabled = new BoolSmartSetting("RV2_Settings_Sounds_SoundsEnabled", true, true);
            if(soundVolumeModifier == null || soundVolumeModifier.IsInvalid())
                soundVolumeModifier = new FloatSmartSetting("RV2_Settings_Sounds_SoundVolumeModifier", 1, 1, 0f, 2f);
        }

        public override void Reset()
        {
            soundsEnabled.Reset();
            soundVolumeModifier.Reset();
            EnabledSounds.Clear();
            EnabledSounds.Sync(RV2_Common.VoreSounds, true);
        }

        public override void ExposeData()
        {
            Scribe_Deep.Look(ref soundsEnabled, "soundsEnabled", new object[0]);
            Scribe_Deep.Look(ref soundVolumeModifier, "soundVolumeModifier", new object[0]);
            Scribe_Deep.Look(ref EnabledSounds, "EnabledSounds");

            PostExposeData();
        }
    }
}
