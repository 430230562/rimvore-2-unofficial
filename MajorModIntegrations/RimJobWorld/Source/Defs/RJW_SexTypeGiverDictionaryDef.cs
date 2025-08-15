using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using rjw;
using RimVore2;
using rjw.Modules.Interactions.Enums;
using Verse;
using static rjw.xxx;

namespace RV2_RJW
{
    public class RJW_SexTypeGiverDictionaryDef : Def
    {
        List<SexTypeGiverEntry> entries = new List<SexTypeGiverEntry>();

        public void DetermineGiver(SexProps sexProps, Pawn initiator, Pawn target, out Pawn giver, out Pawn receiver)
        {
            giver = null;
            receiver = null;
            SexTypeGiverEntry entry = entries.Find(e => e.AppliesTo(sexProps.sexType));
            if(entry == null)
            {
                if(RV2Log.ShouldLog(false, "RJW"))
                    RV2Log.Message("Sex Type not found, no giver to determine", "RJW");
                return;
            }
            bool isReverse = rjw.Modules.Interactions.Helpers.InteractionHelper
                .GetWithExtension(sexProps.dictionaryKey)
                .HasInteractionTag(InteractionTag.Reverse);
            if(isReverse)
            {
                entry.DetermineGiver(target, initiator, out giver, out receiver);
            }
            else
            {
                entry.DetermineGiver(initiator, target, out giver, out receiver);
            }
        }

        private class SexTypeGiverEntry
        {
            GiverType giverType;
            List<xxx.rjwSextype> sexTypes = new List<xxx.rjwSextype>();

            public bool AppliesTo(rjwSextype sexType)
            {
                return sexTypes.Contains(sexType);
            }

            public void DetermineGiver(Pawn initiator, Pawn target, out Pawn giver, out Pawn receiver)
            {
                switch (giverType)
                {
                    case GiverType.Initiator:
                        giver = initiator;
                        receiver = target;
                        return;
                    case GiverType.Receiver:
                        giver = target;
                        receiver = initiator;
                        return;
                    case GiverType.Mutual:
                        if(Rand.Chance(0.5f))
                        {
                            giver = initiator;
                            receiver = target;
                        }
                        else
                        {
                            giver = target;
                            receiver = initiator;
                        }
                        return;
                    default:
                        if(RV2Log.ShouldLog(false, "RJW"))
                            RV2Log.Message("Invalid GiverType, no giver to determine", "RJW");
                        giver = null;
                        receiver = null;
                        return;
                }
            }

            private enum GiverType
            {
                None,
                Initiator,
                Receiver,
                Mutual
            }
        }

    }
}
