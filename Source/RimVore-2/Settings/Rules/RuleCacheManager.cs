using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    /// <summary>
    /// Handles the following:
    ///   - cached rule entries for each pawn 
    ///   - the pawn explanations for rules for the currently selected pawn
    ///   - globally usable sorted lists of defs (or enums) of a given type
    /// </summary>
    public static class RuleCacheManager
    {
        public static string ActiveAbilities;
        public static string WornApparel;
        public static string ActiveHediffs;
        public static string ActiveWorkTypes;
        public static string ActivePrecepts;
        public static string ActiveQuirks;
        private static readonly Dictionary<RuleCacheable, VoreRule> cachedPawnRules = new Dictionary<RuleCacheable, VoreRule>();
        private static readonly Dictionary<RuleCacheable, List<RuleTargetStaleTrigger>> cachedStaleTriggers = new Dictionary<RuleCacheable, List<RuleTargetStaleTrigger>>();
        static List<AbilityDef> allAbilityDefs;
        public static List<AbilityDef> AllAbilityDefs
        {
            get
            {
                if(allAbilityDefs == null)
                    allAbilityDefs = DefDatabase<AbilityDef>.AllDefsListForReading
                        .OrderBy(a => a.label)
                        .ToList();
                return allAbilityDefs;
            }
        }
        static List<PawnKindDef> allAnimalPawnKindDefs;
        public static List<PawnKindDef> AllAnimalPawnKindDefs
        {
            get
            {
                if(allAnimalPawnKindDefs == null)
                    allAnimalPawnKindDefs = DefDatabase<PawnKindDef>.AllDefsListForReading
                        .Where(def => def.RaceProps?.Animal == true)
                        .OrderBy(animal => animal.label)
                        .ToList();
                return allAnimalPawnKindDefs;
            }
        }
        static List<XenotypeDef> allXenotypeDefs;
        public static List<XenotypeDef> AllXenotypeDefs
        {
            get
            {
                if(allXenotypeDefs == null)
                    allXenotypeDefs = DefDatabase<XenotypeDef>.AllDefsListForReading
                        .OrderBy(xenotype => xenotype.label)
                        .ToList();
                return allXenotypeDefs;
            }
        }
        static List<ThingDef> allApparelThingDefs;
        public static List<ThingDef> AllApparelThingDefs
        {
            get
            {
                if(allApparelThingDefs == null)
                    allApparelThingDefs = DefDatabase<ThingDef>.AllDefsListForReading
                        .Where(t => t.IsApparel)
                        .OrderBy(t => t.label)
                        .ToList();
                return allApparelThingDefs;
            }
        }
        public static List<RelationKind> AllColonyRelationKinds = Enum.GetValues(typeof(RelationKind))
            .Cast<RelationKind>()
            .Except(new List<RelationKind>() { RelationKind.Invalid })
            .ToList();
        public static List<FoodTypeFlags> ValidFoodTypeFlags = new List<FoodTypeFlags>()
        {
            FoodTypeFlags.VegetableOrFruit,
            FoodTypeFlags.Meat,
            FoodTypeFlags.Corpse,
            FoodTypeFlags.Seed,
            FoodTypeFlags.AnimalProduct,
            FoodTypeFlags.Plant,
            FoodTypeFlags.Tree,
            FoodTypeFlags.Meal,
            FoodTypeFlags.Processed
        };
        public static List<Gender> AllGenders = Enum.GetValues(typeof(Gender))
            .Cast<Gender>()
            .ToList();

        static List<HediffDef> allHediffDefs;
        public static List<HediffDef> AllHediffDefs
        {
            get
            {
                if(allHediffDefs == null)
                    allHediffDefs = DefDatabase<HediffDef>.AllDefsListForReading
                        .OrderBy(t => t.label)
                        .ToList();
                return allHediffDefs;
            }
        }
        static List<PreceptDef> allPreceptDefs;
        public static List<PreceptDef> AllPreceptDefs
        {
            get
            {
                if(allPreceptDefs == null)
                    allPreceptDefs = DefDatabase<PreceptDef>.AllDefsListForReading
                        .OrderBy(t => t.defName)
                        .ToList();
                return allPreceptDefs;
            }
        }
        static List<QuirkDef> allQuirkDefs;
        public static List<QuirkDef> AllQuirkDefs
        {
            get
            {
                if(allQuirkDefs == null)
                    allQuirkDefs = DefDatabase<QuirkDef>.AllDefsListForReading
                        .OrderBy(t => t.label)
                        .ToList();
                return allQuirkDefs;
            }
        }
        static List<WorkTypeDef> allWorkTypeDefs;
        public static List<WorkTypeDef> AllWorkTypeDefs
        {
            get
            {
                if(allWorkTypeDefs == null)
                    allWorkTypeDefs = DefDatabase<WorkTypeDef>.AllDefsListForReading
                        .OrderBy(t => t.label)
                        .ToList();
                return allWorkTypeDefs;
            }
        }
        static Dictionary<FactionDef, List<RoyalTitleDef>> factionTitleDictionary;
        public static Dictionary<FactionDef, List<RoyalTitleDef>> FactionTitleDictionary
        {
            get
            {
                if(factionTitleDictionary == null)
                    factionTitleDictionary = DefDatabase<FactionDef>.AllDefsListForReading
                        .Where(faction => faction.HasRoyalTitles)
                        .ToDictionary(faction => faction, faction => faction.RoyalTitlesAwardableInSeniorityOrderForReading);
                return factionTitleDictionary;
            }
        }

        public static void Notify_NewPawnPicked(Pawn pawn)
        {
            if(pawn?.abilities?.abilities?.NullOrEmpty() != false)
                ActiveAbilities = "";
            else
                ActiveAbilities = string.Join(", ", pawn.abilities.abilities
                    .Select(ab => ab.def.label.CapitalizeFirst())
                    .OrderBy(ab => ab));
            if(pawn?.apparel?.WornApparel?.NullOrEmpty() != false)
                WornApparel = "";
            else
                WornApparel = string.Join(", ", pawn.apparel.WornApparel
                    .Select(ap => ap.def.label.CapitalizeFirst())
                    .OrderBy(ap => ap));
            if(pawn?.health?.hediffSet?.hediffs?.NullOrEmpty() != false)
                ActiveHediffs = "";
            else
                ActiveHediffs = string.Join(", ", pawn.health.hediffSet.hediffs
                    .Select(hed => $"{hed.def.label.CapitalizeFirst()} ({hed.def.defName})")
                    .OrderBy(hed => hed));
            // the game only tracks disabled work types, we need to invert the entirety of all WorkTypeDef
            if(pawn?.workSettings?.EverWork != true)
                ActiveWorkTypes = "";
            else
            {
                List<WorkTypeDef> disabledWorkTypes = pawn.GetDisabledWorkTypes();
                IEnumerable<string> activeWorkTypes = DefDatabase<WorkTypeDef>.AllDefsListForReading
                    .Where(workType => !disabledWorkTypes.Contains(workType))
                    .Select(workType => workType.label.CapitalizeFirst())
                    .OrderBy(workType => workType);
                ActiveWorkTypes = String.Join(", ", activeWorkTypes);
            }
            if(pawn?.QuirkManager(false) == null)
                ActiveQuirks = "";
            else
                ActiveQuirks = string.Join(", ", pawn.QuirkManager(false).ActiveQuirks
                    .Select(quirk => quirk.def.label)
                    .OrderBy(quirk => quirk));
            if(pawn?.Ideo == null)
                ActivePrecepts = "";
            else
                ActivePrecepts = string.Join(", ", pawn.Ideo.PreceptsListForReading
                    .Select(precept => $"{precept.def.defName}")
                    .OrderBy(precept => precept));
        }
        public static void Notify_AllPawnsStale()
        {
            cachedPawnRules.Clear();
            cachedStaleTriggers.Clear();
        }
        public static void Notify_PawnStale(Pawn pawn)
        {
            foreach(RuleCacheable cacheKey in cachedPawnRules.Keys.ToList())
            {
                if(cacheKey.pawn == pawn)
                {
                    cachedPawnRules.Remove(cacheKey);
                    cachedStaleTriggers.Remove(cacheKey);
                }
            }
        }

        private static List<RuleEntry> Rules => RV2Mod.Settings.rules.Rules;

        private static IEnumerable<RuleEntry> ApplicableRuleEntries(Pawn pawn, RuleTargetRole role)
        {
            return Rules
                .Where(entry => 
                    entry.isEnabled
                    && entry.Target.AppliesTo(pawn, role)
                );
        }


        private static VoreRule GetFinalRule(IEnumerable<RuleEntry> applicableRuleEntries, Func<VoreRule, RuleState> stateGetter, out IEnumerable<RuleTargetStaleTrigger> staleTriggers)
        {
            // Log.Message(string.Join(", ", applicableRules.Select(rule => stateGetter(rule).ToString())));
            applicableRuleEntries = applicableRuleEntries
                .Where(rule => stateGetter(rule.Rule) != RuleState.Copy);
            if(applicableRuleEntries.EnumerableNullOrEmpty())
            {
                string errorMessage = "RimVore2: NO FINAL RULE FOUND, this is a critical error, forcing all rules to be reset and using first rule.";
                NotificationUtility.DoNotification(NotificationType.MessageThreatBig, errorMessage);
                RV2Mod.Settings.rules.Reset();
                RuleEntry firstEntry = Rules.First();
                staleTriggers = firstEntry.Target.GetStaleTriggers();
                return firstEntry.Rule;
            }
            RuleEntry finalEntry = applicableRuleEntries
                .Last(entry => stateGetter(entry.Rule) == RuleState.On || stateGetter(entry.Rule) == RuleState.Off);
            staleTriggers = finalEntry.Target.GetStaleTriggers();
            return finalEntry.Rule;
        }

        public static VoreRule GetFinalRule(Pawn pawn, RuleTargetRole role, Func<VoreRule, RuleState> stateGetter)
        {
            RuleCacheable cacheKey = new RuleCacheable(pawn, role, stateGetter);
            if(cachedPawnRules.ContainsKey(cacheKey))
            {
                if(RV2Log.ShouldLog(true, "Settings"))
                    RV2Log.Message($"Getting cached final rule ({cachedPawnRules[cacheKey]}) for pawn {pawn.LabelShort}, role {role} stategetter hash: {stateGetter.GetHashCode()}", true, "Settings");
                bool cacheStale = cachedStaleTriggers[cacheKey]?.Any(trigger => trigger.ShouldRemove()) == true;
                if(!cacheStale)
                {
                    return cachedPawnRules[cacheKey];
                }
                if(RV2Log.ShouldLog(true, "Settings"))
                    RV2Log.Message("Cached final rule is stale, removing from cache and re-caching", true, "Settings");
                // let code resume outside of if condition
            }
            VoreRule finalRule = GetFinalRule(ApplicableRuleEntries(pawn, role), stateGetter, out IEnumerable<RuleTargetStaleTrigger> staleTriggers);
            cachedPawnRules.SetOrAdd(cacheKey, finalRule);
            cachedStaleTriggers.SetOrAdd(cacheKey, staleTriggers.ToList());
            if(RV2Log.ShouldLog(true, "Settings"))
                RV2Log.Message($"Cached new final rule for pawn {pawn.LabelShort}, role {role} stategetter hash: {stateGetter.GetHashCode()}", true, "Settings");
            return finalRule;
        }

        private class RuleCacheable
        {
            public readonly Pawn pawn;
            public readonly RuleTargetRole role;
            public readonly Func<VoreRule, RuleState> stateGetter;
            public RuleCacheable(Pawn pawn, RuleTargetRole role, Func<VoreRule, RuleState> stateGetter)
            {
                this.pawn = pawn;
                this.role = role;
                this.stateGetter = stateGetter;
            }

            public override bool Equals(Object obj)
            {
                //Check for null and compare run-time types.
                if((obj == null) || !this.GetType().Equals(obj.GetType()))
                {
                    return false;
                }
                else
                {
                    RuleCacheable other = (RuleCacheable)obj;
                    return pawn == other.pawn
                    && role == other.role
                    && stateGetter == other.stateGetter;
                }
            }

            public override int GetHashCode()
            {
                return (pawn == null ? 0 : pawn.GetHashCode()) + role.GetHashCode() + stateGetter.GetHashCode();
            }
        }
    }

}
