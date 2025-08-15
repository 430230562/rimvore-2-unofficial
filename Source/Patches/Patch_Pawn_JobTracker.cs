using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace RimVore2
{
    /// <summary>
    /// Stops the job logic from interrupting grapple interactions
    /// </summary>
    [HarmonyPatch(typeof(Pawn_JobTracker), "CheckForJobOverride")]
    public static class Patch_Pawn_JobTracker
    {
        [HarmonyPrefix]
        private static bool InterceptJobSearchDuringVoreGrapple(Pawn ___pawn)
        {
            try
            {
                if(!CombatUtility.IsInvolvedInGrapple(___pawn))
                {
                    return true;
                }
                return false;
            }
            catch(Exception e)
            {
                Log.Error("Something went wrong trying to intercept the job logic during vore grapples: " + e);
                return true;
            }
        }
    }
}
