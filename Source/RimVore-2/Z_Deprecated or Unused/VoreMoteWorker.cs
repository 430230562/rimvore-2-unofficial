/*using System;
using System.Collections.Generic;
using Verse;
using Verse.Sound;

namespace RimVore2
{
    public class VoreMoteWorker : IExposable
    {
        public VoreMoteWorker() { }

        public ThingDef moteDef = null;
        public int rollIntervalRareTicks = 1;
        public float rollChance = 1f;

        public void ExposeData()
        {
            Scribe_Defs.Look(ref moteDef, "moteDef");
            Scribe_Values.Look(ref rollIntervalRareTicks, "rollIntervalRareTicks");
            Scribe_Values.Look(ref rollChance, "rollChance");
        }
    }
}*/