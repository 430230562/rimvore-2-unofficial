using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace RimVore2
{
    public class QuirkLayer_Ideology : QuirkLayer
    {
        public bool HasContent = false;

        public List<QuirkDef> quirksToRemove = new List<QuirkDef>();
        public List<QuirkDef> quirksToAdd = new List<QuirkDef>();
        public List<List<QuirkDef>> quirksToEnsure = new List<List<QuirkDef>>();

        public QuirkLayer_Ideology() : base() { }
        public QuirkLayer_Ideology(Pawn pawn) : base(pawn) { }

        public override void CalculateQuirks()
        {
            if(!ModsConfig.IdeologyActive)
            {
                HasContent = false;
                return;
            }
            if(pawn.Ideo == null)
            {
                HasContent = false;
                if(RV2Log.ShouldLog(true, "IdeoQuirks"))
                    RV2Log.Message("No ideology, so no ideo quirks", "IdeoQuirks");
                return;
            }
            if(RV2Log.ShouldLog(false, "IdeoQuirks"))
                RV2Log.Message("Calculating ideo quirks", "IdeoQuirks");
            quirksToRemove.Clear();
            quirksToAdd.Clear();
            quirksToEnsure.Clear();
            IEnumerable<PreceptComp> comps = pawn.Ideo.PreceptsListForReading // all precepts
                .SelectMany(precept => precept.def.comps);   // all comps of all precepts
            foreach(PreceptComp comp in comps)
            {
                if(comp is PreceptComp_Quirk_AddQuirks addComp)
                {
                    quirksToAdd.AddRange(addComp.quirks);
                }
                if(comp is PreceptComp_Quirk_RemoveQuirks removeComp)
                {
                    quirksToRemove.AddRange(removeComp.quirks);
                }
                if(comp is PreceptComp_Quirk_EnsureOneOf ensureComp)
                {
                    quirksToEnsure.Add(ensureComp.quirks);
                }
            }
            if(!quirksToAdd.NullOrEmpty() || !quirksToRemove.NullOrEmpty() || !quirksToEnsure.NullOrEmpty())
            {
                HasContent = true;
            }
            IsStale = false;
        }

        List<QuirkDef> currentQuirkDefsCache = new List<QuirkDef>();
        public void Merge(List<Quirk> activeQuirks)
        {
            RecacheQuirks();
            if(!HasContent)
            {
                if(RV2Log.ShouldLog(true, "IdeoQuirks"))
                    RV2Log.Message("Ideo quirk layer has no quirks, nothing to merge", "IdeoQuirks");
                return;
            }
            List<string> keywords = pawn.PawnKeywords(true);
            List<TraitDef> traits = pawn.story?.traits?.allTraits?.ConvertAll(trait => trait.def);
            currentQuirkDefsCache = activeQuirks.Select(quirk => quirk.def).ToList();
            RemoveQuirks(activeQuirks, keywords, traits);
            AddQuirks(activeQuirks, keywords, traits);
            EnsureQuirks(activeQuirks, keywords, traits);
        }

        private void RemoveQuirks(List<Quirk> activeQuirks, List<string> keywords, List<TraitDef> traits)
        {
            foreach(QuirkDef removalQuirkDef in quirksToRemove)
            {
                Quirk existingQuirk = activeQuirks.Find(q => q.def == removalQuirkDef);
                if(existingQuirk == null)
                {
                    continue;   // nothing to do, pawn did not have the quirk we want to remove
                }
                if(!QuirkUtility.CanRemoveQuirkWithoutConflict(pawn, existingQuirk, activeQuirks, out string reason))
                {
                    if(RV2Log.ShouldLog(false, "IdeoQuirks"))
                        RV2Log.Message($"Could not remove quirk {existingQuirk.def.defName} for ideology, reason: {reason}", "IdeoQuirks");
                }
                QuirkPoolDef pool = existingQuirk.Pool;
                IdeologyQuirk replacementQuirk = null;
                if(pool.poolType == QuirkPoolType.PickOne)
                {
                    // we need to find a replacement for the quirk that we are removing

                    List<QuirkDef> validQuirks = pool.quirks
                        .FindAll(quirk => quirk.IsValid(pawn, out _, traits, currentQuirkDefsCache, keywords)   // take all valid quirks
                        && quirk != removalQuirkDef); // filter out the quirk we just removed
                    if(validQuirks.Count <= 0)
                    {
                        if(RV2Log.ShouldLog(false, "IdeoQuirks"))
                            RV2Log.Message($"Could not remove quirk {existingQuirk.def.defName} because no replacement quirk could be found", "IdeoQuirks");
                        continue;
                    }
                    QuirkDef replacementQuirkDef = RandomUtility.RandomElementByWeight(validQuirks, (quirk) => (float)quirk.GetRarity());
                    if(RV2Log.ShouldLog(false, "IdeoQuirks"))
                        RV2Log.Message($"Found replacement quirk: {replacementQuirkDef.defName}", "IdeoQuirks");
                    replacementQuirk = new IdeologyQuirk(replacementQuirkDef, pawn.Ideo, existingQuirk);
                }
                activeQuirks.Remove(existingQuirk);
                currentQuirkDefsCache.Remove(existingQuirk.def);
                if(RV2Log.ShouldLog(false, "IdeoQuirks"))
                    RV2Log.Message($"Successfully suppressed {existingQuirk.def.defName}", "IdeoQuirks");
                if(replacementQuirk != null)
                {
                    activeQuirks.Add(replacementQuirk);
                }
            }
        }

        private void AddQuirks(List<Quirk> activeQuirks, List<string> keywords, List<TraitDef> traits)
        {
            foreach(QuirkDef addingQuirkDef in quirksToAdd)
            {
                if(activeQuirks.Any(quirk => quirk.def == addingQuirkDef))
                {
                    if(RV2Log.ShouldLog(false, "IdeoQuirks"))
                        RV2Log.Message($"Pawn already has quirk {addingQuirkDef.defName}", "IdeoQuirks");
                    continue;
                }
                if(!addingQuirkDef.IsValid(pawn, out string reason, traits, currentQuirkDefsCache, keywords))
                {
                    if(RV2Log.ShouldLog(false, "IdeoQuirks"))
                        RV2Log.Message($"Could not add quirk {addingQuirkDef.defName}, reason: {reason}", "IdeoQuirks");
                    continue;
                }
                IdeologyQuirk newQuirk = new IdeologyQuirk(addingQuirkDef, pawn.Ideo);
                activeQuirks.Add(newQuirk);
                if(RV2Log.ShouldLog(false, "IdeoQuirks"))
                    RV2Log.Message($"Added quirk {addingQuirkDef.defName}", "IdeoQuirks");
                currentQuirkDefsCache.Add(addingQuirkDef);
            }
        }

        private void EnsureQuirks(List<Quirk> activeQuirks, List<string> keywords, List<TraitDef> traits)
        {
            foreach(List<QuirkDef> ensurePool in quirksToEnsure)
            {
                Quirk existingQuirk = activeQuirks.Find(quirk => ensurePool.Contains(quirk.def));
                if(existingQuirk != null)
                {
                    if(RV2Log.ShouldLog(false, "IdeoQuirks"))
                        RV2Log.Message($"Pawn already has quirk {existingQuirk.def.defName} - pool to ensure: {String.Join(", ", ensurePool.Select(q => q.defName))}", "IdeoQuirks");
                    continue;
                }
                List<QuirkDef> validQuirks = ensurePool
                    .FindAll(quirk => quirk.IsValid(pawn, out _, traits, currentQuirkDefsCache, keywords));   // take all valid quirks
                if(validQuirks.NullOrEmpty())
                {
                    if(RV2Log.ShouldLog(false, "IdeoQuirks"))
                        RV2Log.Message("Could not ensure quirk existance, none of the quirks is valid", "IdeoQuirks");
                    continue;
                }
                QuirkDef newQuirkDef = validQuirks.RandomElementByWeight(quirk => (float)quirk.GetRarity());
                IdeologyQuirk newQuirk = new IdeologyQuirk(newQuirkDef, pawn.Ideo);
                if(RV2Log.ShouldLog(false, "IdeoQuirks"))
                    RV2Log.Message($"Added quirk {newQuirkDef.defName} for ensure pool: {String.Join(", ", ensurePool.Select(q => q.defName))}", "IdeoQuirks");
                activeQuirks.Add(newQuirk);
            }
        }

        public void RecacheQuirks()
        {
            if(IsStale)
            {
                CalculateQuirks();
                IsStale = false;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref HasContent, "HasContent");
        }

        public override object Clone()
        {
            return new QuirkLayer_Ideology()
            {
                pawn = this.pawn,
                HasContent = this.HasContent,
                quirks = quirks == null ? null : this.quirks
                    .Select(quirk => (Quirk)quirk.Clone())
                    .ToList()
            };
        }
    }
}