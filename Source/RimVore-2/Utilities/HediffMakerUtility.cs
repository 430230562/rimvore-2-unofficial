using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public static class HediffMakerUtility
    {
        //public static T MakeHediffForPart<T>(HediffDef hediffDef, Pawn pawn, BodyPartRecord appliedBodyPart) where T : Hediff, new() {
        //    T hediff = new T()
        //    {
        //        def = hediffDef,
        //        pawn = pawn,
        //        Part = appliedBodyPart,
        //        loadID = Find.UniqueIDsManager.GetNextHediffID()
        //    };
        //    hediff.PostMake();
        //    return hediff;
        //}

        public static void AddHediffForPart(HediffDef hediffDef, Pawn pawn, string bodyPartName)
        {
            BodyPartRecord appliedBodyPart;
            if(bodyPartName != null)
            {
                appliedBodyPart = BodyPartUtility.GetBodyPartByName(pawn, bodyPartName);
            }
            else
            {
                appliedBodyPart = null;
                RV2Log.Warning("Hediff creation for body part with name could not find a body part to apply to, the hediff will be ablied to the whole body!");
            }
            pawn.health.AddHediff(hediffDef, appliedBodyPart);
        }
    }
}
