using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class TemporaryQuirkGiver : Def
    {
        public List<string> criteriaKeywords = null;
        public List<QuirkWithDuration> quirksToApply = null;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if(criteriaKeywords.NullOrEmpty())
            {
                yield return "Required list \"criteriaKeywords\" not filled";
            }
            if(quirksToApply.NullOrEmpty())
            {
                yield return "Required list \"quirksToApply\" not filled";
            }
            else
            {
                foreach(QuirkWithDuration tempQuirk in quirksToApply)
                {
                    foreach(string error in tempQuirk.ConfigErrors())
                    {
                        yield return error;
                    }
                }
            }

        }
    }

    public class QuirkWithDuration
    {
        public QuirkDef quirk;
        public int duration = -1;

        public IEnumerable<string> ConfigErrors()
        {
            if(quirk == null)
            {
                yield return "Required field \"quirk\" is not set";
            }
            if(duration <= 0)
            {
                yield return "Required field \"duration\" is not set or below 0";
            }
        }
    }
}
