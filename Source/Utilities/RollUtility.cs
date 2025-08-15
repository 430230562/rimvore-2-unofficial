using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public static class RollUtility
    {
        public static List<RollAction> GetAllRollActions(VorePathDef path, Func<VoreStageDef, StageWorker> stageWorkerFunc)
        {
            if(stageWorkerFunc == null)
            {
                throw new ArgumentException();
            }
            List<RollAction> allActions = new List<RollAction>();
            if(path.stages == null)
            {
                Log.Warning("path " + path.defName + " has no stages???");
                return new List<RollAction>();
            }
            foreach(VoreStageDef stage in path.stages)
            {
                allActions.AddRange(GetAllRollActions(stage, stageWorkerFunc));
            }
            return allActions;
        }

        public static List<RollAction> GetAllRollActions(VoreStageDef stage, Func<VoreStageDef, StageWorker> stageWorkerFunc)
        {
            List<RollAction> allActions = new List<RollAction>();
            StageWorker worker = stageWorkerFunc(stage);
            List<RollAction> stageActions = worker?.actions;
            if(stageActions != null)
            {
                allActions.AddRange(stageActions);
            }
            List<Roll> stageRolls = worker?.Rolls;
            if(stageRolls != null)
            {
                foreach(Roll roll in stageRolls)
                {
                    if(roll.actionsOnSuccess != null)
                    {
                        allActions.AddRange(roll.actionsOnSuccess);
                    }
                    if(roll.actionsOnFailure != null)
                    {
                        allActions.AddRange(roll.actionsOnFailure);
                    }
                }
            }
            return allActions;
        }
    }
}
