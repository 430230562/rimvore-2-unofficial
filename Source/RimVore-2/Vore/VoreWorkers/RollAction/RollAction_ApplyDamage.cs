using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace RimVore2
{
    public class RollAction_ApplyDamage : RollAction
    {
        DamageDef damageDef;
        float armorPenetration = 0f;
        float angle = -1f;
        // BodyPartRecord hitPart = null;  // can't really set this in XML, would need to use a matching logic based on BodyPartDef

        public override bool TryAction(VoreTrackerRecord record, float rollStrength)
        {
            base.TryAction(record, rollStrength);
            DamageInfo dinfo = new DamageInfo(damageDef, rollStrength, armorPenetration, angle);
            DamageWorker.DamageResult result = TargetPawn.TakeDamage(dinfo);
            return result.totalDamageDealt > 0;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if(target == VoreRole.Invalid)
            {
                yield return "required field \"target\" is not set";
            }
            if(damageDef == null)
            {
                yield return "required field \"damageDef\" is not set";
            }
        }
    }
}
