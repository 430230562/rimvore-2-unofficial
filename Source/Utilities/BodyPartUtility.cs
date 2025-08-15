using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public static class BodyPartUtility
    {
        public static BodyPartRecord GetBodyPartByDef(this Pawn pawn, BodyPartDef partDef)
        {
            return pawn.GetBodyPartsByDef(partDef).RandomElement();
        }

        public static List<BodyPartRecord> GetBodyPartsByDef(this Pawn pawn, BodyPartDef partDef)
        {
            return pawn.health.hediffSet.GetNotMissingParts()
                .Where(part => part.def == partDef)
                .ToList();
        }

        public static BodyPartRecord GetBodyPartByName(this Pawn pawn, string bodyPartName)
        {
            return GetBodyPartsByName(pawn, bodyPartName).RandomElementWithFallback();
        }

        public static List<BodyPartRecord> GetBodyPartsByName(this Pawn pawn, string bodyPartName)
        {
            List<BodyPartRecord> bodyParts = pawn.health?.hediffSet?.GetNotMissingParts()?.ToList();
            if(bodyParts.NullOrEmpty())
            {
                return null;
            }
            List<string> bodyPartAliases = bodyPartName.GetAliases("BodyPart");
            if(RV2Log.ShouldLog(true, "BodyParts"))
                RV2Log.Message($"Called search on {pawn.LabelShort} for {bodyPartName} with aliases {string.Join(", ", bodyPartAliases)}", true, "BodyParts");
            bodyParts = bodyParts.FindAll(bodyPart => bodyPart.def.defName.ContainsAnyAsSubstring(bodyPartAliases));
            if(RV2Log.ShouldLog(true, "BodyParts"))
                RV2Log.Message($"{pawn.LabelShort} bodyparts: {String.Join(",", bodyParts.ConvertAll(e => e.Label))}", true, "BodyParts");
            if(bodyParts.NullOrEmpty())
            {
                return new List<BodyPartRecord>();
            }
            return bodyParts;
        }

        public static Hediff GetHediffOnBodyPartByAlias(Pawn pawn, string bodyPartName, string hediffAlias)
        {
            BodyPartRecord bodyPart = pawn.GetBodyPartByName(bodyPartName);
            if(bodyPart == null)
            {
                return null;
            }
            List<string> hediffAliases = hediffAlias.GetAliases("Hediff");
            if(RV2Log.ShouldLog(true, "BodyParts"))
                RV2Log.Message($"Called search on {pawn.LabelShort} on part {bodyPart.Label} for hediff {hediffAlias} with aliases {string.Join(", ", hediffAliases)}", true, "BodyParts");

            return pawn.health.hediffSet.hediffs    // all hediffs
                .FindAll(h => h.Part == bodyPart)   // all hediffs for our body part
                .Find(hediff => hediff.def.defName.ContainsAnyAsSubstring(hediffAliases)); // contains any alias
        }

        public static Hediff GetHediffByAlias(Pawn pawn, string hediffAlias)
        {
            return GetHediffsByAlias(pawn, hediffAlias)
                .RandomElementWithFallback();
        }

        public static IEnumerable<Hediff> GetHediffsByAlias(Pawn pawn, string hediffAlias)
        {
            List<Hediff> hediffs = pawn.health?.hediffSet?.hediffs;
            if(hediffs == null)
            {
                return new List<Hediff>();
            }
            List<string> hediffAliases = hediffAlias.GetAliases("Hediff");
            IEnumerable<Hediff> matchingHediffs = hediffs
                .Where(hed => hediffAliases.ContainsSubstring(hed.def.defName));
            if(RV2Log.ShouldLog(true, "BodyParts"))
                RV2Log.Message($"Called search on {pawn.LabelShort} for hediff {hediffAlias} with aliases {string.Join(", ", hediffAliases)}", true, "BodyParts");
            if(matchingHediffs == null)
            {
                if(RV2Log.ShouldLog(true, "BodyParts"))
                    RV2Log.Message($"No matches", true, "BodyParts", pawn.GetHashCode() + "nomatches".GetHashCode());
                return new List<Hediff>();
            }
            if(RV2Log.ShouldLog(true, "BodyParts"))
                RV2Log.Message($"Matching hediffs: {string.Join(", ", matchingHediffs.Select(h => h.def.label))}", true, "BodyParts", pawn.GetHashCode() + "matches".GetHashCode());
            return matchingHediffs;
        }

        public static List<BodyPartRecord> GetAllAcidVulnerableBodyParts(Pawn pawn)
        {
            List<BodyPartRecord> bodyParts = pawn.health.hediffSet.GetNotMissingParts()
                .ToList()
                .FindAll(part => part.def.IsSkinCovered(part, pawn.health.hediffSet));   // this means we don't apply to organs, bones or mechanical parts
            return bodyParts;
        }

        public static float GetAcidModifier(BodyPartRecord bodyPart)
        {
            int depth = GetBodyPartDepth(bodyPart);
            // allow maximum depth of 5 for 100% acid strength
            depth = Math.Min(depth, 5);
            float acidModifier = depth * 0.2f;
            if(bodyPart.IsCorePart)    // completely destroying the core part causes issues with the pawns body, so don't do it
                acidModifier = 0.9f;
            return acidModifier;
        }

        public static int GetBodyPartDepth(BodyPartRecord bodyPart)
        {
            int depth = 1;
            BodyPartRecord searchPart = bodyPart;
            while(!searchPart.IsCorePart)
            {
                depth++;
                searchPart = searchPart.parent;
            }
            return depth;
        }

        public static bool IsVitalOrHasVitalChildren(BodyPartRecord bodyPart)
        {
            List<BodyPartTagDef> vitalTags = DefDatabase<BodyPartTagDef>.AllDefsListForReading.FindAll(tag => tag.vital);
            return vitalTags.Any(tag => bodyPart.HasChildParts(tag));
        }
    }
}
