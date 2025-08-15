using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace RimVore2
{
    public static class QuirkUtility
    {
        public const int ObsessedPreferenceValue = 4;
        public static QuirkManager QuirkManager(this Pawn pawn, bool initializeIfNull = true)
        {
            if(!RV2Mod.Settings.features.VoreQuirksEnabled)
            {
                return null;
            }
            PawnData pawnData = pawn.PawnData(initializeIfNull);
            if(pawnData == null)
            {
                return null;
            }
            return pawnData.QuirkManager(initializeIfNull);
        }

        public static QuirkPoolDef GetPool(this QuirkDef quirk)
        {
            // Log.Message(string.Join(", ", RV2_Common.SortedQuirkPools.ConvertAll(q => q.defName)));
            return RV2_Common.SortedQuirkPools.Find(pool => pool.quirks.Contains(quirk));
        }

        public static bool IsEnabled(this QuirkDef quirk)
        {
            return RV2Mod.Settings.quirks.IsQuirkEnabled(quirk);
        }

        public static bool IsEnabled(this QuirkPoolDef pool)
        {
            return RV2Mod.Settings.quirks.IsPoolEnabled(pool);
        }

        /// <summary>
        /// Adapter method for Settings_Quirks. Factors in potentially set overrides of rarity by the user
        /// </summary>
        /// <param name="quirk">Quirk to retrieve rarity for</param>
        /// <returns>Default quirk rarity or whatever the user has set as overwritten rarity</returns>
        public static QuirkRarity GetRarity(this QuirkDef quirk)
        {
            return RV2Mod.Settings.quirks.GetRarity(quirk);
        }

        public static bool CanRemoveQuirkWithoutConflict(Pawn pawn, Quirk quirk, List<Quirk> existingQuirks, out string reason)
        {
            List<QuirkDef> otherQuirksRequiringQuirk = existingQuirks
                .FindAll(q => q.def.requiredQuirks.Contains(quirk.def))
                .ConvertAll(q => q.def);
            if(!otherQuirksRequiringQuirk.NullOrEmpty())
            {
                string requiringQuirksString = string.Join(", ", otherQuirksRequiringQuirk.ConvertAll(q => q.label));
                reason = "RV2_QuirkInvalid_Reason_WouldRemoveRequiredQuirk".Translate(quirk.def.label, requiringQuirksString);
                return false;
            }
            if(pawn.GetForcedQuirksFromBackstory().Contains(quirk.def))
            {
                reason = "RV2_QuirkInvalid_Reason_WouldRemoveBackstoryForcedQuirk".Translate(quirk.def.label);
                return false;
            }
            if(pawn.GetForcedQuirksByHediffs().Contains(quirk.def))
            {
                reason = "RV2_QuirkInvalid_Reason_WouldRemoveHediffForcedQuirk".Translate(quirk.def.label);
                return false;
            }
            reason = null;
            return true;
        }

        public static List<QuirkDef> GetAllForcedQuirks(this Pawn pawn)
        {
            List<QuirkDef> quirks = new List<QuirkDef>();
            quirks.AddRange(pawn.GetForcedQuirksByHediffs());
            quirks.AddRange(pawn.GetForcedQuirksFromBackstory());
            return quirks;
        }

        public static List<QuirkDef> GetAllBlockedQuirks(this Pawn pawn)
        {
            List<QuirkDef> quirks = new List<QuirkDef>();
            quirks.AddRange(pawn.GetBlockedQuirksFromBackstory());
            return quirks;
        }

        public static List<QuirkDef> GetForcedQuirksByHediffs(this Pawn pawn)
        {
            IEnumerable<QuirkDef> forcedQuirksByHediffs = pawn.health?.hediffSet?
                    .GetAllComps()?
                    .Where(comp => comp is HediffComp_QuirkForcer)?
                    .Cast<HediffComp_QuirkForcer>()?
                    .SelectMany(comp => comp.ForcedQuirks);
            if(forcedQuirksByHediffs == null)
            {
                return new List<QuirkDef>();
            }
            return forcedQuirksByHediffs.ToList();
        }

        public static bool HasAllQuirks(Pawn pawn, List<QuirkDef> requiredQuirks, bool generateQuirkManager = false)
        {
            QuirkManager quirks = pawn.QuirkManager(generateQuirkManager);
            if(quirks == null)
            {
                return false;
            }
            return requiredQuirks
                .TrueForAll(quirk => quirks.HasQuirk(quirk));
        }

        public static void ForceVorenappingQuirks(Pawn pawn)
        {
            QuirkManager predatorQuirks = pawn.QuirkManager();
            // predator can't have quirks, oh well
            if(predatorQuirks == null)
            {
                return;
            }
            predatorQuirks.TryPostInitAddQuirk(QuirkDefOf.Enablers_Core_Type_Oral, out _);
            predatorQuirks.TryPostInitAddQuirk(QuirkDefOf.Enablers_Core_Goal_Longendo, out _);
        }

        private static Dictionary<HediffDef, bool> cachedVoreEnablerHediffs = new Dictionary<HediffDef, bool>();
        public static bool HasVoreEnabler(this HediffDef hediff)
        {
            if(!cachedVoreEnablerHediffs.ContainsKey(hediff))
            {
                bool isEnabler = hediff.comps? // take the hediff comps
                    .Where(hedComp => hedComp is HediffCompProperties_QuirkForcer)  // scan for quirk forcers
                    .Cast<HediffCompProperties_QuirkForcer>()   // cast to quirk forcer
                    .Where(hedComp => !hedComp.quirks.NullOrEmpty())    // safety to make sure we don't grab NULL objects
                    .SelectMany(hedComp => hedComp.quirks)  // retrieve the list of quirks forced by the hediff
                    .Any(quirk => quirk.HasComp<QuirkComp_VoreEnabler>()) == true;  // check if any quirk has the ability of enabling vore
                cachedVoreEnablerHediffs.Add(hediff, isEnabler);
            }
            return cachedVoreEnablerHediffs.TryGetValue(hediff, false);
        }
    }
}
