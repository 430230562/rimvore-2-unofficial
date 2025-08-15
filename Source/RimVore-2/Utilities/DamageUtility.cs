using System;
using System.Collections.Generic;
using Verse;

namespace RimVore2
{
    public static class DamageUtility
    {
        public static float GetCurrentLethalDamage(Pawn pawn)
        {
            Pawn_HealthTracker healthTracker = pawn.health;
            float num = 0f;
            for(int i = 0; i < healthTracker.hediffSet.hediffs.Count; i++)
            {
                if(healthTracker.hediffSet.hediffs[i] is Hediff_Injury)
                {
                    num += healthTracker.hediffSet.hediffs[i].Severity;
                }
            }
            return num;
        }

        public static float GetAvailableDamageUntilLethal(Pawn pawn)
        {
            return pawn.health.LethalDamageThreshold - GetCurrentLethalDamage(pawn);
        }
    }
}
