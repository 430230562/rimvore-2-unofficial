/*using System;
using System.Collections.Generic;
using Verse;
using Verse.Sound;

namespace RimVore2
{
    public class VoreSoundWorker : IExposable
    {
        public VoreSoundWorker() { }

        public SoundDef soundDef = null;
        public int rollIntervalRareTicks = 1;
        public float rollChance = 1f;

        public void ExposeData()
        {
            Scribe_Defs.Look(ref soundDef, "soundDef");
            Scribe_Values.Look(ref rollIntervalRareTicks, "rollIntervalRareTicks");
            Scribe_Values.Look(ref rollChance, "rollChance");
        }
    }
}*/