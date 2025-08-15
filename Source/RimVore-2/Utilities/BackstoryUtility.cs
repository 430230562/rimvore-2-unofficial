using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public static class BackstoryUtility
    {
        public static void UpdateAllBackstoryDescriptions()
        {
            bool scatEnabled = RV2Mod.Settings.features.ScatEnabled;
            bool bonesEnabled = RV2Mod.Settings.features.BonesEnabled;
            List<RV2_BackstoryDef> backstories = DefDatabase<RV2_BackstoryDef>.AllDefsListForReading;
            foreach(RV2_BackstoryDef backstory in backstories)
            {
                backstory.UpdateDescription(scatEnabled, bonesEnabled);
            }
        }

        public static bool TryGetRV2Backstory(this Pawn pawn, out RV2_BackstoryDef rv2AdultBackstory, out RV2_BackstoryDef rv2ChildBackstory)
        {
            string adultIdentifier = pawn.story?.Adulthood?.identifier;
            string childIdentifier = pawn.story?.Childhood?.identifier;
            rv2AdultBackstory = null;
            rv2ChildBackstory = null;
            if(adultIdentifier != null)
            {
                rv2AdultBackstory = DefDatabase<RV2_BackstoryDef>.GetNamedSilentFail(adultIdentifier);
            }
            if(childIdentifier != null)
            {
                rv2ChildBackstory = DefDatabase<RV2_BackstoryDef>.GetNamedSilentFail(childIdentifier);
            }
            if(rv2ChildBackstory == null && rv2AdultBackstory == null)
            {
                return false;
            }
            return true;
        }

        public static List<QuirkDef> GetForcedQuirksFromBackstory(this Pawn pawn)
        {
            if(pawn.TryGetRV2Backstory(out RV2_BackstoryDef adult, out RV2_BackstoryDef child))
            {
                List<QuirkDef> quirks = new List<QuirkDef>();
                if(adult != null)
                {
                    quirks.AddRange(adult.ForcedQuirks);
                }
                if(child != null)
                {
                    quirks.AddRange(child.ForcedQuirks);
                }
                return quirks;
            }
            return new List<QuirkDef>();
        }

        public static List<QuirkDef> GetBlockedQuirksFromBackstory(this Pawn pawn)
        {
            if(pawn.TryGetRV2Backstory(out RV2_BackstoryDef adult, out RV2_BackstoryDef child))
            {
                List<QuirkDef> quirks = new List<QuirkDef>();
                if(adult != null)
                {
                    quirks.AddRange(adult.BlockedQuirks);
                }
                if(child != null)
                {
                    quirks.AddRange(child.BlockedQuirks);
                }
                return quirks;
            }
            return new List<QuirkDef>();
        }
    }
}
