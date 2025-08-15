using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class ScatOrBonesBackstoryDescription : DefModExtension
    {
        public string descriptionForScatAndBones;
        public string descriptionForScat;
        public string descriptionForBones;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if(descriptionForBones == null && descriptionForScat == null && descriptionForScatAndBones == null)
            {
                yield return "One of these fields must be set: \"descriptionForScat\" / \"descriptionForBones\" / \"descriptionForScatAndBones\"";
            }
        }
    }
}
