using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace RimVore2
{
    public interface IGenitalAccess
    {
        void AddSexualPart(Pawn pawn, RimVore2.SexualPart part);

        /// <summary>
        /// RJW removed this in the 5.0 version.
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns>1</returns>
        [Obsolete("Removed by RJW in 5.0, does not yield useful results, do not use")]
        float GetSexAbility(Pawn pawn);
        Need GetSexNeed(Pawn pawn);
        bool HasBreasts(Pawn pawn);
        bool HasPenis(Pawn pawn);
        bool HasVagina(Pawn pawn);
        bool IsFertile(Pawn pawn);
        bool IsSexuallySatisfied(Pawn pawn);
        void PleasurePawn(Pawn pawn, float value);
    }
}
