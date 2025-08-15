using System;
using System.Linq;
using System.Reflection;
using RimVore2;
using rjw;
using Verse;

namespace RV2_RJW
{
    public class GenitalAccess : IGenitalAccess
    {
        Assembly rjwAssembly;
        Type sexPartAdderClass;
        MethodInfo add_genitals;
        MethodInfo add_breasts;

        public GenitalAccess()
        {
            rjwAssembly = AppDomain.CurrentDomain.GetAssemblies().Single(assembly => assembly.GetName().Name == "RJW");
            sexPartAdderClass = rjwAssembly.GetTypes().Single(type => type.Name == "SexPartAdder"); // find the private SexPartAdder class
            add_genitals = sexPartAdderClass
                .GetMethods()   // take all methods
                    .Single(method => method.Name == "add_genitals");   // find the add_genitals method
            add_breasts = sexPartAdderClass
                .GetMethods()
                    .Single(method => method.Name == "add_breasts");
        }

        public void AddSexualPart(Pawn pawn, SexualPart part)
        {
            switch (part)
            {
                case SexualPart.Breasts:
                    add_breasts.Invoke(null, new object[] { pawn, null, Gender.Female });
                    break;
                case SexualPart.Penis:
                    add_genitals.Invoke(null, new object[] { pawn, null, Gender.Male });
                    break;
                case SexualPart.Vagina:
                    add_genitals.Invoke(null, new object[] { pawn, null, Gender.Female });
                    break;
            }
        }

        public RimWorld.Need GetSexNeed(Pawn pawn)
        {
            RimWorld.Need sexNeed = pawn.needs?.TryGetNeed<Need_Sex>();
            if(sexNeed == null)
            {
                if(RV2Log.ShouldLog(true, "RJW"))
                    RV2Log.Warning($"Trying to get sex need of pawn {pawn.Label} without sex need.", true, "RJW");
            }
            return sexNeed;
        }

        public bool HasPenis(Pawn pawn)
        {
            return rjw.Genital_Helper.has_penis_fertile(pawn) || rjw.Genital_Helper.has_penis_infertile(pawn);
        }

        public bool HasVagina(Pawn pawn)
        {
            return rjw.Genital_Helper.has_vagina(pawn);
        }

        public bool HasBreasts(Pawn pawn)
        {
            return rjw.Genital_Helper.has_breasts(pawn);
        }

        public bool IsFertile(Pawn pawn)
        {
            return pawn.health?.capacities?.GetLevel(xxx.reproduction) >= 1;
        }

        public bool IsSexuallySatisfied(Pawn pawn)
        {
            // 0.95 is RJW ahegao threshold
            return GetSexNeed(pawn)?.CurLevel >= 0.96f;
        }

        public void PleasurePawn(Pawn pawn, float pleasureValue)
        {
            RimWorld.Need sexNeed = GetSexNeed(pawn);
            if(sexNeed != null)
            {
                sexNeed.CurLevel += pleasureValue;
            }
        }

        /// <summary>
        /// RJW Removed this in the 5.0 version.
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns>1</returns>
        public float GetSexAbility(Pawn pawn)
        {
            return 1;
        }
    }
}
