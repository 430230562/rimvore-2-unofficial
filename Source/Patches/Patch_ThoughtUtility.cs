using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimVore2
{
    /// <summary>
    /// This patch modifies the list of all situational thoughts for *all* pawns, 
    /// the pawn-based thought replacement is in Patch_SituationalThoughtHandler
    /// 
    /// The actual Reset is being called too early to patch it directly, so we re-call the Reset method once the game loads
    /// </summary>
    [HarmonyPatch(typeof(ThoughtUtility), "Reset")]
    public class Patch_ThoughtUtility
    {
        private static IEnumerable<ThoughtDef> allSituationalOverrideThoughts;

        [HarmonyPostfix]
        private static void ReplaceOverridenThoughts()
        {
            try
            {
                // first make sure we have all the situational thoughts registered, this only happens once
                if(allSituationalOverrideThoughts == null)
                    InitOverrideSituationalThoughts();
                // then remove all override thoughts from the list of thoughts that the game calculates the state for
                // we don't want overrides to be calculated by default, only for pawns where the override is applicable
                foreach(ThoughtDef overrideThought in allSituationalOverrideThoughts)
                {
                    ThoughtUtility.situationalSocialThoughtDefs.Remove(overrideThought);
                    ThoughtUtility.situationalNonSocialThoughtDefs.Remove(overrideThought);
                }
            }
            catch(Exception e)
            {
                Log.Warning("RimVore-2: Something went wrong " + e);
                return;
            }
        }

        private static void InitOverrideSituationalThoughts()
        {
            // go through all quirks and retrieve all the override thoughts that they can apply
            allSituationalOverrideThoughts = DefDatabase<QuirkDef>.AllDefsListForReading   // get all quirks
                .SelectMany(quirk => quirk.GetComps<QuirkComp_ThoughtOverride>())   // get all override comps of all quirks
                .Select(compOverride => compOverride.overrideThought)   // get the override thought
                .Where(thought => thought.IsSituational);   // filter to only situational thoughts

            if(RV2Log.ShouldLog(false, "Thoughts"))
                RV2Log.Message($"Calculated situational override thoughts: {string.Join(", ", allSituationalOverrideThoughts.Select(t => t.defName))}", "Thoughts");
        }
    }
}