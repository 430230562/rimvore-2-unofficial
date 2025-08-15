using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using rjw;
using RimVore2;

namespace RV2_RJW
{
    [HarmonyPatch(typeof(rjw.Sexualizer), "SexualizeGenderedPawn")]
    public class PatchTemplate
    {
        [HarmonyPostfix]
        private static void AddRV2BackstoryGenitals(Pawn pawn)
        {
            try
            {
                if(!pawn.TryGetRV2Backstory(out RV2_BackstoryDef adultBackstory, out RV2_BackstoryDef childBackstory))
                {
                    return;
                }
                AddSexualPartForBackstory(pawn, adultBackstory);
                AddSexualPartForBackstory(pawn, childBackstory);
            }
            catch(Exception e)
            {
                Log.Warning("RimVore-2: Something went wrong while trying to add RV2Backstory enforced genitalia: " + e);
                return;
            }
        }

        private static void AddSexualPartForBackstory(Pawn pawn, RV2_BackstoryDef backstory)
        {
            // for some reason this throws a compilation error in 1.4, I am truly dumbfounded as to why, going to cover the NULL case in the forcedSexualParts list retrieval
            //if(backstory == null)
            //{
            //    return;
            //}
            if(!RV2Mod.Settings.features.BackstoryGenitalsEnabled)
            {
                return;
            }
            List<SexualPart> forcedParts = backstory?.forcedSexualParts;
            if(forcedParts.NullOrEmpty())
            {
                return;
            }
            foreach(SexualPart part in forcedParts)
            {
                Func<Pawn, bool> partCheck;
                Action<Pawn> partAdder;
                switch (part)
                {
                    case SexualPart.Penis:
                        partCheck = (Pawn p) => GenitalUtility.GenitalAccess.HasPenis(p);
                        partAdder = (Pawn p) => GenitalUtility.GenitalAccess.AddSexualPart(p, SexualPart.Penis);
                        break;
                    case SexualPart.Vagina:
                        partCheck = (Pawn p) => GenitalUtility.GenitalAccess.HasVagina(p);
                        partAdder = (Pawn p) => GenitalUtility.GenitalAccess.AddSexualPart(p, SexualPart.Vagina);
                        break;
                    case SexualPart.Breasts:
                        partCheck = (Pawn p) => GenitalUtility.GenitalAccess.HasBreasts(p);
                        partAdder = (Pawn p) => GenitalUtility.GenitalAccess.AddSexualPart(p, SexualPart.Breasts);
                        break;
                    default:
                        throw new NotImplementedException("Unknown SexualPart: " + part.ToString());
                }
                if(partCheck(pawn))
                {
                    continue;
                }
                partAdder(pawn);
            }
        }
    }
}