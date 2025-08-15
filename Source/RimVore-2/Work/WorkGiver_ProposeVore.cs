using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimVore2
{
    /// <note>
    /// Okay, this thing fucking sucks, base games work works on a "yes or no" requirement for a pawn to do a job, vore proposals are supposed to be a "maybe, maybe not"
    /// </note>
    public class WorkGiver_ProposeVore : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.Touch;
        public override Danger MaxPathDanger(Pawn pawn) => Danger.None;
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {   
            foreach(Pawn otherPawn in pawn.Map.mapPawns.AllPawnsSpawned)
            {
                if(otherPawn == pawn)
                    continue;
                if(!PawnIsValid(otherPawn))
                    continue;

                if(pawn.Position.DistanceToSquared(otherPawn.Position) > RV2Mod.Settings.fineTuning.ProposalRange * RV2Mod.Settings.fineTuning.ProposalRange)
                {
                    continue;
                }
                yield return otherPawn;
            }

            #region old
            /// <note>
            /// I struggled a bit with disabling this, but all the smaller cache-lists are useless in the end due to the new settings
            /// Which allow visitors and wild animals to be targeted, which are not cached in smaller lists
            /// 
            /// So ultimately it will be faster and cleaner to iterate all pawns *always* - sadly.
            /// </note>
            //            foreach(Pawn p in pawn.Map.mapPawns.FreeColonistsAndPrisonersSpawned)
            //            {
            //                if(validator(p))
            //                    yield return p;
            //            }
            //            if(RV2Mod.Settings.features.AnimalsEnabled)
            //            {
            //                foreach(Pawn p in RetrieveAnimals(pawn.Map))
            //                {
            //                    if(validator(p))
            //                        yield return p;
            //                }
            //            }
            //            foreach(Pawn p in pawn.Map.mapPawns.SlavesOfColonySpawned)
            //            {
            //                if(validator(p))
            //                    yield return p;
            //            }


            //private IEnumerable<Pawn> RetrieveAnimals(Map map)
            //{
            //    /// <note>
            //    /// There is a mapPawns.ColonyAnimalsSpawned, but no general list for spawned animals, so we have to iterate all pawns
            //    /// </note>
            //    return map.mapPawns.AllPawnsSpawned
            //        .Where(p =>
            //            p.RaceProps?.Animal == true
            //            && IsAnimalFactionValid(p.Faction)
            //        );

            //    bool IsAnimalFactionValid(Faction faction)
            //    {
            //        if(faction == Faction.OfPlayer)
            //        {
            //            return true;
            //        }
            //        if(RV2Mod.Settings.fineTuning.AllowWildAnimalsAsAutoProposalTargets && faction == null)
            //        {
            //            return true;
            //        }
            //        return false;
            //    }
            //}
            #endregion
        }

        private bool PawnIsValid(Pawn pawn)
        {
            if(pawn.IsColonist)
                return true;
            if(pawn.IsPrisonerOfColony)
                return true;
            if(pawn.IsSlaveOfColony)
                return true;
            Faction faction = pawn.Faction;

            if(RV2Mod.Settings.features.AnimalsEnabled)
            {
                if(faction == Faction.OfPlayer)
                    return true;
                if(RV2Mod.Settings.fineTuning.AllowWildAnimalsAsAutoProposalTargets && faction == null)
                    return true;
            }
            if(RV2Mod.Settings.fineTuning.AllowVisitorsAsAutoProposalTargets)
            {
                List<RelationKind> relations = ColonyRelationUtility.GetRelationKinds(pawn);
                if(relations.Contains(RelationKind.Visitor))
                    return true;
            }

            if(!pawn.CanParticipateInVore(out _))
                return false;
            if(ReservedProposalTargetCache.Contains(pawn))
                return false;

            return true;
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            if(base.ShouldSkip(pawn, forced))
            {
                return true;
            }
            bool doNotProposeDuringWork = RV2Mod.Settings.fineTuning.BlockProposalInitiationDuringWorkTime
                && pawn.timetable.CurrentAssignment == TimeAssignmentDefOf.Work;
            if(doNotProposeDuringWork)
            {
                return true;
            }
            return !HasCooldownPassed(pawn);
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if(pawn == null || t == null || !(t is Pawn))
            {
                return false;
            }
            if(RV2Log.ShouldLog(true, "SpamAutoVore"))
                RV2Log.Message($"HasJobOnThing called for {pawn.LabelShort} -> {t.LabelShort}", false, "SpamAutoVore");
            if(!HasCooldownPassed(pawn))
            {
                if(RV2Log.ShouldLog(true, "SpamAutoVore"))
                    RV2Log.Message($"HasJobOnThing called for {pawn.LabelShort} -> {t.LabelShort}", false, "SpamAutoVore");
                return false;
            }
            if(!pawn.WantsToProposeTo(t, out string reason))
            {
                if(RV2Log.ShouldLog(true, "SpamAutoVore"))
                    RV2Log.Message($"{pawn.LabelShort} -> {t.LabelShort} invalid, reason: {reason}", false, "SpamAutoVore");
                return false;
            }
            if(RV2Log.ShouldLog(true, "SpamAutoVore"))
                RV2Log.Message($"HasJobOnThing() passed for {pawn.LabelShort} -> {t.LabelShort}", false, "SpamAutoVore");
            return true;
        }

        public override bool Prioritized => true;   // base game will use GetPriority to consider targets this way

        public override float GetPriority(Pawn pawn, TargetInfo t)
        {
            Pawn targetPawn = t.Thing as Pawn;
            if(targetPawn == null)
                return 0f;
            if(ModsConfig.IdeologyActive)
            {
                // replaces the ideology replacement logic from the JobGiver, no need to check if ideology is installed, IsIdeoPreX does that
                if(Rand.Chance(RV2Mod.Settings.ideology.AutoVoreInterceptionChance))
                {
                    bool validIdeoPredator = IdeologyUtility.IsIdeoPredator(targetPawn)
                        && targetPawn.CanVore(pawn, out _);
                    bool validIdeoPrey = IdeologyUtility.IsIdeoPrey(targetPawn)
                        && pawn.CanVore(targetPawn, out _);
                    if(validIdeoPredator || validIdeoPrey)
                    {
                        if(RV2Log.ShouldLog(false, "AutoVore"))
                            RV2Log.Message($"Vore ideo role detected on target {targetPawn.LabelShort}, forcing high priority", "AutoVore");
                        return 9999f;
                    }
                }
            }
            return pawn.PreferenceFor(targetPawn);
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if(!(t is Pawn target))
            {
                return null;
            }
            // This job is not relevant when forced, as RMB-initiated vore is handled separately
            if(forced == true)
            {
                return null;
            }
            SetProposalTick(pawn);

            if(WantsToBeFeeder(pawn, target))
            {
                if(RV2Log.ShouldLog(false, "AutoVore"))
                    RV2Log.Message($"Pawn {pawn.LabelShort} successfully rolled for a feeder proposal", "AutoVore");
                Job feederJob = TryMakeFeederJob(pawn, target);
                if(feederJob != null)
                    return feederJob;
                // in case the feeder job resulted in null, fall back to normal direct job
                if(RV2Log.ShouldLog(false, "AutoVore"))
                    RV2Log.Message($"Pawn {pawn.LabelShort} originally wanted to make a feeder proposal, but resulted in a null job, falling back to direct proposal", "AutoVore");
            }

            Job directJob = TryMakeDirectJob(pawn, target);
            if(directJob != null)
                return directJob;

            //// should never happen, but prevents the game from throwing an error because there MUST be a job provided by JobOnThing
            //RV2Log.Message($"Could not create vore proposal job for {pawn.LabelShort} and {target.LabelShort}, falling back to wait job", "AutoVore");
            return JobMaker.MakeJob(JobDefOf.Wait, 1);
        }

        /// <summary>
        /// This method only needs to check if the initiator wants to be a feeder, if they want to be anything else the direct vore logic will handle it
        /// </summary>
        private bool WantsToBeFeeder(Pawn initiator, Pawn firstTarget)
        {
            if(!CanBeFeeder(initiator, firstTarget, out _))
            {
                return false;
            }
            // the pawn with the feeder role must always feed
            if(IdeologyUtility.IsIdeoFeeder(initiator))
            {
                if(RV2Log.ShouldLog(false, "AutoVore"))
                    RV2Log.Message($"{initiator.LabelShort} Feeder proposal forced, ideo role!", "AutoVore");
                return true;
            }
            // feeder vore is possible, so now we must calculate the preference of the initiator for all vore roles and if they want to be a feeder, use that
            List<VoreRole> allRoles = new List<VoreRole>()
            {
                VoreRole.Predator,
                VoreRole.Prey,
                VoreRole.Feeder
            };
            if(!VoreRoleHelper.TryMakeWeightedPreference(initiator, allRoles, out Dictionary<VoreRole, float> weightedRoles))
            {
                // no preferences at all, just fall back to normal direct proposal, which will terminate the WorkGiver
                if(RV2Log.ShouldLog(false, "AutoVore"))
                    RV2Log.Message($"{initiator.LabelShort} No feeder proposal possible, no role preferences", "AutoVore");
                return false;
            }
            VoreRole preferredRole = weightedRoles.RandomElementByWeightWithFallback(kvp => kvp.Value).Key;
            bool prefersFeeder = preferredRole == VoreRole.Feeder;
            if(RV2Log.ShouldLog(false, "AutoVore"))
                RV2Log.Message($"{initiator.LabelShort} Picked feeder role ? {prefersFeeder}", "AutoVore");
            return prefersFeeder;
        }

        public bool CanBeFeeder(Pawn initiator, Pawn firstTarget, out string reason)
        {
            if(!RV2Mod.Settings.features.FeederVoreEnabled)
            {
                reason = "RV2_VoreInvalidReasons_FeedingDisabled".Translate();
                return false;
            }
            // try to determine if there is any valid pawn to act as prey
            Pawn secondTarget = PotentialWorkThingsGlobal(initiator)
                .FirstOrDefault(potentialTargetThing =>
                {
                    if(!(potentialTargetThing is Pawn potentialPawn))
                        return false;
                    if(potentialPawn == firstTarget)    // do not select first target again
                        return false;
                    VoreInteractionRequest request = new VoreInteractionRequest(firstTarget, potentialPawn, isForAuto: true, isForProposal: true);
                    return VoreInteractionManager.Retrieve(request).IsValid;    // the manager will return a valid interaction if there is one, so we can just check the IsValid property
                })
                as Pawn;

            // no potential prey means no feeder vore
            if(secondTarget == null)
            {
                if(RV2Log.ShouldLog(false, "AutoVore"))
                    RV2Log.Message($"{initiator.LabelShort} No feeder proposal possible, no secondary target", "AutoVore");
                reason = "RV2_VoreInvalidReasons_NoPreyAvailable".Translate();
                return false;
            }
            reason = null;
            return true;
        }

        public IEnumerable<Pawn> ValidFeederPrey(Pawn feeder, Pawn predator)
        {
            foreach(Thing thing in PotentialWorkThingsGlobal(feeder))
            {
                if(thing == feeder || thing == predator)
                {
                    continue;
                }
                if(!(thing is Pawn prey))
                {
                    continue;
                }
                VoreInteractionRequest request = new VoreInteractionRequest(predator, prey, VoreRole.Predator, isForAuto: true, isForProposal: true);
                if(!VoreInteractionManager.Retrieve(request).IsValid)
                {
                    continue;
                }
                yield return prey;
            }
        }

        public Job TryMakeFeederJob(Pawn feeder, Pawn predator)
        {
            VoreJob job = VoreJobMaker.MakeJob(VoreJobDefOf.RV2_ProposeVore_Feeder, feeder, predator);
            job.targetQueueB = new List<LocalTargetInfo>();
            bool addedFirstTarget = false;
            IEnumerable<Thing> potentialPrey = ValidFeederPrey(feeder, predator);
            int preyCount = RV2Mod.Settings.fineTuning.FeederVorePreyCount.RandomInRange;
            if(RV2Log.ShouldLog(false, "AutoVore"))
                RV2Log.Message($"{feeder.LabelShort} as feeder is trying to find {preyCount} prey", "AutoVore");
            foreach(Pawn prey in potentialPrey)
            {
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
                return null;
            }
            ReservedProposalTargetCache.Add(job.targetA.Pawn);
            ReservedProposalTargetCache.Add(job.targetB.Pawn);
            if(!job.targetQueueB.NullOrEmpty())
                ReservedProposalTargetCache.AddRange(job.targetQueueB.Select(t => t.Pawn));
            job.count = job.targetQueueB.Count + 1; // targetB is currently not in the queue, but counts!
            job.Proposal = new VoreProposal_Feeder_Predator(feeder, predator);
            return job;
        }

        public Job TryMakeDirectJob(Pawn pawn, Pawn target)
        {
            if(RV2Log.ShouldLog(false, "AutoVore"))
                RV2Log.Message($"{pawn.LabelShort} is considering automatic vore proposal with {target.LabelShort}", "AutoVore");
            // feeders are not meant to do vore directly
            if(IdeologyUtility.IsIdeoFeeder(pawn))
            {
                if(RV2Log.ShouldLog(false, "AutoVore"))
                    RV2Log.Message("Prevented chosen feeder from doing direct vore proposal", "AutoVore");
                return null;
            }
            VoreProposal_TwoWay proposal = MakeDirectProposal(pawn, target);
            if(proposal == null)
            {
                RV2Log.Warning("Valid proposal target, but no valid paths, falling back to a wait job - the proposal cooldown is applied anyways!");
                return JobMaker.MakeJob(JobDefOf.Wait, 1);
            }
            VoreJob job = VoreJobMaker.MakeJob(VoreJobDefOf.RV2_ProposeVore, pawn, target);
            job.targetA = target;
            job.Proposal = proposal;
            job.VorePath = proposal.VorePath;
            return job;
        }

        private VoreProposal_TwoWay MakeDirectProposal(Pawn pawn, Pawn target)
        {
            VoreRole predeterminedRole = VoreRole.Invalid;
            if(IdeologyUtility.IsIdeoPredator(target))
            {
                predeterminedRole = VoreRole.Prey;
            }
            if(IdeologyUtility.IsIdeoPrey(target))
            {
                predeterminedRole = VoreRole.Predator;
            }
            if(RV2Log.ShouldLog(false, "AutoVore"))
                RV2Log.Message($"Creating proposal for {pawn.LabelShort} and {target.LabelShort}, predetermined role: {predeterminedRole}", "AutoVore");
            VoreInteractionRequest request = new VoreInteractionRequest(pawn, target, predeterminedRole, isForAuto: true, isForProposal: true);
            VoreInteraction interaction = VoreInteractionManager.Retrieve(request);
            if(interaction.PreferredPath == null)
            {
                return null;
            }
            VorePathDef pathDef = interaction.PreferredPath;
            if(RV2Log.ShouldLog(true, "AutoVore"))
                RV2Log.Message("Picked path for vore proposal: " + pathDef.defName, false, "AutoVore");
            return new VoreProposal_TwoWay(interaction.Predator, interaction.Prey, pawn, target, interaction.PreferredPath);
        }

        public static void SetProposalTick(Pawn pawn)
        {
            Log.Message($"Setting proposal tick for {pawn.LabelShort}");
            ProposalPawnData pawnData = pawn.PawnData()?.ProposalData;
            if(pawnData == null)
            {
                return;
            }
            pawnData.CurrentProposalCooldown = -1;
            pawnData.LastProposalTick = GenTicks.TicksGame;
        }

        public static bool HasCooldownPassed(Pawn pawn)
        {
            ProposalPawnData pawnData = pawn.PawnData()?.ProposalData;
            if(pawnData == null)   // happens during map load sometimes, just ignore the pawns auto vore desire until their pawndata is generated
                return false;
            if(pawnData.CurrentProposalCooldown < 0)
            {
                pawnData.CurrentProposalCooldown = RV2Mod.Settings.fineTuning.VoreProposalCooldownRange.RandomInRange;
                if(RV2Log.ShouldLog(false, "AutoVore"))
                    RV2Log.Message($"{pawn.LabelShort} Set proposal cooldown: {pawnData.CurrentProposalCooldown}", "AutoVore");
                QuirkManager quirks = pawn.QuirkManager();
                if(quirks != null && quirks.TryGetValueModifier("voreRequestMtb", ModifierOperation.Multiply, out float quirkMultiplier))
                {
                    pawnData.CurrentProposalCooldown = (int)(pawnData.CurrentProposalCooldown * quirkMultiplier);
                    if(RV2Log.ShouldLog(false, "AutoVore"))
                        RV2Log.Message($"{pawn.LabelShort} Modified proposal cooldown with quirk multiplier {quirkMultiplier}, new cooldown: {pawnData.CurrentProposalCooldown}", "AutoVore");
                }
                return false;
            }
            if(pawnData.LastProposalTick < 0)
            {
                if(RV2Log.ShouldLog(true, "AutoVore"))
                    RV2Log.Message($"Initiating last proposal tick to current game tick for {pawn.LabelShort}", true, "AutoVore");
                // if we never had a proposal, wait for the cooldown - if we don't, all pawns immediately start proposing vore
                pawnData.LastProposalTick = GenTicks.TicksGame;
            }
            int ticksSinceLastProposal = GenTicks.TicksGame - pawnData.LastProposalTick;
            if(RV2Log.ShouldLog(true, "SpamAutoVore"))
                RV2Log.Message($"{pawn.LabelShort} since last proposal: {ticksSinceLastProposal} - required: {pawnData.CurrentProposalCooldown}", false, "SpamAutoVore");
            return ticksSinceLastProposal > pawnData.CurrentProposalCooldown;
        }
    }
}
