using System;
using HarmonyLib;
using Verse;

namespace LightGenitals
{
    [HarmonyPatch(typeof(PawnGenerator), "GeneratePawn", new[] { typeof(PawnGenerationRequest) })]
    public static class Patch_PawnGenerator
    {
        [HarmonyPostfix]
        public static void AddGenitals(Pawn __result)
        {
            Pawn pawn = __result;
            if(pawn?.health?.hediffSet == null)
            {
                Log.Warning($"Tried to add genitals for pawn {pawn.LabelShort} without hediffs");
                return;
            }
            try
            {
                switch (pawn.gender)
                {
                    case Gender.None:
                        return;
                    case Gender.Male:
                        if(Rand.Chance(RV2_LG_Settings.lg.MaleChanceToSpawnWithPenis))
                        {
                            GenitalUtility.AddGenitals(pawn, GenitalDefOf.LightGenitals_Penis);
                        }
                        if(Rand.Chance(RV2_LG_Settings.lg.MaleChanceToSpawnWithVagina))
                        {
                            GenitalUtility.AddGenitals(pawn, GenitalDefOf.LightGenitals_Vagina);
                        }
                        if(Rand.Chance(RV2_LG_Settings.lg.MaleChanceToSpawnWithBreasts))
                        {
                            GenitalUtility.AddGenitals(pawn, GenitalDefOf.LightGenitals_Breasts);
                        }
                        GenitalUtility.AddGenitals(pawn, GenitalDefOf.LightGenitals_Anus);
                        break;
                    case Gender.Female:
                        if(Rand.Chance(RV2_LG_Settings.lg.FemaleChanceToSpawnWithPenis))
                        {
                            GenitalUtility.AddGenitals(pawn, GenitalDefOf.LightGenitals_Penis);
                        }
                        if(Rand.Chance(RV2_LG_Settings.lg.FemaleChanceToSpawnWithVagina))
                        {
                            GenitalUtility.AddGenitals(pawn, GenitalDefOf.LightGenitals_Vagina);
                        }
                        if(Rand.Chance(RV2_LG_Settings.lg.FemaleChanceToSpawnWithBreasts))
                        {
                            GenitalUtility.AddGenitals(pawn, GenitalDefOf.LightGenitals_Breasts);
                        }
                        GenitalUtility.AddGenitals(pawn, GenitalDefOf.LightGenitals_Anus);
                        break;
                    default:
                        throw new NotImplementedException("Unknown gender " + pawn.gender);
                }
                EnsurePawnHasGenitals(pawn);
            }
            catch(Exception e)
            {
                Log.Error("LightGenitals threw an error while trying to add genitals to pawn: " + pawn?.LabelShort + " - " + e);
            }
        }

        private static void EnsurePawnHasGenitals(Pawn pawn)
        {
            switch(pawn.gender)
            {
                case Gender.Male:
                    if(RV2_LG_Settings.lg.MaleCanSpawnWithNoGenitals)
                    {
                        return;
                    }
                    if(GenitalUtility.HasGenitals(pawn, GenitalDefOf.LightGenitals_Penis) || GenitalUtility.HasGenitals(pawn, GenitalDefOf.LightGenitals_Vagina))
                    {
                        return;
                    }
                    // if pawn has no genitals, use whichever chance is more likely
                    if(RV2_LG_Settings.lg.MaleChanceToSpawnWithVagina > RV2_LG_Settings.lg.MaleChanceToSpawnWithPenis)
                    {
                        GenitalUtility.AddGenitals(pawn, GenitalDefOf.LightGenitals_Vagina);
                    }
                    else
                    {
                        GenitalUtility.AddGenitals(pawn, GenitalDefOf.LightGenitals_Penis);
                    }
                    return;
                case Gender.Female:
                    if(RV2_LG_Settings.lg.FemaleCanSpawnWithNoGenitals)
                    {
                        return;
                    }
                    if(GenitalUtility.HasGenitals(pawn, GenitalDefOf.LightGenitals_Penis) || GenitalUtility.HasGenitals(pawn, GenitalDefOf.LightGenitals_Vagina))
                    {
                        return;
                    }
                    // if pawn has no genitals, use whichever chance is more likely
                    if(RV2_LG_Settings.lg.FemaleChanceToSpawnWithVagina > RV2_LG_Settings.lg.FemaleChanceToSpawnWithPenis)
                    {
                        GenitalUtility.AddGenitals(pawn, GenitalDefOf.LightGenitals_Vagina);
                    }
                    else
                    {
                        GenitalUtility.AddGenitals(pawn, GenitalDefOf.LightGenitals_Penis);
                    }
                    return;
                default:
                    throw new NotImplementedException("Unknown gender " + pawn.gender);
            }
        }
    }
}