using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    [HarmonyPatch(typeof(SocialCardUtility), nameof(SocialCardUtility.PawnsForSocialInfo))]
    public static class Patch_SocialCardUtility
    {
        [HarmonyPostfix]
        public static void AddVoredPawnsForMap(Pawn pawn, List<Pawn> __result)
        {
            if(pawn.MapHeld == null)
            {
                return;
            }
            IEnumerable<Pawn> voredPawnsOnMap = GlobalVoreTrackerUtility.ActivePreyWithRecord.Keys.Where(p => p.MapHeld == pawn.MapHeld);
            __result.AddRange(voredPawnsOnMap);
        }
    }
}