using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace RimVore2
{
    public static class RV2FoodUtility
    {
        public static void AddFood(this Pawn pawn, float value)
        {
            Need_Food foodNeed = pawn.needs?.TryGetNeed<Need_Food>();
            if(foodNeed == null)
            {
                return;
            }
            foodNeed.CurLevel += value;
        }

        public static void SetFoodNeed(this Pawn pawn, float value)
        {
            Need_Food foodNeed = pawn.needs?.TryGetNeed<Need_Food>();
            if(foodNeed == null)
            {
                return;
            }
            foodNeed.CurLevel = value;
        }
    }
}
