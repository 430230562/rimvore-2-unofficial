using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace RimVore2
{
    public class RollAction_PassValue_Subtract : RollAction_PassValue
    {
        public override bool TryAction(VoreTrackerRecord record, float rollStrength)
        {
            base.TryAction(record, rollStrength);
            float newValue = record.PassValues[name] - rollStrength;
            record.ModifyPassValue(name, newValue, minValue, maxValue);
            return true;
        }
    }
}
