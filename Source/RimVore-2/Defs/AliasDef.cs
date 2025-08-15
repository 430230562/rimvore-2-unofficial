using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimVore2
{
    public class AliasDef : Def
    {
        public string category;
        public List<string> aliases;
    }

    public static class AliasUtility
    {
        public static List<string> GetAliases(this string originalValue, string category)
        {
            foreach(AliasDef aliasDef in RV2_Common.AliasDefs)
            {
                // if no alias category is searched for, or this aliasDefs category matches
                if(category != null || aliasDef.category == category)
                {
                    // check if any alias in the aliasDef contains the original value as a substring
                    if(ListContainsString(aliasDef.aliases, originalValue))
                    {
                        return aliasDef.aliases;
                    }
                }
            }
            return new List<string>() { originalValue };
        }

        private static bool ListContainsString(List<string> list, string entry)
        {
            return list.Any(item => item.ToLower().Contains(entry.ToLower()));
        }
    }
}
