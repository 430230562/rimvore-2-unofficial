using System;
using Verse;
using RimVore2;
using RimWorld;

namespace LightGenitals
{
    public class GenitalAccess : IGenitalAccess
    {
        // --------------------
        public void AddSexualPart(Pawn pawn, SexualPart part)
        {
            HediffDef partDef;
            switch (part)
            {
                case SexualPart.Breasts:
                    partDef = GenitalDefOf.LightGenitals_Breasts;
                    break;
                case SexualPart.Penis:
                    partDef = GenitalDefOf.LightGenitals_Penis;
                    break;
                case SexualPart.Vagina:
                    partDef = GenitalDefOf.LightGenitals_Vagina;
                    break;
                default:
                    throw new NotImplementedException();
            }
            GenitalUtility.AddGenitals(pawn, partDef);
        }

        public float GetSexAbility(Pawn pawn)
        {
            return -1f;
        }

        public Need GetSexNeed(Pawn pawn)
        {
            return null;
        }

        public bool HasBreasts(Pawn pawn)
        {
            return GenitalUtility.HasGenitals(pawn, GenitalDefOf.LightGenitals_Breasts);
        }

        public bool HasPenis(Pawn pawn)
        {
            return GenitalUtility.HasGenitals(pawn, GenitalDefOf.LightGenitals_Penis);
        }

        public bool HasVagina(Pawn pawn)
        {
            return GenitalUtility.HasGenitals(pawn, GenitalDefOf.LightGenitals_Vagina);
        }

        public bool IsFertile(Pawn pawn)
        {
            return false;
        }

        public bool IsSexuallySatisfied(Pawn pawn)
        {
            return false;
        }

        public void PleasurePawn(Pawn pawn, float value)
        {
            return;
        }
    }
}
