using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimVore2
{
    public abstract class PreceptComp_Quirk : PreceptComp
    {
        public List<QuirkDef> quirks = new List<QuirkDef>();

        public override IEnumerable<string> GetDescriptions()
        {
            foreach(QuirkDef quirk in quirks)
            {
                yield return "      - " + quirk.label;
            }
        }
        public override IEnumerable<string> ConfigErrors(PreceptDef parent)
        {
            foreach(string error in base.ConfigErrors(parent))
            {
                yield return error;
            }
            if(quirks.NullOrEmpty())
            {
                yield return "Required list \"quirk\" is empty";
            }
        }
    }

    public class PreceptComp_Quirk_AddQuirks : PreceptComp_Quirk { }
    public class PreceptComp_Quirk_RemoveQuirks : PreceptComp_Quirk { }
    public class PreceptComp_Quirk_EnsureOneOf : PreceptComp_Quirk { }
}