using LudeonTK;
using System;
using Verse;

namespace LightGenitals
{
    public static class DebugActions
    {

        [DebugAction("RimVore-2", "Apply Genital Hediffs to all pawns on Map", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ApplyGenitalsToPawnsOnMap()
        {
            foreach(Pawn pawn in Current.Game.CurrentMap.mapPawns.AllPawns)
            {
                try
                {
                    if(pawn.gender != Gender.None && pawn.health?.hediffSet?.HasHediff(GenitalDefOf.LightGenitals_Anus) == false)
                    {
                        Patch_PawnGenerator.AddGenitals(pawn);
                    }
                }
                catch(Exception e)
                {
                    Log.Error($"Could not add genitals to pawn {pawn?.LabelShort}: {e}");
                }
            }
        }
    }
}
