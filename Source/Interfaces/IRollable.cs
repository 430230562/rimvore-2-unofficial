using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimVore2
{
    public interface IRollable
    {
        float GetRollStrength(VoreTrackerRecord record);
        float GetRollChance(VoreTrackerRecord record);
        bool IntervalValid(VoreTrackerRecord record);
        void Work(VoreTrackerRecord record);
    }
}
