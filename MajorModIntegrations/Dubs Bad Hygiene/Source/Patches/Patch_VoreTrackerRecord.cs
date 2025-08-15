using DubsBadHygiene;
using HarmonyLib;
using RimVore2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RV2_DBH
{
    [HarmonyPatch(typeof(VoreTrackerRecord), "PreventPreyFromStarving")]
    internal static class Patch_VoreTrackerRecord
    {
        [HarmonyPostfix]
        public static void AddDehydrationPrevention(Pawn pawn)
        {
            try
            {
                if(pawn.Dead)
                {
                    return;
                }
                Need_Thirst need = pawn.needs?.TryGetNeed<Need_Thirst>();
                if(need == null)
                {
                    return;
                }
                if(need.CurLevel == 0)
                {
                    if(RV2Log.ShouldLog(false, "OngoingVore"))
                        RV2Log.Message($"Preventing prey {pawn.LabelShort} from dehydrating.", "OngoingVore");
                    need.CurLevel = 0.5f;
                }
            }
            catch (Exception e)
            {
                Log.Error("Something went wrong when trying to check for toilets on the current dump spot: " + e);
            }
        }
    }
}
