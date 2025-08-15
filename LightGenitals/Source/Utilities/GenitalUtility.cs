using Verse;

namespace LightGenitals
{
    public static class GenitalUtility
    {
        public static void AddGenitals(Pawn pawn, HediffDef hediffDef)
        {
            if(HasGenitals(pawn, hediffDef))
            {
                return;
            }
            BodyPartDef partDef = null;
            if(hediffDef == GenitalDefOf.LightGenitals_Anus)
            {
                partDef = BodyPartDefOf.Anus;
            }
            else if(hediffDef == GenitalDefOf.LightGenitals_Breasts)
            {
                partDef = BodyPartDefOf.Chest;
            }
            else if(hediffDef == GenitalDefOf.LightGenitals_Vagina || hediffDef == GenitalDefOf.LightGenitals_Penis)
            {
                partDef = BodyPartDefOf.Genitals;
            }
            BodyPartRecord part = null;
            if(partDef != null)
            {
                part = pawn.RaceProps?.body?.GetPartsWithDef(partDef)?.RandomElement();
            }
            if(part == null)
            {
                part = pawn.RaceProps?.body?.corePart;
            }
            Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn, part);
            hediff.PostMake();
            pawn.health?.AddHediff(hediff, part);
        }

        public static bool HasGenitals(Pawn pawn, HediffDef hediffDef)
        {
            if(pawn.health == null)
            {
                return false;
            }
            if(pawn.health.hediffSet == null)
            {
                return false;
            }
            return pawn.health.hediffSet.HasHediff(hediffDef);
        }
    }
}
