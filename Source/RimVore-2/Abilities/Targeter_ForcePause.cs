using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class Targeter_ForcePause : Targeter
    {
        public static Targeter_ForcePause Targeter { get; private set; }

        public Action UpdateAction;

        new public void StopTargeting()
        {
            base.StopTargeting();
            UpdateAction = null;
        }

        new public void TargeterUpdate()
        {
            base.TargeterUpdate();
            if(IsTargeting && UpdateAction != null)
                UpdateAction();
        }

        public Targeter_ForcePause()
        {
            Targeter = this;
        }
    }
}
