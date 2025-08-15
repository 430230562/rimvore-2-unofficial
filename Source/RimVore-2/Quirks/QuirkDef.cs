using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using UnityEngine;
using System.Linq;

namespace RimVore2
{
    public class QuirkDef : ConflictableQuirkObject
    {
        public bool hidden = false;
        public QuirkRarity rarity = QuirkRarity.Invalid;
        public List<QuirkComp> comps = new List<QuirkComp>();

        public bool HasComp<T>() where T : QuirkComp
        {
            if(comps.NullOrEmpty())
            {
                return false;
            }
            return comps.Any(comp => comp is T);
        }

        public IEnumerable<T> GetComps<T>() where T : QuirkComp
        {
            if(!HasComp<T>())
            {
                return new List<T>();
            }
            return comps
                .Where(comp => comp is T)
                .Cast<T>();
        }

        public bool TryGetValueModifierFor(string modifierName, ModifierOperation operation, out float modifierValue)
        {
            IEnumerable<float> values = GetComps<QuirkComp_ValueModifier>() // get all value modifier comps
                .Where(comp => comp.modifierName == modifierName   // limit to those that actually affect the value we are looking at
                    && comp.operation == operation)  // limit to those that do the same operation as what we are looking for
                .OrderBy(comp => comp.priority) // make sure we follow priority, so mathematical operations are done in-order of priority
                .Select(comp => comp.modifierValue); // get the float value
            if(values.EnumerableNullOrEmpty())
            {
                modifierValue = 0;
                return false;
            }
            modifierValue = values.Aggregate((x, y) => operation.Aggregate(x, y));    // aggregate based on operation
            return true;
        }
        public bool HasValueModifiersFor(string modifierName)
        {
            return GetComps<QuirkComp_ValueModifier>()
                .Any(comp => comp.modifierName == modifierName);
        }

        public override bool IsValid(Pawn pawn, out string reason, List<TraitDef> traits = null, List<QuirkDef> quirks = null, List<string> keywords = null)
        {
            if(!base.IsValid(pawn, out reason, traits, quirks, keywords))
            {
                return false;
            }
            if(this.GetPool() == null)
            {
                reason = "RV2_QuirkInvalid_Reason_NoPoolForQuirk".Translate();
                return false;
            }
            if(!this.IsEnabled())
            {
                reason = "RV2_QuirkInvalid_DisabledInSettings".Translate();
                return false;
            }
            reason = null;
            return true;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if(comps != null)
            {
                foreach(QuirkComp comp in comps)
                {
                    foreach(string error in comp.ConfigErrors())
                    {
                        yield return error;
                    }
                }
            }
        }

        public string AppendDebugInformation(Pawn pawn, List<string> pawnKeywords)
        {
            QuirkManager quirkManager = pawn.QuirkManager();
            if(quirkManager == null)
            {
                Log.Error("Trying to retrieve quirk debug information for a pawn that doesn't have quirks");
                return "ERROR";
            }
            TraitSet traitSet = pawn?.story?.traits;
            string append = "";
            append += "\n\nDevMode - Additional information";
            if(!this.IsEnabled())
            {
                append += "\n\nThis quirk has been disabled in the Quirk settings!".Colorize(Color.red);
            }
            if(!requiredQuirks.NullOrEmpty())
            {
                append += "\n\nRequired Quirks:\n";
                // if pawn has quirk, color green, otherwise color red
                List<string> labels = requiredQuirks.ConvertAll(quirk => "- " + (quirkManager.HasQuirk(quirk) ? quirk.label.Colorize(Color.green) : quirk.label.Colorize(Color.red)));
                append += string.Join("\n", labels);
            }
            if(!requiredKeywords.NullOrEmpty())
            {
                append += "\n\nRequired keywords:\n";
                // if pawn has quirk, color green, otherwise color red
                List<string> labels = requiredKeywords.ConvertAll(keyword => "- " + (pawnKeywords.Contains(keyword) ? keyword.Colorize(Color.green) : keyword.Colorize(Color.red)));
                append += string.Join("\n", labels);
            }
            else if(!requiredTraits.NullOrEmpty() && traitSet != null)
            {
                append += "\n\nRequired Traits:\n";
                // if pawn has trait, color green, otherwise red
                List<string> labels = requiredTraits.ConvertAll(trait => "- " + (traitSet.HasTrait(trait) ? trait.defName.Colorize(Color.green) : trait.defName.Colorize(Color.red)));
                append += string.Join("\n", labels);
            }
            if(!blockingQuirks.NullOrEmpty())
            {
                append += "\n\nBlocking Quirks:\n";
                // if pawn has quirk, color red, otherwise color green
                List<string> labels = blockingQuirks.ConvertAll(quirk => "- " + (quirkManager.HasQuirk(quirk) ? quirk.label.Colorize(Color.red) : quirk.label.Colorize(Color.green)));
                append += string.Join("\n", labels);
            }
            else if(!blockingTraits.NullOrEmpty() && traitSet != null)
            {
                append += "\n\nBlocking Traits:\n";
                // if pawn has trait, color red, otherwise green
                List<string> labels = blockingTraits.ConvertAll(trait => "- " + (traitSet.HasTrait(trait) ? trait.defName.Colorize(Color.red) : trait.defName.Colorize(Color.green)));
                append += string.Join("\n", labels);
            }
            if(!blockingKeywords.NullOrEmpty())
            {
                append += "\n\nBlocking keywords:\n";
                // if pawn has quirk, color green, otherwise color red
                List<string> labels = blockingKeywords.ConvertAll(keyword => "- " + (pawnKeywords.Contains(keyword) ? keyword.Colorize(Color.red) : keyword.Colorize(Color.green)));
                append += string.Join("\n", labels);
            }
            if(!comps.NullOrEmpty())
            {
                append += "\n\nQuirk-Comps:\n";
                append += string.Join("\n", comps.ConvertAll(comp => "- " + comp.ToString()));
            }

            return append;
        }
    }
}
