using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public static class QuirkThoughtUtility
    {
        private static List<ThoughtDef> cachedThoughtsRequiringQuirks = null;

        public static bool RequiresQuirk(ThoughtDef thought)
        {
            if(cachedThoughtsRequiringQuirks == null)
                CacheThoughtsRequiringQuirks();

            return cachedThoughtsRequiringQuirks.Contains(thought);
        }

        private static void CacheThoughtsRequiringQuirks()
        {
            cachedThoughtsRequiringQuirks = DefDatabase<QuirkDef>.AllDefsListForReading
                .SelectMany(quirk => quirk.comps)   // get all comps
                .Where(comp => comp is QuirkComp_SituationalThoughtEnabler) // only use the ones that enable situational thoughts
                .Cast<QuirkComp_SituationalThoughtEnabler>()    // cast to those enablers
                .Select(comp => comp.enabledThought)    // retrieve the thought they enable
                .ToList();  // convert to list
        }
    }
}
