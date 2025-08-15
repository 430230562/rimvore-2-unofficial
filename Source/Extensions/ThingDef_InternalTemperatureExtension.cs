using System;
using System.Collections.Generic;
using Verse;

namespace RimVore2
{
    public class InternalTemperatureExtension : DefModExtension
    {
        public float temperature = 30f; // normal human internal temperature is 36.5, but that kills pawns with heat stroke. 30 is very warm, but survivable.
    }
}
