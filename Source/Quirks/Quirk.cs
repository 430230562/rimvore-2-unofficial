using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class Quirk : IExposable, ICloneable
    {
        public QuirkDef def;
        public QuirkPoolDef Pool => def.GetPool();

        public Quirk(QuirkDef def)
        {
            this.def = def;
        }

        public Quirk() { }

        public virtual void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
        }

        public bool IsValid()
        {
            if(def == null)
            {
                return false;
            }
            return true;
        }

        public virtual object Clone()
        {
            return new Quirk()
            {
                def = this.def
            };
        }
    }

    public class TempQuirk : Quirk
    {
        public TempQuirk(QuirkDef def, int duration) : base(def)
        {
            totalDuration = duration;
            durationLeft = duration;
        }

        public TempQuirk() : base() { }

        public Quirk originalQuirk;
        public int totalDuration;
        public int durationLeft;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref originalQuirk, "originalQuirk", new object[0]);
            Scribe_Values.Look(ref totalDuration, "totalDuration");
            Scribe_Values.Look(ref durationLeft, "durationLeft");
        }

        public override object Clone()
        {
            return new TempQuirk()
            {
                def = base.def,
                originalQuirk = this.originalQuirk,
                totalDuration = this.totalDuration,
                durationLeft = this.durationLeft
            };
        }
    }
    public class IdeologyQuirk : Quirk
    {
        public IdeologyQuirk(QuirkDef def, Ideo ideo, Quirk replacedQuirk = null) : base(def)
        {
            this.ideo = ideo;
            this.replacedQuirk = replacedQuirk;
        }

        public IdeologyQuirk() : base() { }

        public Quirk replacedQuirk;
        public Ideo ideo;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref ideo, "ideo");
            Scribe_Deep.Look(ref replacedQuirk, "replacedQuirk", new object[0]);
        }

        public override object Clone()
        {
            return new IdeologyQuirk()
            {
                def = base.def,
                replacedQuirk = this.replacedQuirk,
                ideo = this.ideo
            };
        }
    }
}
