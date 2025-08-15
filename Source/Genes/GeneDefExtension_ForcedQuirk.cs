using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class GeneDefExtension_ForcedHediff : DefModExtension
    {
        public List<HediffDef> hediffs = new List<HediffDef>();
        public bool removeHediffsOnGeneRemoval;


        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
                yield return error;
            if(hediffs.NullOrEmpty())
                yield return $"List \"{nameof(hediffs)}\" is null or empty";
        }
    }
}
