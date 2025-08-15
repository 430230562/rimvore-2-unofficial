using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class RV2StatDef : StatDef
    {
        public List<QuirkFactor> quirkFactors;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if(quirkFactors.NullOrEmpty())
            {
                yield return "Required list \"quirkFactors\" is not set or empty";
            }
        }
    }

    
}
