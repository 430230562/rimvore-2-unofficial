using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using System.Linq;

namespace RimVore2
{
    public class QuirkPoolDef : ConflictableQuirkObject, IComparable<QuirkPoolDef>
    {
        public bool hidden = false;
        // all pools will be applied in ascending generationOrder, this can be used to make sure a quirk was defined / not defined in a previous generationOrder to check it
        public int generationOrder;
        public QuirkPoolType poolType = QuirkPoolType.Invalid;
        public List<QuirkDef> quirks;
        public string category;

        public int CompareTo(QuirkPoolDef otherPool)
        {
            // if they have the same generation order, order alphabetically by label
            if(generationOrder == otherPool.generationOrder)
            {
                return label.CompareTo(otherPool.label);
            }
            // if they have different orders, compare by those
            return generationOrder.CompareTo(otherPool.generationOrder);
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if(generationOrder == 0)
            {
                yield return "Required field \"generationOrder\" not set";
            }
            if(poolType == QuirkPoolType.Invalid)
            {
                yield return "Required field \"poolType \" not set";
            }
            if(quirks == null || quirks.Count == 0)
            {
                yield return "Required list \"quirks\" not set or empty";
            }
        }

    }
}
