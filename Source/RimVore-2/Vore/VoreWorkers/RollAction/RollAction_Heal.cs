using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace RimVore2
{
    public class RollAction_Heal : RollAction
    {
        public override bool TryAction(VoreTrackerRecord record, float rollStrength)
        {
            base.TryAction(record, rollStrength);
            rollStrength *= 10; // there used to be a completely new roll after leeching food from predator, but with the new setup and no boxing rolls, we need to manually change the rolled value (or implement more RollActions that affect the rollStrength)

            IEnumerable<Hediff> tendableInjuries = TargetPawn.health.hediffSet.GetHediffsTendable();
            if(!tendableInjuries.EnumerableNullOrEmpty())
            {
                return TendPawn(tendableInjuries, rollStrength);
            }
            else
            {
                return HealPawn(rollStrength);
            }
        }

        private bool TendPawn(IEnumerable<Hediff> injuries, float rollStrength)
        {
            float quality = rollStrength;
            Hediff injury = injuries.RandomElement();
            injury.Tended(quality, quality);
            if(RV2Log.ShouldLog(false, "OngoingVore"))
                RV2Log.Message($"Tended injury {injury.Label} with quality {rollStrength}", "OngoingVore");
            return true;
        }

        private bool HealPawn(float rollStrength)
        {
            float quality = rollStrength;
            List<Hediff> injuries = TargetPawn.GetHealableInjuries();

            if(injuries.NullOrEmpty())
            {
                if(RV2Log.ShouldLog(false, "OngoingVore"))
                    RV2Log.Message("No injuries to heal", "OngoingVore");
                return false;
            }
            Hediff_Injury injury = (Hediff_Injury)injuries.RandomElement();
            injury.Severity -= quality;
            if(RV2Log.ShouldLog(false, "OngoingVore"))
                RV2Log.Message("Healed " + TargetPawn.Label + "'s hediff " + injury.def.label + " by " + quality, "OngoingVore");
            TargetPawn.health.hediffSet.DirtyCache();
            return true;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if(invert)
            {
                yield return "Cannot use \"invert\" for healing";
            }
        }
    }
}
