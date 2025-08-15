using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace RimVore2
{
    public class QuirkManager : IExposable, ICloneable
    {
        private Pawn pawn;

        private QuirkLayer_Persistent persistentQuirkLayer;
        private QuirkLayer_Persistent PersistentQuirkLayer
        {
            get
            {
                if(persistentQuirkLayer == null)
                {
                    persistentQuirkLayer = new QuirkLayer_Persistent(pawn);
                }
                return persistentQuirkLayer;
            }
        }
        private QuirkLayer_Ideology ideologyQuirkLayer;
        private QuirkLayer_Ideology IdeologyQuirkLayer
        {
            get
            {
                if(ideologyQuirkLayer == null)
                {
                    ideologyQuirkLayer = new QuirkLayer_Ideology(pawn);
                }
                return ideologyQuirkLayer;
            }
        }

        public QuirkManager(Pawn pawn)
        {
            this.pawn = pawn;
        }

        public QuirkManager()
        {
            if(pawn == null)
            {
                RV2Log.Warning("Tried to construct quirk manager without pawn! This will cause issues!", "Quirks");
                return;
            }
        }

        public bool ActiveQuirksStale => PersistentQuirkLayer.IsStale || IdeologyQuirkLayer.IsStale;
        private List<Quirk> activeQuirks;
        public List<Quirk> ActiveQuirks
        {
            get
            {
                if(activeQuirks == null || ActiveQuirksStale)
                {
                    if(RV2Log.ShouldLog(true, "Quirks"))
                        RV2Log.Message($"Active quirks for {pawn?.LabelShort} are stale, refreshing", false, "Quirks");
                    if(activeQuirks == null)
                    {
                        activeQuirks = new List<Quirk>();
                    }
                    else
                    {
                        activeQuirks.Clear();
                    }
                    activeQuirks.AddRange(PersistentQuirkLayer.Quirks());
                    ResolveConflicts();
                    IdeologyQuirkLayer.Merge(activeQuirks);
                    IdeologyQuirkLayer.IsStale = false;
                    if(!activeQuirks.NullOrEmpty())
                    {
                        if(RV2Log.ShouldLog(true, "Quirks"))
                            RV2Log.Message($"Active quirks: {string.Join(", ", activeQuirks.ConvertAll(q => q.def.defName))}", false, "Quirks");
                    }
                    else
                    {
                        if(RV2Log.ShouldLog(true, "Quirks"))
                            RV2Log.Message("Active quirks are empty!", false, "Quirks");
                    }

                    PersistentQuirkLayer.IsStale = false;
                    areGroupedQuirksStale = true;
                    areQuirkCompsStale = true;
                }
                return activeQuirks;
            }
        }

        private Dictionary<Type, List<QuirkComp>> quirksByCompType;
        private bool areQuirkCompsStale = true;
        public Dictionary<Type, List<QuirkComp>> QuirksByCompType
        {
            get
            {
                List<Quirk> activeQuirks = ActiveQuirks;

                if(areQuirkCompsStale || quirksByCompType == null)
                {
                    if(RV2Log.ShouldLog(true, "Quirks"))
                        RV2Log.Message("Quirk comps are stale, regrouping", false, "Quirks");
                    quirksByCompType = activeQuirks
                        .SelectMany(quirk => quirk.def.comps)
                        .GroupBy(comp => comp.GetType())
                        .ToDictionary(
                            group => group.Key,
                            group => group.AsEnumerable().ToList()
                        );
                    areQuirkCompsStale = false;
                    areValueModifierCompsStale = true;
                }
                return quirksByCompType;
            }
        }

        private Dictionary<string, List<QuirkComp_ValueModifier>> valueModifierComps;
        private bool areValueModifierCompsStale = true;
        public Dictionary<string, List<QuirkComp_ValueModifier>> ValueModifierComps
        {
            get
            {
                IEnumerable<QuirkComp_ValueModifier> valueModifiers = GetAllCompsByType<QuirkComp_ValueModifier>();
                if(areValueModifierCompsStale || valueModifierComps == null)
                {
                    valueModifierComps = valueModifiers
                        .GroupBy(comp => comp.modifierName)
                        .ToDictionary(
                            group => group.Key,
                            group => group.AsEnumerable().OrderBy(comp => comp.priority).ToList()
                        );
                        areValueModifierCompsStale = false;
                }
                return valueModifierComps;
            }
        }

        private SortedDictionary<QuirkPoolDef, List<Quirk>> groupedQuirks;
        private bool areGroupedQuirksStale = true;
        public SortedDictionary<QuirkPoolDef, List<Quirk>> GroupedQuirks
        {
            get
            {
                // this calls the property creator, which may or may not set the grouped quirks to stale. It is important to do this before checking for areGroupedQuirksStale, because that value is only set within the ActiveQuirks property itself!
                List<Quirk> activeQuirks = ActiveQuirks;

                // grouped quirks should never be null, but it doesn't hurt to check and set them if they are
                if(areGroupedQuirksStale || groupedQuirks == null)
                {
                    if(RV2Log.ShouldLog(true, "Quirks"))
                        RV2Log.Message("Quirks are stale, regrouping", false, "Quirks");
                    Dictionary<QuirkPoolDef, List<Quirk>> quirks = activeQuirks
                        .GroupBy(quirk => quirk.Pool)
                        .ToDictionary(
                            group => group.Key,
                            group => group.AsEnumerable().ToList()
                        );
                    groupedQuirks = new SortedDictionary<QuirkPoolDef, List<Quirk>>(quirks);
                    areGroupedQuirksStale = false;
                }
                return groupedQuirks;
            }
        }

        public bool HasQuirk(QuirkDef quirkDef)
        {
            return ActiveQuirks.Any(quirk => quirk.def == quirkDef);
        }

        public void Tick()
        {
            PersistentQuirkLayer.Tick();
        }

        public void ApplyPreset(QuirkPreset preset)
        {
            PersistentQuirkLayer.ApplyPreset(preset);
        }

        public void RerollAll()
        {
            PersistentQuirkLayer.CalculateQuirks();
            IdeologyQuirkLayer.CalculateQuirks();
        }
        public void Notify_IdeologyChanged()
        {
            IdeologyQuirkLayer.IsStale = true;
        }

        public void RerollPersistentQuirkPool(QuirkPoolDef pool)
        {
            PersistentQuirkLayer.CalculatePool(pool);
        }

        public bool HasAllQuirksInPersistentPool(QuirkPoolDef pool)
        {
            return PersistentQuirkLayer.HasAllQuirksInPool(pool);
        }

        public void RemovePersistentQuirk(QuirkDef quirk)
        {
            PersistentQuirkLayer.RemoveQuirk(quirk);
        }
        public void RemovePersistentQuirk(Quirk quirk)
        {
            PersistentQuirkLayer.RemoveQuirk(quirk);
        }

        public bool CanPostInitAddQuirk(QuirkDef quirk, out string reason)
        {
            return PersistentQuirkLayer.IsQuirkApplicable(quirk, out reason);
        }

        public bool TryPostInitAddQuirk(QuirkDef quirk, out string reason)
        {
            if(!PersistentQuirkLayer.IsQuirkApplicable(quirk, out reason))
            {
                return false;
            }
            PersistentQuirkLayer.ForceQuirk(quirk);
            return true;
        }

        private void ResolveConflicts()
        {
            List<Quirk> resolvedQuirks = new List<Quirk>();
            List<QuirkPoolDef> resolvedPools = new List<QuirkPoolDef>();
            if(activeQuirks == null)
            {
                return;
            }
            foreach(Quirk activeQuirk in activeQuirks)
            {
                if(!activeQuirk.IsValid())
                {
                    RV2Log.Warning("invalid quirk, removing from quirks, usually caused by updates", "Quirks");
                    RemovePersistentQuirk(activeQuirk);
                    continue;
                }
                QuirkPoolDef pool = activeQuirk.Pool;
                if(pool.poolType == QuirkPoolType.RollForEach)
                {
                    resolvedQuirks.Add(activeQuirk);
                    continue;
                }
                // first of its pool, add it
                if(!resolvedPools.Contains(pool))
                {
                    resolvedQuirks.Add(activeQuirk);
                    resolvedPools.Add(pool);
                    continue;
                }
                else
                {
                    // this should never happen, we have a PickOne pool with two persistent quirks, simply ignore the current activeQuirk
                    Log.Error("Collision of persistent quirks, removing quirk " + activeQuirk.def.defName);
                }
            }
            activeQuirks.Clear();
            activeQuirks.AddRange(resolvedQuirks);
        }

        // ---------------- UI methods ------------------
        public void PickQuirkDialogue(QuirkPoolDef pool)
        {
            PersistentQuirkLayer.PickQuirkDialogue(pool);
        }
        public void AddQuirkDialogue(QuirkPoolDef pool)
        {
            PersistentQuirkLayer.AddQuirkDialogue(pool);
        }

        // ---------------- comp methods ------------------

        public bool TryGetOverriddenThought(ThoughtDef originalThought, out ThoughtDef overrideThought)
        {
            overrideThought = null;
            List<QuirkComp_ThoughtOverride> overridingThoughtComps = GetAllCompsByType<QuirkComp_ThoughtOverride>()?
                .Where(comp => comp.originalThought == originalThought)
                .ToList();
            if(overridingThoughtComps.NullOrEmpty())
            {
                // Log.Message("No override thoughts found");
                return false;
            }
            // if multiple exist, sort by priority, if only one exists, this will simply return that one
            overrideThought = overridingThoughtComps.MaxBy(comp => comp.priority).overrideThought;
            return overrideThought != null;
        }

        public bool TryGetOverriddenHediff(HediffDef originalHediff, out HediffDef overrideHediff)
        {
            overrideHediff = null;
            List<QuirkComp_HediffOverride> overridingHediffComps = GetAllCompsByType<QuirkComp_HediffOverride>()?
                .Where(comp => comp.originalHediff == originalHediff)
                .ToList();
            if(overridingHediffComps.NullOrEmpty())
            {
                return false;
            }
            // if multiple exist, sort by priority, if only one exists, this will simply return that one
            overrideHediff = overridingHediffComps.MaxBy(comp => comp.priority).overrideHediff;
            return true;
        }

        public bool EnablesSituationalThought(ThoughtDef thought)
        {
            return GetAllCompsByType<QuirkComp_SituationalThoughtEnabler>()
                .Any(comp => comp.enabledThought == thought);
        }

        public IEnumerable<ThoughtDef> GetPostVoreMemories(List<string> keywords)
        {
            IEnumerable<QuirkComp_PostVoreMemory> postVoreThoughtComps = GetAllCompsByType<QuirkComp_PostVoreMemory>();
            // for each thought comp check the keywords for applicable thoughts
            IEnumerable<ThoughtDef> memoriesForComp = postVoreThoughtComps
                .Where(comp => comp.keywords  // take each comps keywords
                    .TrueForAll(keyword => keywords.Contains(keyword))) // and check if the current vores keywords has them all
                .Select(comp => comp.memory);   // then use the memory
            if(RV2Log.ShouldLog(false, "Quirks"))
                RV2Log.Message($"keywords {string.Join(",", keywords)} generate these memories: {string.Join(",", memoriesForComp.Select(memory => memory.defName))}", "Quirk");
            return memoriesForComp;
        }

        public bool HasSpecialFlag(string flag)
        {
            return GetAllCompsByType<QuirkComp_SpecialFlag>().Any(quirk => quirk.flag == flag);
        }

        public bool HasVoreEnabler(VoreTargetSelectorRequest request)
        {
            return GetAllCompsByType<QuirkComp_VoreEnabler>().Any(comp => comp.selector.Matching(request));
        }

        public IEnumerable<T> GetAllCompsByType<T>() where T : QuirkComp
        {
            if(!HasComp<T>())
            {
                return new List<T>();
            }
            return QuirksByCompType[typeof(T)].Cast<T>();
        }

        public bool HasComp<T>() where T : QuirkComp
        {
            return QuirksByCompType.ContainsKey(typeof(T));
        }

        public bool HasValueModifier(string modifierName)
        {
            return ActiveQuirks.Any(quirk => quirk.def.HasValueModifiersFor(modifierName));
        }

        public bool TryGetValueModifier(string modifierName, ModifierOperation operation, out float modifierValue)
        {
            IOrderedEnumerable<QuirkComp_ValueModifier> comps = GetAllCompsByType<QuirkComp_ValueModifier>()
                .Where(comp => comp.modifierName == modifierName   // limit to those that actually affect the value we are looking at
                    && comp.operation == operation)  // limit to those that do the same operation as what we are looking for
                .OrderBy(comp => comp.priority); // make sure we follow priority, so mathematical operations are done in-order of priority
            if(comps.EnumerableNullOrEmpty())
            {
                modifierValue = 0;
                return false;
            }
            IEnumerable<float> values = comps.Select(comp => comp.modifierValue); // get the float value
            if(values.EnumerableNullOrEmpty())
            {
                modifierValue = 0;
                return false;
            }
            modifierValue = values.Aggregate((x, y) => operation.Aggregate(x, y));    // aggregate based on operation
            if(RV2Log.ShouldLog(true, "Quirks"))
                RV2Log.Message($"pawn {pawn?.LabelShort} has modifiers for {modifierName}: {modifierValue}", true, "Quirks");
            return true;
        }

        public float ModifyValue(string modifierName, float value)
        {
            if(ValueModifierComps.ContainsKey(modifierName))
            {
                foreach(QuirkComp_ValueModifier comp in ValueModifierComps[modifierName])
                {
                    value = comp.Modify(value);
                }
            }
            return value;
        }

        public bool HasTotalSelectorModifier(VoreTargetSelectorRequest request)
        {
            IEnumerable<float> values = GetAllCompsByType<QuirkComp_VoreTargetSelectorModifier>()
                .Where(comp => comp.ModifierValid(request))
                .Select(comp => comp.GetModifierValue(request))
                .Where(value => value != float.MinValue);
            return !values.EnumerableNullOrEmpty();
        }

        public float GetTotalSelectorModifierForDirect(VoreTargetSelectorRequest request, ModifierOperation modifierOperation = ModifierOperation.Add)
        {
            return GetTotalSelectorModifier(GetAllCompsByType<QuirkComp_VoreTargetSelectorModifier>(), request, modifierOperation);
        }
        public float GetTotalSelectorModifierForFeeder(VoreTargetSelectorRequest request, ModifierOperation modifierOperation = ModifierOperation.Add)
        {
            return GetTotalSelectorModifier(GetAllCompsByType<QuirkComp_FeederVoreTargetSelectorModifier>(), request, modifierOperation);
        }
        private float GetTotalSelectorModifier(IEnumerable<QuirkComp_VoreTargetSelectorModifier> selectorList, VoreTargetSelectorRequest request, ModifierOperation modifierOperation = ModifierOperation.Add)
        {
            IEnumerable<float> values = selectorList.Where(comp => comp.ModifierValid(request))
                .Select(comp => comp.GetModifierValue(request))
                .Where(value => value != float.MinValue);
            if(!values.EnumerableNullOrEmpty())
            {
                return values.Aggregate((a, b) => modifierOperation.Aggregate(a, b));    // aggregate all modifier values
            }
            else
            {
                return modifierOperation.DefaultModifierValue();
            }
        }

        public float GetTotalSelectorModifier(VoreRole role, ModifierOperation modifierOperation = ModifierOperation.Add)
        {
            VoreTargetSelectorRequest request = new VoreTargetSelectorRequest(true)
            {
                role = role
            };
            return GetTotalSelectorModifierForDirect(request, modifierOperation);
        }
        public float GetTotalSelectorModifier(RaceType raceType, ModifierOperation modifierOperation = ModifierOperation.Add)
        {
            VoreTargetSelectorRequest request = new VoreTargetSelectorRequest(true)
            {
                raceType = raceType
            };
            return GetTotalSelectorModifierForDirect(request, modifierOperation);
        }
        public bool HasDesignationBlock(RV2DesignationDef designation)
        {
            IEnumerable<QuirkComp_DesignationBlock> blocks = GetAllCompsByType<QuirkComp_DesignationBlock>();
            // Log.Message("blocks: " + string.Join(", ", blocks.Select(b => b.ToString())));
            return blocks.Any(block => block.designation == designation);
        }

        private Dictionary<PawnCapacityDef, float> capacityModifiers = new Dictionary<PawnCapacityDef, float>();
        public float CapModOffsetModifierFor(PawnCapacityDef capDef, IEnumerable<QuirkComp_CapacityOffsetModifier> modifiers = null)
        {
            if(!capacityModifiers.ContainsKey(capDef))
            {
                if(modifiers == null)
                {
                    modifiers = GetAllCompsByType<QuirkComp_CapacityOffsetModifier>();
                }
                if(!modifiers.Any(m => m.capacity == capDef))
                {
                    return 1;
                }
                IEnumerable<float> modifierValues = modifiers
                    .Where(m => m.capacity == capDef)
                    .Select(m => m.modifierValue);
                float modifier = modifierValues.Aggregate((a, b) => ModifierOperation.Multiply.Aggregate(a, b));
                capacityModifiers.Add(capDef, modifier);
            }
            return capacityModifiers.TryGetValue(capDef, 1);
        }

        public void DoPostVoreActions(VoreRole role, VoreTrackerRecord record)
        {
            try
            {
                IEnumerable<QuirkComp_PostVoreAction> actions = GetAllCompsByType<QuirkComp_PostVoreAction>();
                if(actions.EnumerableNullOrEmpty())
                    return;
                foreach(QuirkComp_PostVoreAction action in actions)
                {
                    try
                    {
                        action.DoRolls(role, record);
                    }
                    catch(Exception e)
                    {
                        RV2Log.Error("Exception for post-vore roll: " + e);
                    }
                }
            }
            catch(Exception e)
            {
                RV2Log.Error("Uncaught exception in post vore actions: " + e);
            }
        }

        public void ExposeData()
        {
            Scribe_Deep.Look(ref persistentQuirkLayer, "persistentQuirkHelper", pawn);
            //Scribe_Deep.Look(ref temporaryQuirkHelper, "temporaryQuirkHelper", pawn);
            Scribe_Collections.Look(ref activeQuirks, "activeQuirks", LookMode.Deep);
            Scribe_References.Look(ref pawn, "pawn", true);
            // when loading, validate that all quirks are valid, remove invalid ones
            if(Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if(!activeQuirks.NullOrEmpty())
                {
                    int removedQuirks = activeQuirks.RemoveAll(quirk => !quirk.IsValid());
                    if(removedQuirks > 0)
                    {
                        if(RV2Log.ShouldLog(false, "Quirks"))
                            RV2Log.Message($"Had to remove {removedQuirks} quirks due to being invalid", "Quirks");
                    }
                }
            }
        }

        public object Clone()
        {
            return new QuirkManager()
            {
                pawn = this.pawn,
                persistentQuirkLayer = (QuirkLayer_Persistent)this.persistentQuirkLayer.Clone(),
                ideologyQuirkLayer = (QuirkLayer_Ideology)this.ideologyQuirkLayer.Clone(),
                activeQuirks = this.activeQuirks == null ? null : this.activeQuirks
                    .Select(quirk => (Quirk)quirk.Clone())
                    .ToList()
            };
        }
    }
}
