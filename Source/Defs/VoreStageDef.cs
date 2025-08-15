using RimWorld;
using System.Collections.Generic;
using System;
using Verse;
using System.Linq;

namespace RimVore2
{
    public class VoreStageDef : Def
    {
        public string partName; // used to determine the target body part to apply to
        [NoTranslate]
        public string displayPartName;
        public string displayPartTranslationKey;
        public string DisplayPartName
        {
            get
            {
                if(displayPartTranslationKey != null && displayPartTranslationKey.CanTranslate())
                {
                    return displayPartTranslationKey.Translate();
                }
                if(displayPartName != null)
                {
                    return displayPartName;
                }
                return partName;
            }
        }
        public PartGoalDef partGoal;
        public HediffDef predatorHediffDef = null;
        public ThoughtSelectorDef predatorThoughtSelector = null;
        public ThoughtDef predatorThoughtDef = null;
        public bool canReverseDirection = false;
        public List<StagePassCondition> passConditions = new List<StagePassCondition>();
        public List<TargetedRequirements> requirements = new List<TargetedRequirements>();
        public string jumpKey;  // stages with a jumpKey can jump between all stages that share the same jumpKey and whose vorePath has the same voreType

        public StageWorker onStart = null;
        public StageWorker onCycle = null;
        public StageWorker onEnd = null;

        public ThoughtDef CurrentThought(Pawn pawn)
        {
            if(predatorThoughtSelector != null)
            {
                return predatorThoughtSelector.GetThought(pawn);
            }
            return predatorThoughtDef;
        }

        public float AbstractDuration()
        {
            return passConditions.Max(condition => condition.AbstractDuration(onCycle, onStart));
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if(partName == null)
            {
                yield return "required parameter \"partName\" not provided";
            }
            if(predatorHediffDef == null)
            {
                yield return "required parameter \"predatorHediffDef\" not provided";
            }
            if(passConditions.Count < 1)
            {
                yield return "required list \"passConditions\" is empty";
            }
            foreach(StagePassCondition condition in passConditions)
            {
                foreach(string error in condition.ConfigErrors())
                {
                    yield return error;
                }
            }
            /*if(predatorThoughtDef == null && predatorThoughtSelector == null)
            {
                yield return "WARN: No \"predatorThoughtDef\" or \"predatorThoughtSelector\" set!";
            }*/
            if(onStart != null)
            {
                foreach(string error in onStart.ConfigErrors())
                {
                    yield return error;
                }
            }
            if(onCycle != null)
            {
                foreach(string error in onCycle.ConfigErrors())
                {
                    yield return error;
                }
            }
            if(onEnd != null)
            {
                foreach(string error in onEnd.ConfigErrors())
                {
                    yield return error;
                }
            }
            foreach(TargetedRequirements requirement in requirements)
            {
                foreach(string error in requirement.ConfigErrors())
                {
                    yield return error;
                }
            }
        }
    }
}
