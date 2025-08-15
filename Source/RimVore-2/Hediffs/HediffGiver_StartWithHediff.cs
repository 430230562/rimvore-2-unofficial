using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class HediffGiver_StartWithHediff : HediffGiver
    {
        public float chance = 1f;
        public float maleCommonality = 1f;
        public float femaleCommonality = 1f;
        public string bodyPartName;

        public new void TryApply(Pawn pawn, List<Hediff> outAddedHediffs)
        {
            if(!Rand.Chance(chance))
            {
                return;
            }
            if(pawn.gender == Gender.Male && !Rand.Chance(maleCommonality))
            {
                return;
            }
            if(pawn.gender == Gender.Female && !Rand.Chance(femaleCommonality))
            {
                return;
            }
            // retrieve all body parts on pawn that match alias, then convert to usable list of BodyPartDef
            partsToAffect = BodyPartUtility.GetBodyPartsByName(pawn, bodyPartName)
                .Select(part => part.def)
                .Distinct() // eliminate duplicates
                .ToList();  // base field is List, not IEnumerable
            base.TryApply(pawn, outAddedHediffs);
        }
    }
}