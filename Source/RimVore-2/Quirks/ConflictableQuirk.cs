using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    // TODO this needs a good amount of clean-up
    public abstract class ConflictableQuirkObject : Def
    {
        public List<TraitDef> requiredTraits = new List<TraitDef>();
        public List<TraitDef> blockingTraits = new List<TraitDef>();
        public List<QuirkDef> requiredQuirks = new List<QuirkDef>();
        public List<QuirkDef> blockingQuirks = new List<QuirkDef>();
        public List<string> requiredKeywords = new List<string>();
        public List<string> blockingKeywords = new List<string>();

        public virtual bool IsValid(Pawn pawn, out string reason, List<TraitDef> traits = null, List<QuirkDef> quirks = null, List<string> keywords = null)
        {
            if(traits == null)
            {
                traits = pawn.story?.traits?.allTraits?.ConvertAll(trait => trait.def);
            }
            if(!TraitsValid(traits, out reason))
            {
                return false;
            }
            if(RV2Log.ShouldLog(true, "Quirks"))
                RV2Log.Message($"{defName} - Traits valid", true, "Quirks");
            if(quirks == null)
            {
                quirks = pawn.QuirkManager()?.ActiveQuirks?.ConvertAll(quirk => quirk.def);
            }
            if(!QuirksValid(quirks, out reason))
            {
                return false;
            }
            if(RV2Log.ShouldLog(true, "Quirks"))
                RV2Log.Message($"{defName} - Quirks valid", true, "Quirks");
            if(keywords == null)
            {
                keywords = pawn.PawnKeywords();
            }
            if(!KeywordsValid(keywords, out reason))
            {
                return false;
            }
            if(RV2Log.ShouldLog(true, "Quirks"))
                RV2Log.Message($"{defName} - keywords valid - all valid", true, "Quirks");
            reason = null;
            return true;
        }

        public bool TraitsValid(Pawn pawn, out string reason)
        {
            return TraitsValid(GetTraits(pawn), out reason);
        }
        public bool TraitsValid(List<TraitDef> existing, out string reason)
        {
            Func<TraitDef, string> labelGetter = (TraitDef t) => t.label;
            return IsValid(existing, requiredTraits, blockingTraits, labelGetter, out reason);
        }

        public bool QuirksValid(Pawn pawn, out string reason)
        {
            return QuirksValid(GetQuirks(pawn), out reason);
        }
        public bool QuirksValid(List<QuirkDef> existing, out string reason)
        {
            Func<QuirkDef, string> labelGetter = (QuirkDef q) => q.label;
            return IsValid(existing, requiredQuirks, blockingQuirks, labelGetter, out reason);
        }

        public bool KeywordsValid(Pawn pawn, out string reason)
        {
            return KeywordsValid(pawn.PawnKeywords(), out reason);
        }
        public bool KeywordsValid(List<string> existing, out string reason)
        {
            Func<string, string> labelGetter = (string s) => s;
            return IsValid(existing, requiredKeywords, blockingKeywords, labelGetter, out reason);
        }

        private bool IsValid<T>(List<T> existing, List<T> required, List<T> blocking, Func<T, string> labelGetter, out string reason)
        {
            if(!existing.NullOrEmpty())
            {
                if(RV2Log.ShouldLog(true, "Quirks"))
                    RV2Log.Message($"{defName}|{typeof(T)} - existing: {string.Join(", ", existing.ConvertAll(i => labelGetter(i)))}", true, "Quirks");
            }
            if(!required.NullOrEmpty())
            {
                if(RV2Log.ShouldLog(true, "Quirks"))
                    RV2Log.Message($"{defName}|{typeof(T)} - required: {string.Join(", ", required.ConvertAll(i => labelGetter(i)))}", true, "Quirks");
            }
            if(!blocking.NullOrEmpty())
            {
                if(RV2Log.ShouldLog(true, "Quirks"))
                    RV2Log.Message($"{defName}|{typeof(T)} - blocking: {string.Join(", ", blocking.ConvertAll(i => labelGetter(i)))}", true, "Quirks");
            }
            if(!required.NullOrEmpty())
            {
                List<T> currentlyRequired;
                if(existing.NullOrEmpty())
                {
                    currentlyRequired = required;
                }
                else
                {
                    currentlyRequired = required
                        .Where(item => !existing.Contains(item))
                        .ToList();
                }
                if(!currentlyRequired.NullOrEmpty())
                {
                    if(RV2Log.ShouldLog(true, "Quirks"))
                        RV2Log.Message($"{defName} - currently required: {string.Join(", ", currentlyRequired.Select(i => labelGetter(i)))}", true, "Quirks");
                    string currentlyRequiredString = string.Join(", ", currentlyRequired.ConvertAll(item => labelGetter(item)));
                    reason = "RV2_QuirkInvalid_Required".Translate(typeof(T).ToString(), currentlyRequiredString);
                    return false;
                }
            }
            if(!blocking.NullOrEmpty())
            {
                List<T> currentlyBlocking;
                if(existing.NullOrEmpty())
                {
                    currentlyBlocking = null;
                }
                else
                {
                    currentlyBlocking = blocking
                        .Where(item => existing.Contains(item))
                        .ToList();
                }
                if(!currentlyBlocking.NullOrEmpty())
                {
                    if(RV2Log.ShouldLog(true, "Quirks"))
                        RV2Log.Message($"{defName} - currently blocking: {string.Join(", ", currentlyBlocking.Select(i => labelGetter(i)))}", true, "Quirks");
                    string currentlyBlockingString = string.Join(", ", currentlyBlocking.ConvertAll(item => labelGetter(item)));
                    reason = "RV2_QuirkInvalid_Blocking".Translate(typeof(T).ToString(), currentlyBlockingString);
                    return false;
                }
            }
            reason = null;
            return true;
        }

        private List<TraitDef> GetTraits(Pawn pawn)
        {
            return pawn.story?.traits?.allTraits?.ConvertAll(trait => trait.def);
        }

        private List<QuirkDef> GetQuirks(Pawn pawn)
        {
            return pawn.QuirkManager().ActiveQuirks.ConvertAll(quirk => quirk.def);
        }
    }
}
