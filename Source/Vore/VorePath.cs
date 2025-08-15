using System;
using System.Collections.Generic;
using Verse;

namespace RimVore2
{
    public class VorePath : IExposable
    {
        public VorePathDef def;
        public List<VoreStage> path;

        public VoreTypeDef VoreType => def.voreType;
        public VoreGoalDef VoreGoal => def.voreGoal;

        public VorePath() { }

        public VorePath(VorePathDef def)
        {
            this.def = def;
            path = def.stages.ConvertAll(partDef => new VoreStage(partDef));
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_Collections.Look(ref path, "path", LookMode.Deep);
        }
    }
}
