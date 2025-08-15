using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimVore2
{
    public static class Debug_PawnTools
    {
        [DebugAction("RimVore-2", "Eject (normal)", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void Eject(Pawn p)
        {
            VoreTracker tracker = p.PawnData()?.VoreTracker;
            if(tracker == null || tracker.VoreTrackerRecords.Count <= 0)
            {
                return;
            }
            List<FloatMenuOption> options = tracker.VoreTrackerRecords.ConvertAll(record => new FloatMenuOption("Eject " + record.Prey.Label, () => tracker.Eject(record)));
            Find.WindowStack.Add(new FloatMenu(options));
        }
        [DebugAction("RimVore-2", "Emergency eject", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void EmergencyEject(Pawn p)
        {
            VoreTracker tracker = p.PawnData()?.VoreTracker;
            if(tracker == null || tracker.VoreTrackerRecords.Count <= 0)
            {
                return;
            }
            List<FloatMenuOption> options = tracker.VoreTrackerRecords.ConvertAll(record => new FloatMenuOption("Eject " + record.Prey.Label, () => tracker.EmergencyEject(record)));
            Find.WindowStack.Add(new FloatMenu(options));
        }

        [DebugAction("RimVore-2", "Emergency eject all", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void EmergencyEjectAll(Pawn p)
        {
            VoreTracker tracker = p.PawnData()?.VoreTracker;
            if(tracker == null || tracker.VoreTrackerRecords.Count <= 0)
            {
                return;
            }
            tracker.EmergencyEjectAll();
        }
        [DebugAction("RimVore-2", "Remove cached VoreInteractions", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void RemoveCachedVoreInteractions(Pawn p)
        {
            VoreInteractionManager.Reset(p);
        }
        [DebugAction("RimVore-2", "Log vore preferences", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void CalcPathPreferences(Pawn p)
        {
            Log.Message("Calculated preferences for " + p.LabelShort);
            float predPreference = p.PreferenceFor(VoreRole.Predator);
            float preyPreference = p.PreferenceFor(VoreRole.Prey);
            List<float> otherPawnPreferences = new List<float>() { -4, -2, 1, 2, 4 };
            Log.Message($"pred: {predPreference}, prey: {preyPreference}");
            foreach(VorePathDef path in DefDatabase<VorePathDef>.AllDefsListForReading.InRandomOrder())
            {
                float otherPawnPreference = otherPawnPreferences.RandomElement();
                float preyPreferenceGoalType = p.PreferenceFor(path.voreGoal, VoreRole.Prey) + p.PreferenceFor(path.voreType, VoreRole.Prey);
                float predPreferenceGoalType = p.PreferenceFor(path.voreGoal, VoreRole.Predator) + p.PreferenceFor(path.voreType, VoreRole.Predator);
                Log.Message($"{path.defName}: pred: {predPreference + otherPawnPreference + predPreferenceGoalType} - " +
                    $"prey: {preyPreference + otherPawnPreference + preyPreferenceGoalType}");
            }
        }
        [DebugAction("RimVore-2", "feeder job", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void TakeFeederJob(Pawn p)
        {
            TargetingParameters param = new TargetingParameters()
            {
                canTargetBuildings = false
            };
            Find.Targeter.BeginTargeting(param, (LocalTargetInfo target) =>
            {
                VoreJob job = VoreJobMaker.MakeJob(VoreJobDefOf.RV2_ProposeVore_Feeder, p, target.Pawn);
                job.targetQueueB = new List<LocalTargetInfo>();
                bool addedFirstTarget = false;
                Predicate<Pawn> validator = (Pawn p1) => p1 != p // do not consider self as target
                    && p.CanParticipateInVore(out _)    // only pawns that are of age / allowed in rules
                    && !ReservedProposalTargetCache.Contains(p1);    // only pawns that are not already involved in a proposal
                List<Pawn> potentialPrey = new List<Pawn>();
                foreach(Pawn potentialP in p.Map.mapPawns.FreeColonistsAndPrisonersSpawned)
                {
                    bool valid = validator(potentialP);
                    if(valid)
                    {
                        potentialPrey.Add(potentialP);
                        Log.Message($"{potentialP.LabelShort} is valid");
                    }
                    else
                        Log.Message($"{potentialP.LabelShort} is not valid");
                }
                int preyCount = RV2Mod.Settings.fineTuning.FeederVorePreyCount.RandomInRange;
                if(RV2Log.ShouldLog(false, "AutoVore"))
                    RV2Log.Message($"{p.LabelShort} as feeder is trying to find {preyCount} prey", "AutoVore");
                foreach(Thing preyThing in potentialPrey)
                {
                    if(!(preyThing is Pawn prey))
                    {
                        continue;
                    }
                    VoreInteractionRequest request = new VoreInteractionRequest(target.Pawn, prey, VoreRole.Predator, isForAuto: true);
                    if(!VoreInteractionManager.Retrieve(request).IsValid)
                    {
                        continue;
                    }
                    // first target needs to be set directly as TargetB, otherwise we have an exception on initial jobDriver checking if targetB is valid
                    // the targetB will initially exist and then immediately be moved into the queue upon toil iteration
                    if(!addedFirstTarget)
                    {
                        job.targetB = prey;
                        addedFirstTarget = true;
                    }
                    else
                    {
                        job.targetQueueB.Add(prey);
                    }

                    // queue has been populated with enough entries to satisfy the settings
                    if(job.targetQueueB.Count + 1 >= preyCount) // +1 due to TargetB being directly loaded into the index
                    {
                        break;
                    }
                }
                if(!addedFirstTarget)
                {
                    RV2Log.Warning("Tried to create feeder job, but no prey have been pushed into the job! Aborting feeder vore job!", "AutoVore");
                    return;
                }
                ReservedProposalTargetCache.Add(job.targetA.Pawn);
                ReservedProposalTargetCache.Add(job.targetB.Pawn);
                if(!job.targetQueueB.NullOrEmpty())
                    ReservedProposalTargetCache.AddRange(job.targetQueueB.Select(t => t.Pawn));
                job.count = job.targetQueueB.Count + 1; // targetB is currently not in the queue, but counts!
                job.Proposal = new VoreProposal_Feeder_Predator(p, target.Pawn);
                p.jobs.TryTakeOrderedJob(job);
            });
        }

        [DebugAction("RimVore-2", "Global emergency eject all", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void GlobalEmergencyEjectAll()
        {
            GlobalVoreTrackerUtility.ActiveVoreTrackers.ForEach(tracker => tracker.EmergencyEjectAll());
        }
    }
}
