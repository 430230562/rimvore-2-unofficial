using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class VoreQuirkThoughtDef : ThoughtDef
    {
        public List<QuirkDef> thisPawnQuirks;
        public List<QuirkDef> otherPawnQuirks;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if(thisPawnQuirks.NullOrEmpty() && otherPawnQuirks.NullOrEmpty())
            {
                yield return "Neither \"thisPawnQuirks\" or \"otherPawnQuirks\" are set, the thought will always be applied!";
            }
        }
    }
}
