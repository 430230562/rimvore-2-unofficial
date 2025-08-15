using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace LightGenitals
{
    [DefOf]
    public static class GenitalDefOf
    {
        static GenitalDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(GenitalDefOf));
        }
        public static HediffDef LightGenitals_Penis;
        public static HediffDef LightGenitals_Vagina;
        public static HediffDef LightGenitals_Breasts;
        public static HediffDef LightGenitals_Anus;
    }

    [DefOf]
    public static class BodyPartDefOf
    {
        static BodyPartDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(BodyPartDefOf));
        }
        public static BodyPartDef Genitals;
        public static BodyPartDef Chest;
        public static BodyPartDef Anus;
    }
}
