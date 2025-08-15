#if v1_5
using LudeonTK;
#endif
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public static class Misc
    {
        //[DebugAction("RimVore-2", "LogStuff", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        //private static void LogAtMouse()
        //{
        //    IntVec3 clickPos = UI.MouseCell();
        //    Map map = Find.CurrentMap;
        //    clickPos.GetThingList(map).ForEach(thing =>
        //    {
        //        Log.Message($"Can ever sell {thing.Label} ? {TradeUtility.EverPlayerSellable(thing.def)}");
        //    });
        //}

        //[DebugAction("RimVore-2", "Log stuff", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        //public static void Logggg(Pawn p)
        //{
        //    Log.Message($"{p} has nutrition: {VoreCalculationUtility.CalculatePreyNutrition(p)}");
        //}


        [DebugAction("RimVore-2", "Toggle Verbose Warning", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ToggleVerbose()
        {
            RV2Mod.Settings.debug.EnableVerboseLoggingWarning = !RV2Mod.Settings.debug.EnableVerboseLoggingWarning;
            if(!RV2Mod.Settings.debug.EnableVerboseLoggingWarning)
            {
                Log.Warning($"I fucking hope you know what you are doing");
            }
        }

        [DebugAction("RimVore-2", "Log PawnData state", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void LogStuff()
        {
            IEnumerable<PawnData> data = Current.Game.GetComponent<RV2Component>().AllPawnData;
            string message = $@"PawnData dictionary size: {data.Count()}
Tracked predators: {GlobalVoreTrackerUtility.ActivePredators.Count}
Tracked prey: {GlobalVoreTrackerUtility.ActivePreyWithRecord.Count}
Vore trackers: {GlobalVoreTrackerUtility.ActiveVoreTrackers.Count}
Exact trackers: 
{string.Join("\n", GlobalVoreTrackerUtility.ActiveVoreTrackers.Select(t => TrackerToString(t)))}
";

            Log.Message(message);
            message = $@"
PawnData entries:
{ string.Join("\n", data.Select(pd => pd.Pawn == null ? $"NULL! DEBUG: {pd.debug_pawnName}" : $"{pd.Pawn.LabelShort} - {pd.Pawn.GetUniqueLoadID()}"))}
";
            Log.Message(message);

            string TrackerToString(VoreTracker tracker)
            {
                string msg;
                if(tracker.pawn == null)
                {
                    msg = $"NULL! DEBUG: {tracker.debug_pawnName}";
                }
                else
                {
                    msg = $"{tracker.pawn.LabelShort} - {tracker.pawn.GetUniqueLoadID()}";
                }
                if(!tracker.IsTrackingVore)
                {
                    msg += " - not tracking vore";
                    return msg;
                }
                foreach(VoreTrackerRecord record in tracker.VoreTrackerRecords)
                {
                    msg += $"\n - {record.Prey.LabelShort} ({record.Prey})";
                }
                return msg;
            }
        }
    }
}
