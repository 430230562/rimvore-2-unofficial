#if v1_5
using LudeonTK;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2.DebugTools
{
    public static class Reset
    {
        [DebugAction("RimVore-2", "Reset RV2 PawnData", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void HardReset()
        {
            foreach(Pawn predator in GlobalVoreTrackerUtility.ActivePredators)
            {
                Debug_PawnTools.EmergencyEjectAll(predator);
            }
            RV2Mod.RV2Component.HardReset();
        }
        [DebugAction("RimVore-2", "Remove ALL cached VoreInteractions", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void RemoveAllCachedVoreInteractions()
        {
            VoreInteractionManager.ClearCachedInteractions();
        }

        [DebugAction("RimVore-2", "Add Nabbers Preset To Rules", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Entry)]
        public static void AddNabbersPresetEntry()
        {
            string presetName = "Nabbers' Recommendation";
            if(RV2Mod.Settings.rules.Presets.ContainsKey(presetName))
            {
                RV2Mod.Settings.rules.Presets.Remove(presetName);
            }
            RV2Mod.Settings.rules.Presets.Add(RulePresets.NabbersChoice());
        }
        [DebugAction("RimVore-2", "Add Nabbers Preset To Rules", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void AddNabbersPresetPlaying()
        {
            AddNabbersPresetEntry();
        }

        [DebugAction("RimVore-2", "Resync All Vore Hediffs", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void ResyncAllHediffs()
        {
            IEnumerable<Pawn> pawns = Find.CurrentMap?.mapPawns?.AllPawnsSpawned;
            if(pawns.EnumerableNullOrEmpty())
            {
                return;
            }
            foreach(Pawn pawn in pawns)
            {
                if(RV2Log.ShouldLog(true, "Debug"))
                    RV2Log.Message($"Debug resync for pawn {pawn.LabelShort}", false, "Debug");
                pawn.PawnData()?.VoreTracker?.SynchronizeHediffs();
            }
        }

        [DebugAction("RimVore-2", "Nuke RV-2 Settings", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Entry)]
        public static void NukeSettingsEntry()
        {
            GeneralUtility.NukeSettings();
        }
        [DebugAction("RimVore-2", "Nuke RV-2 Settings", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Playing)]
        public static void NukeSettingsPlaying()
        {
            NukeSettingsEntry();
        }
    }
}
