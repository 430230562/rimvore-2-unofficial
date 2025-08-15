using RimWorld;
using System.Collections.Generic;
using System;
using Verse;
using Verse.Sound;

namespace RimVore2
{
    public static class SoundManager
    {
        public static void PlaySingleSound(Pawn pawn, SoundDef soundDef)
        {
            if(pawn.Map == null)
            {
                return;
            }
            if(!RV2Mod.Settings.sounds.SoundsEnabled)
            {
                return;
            }
            string defName = soundDef.defName;
            if(defName.Contains("Moan") || defName.Contains("Orgasm"))
            {
                if(pawn.RaceProps?.Animal == true)
                {
                    return;
                }
                switch(pawn.gender)
                {
                    case Gender.Male:
                        defName = defName + "Male";
                        soundDef = DefDatabase<SoundDef>.GetNamed(defName, false);
                        break;
                    case Gender.Female:
                        defName = defName + "Female";
                        soundDef = DefDatabase<SoundDef>.GetNamed(defName, false);
                        break;
                    case Gender.None:
                        if(RV2Log.ShouldLog(true, "Sounds"))
                            RV2Log.Message("No moans and orgasms for genderless pawns.", false, "Sounds");
                        return;
                }
            }
            if(soundDef == null)
            {
                if(RV2Log.ShouldLog(true, "Sounds"))
                    RV2Log.Message($"No soundDef found for {defName}, not playing sound", false, "Sounds");
                return;
            }
            // check if sound is enabled in settings
            if(!RV2Mod.Settings.sounds.IsEnabled(soundDef))
            {
                if(RV2Log.ShouldLog(true, "Sounds"))
                    RV2Log.Message("Not playing blocked sound (or missing soundDef in settings)", false, "Sounds");
                return;
            }
            if(RV2Log.ShouldLog(true, "Sounds"))
                RV2Log.Message("Playing sound: " + soundDef.defName, false, "Sounds");
            SoundInfo sinfo = new TargetInfo(pawn.Position, pawn.Map);
            sinfo.volumeFactor = RV2Mod.Settings.sounds.SoundVolumeModifier;
            soundDef.PlayOneShot(sinfo);
        }
    }
}
