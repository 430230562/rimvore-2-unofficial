using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class Stance_Grapple : Stance_Busy
    {
        public Stance_Grapple() : base()
        {
            ticksLeft = 5000;
        }
    }
}
