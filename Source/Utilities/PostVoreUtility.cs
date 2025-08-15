using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RimVore2
{
    public static class PostVoreUtility
    {
        public static void ResolveVore(VoreTrackerRecord record)
        {
            List<string> keywords = record.RecordKeywords();
            record.CurrentVoreStage.End(record);
            ApplyPostVore(record, keywords, true);
            ApplyPostVore(record, keywords, false);
            PostVoreIdeology(record);
            record.IsFinished = true;
        }

        private static void ApplyPostVore(VoreTrackerRecord record, List<string> originalKeywords, bool isForPredator)
        {
            // do not modify referenced list, otherwise predator keywods will show up in prey keywords
            List<string> keywords = new List<string>(originalKeywords);

            Pawn pawn;
            Pawn otherPawn;
            if(isForPredator)
            {
                if(RV2Log.ShouldLog(false, "PostVore"))
                    RV2Log.Message("Applying post vore to predator.", "PostVore");
                keywords.Add("PawnIsPredator");
                pawn = record.Predator;
                otherPawn = record.Prey;
            }
            else
            {
                if(RV2Log.ShouldLog(false, "PostVore"))
                    RV2Log.Message("Applying post vore to prey.", "PostVore");
                keywords.Add("PawnIsPrey");
                pawn = record.Prey;
                otherPawn = record.Predator;
            }
            if(pawn == null)
            {
                Log.Error("PAWN IS NULL");
                return;
            }
            if(pawn.Dead)
            {
                if(RV2Log.ShouldLog(false, "PostVore"))
                    RV2Log.Message("Pawn dead, skipping post vore.", "PostVore");
                // if the pawn died, remove their "ReservedPrey" status if they had one
                //VoreFeedUtility.UnReserve(pawn, false);
                return;
            }
            // call the pawn based post-vore
            if(isForPredator)
            {
                ApplyPredatorPostVore(record);
            }
            else
            {
                ApplyPreyPostVore(record);
            }
            bool pawnHasThoughts = pawn.needs.mood != null;
            // only apply thoughts if the pawn can take them and the vore wasn't interrupted
                // only applying on non-interrupted vore means that prey struggling out won't receive negative thoughts like they should
                // TODO: This may warrant further tweaking to only allow negative thoughts or have thoughts with requirements where an interrupted vore negates the thought? Tricky...
            //if(pawnHasThoughts && !record.IsInterrupted)
            if(pawnHasThoughts)
            {
                QuirkManager pawnQuirks = pawn.QuirkManager();
                if(pawnQuirks != null)
                {
                    IEnumerable<ThoughtDef> memories = pawnQuirks.GetPostVoreMemories(keywords);
                    // Log.Message("Calculated comp memories: " + string.Join(", ", memories.ConvertAll(memory => memory.defName)));
                    if(memories != null)
                    {
                        foreach(ThoughtDef memory in memories)
                        {
                            pawn.needs.mood.thoughts.memories.TryGainMemory(memory, otherPawn);
                        }
                    }
                }
            }
            if(!record.IsInterrupted)
            {
                IncrementRecords(record);
            }
        }

        private static void ApplyPredatorPostVore(VoreTrackerRecord record)
        {
            Pawn predator = record.Predator;
            // in case the vore would have fed the predator, but the digestion was interrupted, empty the predators food
            bool resetPredatorFood = record.IsInterrupted && record.VorePath.def.feedsPredator;
            if(resetPredatorFood)
            {
                predator.SetFoodNeed(0f);
            }
            if(!record.IsInterrupted)
            {
                predator.QuirkManager()?.DoPostVoreActions(VoreRole.Predator, record);
            }
        }

        private static void ApplyPreyPostVore(VoreTrackerRecord record)
        {
            //Log.Message("Applying real prey post vore");
            //Log.Message("goal: " + record.VoreGoal.defName + " - interrupted: " + record.IsInterrupted);
            // apply acid "bookmark" hediff if digestion was interrupted
            if(record.VoreGoal.IsLethal && record.IsInterrupted)
            {
                DigestionUtility.ApplyDigestionBookmark(record);
            }
            if(!record.IsInterrupted)
            {
                record.Prey.QuirkManager()?.DoPostVoreActions(VoreRole.Prey, record);
            }
        }

        private static void IncrementRecords(VoreTrackerRecord record)
        {
            record.VoreGoal.IncrementRecords(record.Predator, record.Prey);
        }
        /// <summary>
        /// The ideology precept triggers are based on a bastardized version of the keyword system, I am not exactly happy
        /// with it, but it's the most extendable approach that also allows easy sub-modding to take place.
        /// </summary>
        /// <param name="record"></param>
        [IdeologyRelated]
        private static void PostVoreIdeology(VoreTrackerRecord record)
        {
            if(!ModsConfig.IdeologyActive)
            {
                return;
            }
            Pawn prey = record.Prey;
            Pawn predator = record.Predator;
            if(prey == null)
            {
                RV2Log.Error("Tried to call PostVoreIdeology with NULL prey!", "PostVore");
                return;
            }
            if(predator == null)
            {
                RV2Log.Error("Tried to call PostVoreIdeology with NULL predator!", "PostVore");
                return;
            }
            IdeologyPawnData preyData = prey.PawnData()?.Ideology;
            IdeologyPawnData predatorData = predator.PawnData()?.Ideology;
            if(preyData != null)
            {
                AmbiguousIdeologyKeywords(preyData, record, false);
                if(predator.IsHumanoid())
                {
                    predatorData.UpdateKeyword("HumanoidPredator");
                }
                else
                {
                    predatorData.UpdateKeyword("NonHumanoidPredator");
                }
            }
            else
            {
                if(RV2Log.ShouldLog(false, "PostVore"))
                    RV2Log.Message("No PawnData for prey found, this is unusual", "PostVore");
            }
            if(predatorData != null)
            {
                AmbiguousIdeologyKeywords(predatorData, record, true);
                if(prey.IsHumanoid())
                {
                    predatorData.UpdateKeyword("HumanoidPrey");
                }
                else
                {
                    predatorData.UpdateKeyword("NonHumanoidPrey");
                }
            }
            else
            {
                if(RV2Log.ShouldLog(false, "PostVore"))
                    RV2Log.Message("No PawnData for predator found, this is unusual", "PostVore");
            }
            // if there was an initiator and they were not pred or prey, we have a feeder
            if(record.Initiator != null && record.Initiator != record.Prey && record.Initiator != record.Predator)
            {
                IdeologyPawnData feederData = record.Initiator.PawnData()?.Ideology;
                if(feederData != null)
                {
                    feederData.UpdateKeyword("Feeder");
                }
                else
                {
                    if(RV2Log.ShouldLog(false, "PostVore"))
                        RV2Log.Message("No PawnData for feeder found, this is unusual", "PostVore");
                }
            }
            IEnumerable<HistoryEvent> events = GetIdeologyEvents(record);
            foreach(HistoryEvent historyEvent in events)
            {
                Find.HistoryEventsManager.RecordEvent(historyEvent, true);
            }
            if(RV2Log.ShouldLog(false, "PostVore"))
                RV2Log.Message($"Post vore events: {String.Join(", ", events.Select(e => e.def.label))}", "PostVore");
        }

        [IdeologyRelated]
        private static void AmbiguousIdeologyKeywords(IdeologyPawnData data, VoreTrackerRecord record, bool forPredator)
        {
            Pawn otherPawn = forPredator ? record.Prey : record.Predator;

            data.UpdateKeyword("Vore");
            if(record.VoreGoal.IsLethal)
            {
                data.UpdateKeyword("FatalVore");
            }
            else
            {
                data.UpdateKeyword("EndoVore");
            }
            if(forPredator)
            {
                data.UpdateKeyword("Predator");
            }
            else
            {
                data.UpdateKeyword("Prey");
            }
            data.UpdateKeyword("VoreType_" + record.VoreType.defName);
        }

        [IdeologyRelated]
        private static IEnumerable<HistoryEvent> GetIdeologyEvents(VoreTrackerRecord record)
        {
            bool colonyRelated = record.Prey.Faction == Faction.OfPlayer || record.Predator.Faction == Faction.OfPlayer;
            if(!colonyRelated)
            {
                yield break;
            }
            if(record.VoreGoal.IsLethal)
            {
                yield return new HistoryEvent(IdeologyVoreEventDefOf.RV2_FatalVore, record.Initiator.Named(HistoryEventArgsNames.Doer));
            }
            else
            {
                yield return new HistoryEvent(IdeologyVoreEventDefOf.RV2_EndoVore, record.Initiator.Named(HistoryEventArgsNames.Doer));
            }
            if(record.Prey.IsHumanoid())
            {
                yield return new HistoryEvent(IdeologyVoreEventDefOf.RV2_HumanoidPrey, record.Initiator.Named(HistoryEventArgsNames.Doer));
            }
            else if(record.Prey.IsAnimal())
            {
                yield return new HistoryEvent(IdeologyVoreEventDefOf.RV2_AnimalPrey, record.Initiator.Named(HistoryEventArgsNames.Doer), record.Prey.def.Named(HistoryEventArgsNames.Subject));
            }
            if(record.Predator.IsHumanoid())
            {
                yield return new HistoryEvent(IdeologyVoreEventDefOf.RV2_HumanoidPredator, record.Initiator.Named(HistoryEventArgsNames.Doer));
            }
            else if(record.Predator.IsAnimal())
            {
                yield return new HistoryEvent(IdeologyVoreEventDefOf.RV2_AnimalPredator, record.Initiator.Named(HistoryEventArgsNames.Doer), record.Predator.def.Named(HistoryEventArgsNames.Subject));
            }
            if(record.Initiator == record.Prey)
            {
                yield return new HistoryEvent(IdeologyVoreEventDefOf.RV2_PreyInitiatedVore, record.Initiator.Named(HistoryEventArgsNames.Doer));
            }
            else if(record.Initiator == record.Predator)
            {
                yield return new HistoryEvent(IdeologyVoreEventDefOf.RV2_PredatorInitiatedVore, record.Initiator.Named(HistoryEventArgsNames.Doer));
            }
            else if(record.Initiator != null)
            {
                yield return new HistoryEvent(IdeologyVoreEventDefOf.RV2_FeederInitiatedVore, record.Initiator.Named(HistoryEventArgsNames.Doer));
            }
            HistoryEventDef typeEvent = record.VoreType.postVoreEvent;
            if(typeEvent != null)
            {
                yield return new HistoryEvent(typeEvent, record.Initiator.Named(HistoryEventArgsNames.Doer));
            }
        }
        [IdeologyRelated]
        public static void RegisterInterruptedVoreEvent(Pawn pawn)
        {
            if(!ModsConfig.IdeologyActive)
            {
                return;
            }
            if(RV2Log.ShouldLog(false, "PostVore"))
                RV2Log.Message("Vore interrupted event", "PostVore");

            Find.HistoryEventsManager.RecordEvent(new HistoryEvent(IdeologyVoreEventDefOf.RV2_InterruptedVore, pawn.Named(HistoryEventArgsNames.Doer)), true);
        }
    }
}