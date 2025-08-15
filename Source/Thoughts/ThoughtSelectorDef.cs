using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace RimVore2
{
    public class ThoughtSelectorDef : Def
    {
        public ThoughtDef baseThought;
        public List<ThoughtOverride> overrides;

        public ThoughtDef GetThought(Pawn pawn)
        {
            if(overrides == null || overrides.Count == 0)
            {
                return baseThought;
            }
            if(pawn.QuirkManager() == null)
            {
                return baseThought;
            }
            List<ThoughtOverride> validOverrides = overrides?.FindAll(ov => pawn.QuirkManager().HasQuirk(ov.quirk));
            switch(validOverrides.Count)
            {
                case 0:
                    return baseThought;
                case 1:
                    return validOverrides[0].thought;
                default:
                    return validOverrides.MaxBy(ov => ov.priority).thought;
            }
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if(baseThought == null)
            {
                yield return "Required field \"baseThought\" is null";
            }
            if(overrides != null && overrides.Count > 0)
            {
                IEnumerable<IGrouping<float, ThoughtOverride>> groupedByPriority = overrides.GroupBy(ov => ov.priority);
                if(groupedByPriority.Any(group => group.Count() > 1))
                {
                    yield return "List \"overrides\" has multiple entries, but not all list elements have a unique priority number to sort them with. Duplicate priorities: " + string.Join(", ", groupedByPriority.Where(group => group.Count() > 1).Select(group => group.Key));
                }
                IEnumerable<IGrouping<QuirkDef, ThoughtOverride>> groupedByQuirks = overrides.GroupBy(ov => ov.quirk);
                if(groupedByQuirks.Any(group => group.Count() > 1))
                {
                    yield return "List \"overrides\" has a duplicate quirk, the duplicate with the highest priority will be used. Duplicate quirks: " + string.Join(", ", groupedByQuirks.Where(group => group.Count() > 1).Select(group => group.Key.defName));
                }
            }
        }
    }

    public class ThoughtOverride
    {
        public float priority = 0f;
        public QuirkDef quirk;
        public ThoughtDef thought;
    }
}
