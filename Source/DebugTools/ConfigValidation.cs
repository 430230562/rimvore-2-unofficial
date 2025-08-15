using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimVore2
{
    public static class ConfigValidation
    {
        const string decorationS = "--";
        const string decorationL = "------";

        [DebugAction("RimVore-2", "Guess path durations", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Entry)]
        public static void GuessPathDurations()
        {
            VorePathDurationUtility.CalculateAllPathDurations();
        }
        [DebugAction("RimVore-2", "PassValue validation", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Entry)]
        public static void PassValueValidation()
        {
            Log.Message(decorationL + "PassValue validation" + decorationL);
            IEnumerable<string> messages = GetPassValueValidationMessages();
            if(!messages.EnumerableNullOrEmpty())
            {
                Log.Message(string.Join("\n", messages));
            }
            else
            {
                Log.Message("No errors.");
            }
        }
        [DebugAction("RimVore-2", "PassValue validation (errors only)", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Entry)]
        public static void PassValueValidationErrorsOnly()
        {
            Log.Message(decorationL + "PassValue validation (errors only)" + decorationL);
            IEnumerable<string> messages = GetPassValueValidationMessages(true);
            if(!messages.EnumerableNullOrEmpty())
            {
                Log.Message(string.Join("\n", messages));
            }
            else
            {
                Log.Message("No errors.");
            }
        }
        [DebugAction("RimVore-2", "Thought Usage Validation", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Entry)]
        public static void ThoughtUsageValidation()
        {
            Log.Message(decorationL + "Thought usage validation" + decorationL);
            IEnumerable<string> messages = GetThoughtUsageValidationMessages();
            if(!messages.EnumerableNullOrEmpty())
            {
                Log.Message(string.Join("\n", messages));
            }
            else
            {
                Log.Message("No errors.");
            }
        }
        [DebugAction("RimVore-2", "Thought Usage Validation (errors only)", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Entry)]
        public static void ThoughtUsageValidationErrorsOnly()
        {
            Log.Message(decorationL + "Thought usage validation (errors only)" + decorationL);
            IEnumerable<string> messages = GetThoughtUsageValidationMessages(true);
            if(!messages.EnumerableNullOrEmpty())
            {
                Log.Message(string.Join("\n", messages));
            }
            else
            {
                Log.Message("No errors.");
            }
        }
        [DebugAction("RimVore-2", "Hediff Usage Validation", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Entry)]
        public static void HediffUsageValidation()
        {
            Log.Message(decorationL + "Hediff usage validation" + decorationL);
            IEnumerable<string> messages = GetHediffUsageValidationMessages();
            if(!messages.EnumerableNullOrEmpty())
            {
                Log.Message(string.Join("\n", messages));
            }
            else
            {
                Log.Message("No errors.");
            }
        }
        [DebugAction("RimVore-2", "Hediff Usage Validation (errors only)", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Entry)]
        public static void HediffUsageValidationErrorsOnly()
        {
            Log.Message(decorationL + "Thought usage validation (errors only)" + decorationL);
            IEnumerable<string> messages = GetHediffUsageValidationMessages(true);
            if(!messages.EnumerableNullOrEmpty())
            {
                Log.Message(string.Join("\n", messages));
            }
            else
            {
                Log.Message("No errors.");
            }
        }
        [DebugAction("RimVore-2", "VoreStage Usage Validation", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Entry)]
        public static void VoreStageUsageValidation()
        {
            Log.Message(decorationL + "Thought usage validation" + decorationL);
            IEnumerable<string> messages = GetVoreStageUsageValidationMessages();
            if(!messages.EnumerableNullOrEmpty())
            {
                Log.Message(string.Join("\n", messages));
            }
            else
            {
                Log.Message("No errors.");
            }
        }
        [DebugAction("RimVore-2", "VoreStage Usage Validation (errors only)", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Entry)]
        public static void VoreStageUsageValidationErrorsOnly()
        {
            Log.Message(decorationL + "Thought usage validation (errors only)" + decorationL);
            IEnumerable<string> messages = GetVoreStageUsageValidationMessages(true);
            if(!messages.EnumerableNullOrEmpty())
            {
                Log.Message(string.Join("\n", messages));
            }
            else
            {
                Log.Message("No errors.");
            }
        }
        [DebugAction("RimVore-2", "Roll duplicate Validation", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Entry)]
        public static void RollDuplicateValidation()
        {
            Log.Message(decorationL + "Roll duplicate validation" + decorationL);
            IEnumerable<string> messages = GetRollDuplicateValidationMessages();
            if(!messages.EnumerableNullOrEmpty())
            {
                Log.Message(string.Join("\n", messages));
            }
            else
            {
                Log.Message("No errors.");
            }
        }
        [DebugAction("RimVore-2", "Stage Tale Validation", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Entry)]
        public static void StageTaleValidation()
        {
            Log.Message(decorationL + "Stage Tale validation" + decorationL);
            IEnumerable<string> messages = GetStageTaleUsageValidationMessages();
            if(!messages.EnumerableNullOrEmpty())
            {
                Log.Message(string.Join("\n", messages));
            }
            else
            {
                Log.Message("No errors.");
            }
        }
        [DebugAction("RimVore-2", "Stage Tale Validation (errors only)", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Entry)]
        public static void StageTaleValidationErrorsOnly()
        {
            Log.Message(decorationL + "Stage Tale validation (errors only)" + decorationL);
            IEnumerable<string> messages = GetStageTaleUsageValidationMessages(true);
            if(!messages.EnumerableNullOrEmpty())
            {
                Log.Message(string.Join("\n", messages));
            }
            else
            {
                Log.Message("No errors.");
            }
        }
        [DebugAction("RimVore-2", "Path Tale Validation", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Entry)]
        public static void PathTaleValidation()
        {
            Log.Message(decorationL + "Path Tale validation" + decorationL);
            IEnumerable<string> messages = GetPathTaleUsageValidationMessages();
            if(!messages.EnumerableNullOrEmpty())
            {
                Log.Message(string.Join("\n", messages));
            }
            else
            {
                Log.Message("No errors.");
            }
        }
        [DebugAction("RimVore-2", "Path Tale Validation (errors only)", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Entry)]
        public static void PathTaleValidationErrorsOnly()
        {
            Log.Message(decorationL + "Path Tale validation (errors only)" + decorationL);
            IEnumerable<string> messages = GetPathTaleUsageValidationMessages(true);
            if(!messages.EnumerableNullOrEmpty())
            {
                Log.Message(string.Join("\n", messages));
            }
            else
            {
                Log.Message("No errors.");
            }
        }
        [DebugAction("RimVore-2", "Record Validation", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Entry)]
        public static void RecordValidation()
        {
            Log.Message(decorationL + "Record validation" + decorationL);
            IEnumerable<string> messages = GetRecordValidationMessages();
            if(!messages.EnumerableNullOrEmpty())
            {
                Log.Message(string.Join("\n", messages));
            }
            else
            {
                Log.Message("No errors.");
            }
        }
        [DebugAction("RimVore-2", "Record Validation (errors only)", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Entry)]
        public static void RecordValidationErrorsOnly()
        {
            Log.Message(decorationL + "Record validation (errors only)" + decorationL);
            IEnumerable<string> messages = GetRecordValidationMessages(true);
            if(!messages.EnumerableNullOrEmpty())
            {
                Log.Message(string.Join("\n", messages));
            }
            else
            {
                Log.Message("No errors.");
            }
        }

        /// <summary>
        /// Validate that all passValues are correctly set, increased/decreased
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetPassValueValidationMessages(bool onlyErrors = false)
        {
            IEnumerable<VorePathDef> pathDefs = DefDatabase<VorePathDef>.AllDefsListForReading;
            foreach(VorePathDef path in pathDefs)
            {
                IEnumerable<string> messages = GetPassValueValidationMessages(path, onlyErrors);
                if(!messages.EnumerableNullOrEmpty())
                {
                    yield return decorationS + path.defName + decorationS;
                    foreach(string message in messages)
                    {
                        yield return message;
                    }
                }
            }
        }

        public static IEnumerable<string> GetPassValueValidationMessages(VorePathDef path, bool onlyErrors = false)
        {
            IEnumerable<string> requiredPassValueNames = path.stages // take all stages
                .SelectMany(stage => stage.passConditions)  // flatten to list of pass conditions
                .Where(cond => cond is StagePassCondition_PassValue)
                .Cast<StagePassCondition_PassValue>()   // cast to passValue condition
                .Select(cond => cond.passValueName);    // extract the name of the pass value
            if(requiredPassValueNames.EnumerableNullOrEmpty())
            {
                if(!onlyErrors) yield return "No passValues";
                yield break;
            }
            if(!onlyErrors) yield return "PassValues: " + string.Join("\n", requiredPassValueNames);

            List<RollAction> startActions = RollUtility.GetAllRollActions(path, (VoreStageDef stage) => stage.onStart); // get all onStart actions
            List<RollAction> onCycleActions = RollUtility.GetAllRollActions(path, (VoreStageDef stage) => stage.onCycle);   // get all onCycle actions
            Func<RollAction, string, bool> actionSetsPassValue = (RollAction action, string passValueName) =>
                action is RollAction_PassValue_Set setter && setter.name == passValueName;
            Func<RollAction, string, bool> actionModifiesPassValue = (RollAction action, string passValueName) =>
                action is RollAction_PassValue_Add adder && adder.name == passValueName
                || action is RollAction_PassValue_Subtract subtractor && subtractor.name == passValueName;

            foreach(string passValueName in requiredPassValueNames)
            {
                // validate SET
                if(startActions.Any(action => actionSetsPassValue(action, passValueName)))
                {
                    if(!onlyErrors) yield return "PassValue " + passValueName + " correctly set";
                }
                else
                {
                    yield return "!!!PassValue " + passValueName + " is not correctly set! A PassValue MUST be set with RollAction_PassValue_Set in an onStart action or roll!!!";
                }
                // validate MODIFY
                if(onCycleActions.Any(action => actionModifiesPassValue(action, passValueName)))
                {
                    if(!onlyErrors) yield return "PassValue " + passValueName + " correctly modified";
                }
                else
                {
                    yield return "!!!PassValue " + passValueName + " is not correctly modified! A PassValue MUST be modified with RollAction_PassValue_Add or RollAction_PassValue_Subtract in an onCycle action or roll!!!";
                }
            }
        }

        public static IEnumerable<string> GetThoughtUsageValidationMessages(bool onlyErrors = false)
        {
            List<ThoughtDef> allRV2Thoughts = DefDatabase<ThoughtDef>.AllDefsListForReading
                .FindAll(def => def.defName.StartsWith("RV2_"));
            List<ThoughtDef> usedRV2Thoughts = new List<ThoughtDef>();

            // used in DefOf (used in various parts of hard-coded methods)

            IEnumerable<ThoughtDef> codeThoughts = ReflectionUtility.AllDefOfEntries<ThoughtDef>(typeof(VoreThoughtDefOf));
            if(!codeThoughts.EnumerableNullOrEmpty())
            {
                usedRV2Thoughts.AddRange(codeThoughts);
            }

            // vore paths

            foreach(VorePathDef vorePath in DefDatabase<VorePathDef>.AllDefsListForReading)
            {
                // post vore memories

                if(vorePath.postVoreMemories != null)
                {
                    List<ThoughtDef> memories = new List<ThoughtDef>() {
                        vorePath.postVoreMemories.predatorPostVoreSuccess,
                        vorePath.postVoreMemories.preyPostVoreSuccess,
                        vorePath.postVoreMemories.predatorPostVoreInterrupted,
                        vorePath.postVoreMemories.preyPostVoreInterrupted
                    };
                    memories = memories.FindAll(m => m != null);
                    if(memories != null)
                    {
                        usedRV2Thoughts.AddRange(memories);
                    }
                }

                // stages 

                foreach(VoreStageDef stage in vorePath.stages)
                {
                    // situational thoughts (and their quirk overrides when set)

                    if(stage.predatorThoughtDef != null)
                    {
                        usedRV2Thoughts.Add(stage.predatorThoughtDef);
                    }
                    ThoughtSelectorDef selector = stage.predatorThoughtSelector;
                    if(selector != null)
                    {
                        usedRV2Thoughts.Add(selector.baseThought);
                        if(selector.overrides != null)
                        {
                            usedRV2Thoughts.AddRange(selector.overrides.Select(or => or.thought));
                        }
                    }
                }
            }

            // quirks 

            foreach(QuirkDef quirk in DefDatabase<QuirkDef>.AllDefsListForReading)
            {
                // post vore memories

                IEnumerable<ThoughtDef> postVoreMemories = quirk.comps?
                    .Where(comp => comp is QuirkComp_PostVoreMemory)
                    .Cast<QuirkComp_PostVoreMemory>()
                    .Select(comp => comp.memory);
                if(postVoreMemories != null)
                {
                    usedRV2Thoughts.AddRange(postVoreMemories);
                }

                // thought overrides 

                IEnumerable<ThoughtDef> thoughtOverrides = quirk.comps?
                    .Where(comp => comp is QuirkComp_ThoughtOverride)
                    .Cast<QuirkComp_ThoughtOverride>()
                    .Select(comp => comp.overrideThought);
                if(thoughtOverrides != null)
                {
                    usedRV2Thoughts.AddRange(thoughtOverrides);
                }
            }

            // thoughts apply themselves via workerClass
            IEnumerable<ThoughtDef> workerClassThoughts = DefDatabase<ThoughtDef>.AllDefsListForReading
                .Where(t => t.workerClass == typeof(ThoughtWorker_Quirk));
            if(!workerClassThoughts.EnumerableNullOrEmpty())
            {
                usedRV2Thoughts.AddRange(workerClassThoughts);
            }

            // thoughts applied by consumable things
            IEnumerable<ThoughtDef> ingestibleThoughts = DefDatabase<ThingDef>.AllDefsListForReading
                .Select(thing => thing.ingestible?.tasteThought)
                .Where(thought => thought != null);
            if(!ingestibleThoughts.EnumerableNullOrEmpty())
            {
                usedRV2Thoughts.AddRange(ingestibleThoughts);
            }

            IEnumerable<PreceptComp> preceptComps = DefDatabase<PreceptDef>.AllDefsListForReading
                .Where(precept => precept.defName.StartsWith("RV2_"))
                .SelectMany(precept => precept.comps);
            foreach(PreceptComp comp in preceptComps)
            {
                if(comp is PreceptComp_SelfTookMemoryThought selfTookMemoryComp)
                    usedRV2Thoughts.Add(selfTookMemoryComp.thought);
                if(comp is PreceptComp_KnowsMemoryThought knowsMemoryComp)
                    usedRV2Thoughts.Add(knowsMemoryComp.thought);
                if(comp is PreceptComp_SituationalThought situationalThoughtComp)
                    usedRV2Thoughts.Add(situationalThoughtComp.thought);
            }

            IEnumerable<ThoughtDef> ritualOutcomeThoughts = DefDatabase<RitualOutcomeEffectDef>.AllDefsListForReading
                .Where(effect => effect.defName.StartsWith("RV2_"))
                .SelectMany(effect => effect.outcomeChances)
                .Select(outcomeChance => outcomeChance.memory);
            usedRV2Thoughts.AddRange(ritualOutcomeThoughts);

            foreach(ThoughtDef thought in allRV2Thoughts)
            {
                if(usedRV2Thoughts.Contains(thought))
                {
                    if(!onlyErrors) yield return "Used: " + thought.defName;
                }
                else
                {
                    yield return "!!!Unused: " + thought.defName + "!!!";
                }
            }
        }

        public static IEnumerable<string> GetHediffUsageValidationMessages(bool onlyErrors = false)
        {
            IEnumerable<HediffDef> allRV2Hediffs = DefDatabase<HediffDef>.AllDefsListForReading
                .Where(hed => hed.defName.StartsWith("RV2_"));
            List<HediffDef> usedHediffs = new List<HediffDef>();

            // hard coded hediffs

            usedHediffs.Add(RV2_Common.DigestionBookmarkHediff);
            usedHediffs.Add(RV2_Common.GrappledHediff);

            // predator hediffs

            foreach(VorePathDef path in DefDatabase<VorePathDef>.AllDefsListForReading)
            {
                foreach(VoreStageDef stage in path.stages)
                {
                    HediffDef predatorHediff = stage.predatorHediffDef;
                    if(predatorHediff != null)
                    {
                        usedHediffs.Add(predatorHediff);
                    }
                }
            }

            // damage def hediffs

            IEnumerable<DamageDef> RV2DamageDefs = DefDatabase<DamageDef>.AllDefsListForReading
                .Where(d => d.defName.StartsWith("RV2_"));
            foreach(DamageDef damage in RV2DamageDefs)
            {
                HediffDef hediffDef = damage.hediff;
                if(hediffDef != null)
                {
                    usedHediffs.Add(hediffDef);
                }
            }

            // enabler hediffs
            IEnumerable<RecipeDef> RV2RecipeDefs = DefDatabase<RecipeDef>.AllDefsListForReading
                .Where(r => r.defName.StartsWith("RV2_"));
            foreach(RecipeDef recipe in RV2RecipeDefs)
            {
                HediffDef hediffDef = recipe.addsHediff;
                if(hediffDef != null)
                {
                    usedHediffs.Add(hediffDef);
                }
            }

            foreach(HediffDef hed in allRV2Hediffs)
            {
                if(usedHediffs.Contains(hed))
                {
                    if(!onlyErrors) yield return "Used: " + hed.defName;
                }
                else
                {
                    yield return "!!!Unused: " + hed.defName + "!!!";
                }
            }

            // added hediffs via rollaction
            IEnumerable<Roll> rolls = DefDatabase<VoreStageDef>.AllDefsListForReading
                .SelectMany(stage => stage.onStart.Rolls);
            foreach(Roll roll in rolls)
            {
                foreach(RollAction action in roll.actionsOnSuccess)
                {
                    if(action is RollAction_AddHediff addHediff)
                        usedHediffs.Add(addHediff.hediff);
                }
            }
        }

        public static IEnumerable<string> GetVoreStageUsageValidationMessages(bool onlyErrors = false)
        {
            List<VoreStageDef> allVoreStages = DefDatabase<VoreStageDef>.AllDefsListForReading;
            List<VoreStageDef> usedVoreStages = new List<VoreStageDef>();

            foreach(VorePathDef path in DefDatabase<VorePathDef>.AllDefsListForReading)
            {
                if(path.stages != null)
                {
                    usedVoreStages.AddRange(path.stages);
                }
            }

            foreach(VoreStageDef stage in allVoreStages)
            {
                if(usedVoreStages.Contains(stage))
                {
                    if(!onlyErrors) yield return "Used: " + stage.defName;
                }
                else
                {
                    yield return "!!!Unused: " + stage.defName + "!!!";
                }
            }
        }

        public static IEnumerable<string> GetRollDuplicateValidationMessages()
        {
            IEnumerable<string> WorkerRollDuplicates(StageWorker worker)
            {
                if(worker == null)
                {
                    yield break;
                }
                IEnumerable<RollPresetDef> duplicateRolls = worker.Rolls?
                    .Where(roll => roll.presetDef != null)
                    .GroupBy(roll => roll.presetDef)
                    .Where(group => group.Count() > 1)
                    .Select(group => group.Key);

                if(!duplicateRolls.EnumerableNullOrEmpty())
                {
                    foreach(RollPresetDef preset in duplicateRolls)
                    {
                        yield return "Duplicate roll by preset: " + preset.defName;
                    }
                }
            }

            foreach(VoreStageDef stage in DefDatabase<VoreStageDef>.AllDefsListForReading)
            {
                List<string> messages = new List<string>();
                messages.AddRange(WorkerRollDuplicates(stage.onStart));
                messages.AddRange(WorkerRollDuplicates(stage.onCycle));
                messages.AddRange(WorkerRollDuplicates(stage.onEnd));
                if(!messages.NullOrEmpty())
                {
                    yield return decorationS + stage.defName + decorationS;
                    foreach(string message in messages)
                    {
                        yield return message;
                    }
                }
            }
        }

        public static IEnumerable<string> GetStageTaleUsageValidationMessages(bool errorsOnly = false)
        {
            foreach(VorePathDef path in DefDatabase<VorePathDef>.AllDefsListForReading)
            {
                IEnumerable<string> pathMessages = GetStageTaleUsageValidationMessages(path, errorsOnly);
                if(!pathMessages.EnumerableNullOrEmpty() || !errorsOnly)
                {
                    yield return decorationS + path.defName + decorationS;
                }
                foreach(string message in pathMessages)
                {
                    yield return message;
                }
            }
        }

        private static IEnumerable<string> GetStageTaleUsageValidationMessages(VorePathDef path, bool errorsOnly = false)
        {
            List<RollAction> allActions = new List<RollAction>();
            allActions.AddRange(RollUtility.GetAllRollActions(path, (VoreStageDef stage) => stage.onStart));
            allActions.AddRange(RollUtility.GetAllRollActions(path, (VoreStageDef stage) => stage.onEnd));

            if(allActions.Any(action => action is RollAction_RecordTale_VoreInitiation))
            {
                if(!errorsOnly) yield return "Init tale preset";
            }
            else
            {
                yield return "!!!Init tale not present!!!";
            }
            if(allActions.Any(action => action is RollAction_RecordTale_GoalFinish))
            {
                if(!errorsOnly) yield return "Goal finish tale preset";
            }
            else
            {
                yield return "!!!Goal finish tale not present!!!";
            }
            if(allActions.Any(action => action is RollAction_RecordTale_VoreExit))
            {
                if(!errorsOnly) yield return "Exit tale preset";
            }
            else
            {
                yield return "!!!Exit tale not present!!!";
            }
        }

        public static IEnumerable<string> GetPathTaleUsageValidationMessages(bool onlyErrors = false)
        {
            foreach(VorePathDef path in DefDatabase<VorePathDef>.AllDefsListForReading)
            {
                IEnumerable<string> messages = GetPathTaleUsageValidationMessages(path, onlyErrors);
                if(!messages.EnumerableNullOrEmpty())
                {
                    yield return decorationS + path.defName + decorationS;
                    foreach(string message in messages)
                    {
                        yield return message;
                    }
                }
            }

            foreach(VoreGoalDef goal in DefDatabase<VoreGoalDef>.AllDefsListForReading)
            {
                IEnumerable<string> messages = GetGoalTaleUsageValidationMessages(goal, onlyErrors);
                if(!messages.EnumerableNullOrEmpty())
                {
                    yield return decorationS + goal.defName + decorationS;
                    foreach(string message in messages)
                    {
                        yield return message;
                    }
                }
            }
        }

        private static IEnumerable<string> GetPathTaleUsageValidationMessages(VorePathDef path, bool onlyErrors = false)
        {
            if(path.initTale != null)
            {
                if(!onlyErrors) yield return "Init tale present";
            }
            else
            {
                yield return "!!!Init tale not present!!!";
            }
            if(path.exitTale != null)
            {
                if(!onlyErrors) yield return "Exit tale present";
            }
            else
            {
                yield return "!!!Exit tale not present!!!";
            }
        }

        private static IEnumerable<string> GetGoalTaleUsageValidationMessages(VoreGoalDef goal, bool onlyErrors = false)
        {
            if(goal.goalFinishTale != null)
            {
                if(!onlyErrors) yield return "Goal finish tale present";
            }
            else
            {
                yield return "!!!Goal finish tale not present!!!";
            }
        }

        private static IEnumerable<string> GetRecordValidationMessages(bool onlyErrors = false)
        {
            IEnumerable<RecordDef> allRV2RecordDefs = DefDatabase<RecordDef>.AllDefsListForReading
                .Where(r => r.defName.StartsWith("RV2_"));
            List<RecordDef> usedRecordDefs = new List<RecordDef>();

            usedRecordDefs.Add(RV2_Common.predatorRecordDef);
            usedRecordDefs.Add(RV2_Common.preyRecordDef);

            foreach(VorePathDef path in DefDatabase<VorePathDef>.AllDefsListForReading)
            {
                IEnumerable<string> pathMessages = GetRecordValidationMessages(path, onlyErrors);
                if(!pathMessages.EnumerableNullOrEmpty())
                {
                    yield return decorationS + path.defName + decorationS;
                    foreach(string message in pathMessages)
                    {
                        yield return message;
                    }
                }
                List<RecordDef> pathRecords = new List<RecordDef>()
                {
                    path.voreType.initiationRecordPredator,
                    path.voreType.initiationRecordPrey,
                    path.voreGoal.goalFinishRecordPredator,
                    path.voreGoal.goalFinishRecordPrey
                };
                pathRecords = pathRecords.FindAll(r => r != null);
                usedRecordDefs.AddRange(pathRecords);
            }

            foreach(RecordDef recordDef in allRV2RecordDefs)
            {
                if(!usedRecordDefs.Contains(recordDef))
                {
                    yield return "Unused record: " + recordDef.defName;
                }
            }
        }

        private static IEnumerable<string> GetRecordValidationMessages(VorePathDef path, bool onlyErrors = false)
        {
            RecordDef predInit = path.voreType.initiationRecordPredator;
            RecordDef preyInit = path.voreType.initiationRecordPrey;
            RecordDef predFin = path.voreGoal.goalFinishRecordPredator;
            RecordDef preyFin = path.voreGoal.goalFinishRecordPrey;
            if(predInit != null)
            {
                if(!onlyErrors) yield return "Pred type init record valid";
            }
            else
            {
                yield return "Pred type init record not set";
            }
            if(preyInit != null)
            {
                if(!onlyErrors) yield return "Prey type init record valid";
            }
            else
            {
                yield return "Prey type init record not set";
            }
            if(predFin != null)
            {
                if(!onlyErrors) yield return "Pred goal finish record valid";
            }
            else
            {
                yield return "Pred goal finish record not set";
            }
            if(preyFin != null)
            {
                if(!onlyErrors) yield return "Prey goal finish record valid";
            }
            else
            {
                yield return "Prey goal finish record not set";
            }
        }
    }
}

/* Validate post-game-load (or maybe as on-demand debug action if too performance intensive?)
- Thoughts & Hediffs: Ensure usage in any vore stage
- VoreStages: ensure usage in any VorePath
- VoreStages: detect duplicate rolls
 - PassConditions validate set, subtract/add
- voreInit, voreExit, voreGoalFinish tales in any stage 
- records
*/