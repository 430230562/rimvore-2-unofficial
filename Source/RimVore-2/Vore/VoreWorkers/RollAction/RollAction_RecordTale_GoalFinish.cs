using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace RimVore2
{
    public class RollAction_RecordTale_GoalFinish : RollAction_RecordTale
    {
        protected override TaleDef Tale => record.VorePath.VoreGoal.goalFinishTale;
        protected override Def AdditionalDef => record.VorePath.VoreGoal;
    }
}
