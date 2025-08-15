using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse.AI;

namespace RimVore2
{
    public static class RV2_Common
    {
        public static readonly List<VoreTypeDef> VoreTypes = DefDatabase<VoreTypeDef>.AllDefsListForReading;
        public static readonly List<VoreGoalDef> VoreGoals = DefDatabase<VoreGoalDef>.AllDefsListForReading;
        public static readonly List<VorePathDef> VorePaths = DefDatabase<VorePathDef>.AllDefsListForReading;
        public static readonly List<VorePathDef> VorePathsFeedingPredator = VorePaths
            .Where(p => p.feedsPredator)
            .ToList();

        public static readonly List<RV2DesignationDef> VoreDesignations = DefDatabase<RV2DesignationDef>.AllDefsListForReading;

        public static readonly List<SoundDef> VoreSounds = DefDatabase<SoundDef>.AllDefsListForReading.FindAll(def => def.defName.StartsWith("RV2_"));

        public static readonly ThingDef VoreContainerNone = DefDatabase<ThingDef>.GetNamed("RV2_Container_None");
        public static readonly ThingDef DestroyedImplant = DefDatabase<ThingDef>.GetNamed("RV2_DestroyedImplant");
        public static readonly ToolCapacityDef VoreGrappleToolCapacity = DefDatabase<ToolCapacityDef>.GetNamed("RV2_VoreGrapple");

        public static readonly HediffDef DigestionBookmarkHediff = HediffDef.Named("RV2_DigestionRecovery");
        public static readonly HediffDef GrappledHediff = HediffDef.Named("RV2_Grappled");

        public static readonly HediffDef VoredHediff = HediffDef.Named("RV2_CurrentlyVored");

        public static readonly List<AliasDef> AliasDefs = DefDatabase<AliasDef>.AllDefsListForReading;

        public static readonly MentalStateDef VoreFighting_Attacker = DefDatabase<MentalStateDef>.GetNamed("RV2_VoreFighting_Attacker");
        public static readonly MentalStateDef VoreFighting_Defender = DefDatabase<MentalStateDef>.GetNamed("RV2_VoreFighting_Defender");

        public static readonly List<QuirkPoolDef> SortedQuirkPools = DefDatabase<QuirkPoolDef>.AllDefsListForReading.OrderBy(pool => pool.generationOrder).ToList();

        public static RecordDef predatorRecordDef = DefDatabase<RecordDef>.GetNamed("RV2_Predator");
        public static RecordDef preyRecordDef = DefDatabase<RecordDef>.GetNamed("RV2_Prey");

        public static readonly List<TemporaryQuirkGiver> TemporaryQuirkGivers = DefDatabase<TemporaryQuirkGiver>.AllDefsListForReading;
        public static List<TemporaryQuirkGiver> TemporaryQuirkGiversForKeyword(string keyword) => TemporaryQuirkGivers?.FindAll(giver => giver.criteriaKeywords?.Contains(keyword) == true);

        public static List<ThingDef> AllAlienRaces = DefDatabase<ThingDef>.AllDefsListForReading
            .Where(thing => thing.race != null)
            .ToList();

        public static RoomRoleDef DiningRoomRoleDef = DefDatabase<RoomRoleDef>.GetNamed("DiningRoom");
        public static DutyDef PartyDutyDef = DefDatabase<DutyDef>.GetNamed("Party");

        public static Dictionary<QuirkRarity, Color> QuirkRarityColors = new Dictionary<QuirkRarity, Color>()
        {
            { QuirkRarity.Guaranteed, Color.white },
            { QuirkRarity.Abundant, Color.white },
            { QuirkRarity.Common, Color.white },
            { QuirkRarity.Uncommon, Color.green },
            { QuirkRarity.Rare, Color.magenta },
            { QuirkRarity.VeryRare, Color.red }
        };

        public static Func<NotificationType, string> NotificationPresenter = (NotificationType n) =>
        {
            switch(n)
            {
                case NotificationType.MessageNeutral:
                    return "RV2_Settings_MessageNeutral".Translate();
                case NotificationType.MessageThreatSmall:
                    return "RV2_Settings_MessageThreatSmall".Translate();
                case NotificationType.MessageThreatBig:
                    return "RV2_Settings_MessageThreatBig".Translate();
                case NotificationType.Letter:
                    return "RV2_Settings_Letter".Translate();
                case NotificationType.LetterThreatSmall:
                    return "RV2_Settings_LetterThreatSmall".Translate();
                case NotificationType.LetterThreatBig:
                    return "RV2_Settings_LetterThreatBig".Translate();
                default:
                    return n.ToString();
            }
        };

        public static List<RelationKind> relationKindsToNeverStrip = new List<RelationKind>()
        {
            RelationKind.Trader,
            RelationKind.Visitor
        };

        public static ThingDef ReleaseSpotPawn = ThingDef.Named("RV2_ReleaseSpotPawn");
        public static ThingDef ReleaseSpotProduct = ThingDef.Named("RV2_ReleaseSpotProduct");

        public static WorkGiverDef WorkGiver_ProposeVoreDef = DefDatabase<WorkGiverDef>.GetNamed("RV2_ProposeVore");

        public const float VoreStorageCapacityToBeConsideredInfinite = 50f;

        /// <summary>
        /// TODO: This requires a good refactoring to be less ugly - a lot of this can probably be off-loaded into the QuirkDef itself
        /// </summary>
        private static List<VoreTargetSelectorRequest> voreCombinationsRequiringEnablers;
        public static List<VoreTargetSelectorRequest> VoreCombinationsRequiringEnablers
        {
            get
            {
                if(voreCombinationsRequiringEnablers == null)
                {
                    voreCombinationsRequiringEnablers = new List<VoreTargetSelectorRequest>();
                    RequiredQuirkLookup = new Dictionary<QuirkDef, IEnumerable<VoreTargetSelectorRequest>>();
                    foreach(QuirkDef quirk in DefDatabase<QuirkDef>.AllDefsListForReading)
                    {
                        if(quirk.comps == null) // only pick quirks with comps
                        {
                            continue;
                        }
                        IEnumerable<VoreTargetSelectorRequest> requests = quirk.comps  // get all comps
                            .Where(comp => comp is QuirkComp_VoreEnabler)   // limit to VoreEnabler comps
                            .Cast<QuirkComp_VoreEnabler>()  // cast to special comp
                            .Select(comp => comp.selector); // retrieve the request enabled by this comp
                        if(requests.EnumerableNullOrEmpty())
                        {
                            continue;
                        }
                        voreCombinationsRequiringEnablers.AddRange(requests);
                        RequiredQuirkLookup.Add(quirk, requests);
                    }
                    if(RV2Log.ShouldLog(false, "VoreTargetSelectorRequestCalculations"))
                    {
                        IEnumerable<string> selectors = RequiredQuirkLookup
                            .Select(kvp => $"quirk {kvp.Key.label} provides requests: {string.Join(", ", kvp.Value)}");
                        string joinedSelectors = String.Join("\n", selectors);
                        RV2Log.Message($"Calculated quirk <-> VoreTargetSelectorRequests: {joinedSelectors}", "VoreTargetSelectorRequestCalculations");
                    }
                        
                }
                return voreCombinationsRequiringEnablers;
            }
        }
        public static Dictionary<QuirkDef, IEnumerable<VoreTargetSelectorRequest>> RequiredQuirkLookup;
        public static IEnumerable<QuirkDef> GetRequiredQuirksForRequest(VoreTargetSelectorRequest request)
        {
            if(RequiredQuirkLookup == null)
            {
                return new List<QuirkDef>();
            }
            IEnumerable<QuirkDef> x = RequiredQuirkLookup
                .Where(kvp => kvp.Value.Any(req => req.Matching(request)))
                .Select(kvp => kvp.Key);
            return x;
        }

        #region Ideology related
        [MayRequireIdeology]
        public static PreceptDef IdeoRolePredator = DefDatabase<PreceptDef>.GetNamedSilentFail("RV2_IdeoRole_AlphaPredator");
        [MayRequireIdeology]
        public static PreceptDef IdeoRolePrey = DefDatabase<PreceptDef>.GetNamedSilentFail("RV2_IdeoRole_ChosenPrey");
        [MayRequireIdeology]
        public static PreceptDef IdeoRoleFeeder = DefDatabase<PreceptDef>.GetNamedSilentFail("RV2_IdeoRole_Feeder");
        [MayRequireIdeology]
        public static MemeDef MemePredation = DefDatabase<MemeDef>.GetNamedSilentFail("RV2_Predation");
        [MayRequireIdeology]
        public static MemeDef MemePreyism = DefDatabase<MemeDef>.GetNamedSilentFail("RV2_Preyism");
        [MayRequireIdeology]
        public static DutyDef VoreFeastDuty = DefDatabase<DutyDef>.GetNamedSilentFail("RV2_RitualVoreFeast");

        #endregion

    }

    #region DefOfs
    [DefOf]
    public static class VoreJobDefOf
    {
        static VoreJobDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(VoreJobDefOf));
        }
        public static JobDef RV2_VoreInitAsPredator;
        public static JobDef RV2_VoreInitAsPrey;
        public static JobDef RV2_VoreInitAsFeeder;
        public static JobDef RV2_EjectPreySelf;
        public static JobDef RV2_EjectPreyForce;
        public static JobDef RV2_HuntVorePrey;
        public static JobDef RV2_KidnapVorePrey;
        public static JobDef RV2_ProposeVore;
        public static JobDef RV2_ProposeVore_Feeder;
        public static JobDef RV2_ReleasePrey;
        public static JobDef RV2_VoreGrapple;
    }

    [DefOf]
    public static class VoreGoalDefOf
    {
        static VoreGoalDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(VoreGoalDefOf));
        }

        public static VoreGoalDef Digest;
        public static VoreGoalDef Pleasure;
        public static VoreGoalDef Store;
        public static VoreGoalDef Heal;
    }
    [DefOf]
    public static class VoreTypeDefOf
    {
        static VoreTypeDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(VoreTypeDefOf));
        }

        public static VoreTypeDef Oral;
        public static VoreTypeDef Anal;
        public static VoreTypeDef Cock;
        public static VoreTypeDef Vaginal;
    }

    [DefOf]
    public static class VoreHistoryDefOf
    {
        static VoreHistoryDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(HistoryEventDefOf));
        }

        public static HistoryEventDef RV2_DigestedMember;
    }
    [DefOf]
    public static class RV2DesignationDefOf
    {
        static RV2DesignationDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(RV2DesignationDefOf));
        }

        public static RV2DesignationDef predator;
        public static RV2DesignationDef fatal;
        public static RV2DesignationDef endo;
    }

    [DefOf]
    public static class QuirkDefOf
    {
        static QuirkDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(QuirkDefOf));
        }

        public static QuirkDef Enablers_Core_Type_Oral;
        public static QuirkDef Enablers_Core_Goal_Longendo;
        public static QuirkDef Cheat_InstantSwallow;
        public static QuirkDef Enablers_Core_Goal_Digest;
    }

    [DefOf]
    public static class VoreThoughtDefOf
    {
        static VoreThoughtDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(VoreThoughtDefOf));
        }

        public static ThoughtDef RV2_DeniedVoreProposal_Social;
        public static ThoughtDef RV2_DeniedVoreProposalObsessed_Social;
        public static ThoughtDef RV2_DeniedVoreProposal_Mood;
        public static ThoughtDef RV2_DeniedVoreProposalObsessed_Mood;
        public static ThoughtDef RV2_FatallyVoredMemory_Social;
        public static ThoughtDef RV2_FatallyVoredMemory_Rival_Social;
        public static ThoughtDef RV2_FatallyVoredMemory_Mood;
        public static ThoughtDef RV2_FatallyVoredMemory_Rival_Mood;

        public static ThoughtDef RV2_SuccessfulStruggle_Prey;
        public static ThoughtDef RV2_SuccessfulStruggle_Predator;

        public static ThoughtDef RV2_PreyStruggling;

        public static ThoughtDef RV2_FedMePrey_VeryGood;
        public static ThoughtDef RV2_FedMePrey_Good;
        public static ThoughtDef RV2_FedMePrey_Bad;
        public static ThoughtDef RV2_FedMePrey_VeryBad;

        public static ThoughtDef RV2_SwitchedGoalOnMe_Social;
    }

    [DefOf]
    public static class VoreInteractionDefOf
    {
        static VoreInteractionDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(VoreInteractionDefOf));
        }

        public static InteractionDef RV2_FatalVore;
        public static InteractionDef RV2_EndoVore;
        public static InteractionDef RV2_Proposal;
        public static InteractionDef RV2_SuccessfulStruggle;
        public static InteractionDef RV2_ActiveStruggle;
        public static InteractionDef RV2_FailedStruggle;

        public static InteractionDef RV2_Feeding_Predator;
        public static InteractionDef RV2_Feeding_Prey;

        public static InteractionDef RV2_SwitchedGoal;
    }

    [DefOf]
    public static class VoreRulePackDefOf
    {
        static VoreRulePackDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(VoreRulePackDefOf));
        }

        public static RulePackDef RV2_Proposal_Accepted;
        public static RulePackDef RV2_Proposal_Denied;
        public static RulePackDef RV2_Proposal_Forced;
    }

    [DefOf]
    public static class RV2KeyBindingDefOf
    {
        static RV2KeyBindingDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(RV2KeyBindingDefOf));
        }

        public static KeyBindingDef RV2_QuirkMenuPrevious;
        public static KeyBindingDef RV2_QuirkMenuNext;
    }

    [DefOf]
    public static class VoreStatDefOf
    {
        static VoreStatDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(VoreStatDefOf));
        }

        public static StatDef RV2_SwallowSpeed;
        public static StatDef RV2_GrappleChance;
        public static StatDef RV2_GrappleStrength_General;
        public static StatDef RV2_GrappleStrength_Attacker;
        public static StatDef RV2_GrappleStrength_Defender;
        public static StatDef RV2_ExternalEjectChance;

    }

    [DefOf]
    public static class IdeologyVoreEventDefOf
    {
        static IdeologyVoreEventDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(IdeologyVoreEventDefOf));
        }

        [MayRequireIdeology]
        public static HistoryEventDef RV2_Vore;
        [MayRequireIdeology]
        public static HistoryEventDef RV2_FatalVore;
        [MayRequireIdeology]
        public static HistoryEventDef RV2_EndoVore;
        [MayRequireIdeology]
        public static HistoryEventDef RV2_InstalledVoreEnabler;
        [MayRequireIdeology]
        public static HistoryEventDef RV2_HumanoidPrey;
        [MayRequireIdeology]
        public static HistoryEventDef RV2_AnimalPrey;
        [MayRequireIdeology]
        public static HistoryEventDef RV2_AnimalPrey_Venerated;
        [MayRequireIdeology]
        public static HistoryEventDef RV2_HumanoidPredator;
        [MayRequireIdeology]
        public static HistoryEventDef RV2_AnimalPredator;
        [MayRequireIdeology]
        public static HistoryEventDef RV2_AnimalPredator_Venerated;
        [MayRequireIdeology]
        public static HistoryEventDef RV2_ConsumedVoreProduct;
        [MayRequireIdeology]
        public static HistoryEventDef RV2_PreyInitiatedVore;
        [MayRequireIdeology]
        public static HistoryEventDef RV2_PredatorInitiatedVore;
        [MayRequireIdeology]
        public static HistoryEventDef RV2_FeederInitiatedVore;
        [MayRequireIdeology]
        public static HistoryEventDef RV2_InterruptedVore;
    }

    [DefOf]
    public static class VoreBodyPartDefOf
    {
        static VoreBodyPartDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(VoreBodyPartDefOf));
        }

        public static BodyPartDef Stomach;
        public static BodyPartDef Jaw;
    }
    #endregion
}