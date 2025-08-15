using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class PreceptThoughtWorker_ListeningKeyword : DefModExtension
    {
        public string keyword;
        public int maxTimeSinceLast = 8;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if(keyword == null)
            {
                yield return "Required field \"keyword\" not set";
            }
            if(maxTimeSinceLast < 0)
            {
                yield return "Field \"maxTimeSinceLast\" below 0";
            }
        }
    }

}
