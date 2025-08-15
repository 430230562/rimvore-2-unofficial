using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace RimVore2
{
    public class RollAction_Reform : RollAction
    {
        public PawnKindDef resultingPawnKind;
        ThingDef resultingRace;
        bool usePredatorRace = true;
        float eachPredatorTraitCopyChance = 0.5f;
        float eachPreyTraitCopyChance = 0.5f;
        bool changeFactionToPlayer = false;
        bool changeFactionToPredator = true;
        Gender? fixedGender = null;

        Pawn Predator => record.Predator;
        ThingDef PredatorRace => Predator.def;

        Pawn ResultSourcePawn => usePredatorRace ? Predator : TargetPawn;

        PawnKindDef ResultPawnKindDef
        {
            get
            {
                if(resultingPawnKind != null)
                {
                    return resultingPawnKind;
                }
                return ResultSourcePawn.kindDef;
            }
        }
        ThingDef ResultRaceDef
        {
            get
            {
                if(resultingRace != null)
                {
                    return resultingRace;
                }
                return ResultSourcePawn.def;
            }
        }

        Gender ResultGender => fixedGender != null ? fixedGender.Value : TargetPawn.gender;

        public RollAction_Reform()
        {
            target = VoreRole.Prey;
        }

        public override bool TryAction(VoreTrackerRecord record, float rollStrength)
        {
            base.TryAction(record, rollStrength);
            if(TargetPawn.Dead)
            {
                NotificationUtility.DoNotification(NotificationType.MessageNeutral, "RV2_Message_ReformationFailed_PawnDead".Translate());
                return false;
            }
            VoreTracker tracker = record.Predator.PawnData().VoreTracker;
            float currentProgress = record.CurrentVoreStage.PercentageProgress;
            if(currentProgress <= 0.95)
            {
                if(RV2Log.ShouldLog(false, "OngoingVore"))
                    RV2Log.Message($"Not reforming pawn because progress is < 95% ({currentProgress})", "OngoingVore");
                return false;
            }
            Pawn newPawn = MakeReformedPawn();
            if(newPawn == null)
            {
                return false;
            }
            VoreTrackerRecord newRecord = tracker.SplitOffNewVore(record, newPawn, null, record.VorePathIndex + 1);
            tracker.UntrackVore(record);
            FakeNewPawnRecords(newRecord);
            StripAndDestroyOriginalPawn(newRecord);
            //GenSpawn.Spawn(newPawn, Predator.Position, Predator.Map);
            return true;
        }

        private void StripAndDestroyOriginalPawn(VoreTrackerRecord record)
        {
            record.VoreContainer.TryAddOrTransfer(TargetPawn.apparel?.WornApparel);
            record.VoreContainer.TryAddOrTransfer(TargetPawn.equipment?.AllEquipmentListForReading);
            TargetPawn.Destroy();
        }

        private Pawn MakeReformedPawn()
        {
            if(ResultPawnKindDef == null)
            {
                RV2Log.Error("Can not reform prey " + TargetPawn?.Label + ", ResultPawnKind is NULL");
                return null;
            }
            string originalPawnKindRaceDefName = ResultPawnKindDef.race.defName; // remember the original race to re-set it after generation
            try
            {
                if(ResultRaceDef == null)
                {
                    RV2Log.Error("Can not reform prey " + TargetPawn?.Label + ", ResultRaceDef is NULL");
                    return null;
                }
                ResultPawnKindDef.race = ResultRaceDef;    // temporarily overwrites the global PawnKindDef's race! Will be forced to be reset after generation
                List<Trait> newTraits = MakeNewTraits();
                Faction newFaction;
                if(changeFactionToPlayer)
                {
                    newFaction = Faction.OfPlayer;
                }
                else if(changeFactionToPredator)
                {
                    newFaction = Predator.Faction;
                }
                else
                {
                    newFaction = TargetPawn.Faction;
                }
                string fixedLastName = null;
                if(Predator.Name is NameTriple tripleName)
                {
                    fixedLastName = tripleName.Last;
                }
                PawnGenerationRequest request = new PawnGenerationRequest(
                    kind: ResultPawnKindDef,
                    fixedGender: ResultGender,
                    faction: newFaction,
                    forceGenerateNewPawn: true,
                    allowDowned: true,
                    canGeneratePawnRelations: false,
                    colonistRelationChanceFactor: 0,
                    allowFood: false,
                    allowAddictions: false,
                    relationWithExtraPawnChanceFactor: 0,
                    forcedTraits: null, // can't be used to pass predator / old pawn traits, due to forcing all degree to be set to 0 (why, tynan)
                                        //fixedBiologicalAge: 0.01f,  // will be overwritten post-generation
                                        //fixedChronologicalAge: TargetPawn.ageTracker?.AgeChronologicalYearsFloat,
                    fixedLastName: fixedLastName
                );
                // for non-humanoids we set the new pawn to be a baby, or if the user is fine with generating age 0 humanoids, we use that
                bool generateNewborn = !ResultSourcePawn.IsHumanoid()
                    || (ResultSourcePawn.IsHumanoid() && RV2Mod.Settings.fineTuning.ReformHumanoidsAsNewborn);

                if(generateNewborn)
                {
                    request.AllowedDevelopmentalStages = DevelopmentalStage.Newborn;
                }
                else
                {
                    request.AllowedDevelopmentalStages = DevelopmentalStage.Adult;
                }
                // only non-babies can have an ideology
                if(!generateNewborn)
                {
                    request.FixedIdeo = Predator.Ideo ?? base.TargetPawn.ideo?.Ideo; // use predators ideo if available, otherwise own ideo or null ideo
                }

                Pawn generatedPawn = PawnGenerator.GeneratePawn(request);
                // if the generated pawn is not a newborn, we need to calculate a biological age for them
                if(!generateNewborn)
                {
                    SetReformedBiologicalAge(generatedPawn);
                }

                // the chronological age we always try to set to the previous pawns age
                SetReformedChronologicalAge(generatedPawn);
                // get rid of any apparel the pawn generated with
                generatedPawn.apparel?.DestroyAll();
                ForceReformedName(generatedPawn);
                // base game is retarded. genuinely fucking stupid, the following line is what happens when you pass a forcedTraits via request
                // pawn.story.traits.GainTrait(new Trait(traitDef, 0, true));
                // it just forces degree 0 - which traits with ACTUAL degrees do not have, 
                // so we skip the INTENDED mechanic of forcing traits and do it after the pawn is generated
                // this solution is dogshit, because at this point the traits don't have an impact on other aspects of pawn generation
                RV2PawnUtility.SetTraits(generatedPawn, newTraits);
                ForceParent(generatedPawn);
                DestroyAllBelongings(generatedPawn);

                ForceXenotype(generatedPawn);

                return generatedPawn;
            }
            finally
            {
                // this appears to be unnecessary, but not doing it would leave the pawnKind with an incorrect ThingDef reference as race
                ResultPawnKindDef.race = ThingDef.Named(originalPawnKindRaceDefName);
            }
        }

        /// <summary>
        /// artificially increase the new pawns records to make the current vore count
        /// </summary>
        private void FakeNewPawnRecords(VoreTrackerRecord record)
        {
            record.Prey.records?.Increment(RV2_Common.preyRecordDef);
            record.VoreType.IncrementRecords(record.Predator, record.Prey);
        }

        private void ForceParent(Pawn newPawn)
        {
            if(Predator.gender == Gender.Male)
            {
                newPawn.SetFather(Predator);
            }
            else if(Predator.gender == Gender.Female)
            {
                newPawn.SetMother(Predator);
            }
        }

        private void ForceXenotype(Pawn newPawn)
        {
            XenotypeDef predatorXenotype = Predator.genes?.Xenotype;
            if(predatorXenotype != null)
            {
                newPawn.genes?.SetXenotype(predatorXenotype);
            }
        }

        private void DestroyAllBelongings(Pawn pawn)
        {
            pawn.equipment?.DestroyAllEquipment();
            pawn.apparel?.DestroyAll();
            pawn.inventory?.DestroyAll();
        }

        private void SetReformedBiologicalAge(Pawn pawn)
        {
            // animals, mechs, all the other stuff is set to age 0, they should have just been tagged as newborn
            if(!pawn.IsHumanoid())
            {
                pawn.ageTracker.AgeBiologicalTicks = 0;
                return;
            }
            // but because humans are weird, we do a whole lot of calculations for a good target age
            //float oldAge = TargetPawn.ageTracker.AgeBiologicalYearsFloat;
            // first get the minimum age required for this pawn to participate in vore
            float targetAge = RV2Mod.Settings.rules.GetValidAge(pawn);
            //// if our target age is above the previous age, reduce it to the old age
            //if(targetAge > oldAge)
            //{
            //    targetAge = oldAge;
            //}
            // if our target age is above chronological age, reduce to chronological age
            float chronoAge = pawn.ageTracker.AgeChronologicalYearsFloat;
            if(targetAge > chronoAge)
            {
                targetAge = chronoAge;
            }
            long newAge = Math.Max(0, (long)(targetAge * 3600000L));
            pawn.ageTracker.AgeBiologicalTicks = newAge;
        }

        private void SetReformedChronologicalAge(Pawn pawn)
        {
            // the new generated age MIGHT be larger than the old pawns chronological age, but chrono age must always be higher than bio age
            float newAge = pawn.ageTracker.AgeBiologicalYearsFloat;
            float chronoAge = TargetPawn.ageTracker.AgeChronologicalYears;
            if(newAge > chronoAge)
            {
                chronoAge = newAge;
            }
            pawn.ageTracker.AgeChronologicalTicks = (long)(chronoAge * 3600000L);
        }

        private void ForceReformedName(Pawn pawn)
        {
            // Animals have no real name, so no name to take
            if(TargetPawn.Name == null)
            {
                return;
            }
            // new name is triple name
            if(pawn.Name is NameTriple newName)
            {
                // both pawns have triple names, keep first and nick name, adopt last name
                if(TargetPawn.Name is NameTriple oldName)
                {
                    pawn.Name = new NameTriple(
                        oldName.First,
                        oldName.Nick,
                        newName.Last
                    );
                }
                // only new name is triple, old is single, so set first name to old single name
                else
                {
                    NameSingle oldSingleName = (NameSingle)TargetPawn.Name;
                    pawn.Name = new NameTriple(
                        oldSingleName.Name,
                        newName.Nick,
                        newName.Last
                    );
                }
            }
            // new name is single name, we always want to keep the old name
            else
            {
                pawn.Name = TargetPawn.Name;
            }
        }

        public override IEnumerable<string> ConfigErrors()
        {
            if(!usePredatorRace && resultingRace == null)
            {
                yield return "when using \"usePredatorRace\" = false the field \"resultingRace\" must be set!";
            }
        }

        protected virtual List<Trait> MakeNewTraits(PawnGenerationRequest request = default(PawnGenerationRequest))
        {
            List<Trait> newTraits = new List<Trait>();
            newTraits.AddRange(GetTraitsFromPawn(TargetPawn, eachPreyTraitCopyChance));
            newTraits.AddRange(GetTraitsFromPawn(Predator, eachPredatorTraitCopyChance));
            if(RV2Log.ShouldLog(false, "TraitGeneration"))
                RV2Log.Message($"total new traits: {string.Join(", ", newTraits.ConvertAll(t => t.Label))}", "TraitGeneration");

            int maxTraitsAllowed = RV2Mod.Settings.cheats.ReformedPawnTraitCount;
            if(newTraits.Count > maxTraitsAllowed)
            {
                if(RV2Log.ShouldLog(false, "TraitGeneration"))
                    RV2Log.Message($"too many new traits, current count: {newTraits.Count} allowed: {maxTraitsAllowed}", "TraitGeneration");
                newTraits = newTraits.GetRange(0, maxTraitsAllowed - 1);
            }
            return newTraits;
        }

        protected virtual List<Trait> GetTraitsFromPawn(Pawn pawn, float chance)
        {
            List<Trait> traits;
            traits = pawn.story?.traits?.allTraits?
                .FindAll(t => Rand.Chance(chance));
            if(traits.NullOrEmpty())
            {
                return new List<Trait>();
            }
            if(RV2Log.ShouldLog(false, "TraitGeneration"))
                RV2Log.Message($"Pawn {pawn.Label} provides traits: {string.Join(", ", traits.ConvertAll(t => t.Label))}", "TraitGeneration");
            return traits;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref resultingRace, "resultingRace");
            Scribe_Values.Look(ref usePredatorRace, "usePredatorRace");
            Scribe_Values.Look(ref eachPredatorTraitCopyChance, "eachPredatorTraitCopyChance");
            Scribe_Values.Look(ref eachPreyTraitCopyChance, "eachPreyTraitCopyChance");
            Scribe_Values.Look(ref changeFactionToPlayer, "changeFactionToPlayer");
        }
    }
}
