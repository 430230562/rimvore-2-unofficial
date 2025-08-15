using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace RimVore2
{
    public class QuirkLayer_Persistent : QuirkLayer
    {
        public QuirkLayer_Persistent(Pawn pawn) : base(pawn) { }

        public QuirkLayer_Persistent() : base() { }

        private List<QuirkDef> forcedQuirks = new List<QuirkDef>();
        private List<QuirkDef> blockedQuirks = new List<QuirkDef>();
        private List<QuirkDef> blockedQuirksByBackstories = new List<QuirkDef>();

        private QuirkPreset presetToApply = null;

        private bool IsBlockedByBackstory(QuirkDef quirkDef) => blockedQuirksByBackstories.Contains(quirkDef);

        public bool IsQuirkApplicable(QuirkDef quirkDef, out string reason)
        {
            if(!quirkDef.IsValid(pawn, out reason))
            {
                return false;
            }
            if(IsBlockedByBackstory(quirkDef))
            {
                reason = "RV2_QuirkInvalid_BackstoryBlocking".Translate();
                return false;
            }
            if(HasQuirk(quirkDef))
            {
                reason = "RV2_QuirkInvalid_AlreadyHasQuirk".Translate();
                return false;
            }
            // check if we need to remove the old quirk
            if(quirkDef.GetPool().poolType == QuirkPoolType.PickOne)
            {
                Quirk oldQuirk = Quirks().Find(q => q.Pool == quirkDef.GetPool());
                if(!CanRemoveQuirkWithoutConflict(pawn, oldQuirk, out reason))
                {
                    return false;
                }
            }

            reason = null;
            return true;
        }

        public bool CanRemoveQuirkWithoutConflict(Pawn pawn, Quirk quirk, out string reason)
        {
            return QuirkUtility.CanRemoveQuirkWithoutConflict(pawn, quirk, Quirks(), out reason);
        }

        public void ForceQuirk(QuirkDef quirkDef)
        {
            Quirk newQuirk = new Quirk(quirkDef);
            if(newQuirk.Pool.poolType == QuirkPoolType.PickOne)
            {
                Quirk oldQuirk = quirks.Find(quirk => quirk.Pool == newQuirk.Pool);
                if(oldQuirk != null)
                {
                    RemoveQuirk(oldQuirk);
                }
            }
            AddQuirk(newQuirk);
        }

        public void ApplyPreset(QuirkPreset preset)
        {
            presetToApply = preset;
            CalculateQuirks();
        }

        public void CalculatePool(QuirkPoolDef pool, List<string> keywords = null, List<TraitDef> traits = null)
        {
            // make sure the pool is empty before calculating it
            quirks.RemoveAll(quirk => quirk.Pool == pool);
            if(keywords.NullOrEmpty())
            {
                keywords = pawn.PawnKeywords(true);
            }
            if(traits.NullOrEmpty())
            {
                traits = pawn.story?.traits?.allTraits?.ConvertAll(trait => trait.def);
            }
            List<QuirkDef> quirkDefs = quirks.ConvertAll(quirk => quirk.def);
            if(pool.IsValid(pawn, out string reason, traits, quirkDefs, keywords))
            {
                // check backstories for forced quirks
                List<QuirkDef> forcedQuirksForPool = forcedQuirks
                    .Where(forcedQuirk => forcedQuirk.GetPool() == pool  // only take forced quirks for this pool
                    && !blockedQuirks.Contains(forcedQuirk) // remove blocked quirks
                    && forcedQuirk.IsValid(pawn, out _, traits, quirkDefs, keywords)) // only allow valid quirks
                    .ToList();

                if(!forcedQuirksForPool.NullOrEmpty())
                {
                    if(RV2Log.ShouldLog(false, "Quirks"))
                        RV2Log.Message($"forced quirks via backstory for pool {pool.defName} : {string.Join(", ", forcedQuirksForPool.ConvertAll(q => q.defName))}", "Quirks");
                }
                switch(pool.poolType)
                {
                    case QuirkPoolType.PickOne:
                        QuirkDef pickedQuirkDef;
                        if(forcedQuirksForPool.Count > 0)
                        {
                            if(forcedQuirksForPool.Count >= 2)
                            {
                                RV2Log.Warning("Trying to force two quirks via backstories for a PickOne quirk pool, picking a random one", "Quirks");
                            }
                            pickedQuirkDef = forcedQuirksForPool.RandomElement();
                        }
                        else
                        {
                            List<QuirkDef> validQuirks = pool.quirks
                                .FindAll(quirk => quirk.IsValid(pawn, out _, traits, quirkDefs, keywords)   // take all valid quirks
                                && !blockedQuirks.Contains(quirk)); // filter out blocked quirks
                            if(validQuirks.Count <= 0)
                            {
                                return;
                            }
                            pickedQuirkDef = RandomUtility.RandomElementByWeight(validQuirks, (quirk) => (float)quirk.GetRarity());
                        }
                        if(RV2Log.ShouldLog(true, "Quirks"))
                            RV2Log.Message($"Picked quirk {pickedQuirkDef}, chance: { pickedQuirkDef.GetRarity()}", false, "Quirks");
                        AddQuirk(pickedQuirkDef);
                        quirkDefs.Add(pickedQuirkDef);
                        return;
                    case QuirkPoolType.RollForEach:
                        foreach(QuirkDef forcedQuirkDef in forcedQuirksForPool)
                        {
                            AddQuirk(forcedQuirkDef);
                            quirkDefs.Add(forcedQuirkDef);
                        }

                        foreach(QuirkDef quirkDef in pool.quirks)
                        {
                            if(IsBlockedByBackstory(quirkDef))
                            {
                                if(RV2Log.ShouldLog(false, "Quirks"))
                                    RV2Log.Message("Quirk " + quirkDef.label + " is blocked", "Quirks");
                                continue;
                            }

                            float chanceToRoll = (float)quirkDef.GetRarity() / 100;
                            if(Rand.Chance(chanceToRoll))
                            {
                                if(quirkDef.IsValid(pawn, out reason, traits, quirkDefs, keywords))
                                {
                                    if(RV2Log.ShouldLog(true, "Quirks"))
                                        RV2Log.Message($"Rolled quirk {quirkDef.defName}, chance: {quirkDef.GetRarity()}", false, "Quirks");
                                    AddQuirk(quirkDef);
                                    quirkDefs.Add(quirkDef);
                                }
                                else
                                {
                                    if(RV2Log.ShouldLog(true, "Quirks"))
                                        RV2Log.Message($"Quirk {quirkDef.label} not allowed: {reason}", false, "Quirks");
                                }
                            }
                        }
                        return;
                    default:
                        RV2Log.Warning("Skipping unknown pool type: " + pool.poolType, "Quirks");
                        return;
                }
            }
            else
            {
                if(RV2Log.ShouldLog(true, "Quirks"))
                    RV2Log.Message("Pool " + pool.label + " not allowed: " + reason, false, "Quirks");
            }
            SetStale();
        }

        public override void CalculateQuirks()
        {
            // recache existing quirk forces / blocks
            forcedQuirks = pawn.GetAllForcedQuirks();
            if(presetToApply != null)
            {
                if(RV2Log.ShouldLog(false, "Quirks"))
                {
                    RV2Log.Message($"Adding forced quirks from preset: {presetToApply.Quirks.Select(def => def.defName)}", "Quirks");
                }
                forcedQuirks.AddRange(presetToApply.Quirks);
                presetToApply = null;
            }
            blockedQuirks = pawn.GetAllBlockedQuirks();

            if(quirks == null)
            {
                quirks = new List<Quirk>();
            }
            else
            {
                quirks.Clear();
            }
            List<TraitDef> traits = pawn.story?.traits?.allTraits?.ConvertAll(trait => trait.def);
            List<string> keywords = pawn.PawnKeywords(true);

            if(RV2Log.ShouldLog(true, "Quirks"))
                RV2Log.Message($"Quirk recalculation\nOrder: {string.Join(", ", RV2_Common.SortedQuirkPools.ConvertAll(q => q.label))}" +
                    $"\nRecached traits: {(traits.NullOrEmpty() ? "None" : string.Join(", ", traits.ConvertAll(t => t.ToString())))}" +
                    $"\nRecached keywords: {(keywords.NullOrEmpty() ? "None" : string.Join(", ", keywords))}",
                    false, "Quirks");

            foreach(QuirkPoolDef pool in RV2_Common.SortedQuirkPools)
            {
                CalculatePool(pool, keywords, traits);
            }
            SetStale();
        }

        // ---------------- UI ----------------

        public void PickQuirkDialogue(QuirkPoolDef pool)
        {
            List<QuirkDef> quirks = pool.quirks;
            if(quirks != null && quirks.Count > 0)
            {
                List<FloatMenuOption> options = quirks.ConvertAll(quirk =>
                    new FloatMenuOption(quirk.label, () => ForceQuirk(quirk))
                );
                Find.WindowStack.Add(new FloatMenu(options));
            }
        }

        public void AddQuirkDialogue(QuirkPoolDef pool)
        {
            List<QuirkDef> quirks = pool.quirks // take all quirks for pool
                .FindAll(quirk => !HasQuirk(quirk));      // and only show the ones the pawn doesn't already have
            if(quirks.Count == 0)
            {
                RV2Log.Warning("Tried to create add quirk dialogue, but pawn already has all quirks in pool.", "Quirks");
                return;
            }
            List<FloatMenuOption> options = quirks.ConvertAll(quirk =>
                new FloatMenuOption(quirk.label, () => AddQuirk(quirk))
            );
            Find.WindowStack.Add(new FloatMenu(options));
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override object Clone()
        {
            return new QuirkLayer_Persistent()
            {
                pawn = this.pawn,
                quirks = quirks == null ? null : this.quirks
                    .Select(quirk => (Quirk)quirk.Clone())
                    .ToList()
            };
        }
    }
}
