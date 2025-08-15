using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace RimVore2
{
    public class RollAction_RecordTale_VoreInitiation : RollAction_RecordTale
    {
        protected override TaleDef Tale => record.VorePath.def.initTale;
        protected override Def AdditionalDef => record.VorePath.def;
    }
}
