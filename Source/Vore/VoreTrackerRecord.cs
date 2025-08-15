using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;

namespace RimVore2
{
    public class VoreTrackerRecord : IExposable, ILoadReferenceable
    {
        public VoreTracker VoreTracker => Predator.PawnData().VoreTracker;
        public Pawn Predator;
        public Pawn Prey;
        public Pawn ForcedBy;
        public Pawn Initiator;
        public VoreContainer VoreContainer;
        public StruggleManager StruggleManager;
        public VorePath VorePath;
        public VoreTypeDef VoreType => VorePath.VoreType;
        public VoreGoalDef VoreGoal => VorePath.VoreGoal;
        public int VorePathIndex;
        public bool PreyStartedNaked = false;
        public bool IsInterrupted = false;
        public bool IsManuallyPassed = false;
        public bool IsFinished = false;
        public bool IsRitualRelated = false;
        public bool IsResultOfSwitchedPath = false;
        public bool WasExternalEjectAttempted = false;
        public HediffDef CurrentHediffDef => CurrentVoreStage?.def.predatorHediffDef;
        public bool IsForced => ForcedBy != null;
        public bool IsPlayerForced = false;
        public VorePathDef PathToJumpTo = null;
        public bool WillJumpVorePath => PathToJumpTo != null;
        public int loadID;

        public Dictionary<string, float> PassValues = new Dictionary<string, float>();
        // used to calculate progress on values that are not 0.0 -> 1.0 progress
        public Dictionary<string, float> InitialPassValues = new Dictionary<string, float>();

        public int VorePathCount => VorePath.path.Count;
        public BodyPartRecord CurrentBodyPart => BodyPartUtility.GetBodyPartByName(Predator, CurrentVoreStage.def.partName);
        public VoreStage CurrentVoreStage => VorePath?.path[VorePathIndex];
        public VoreStage PreviousVoreStage
        {
            get
            {
                int previousIndex = VorePathIndex - 1;
                // if out of range, return NULL
                if(previousIndex < 0 || previousIndex >= VorePathCount)
                {
                    return null;
                }
                return VorePath.path[previousIndex];
            }
        }
        public VoreStage NextVoreStage
        {
            get
            {
                int nextIndex = VorePathIndex + 1;
                // if out of range, return NULL
                if(nextIndex < 0 || nextIndex >= VorePathCount)
                {
                    return null;
                }
                return VorePath.path[nextIndex];
            }
        }
        public bool HasReachedEntrance => PreviousVoreStage == null;
        public bool HasReachedEnd => NextVoreStage == null;
        public bool CanMoveToNextPart => !HasReachedEnd && CurrentVoreStage.PassConditionsFulfilled(this);
        public bool CanReverse => CurrentVoreStage.def.canReverseDirection;
        // TODO reversal instead of directly exiting
        public bool CanEject
        {
            get
            {
                if(HasReachedEnd)
                {
                    return true;
                }
                if(CanReverse)
                {
                    // check if any predator quirks prevent eject
                    QuirkManager predatorQuirks = Predator.QuirkManager();
                    if(predatorQuirks != null && predatorQuirks.HasSpecialFlag("InescapablePredator"))
                    {
                        return false;
                    }
                    return true;
                }
                return false;
            }
        }

        public bool IsCurrentPartValid => BodyPartUtility.GetBodyPartByName(Predator, CurrentVoreStage.def.partName) != null;
        public bool IsNextPartValid => BodyPartUtility.GetBodyPartByName(Predator, NextVoreStage.def.partName) != null;

        public string LogLabel
        {
            get
            {
                return "Predator: " + Predator?.LabelShort +
                "|Prey: " + Prey?.LabelShort +
                "|Stage: " + CurrentVoreStage?.def.defName;
            }
        }

        public string DisplayLabel
        {
            get
            {
                string name = Prey.Name?.ToStringFull;
                if(name == null) name = Prey.def.label;
                string partGoal = CurrentVoreStage.def.partGoal?.label ?? string.Empty;
                string partName = CurrentVoreStage.def.DisplayPartName;
                int percentageProgress = (int)(CurrentVoreStage.PercentageProgress * 100);
                string label = $"{name} - [{partGoal}] {partName}";
                if(percentageProgress > 0)
                {
                    label += $" {percentageProgress}%";
                }
                return label;
            }
        }
        public Pawn TopPredator
        {
            get
            {
                if(this.Predator == null) return null;
                Pawn TopPred = this.Predator;
                int attemptCount = 0;
                while(TopPred.GetVoreRecord() != null)
                {
                    if(attemptCount++ == 100)//This is overkill if this happens and its not a loop I'm impressed.
                    {
                        RV2Log.Error("Vore Record top pred hit max attempts.");
                        return this.Predator;
                    }
                    TopPred = TopPred.GetVoreRecord().Predator;
                }
                return TopPred;
            }
        }

        public VoreTrackerRecord() { }

        public VoreTrackerRecord(Pawn predator, Pawn prey, bool isForced, Pawn initiator, VorePath vorePath, int index, bool isRitualRelated)
        {
            Predator = predator;
            Prey = prey;
            ForcedBy = isForced ? initiator : null;
            Initiator = initiator;
            VorePath = vorePath;
            VorePathIndex = index;
            IsRitualRelated = isRitualRelated;
            StruggleManager = new StruggleManager(this);
            loadID = RV2Mod.RV2Component.RuntimeUniqueIDsManager.GetNextVoreTrackerRecordID();
        }

        public VoreTrackerRecord(VoreTrackerRecord oldRecord)
        {
            Predator = oldRecord.Predator;
            Prey = oldRecord.Prey;
            ForcedBy = oldRecord.ForcedBy;
            VorePath = new VorePath(oldRecord.VorePath.def);
            VorePathIndex = oldRecord.VorePathIndex;
            StruggleManager = new StruggleManager(this);
            loadID = RV2Mod.RV2Component.RuntimeUniqueIDsManager.GetNextVoreTrackerRecordID();
        }

        public void Initialize()
        {
            VoreTracker.SynchronizeHediffs();
            //SetPredatorHediff();
            if(RV2Log.ShouldLog(false, "OngoingVore"))
                RV2Log.Message($"Added {Prey.LabelShort} to predator {Predator.LabelShort}'s {CurrentBodyPart.Label}. Record: {this}", "OngoingVore");
            CurrentVoreStage.Start(this);
        }

        public void Tick()
        {
            VoreContainer.Tick();
        }

        public void TickRare()
        {
            InterruptOffMapVore();

            CurrentVoreStage.PassedRareTicks++;
            VoreContainer.TickRare();
            // move prey through body if possible
            if(CanMoveToNextPart)
            {
                MovePreyToNextStage();
            }
            CurrentVoreStage.Cycle(this);
            if(RV2Mod.Settings.cheats.PreventStarvingPrey)
            {
                PreventPreyFromStarving(Prey);
            }
            EjectIfDumpingOverdue();
            StruggleManager.Tick();
        }

        private void EjectIfDumpingOverdue()
        {
            if(HasReachedEnd && CurrentVoreStage.PassConditionsFulfilled(this))
            {
                if(RV2Log.ShouldLog())
                    RV2Log.Message($"Prey {Prey.LabelShort} is overdue to be released, forcing release");
                VoreTracker.Eject(this);
            }
        }

        /// <summary>
        /// Through some odd scribing issue, any prey that leaves the map inside of a predator and dies cause a warning and NULL references for all attempts to scribe a reference to that pawn
        /// In order to prevent this behaviour, the vore tracker record ensures that the prey is never off-map in an uncaught way
        /// </summary>
        private void InterruptOffMapVore()
        {
            if(IsOnMap())
            {
                return;
            }
            if(RV2Log.ShouldLog(false, "OffMapVore"))
                RV2Log.Message($"Detected off-map predator {Predator.LabelShort} that is not in a caravan. Interrupting vore and killing/releasing prey {Prey.LabelShort} depending on vore", "OffMapVore");
            VoreTracker.EmergencyEject(this);
            if(VoreGoal.IsLethal)
            {
                if(RV2Log.ShouldLog(false, "OffMapVore"))
                    RV2Log.Message($"Vore was lethal, destroying prey {Prey.LabelShort}", "OffMapVore");
                Prey.Destroy();
            }
            else
            {
                if(Predator.Faction.HostileTo(Faction.OfPlayer))
                {
                    if(RV2Log.ShouldLog(false, "OffMapVore"))
                        RV2Log.Message($"Predator {Predator.LabelShort} was hostile ({Predator.Faction}), kidnapping prey {Prey.LabelShort}", "OffMapVore");
                    Predator.Faction.kidnapped.Kidnap(Prey, Predator);
                }
                else
                {
                    if(RV2Log.ShouldLog(false, "OffMapVore"))
                        RV2Log.Message($"Predator {Predator.LabelShort} was not hostile ({Predator.Faction}), releasing prey {Prey.LabelShort} to world pawns", "OffMapVore");
                    Find.WorldPawns.PassToWorld(Prey);
                }
            }
        }

        public bool IsOnMap()
        {
            Pawn topPredator = TopPredator;
            if(topPredator.Spawned)
            {
                return true;
            }
            if(topPredator.GetCaravan() != null)
            {
                return true;
            }
            if(topPredator.CarriedBy != null)
            {
                return true;
            }
            if(topPredator.MapHeld != null)
            {
                return true;
            }
#if v1_5
            if(topPredator.ParentHolder is ActiveDropPodInfo)
#else
            if(topPredator.ParentHolder is DropPodIncoming)
#endif
            {
                return true;
            }
            return false;
        }

        private void PreventPreyFromStarving(Pawn pawn)
        {
            if(pawn.Dead)
            {
                return;
            }
            Need_Food need = pawn.needs?.food;
            if(need == null)
            {
                return;
            }
            if(need.Starving)
            {
                if(RV2Log.ShouldLog(true, "General"))
                    RV2Log.Message($"Preventing prey {pawn.LabelShort} from starving.", "General");
                need.CurLevel = 0.5f;
            }
        }

        public void MovePreyToNextStage()
        {
            if(TryJumpToOtherPath())
                return;
            // check for validity is done in Tick
            if(!IsCurrentPartValid || !IsNextPartValid)
            {
                RV2Log.Warning("The " + (!IsCurrentPartValid ? "current" : "next") + " body part for the prey is invalid, calling emergency eject.", "OngoingVore");
                VoreTracker.EmergencyEject(this);
                return;
            }
            CurrentVoreStage.End(this);
            VorePathIndex++;
            if(RV2Log.ShouldLog(false, "OngoingVore"))
                RV2Log.Message($"Moved prey {Prey.LabelShort} from {PreviousVoreStage.def.partName}|{PreviousVoreStage.def.predatorHediffDef.defName} " +
                    $"to {CurrentVoreStage.def.partName}|{CurrentVoreStage.def.predatorHediffDef.defName} " +
                    $"stage defName: {CurrentVoreStage.def.defName}", "OngoingVore");
            VoreTracker.SynchronizeHediffs();
            CurrentVoreStage.Start(this);
        }
        /// <summary>
        /// Pawns with the appropriate quirk may decide to change the active vore goal and initaite a new vore record with their desired goal
        /// </summary>
        /// <returns>True if jump was executed, False if no jump was executed</returns>
        private bool TryJumpToOtherPath()
        {
            if(!WillJumpVorePath)
                return false;
            if(!IsNextPartValid)
                return false;
            string nextJumpKey = NextVoreStage.def.jumpKey;
            if(nextJumpKey == null)
            {
                if(RV2Log.ShouldLog(true, "VoreJump"))
                    RV2Log.Message($"{DisplayLabel} - Could not jump stage, next stage has no jumpable key", false, "VoreJump");
                return false;
            }
            VoreStageDef targetStage = PathToJumpTo.stages.FirstOrFallback(stage => stage.jumpKey == nextJumpKey);
            if(targetStage == null)
            {
                if(RV2Log.ShouldLog(true, "VoreJump"))
                    RV2Log.Message($"{DisplayLabel} - Could not jump stage, no jump keys in the desired path match with the next stages key: {nextJumpKey}", false, "VoreJump");
                return false;
            }
            if(RV2Log.ShouldLog(false, "VoreJump"))
                RV2Log.Message($"{DisplayLabel} - Doing vore jump, key: {nextJumpKey} path: {PathToJumpTo.defName}", "VoreJump");
            JumpToOtherPath(targetStage);
            return true;
        }
        private void JumpToOtherPath(VoreStageDef stage)
        {
            DoInteractions();
            DoPreyImpact();
            VoreJump jump = new VoreJump(PathToJumpTo, stage);
            jump.Jump(this, true);
            PathToJumpTo = null;

            void DoInteractions()
            {
                List<RulePackDef> rulePacks = new List<RulePackDef>();
                List<RulePackDef> typeRules = PathToJumpTo.voreType?.relatedRulePacks;
                List<RulePackDef> goalRules = PathToJumpTo.voreGoal?.relatedRulePacks;
                if(typeRules != null)
                    rulePacks.AddRange(typeRules);
                if(goalRules != null)
                    rulePacks.AddRange(goalRules);
                PlayLogEntry_Interaction interaction = new PlayLogEntry_Interaction(VoreInteractionDefOf.RV2_SwitchedGoal, Predator, Prey, rulePacks);
                Find.PlayLog.Add(interaction);
            }

            void DoPreyImpact()
            {
                MemoryThoughtHandler memories = Prey.needs?.mood?.thoughts?.memories;
                if(memories == null)
                {
                    Log.Error("Memories of the prey were null");
                    return;
                }

                memories.TryGainMemory(VoreThoughtDefOf.RV2_SwitchedGoalOnMe_Social, Predator);
            }
        }

        public void SetPassValue(string passValueName, float setValue)
        {
            PassValues.SetOrAdd(passValueName, setValue);
            InitialPassValues.SetOrAdd(passValueName, setValue);
        }

        public void ModifyPassValue(string passValueName, float modifyValue, float minValue = float.MinValue, float maxValue = float.MaxValue)
        {
            if(!PassValues.ContainsKey(passValueName))
            {
                throw new Exception("Trying to modify key that has not been set before!");
            }
            if(modifyValue < minValue)
            {
                if(RV2Log.ShouldLog(true, "OngoingVore"))
                    RV2Log.Message($"Prevented value to be lower than minValue, forced {modifyValue} to become {minValue}", false, "OngoingVore");
                modifyValue = minValue;
            }
            if(modifyValue > maxValue)
            {
                if(RV2Log.ShouldLog(true, "OngoingVore"))
                    RV2Log.Message($"Prevented value to be higher than maxValue, forced {modifyValue} to become {maxValue}", false, "OngoingVore");
                modifyValue = maxValue;
            }

            PassValues[passValueName] = modifyValue;
        }

        public Pawn GetPawnByRole(VoreRole role)
        {
            switch(role)
            {
                case VoreRole.Predator:
                    return Predator;
                case VoreRole.Prey:
                    return Prey;
                case VoreRole.Feeder:
                    return ForcedBy;
                default:
                    return null;
            }
        }
        public VoreRole GetRoleForPawn(Pawn pawn)
        {
            if(pawn == Predator)
                return VoreRole.Predator;
            if(pawn == Prey)
                return VoreRole.Prey;
            if(pawn == Initiator)
                return VoreRole.Feeder;
            return VoreRole.Invalid;
        }

        public string GetPreyName()
        {
            string preyName = Prey?.LabelShort;
            if(preyName == null)  // should never happen, if it does, make sure we get a unique label
            {
                Log.Error("Prey is null, something is fatally causing issues and set a currently vored pawn to NULL! Trying to remove VoreTrackerRecord");
                VoreTracker?.EmergencyEject(this);
                return null;
            }
            return preyName;
        }

        public override string ToString()
        {
            if(Scribe.mode != LoadSaveMode.Inactive)
            {
                return base.ToString();
            }
            return "Predator: " + Predator?.LabelShort +
                "|Prey: " + Prey?.LabelShort +
                "|VoreType: " + VoreType?.defName +
                "|VoreGoal: " + VoreGoal?.defName +
                "|CurrentBodyPart: " + CurrentBodyPart?.LabelShort +
                "|ContainedThings: " + VoreContainer?.ToString() +
                //"|VorePathHediffs: " + string.Join(", ", VorePath.path.ConvertAll(e => e.def.predatorHediffDef.ToString())) +
                "|VorePathIndex: " + VorePathIndex;
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref Predator, "Predator", true);
            Scribe_References.Look(ref Prey, "Prey", true);
            Scribe_References.Look(ref ForcedBy, "ForcedBy", true);
            Scribe_References.Look(ref Initiator, "Initiator", true);
            Scribe_Deep.Look(ref VoreContainer, "ContainedThings", new object[0]);
            Scribe_Deep.Look(ref VorePath, "VorePath", new object[0]);
            Scribe_Values.Look(ref VorePathIndex, "VorePathIndex");
            Scribe_Values.Look(ref PreyStartedNaked, "PreyStartedNaked", PreyStartedNaked);
            Scribe_Values.Look(ref IsInterrupted, "IsInterrupted", IsInterrupted);
            Scribe_Values.Look(ref IsManuallyPassed, "IsManuallyPassed", IsManuallyPassed);
            Scribe_Values.Look(ref IsFinished, "IsFinished", IsFinished);
            Scribe_Values.Look(ref IsRitualRelated, "IsRitualRelated", IsRitualRelated);
            Scribe_Values.Look(ref IsResultOfSwitchedPath, "IsResultOfSwitchedPath", IsResultOfSwitchedPath);
            Scribe_Values.Look(ref WasExternalEjectAttempted, "WasExternalEjectAttempted");
            ScribeUtilities.ScribeVariableDictionary(ref PassValues, "PassValues");
            ScribeUtilities.ScribeVariableDictionary(ref InitialPassValues, "InitialPassValues");
            Scribe_Deep.Look(ref StruggleManager, "StruggleManager", new object[0]);
            Scribe_Values.Look(ref loadID, "loadID");
            Scribe_Values.Look(ref IsPlayerForced, "IsPlayerForced");
            Scribe_Defs.Look(ref PathToJumpTo, "GoalToSwitchTo");

            #region Field Insurance
            if(Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if(StruggleManager == null)
                    StruggleManager = new StruggleManager(this);
            }
            #endregion
        }

        string ILoadReferenceable.GetUniqueLoadID()
        {
            return "VoreTrackerRecord_" + loadID;
        }
    }
}
