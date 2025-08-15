using LudeonTK;
using RimWorld;
using System;
using Verse;

namespace RimVore2
{
    public static class VoreCalculationUtility
    {
        [DebugAction("RimVore-2", "Log prey nutrition", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void CallGrappleDefense(Pawn p)
        {
            CalculatePreyNutrition(p);
        }

        public static float CalculatePreyNutrition(Pawn prey, Pawn predator = null)
        {
            float totalNutrition;
            string reportString = $"{prey.LabelShort} ({prey.def.label}) has this much nutrition: ";
            ThingDef foodDef = FoodUtility.GetFinalIngestibleDef(prey);
            if(foodDef != null)
            {
                totalNutrition = FoodUtility.GetNutrition(predator, prey, foodDef);
                reportString += $"FinalIngestibleDef: {totalNutrition}";
            }
            else
            {
                float meatAmount = prey.GetStatValue(StatDefOf.MeatAmount, true);
                ThingDef meatDef = prey.def?.race?.meatDef;
                if(meatAmount > 0 && meatDef != null)
                {
                    float meatNutrition = meatDef.GetStatValueAbstract(StatDefOf.Nutrition);
                    totalNutrition = meatAmount * meatNutrition;
                    reportString += $"Manual calculation: {meatAmount} (amount) * {meatNutrition} (nutrition) = {totalNutrition}";
                }
                else
                {
                    totalNutrition = RV2Mod.Settings.cheats.FallbackNutritionValue;
                    RV2Log.Error($"RimVore-2 was unable to determine how much nutrition pawn {prey.LabelShort} would have, returning default fallback: {totalNutrition}");
                    reportString += $"Fallback: {totalNutrition}";
                }
            }

            // quirk influence on nutrition
            QuirkManager preyQuirks = prey.QuirkManager(false);
            if(preyQuirks != null)
            {
                reportString += $" - Prey quirks modify from {totalNutrition}";
                totalNutrition = preyQuirks.ModifyValue("PreyNutritionMultiplierAsPrey", totalNutrition);
                reportString += $" to {totalNutrition}";
            }
            QuirkManager predatorQuirks = predator?.QuirkManager(false);
            if(predatorQuirks != null)
            {
                reportString += $" - Predator quirks modify from {totalNutrition}";
                totalNutrition = predatorQuirks.ModifyValue("PreyNutritionMultiplierAsPredator", totalNutrition);
                reportString += $" to {totalNutrition}";
            }
            reportString += $" | Final nutrition value: {totalNutrition}";
            if(RV2Log.ShouldLog(false, "NutritionCalculation"))
                RV2Log.Message(reportString, false, "NutritionCalculation");
            return totalNutrition;
        }

        public static float GetSizeInRelationTo(this Pawn pawn1, Pawn pawn2)
        {
            if(pawn1 == null || pawn2 == null)
            {
                RV2Log.Warning("Either pawn for size difference calculation was null, forcing modifier 1f!");
                return 1f;
            }
            return pawn1.BodySize / pawn2.BodySize;
        }

        public static JobDef GetInitJobDefFor(this VoreRole role)
        {
            switch(role)
            {
                case VoreRole.Predator:
                    return VoreJobDefOf.RV2_VoreInitAsPredator;
                case VoreRole.Prey:
                    return VoreJobDefOf.RV2_VoreInitAsPrey;
                case VoreRole.Feeder:
                    return VoreJobDefOf.RV2_VoreInitAsFeeder;
                default:
                    return null;
            }
        }

        public static float DefaultModifierValue(this ModifierOperation operation)
        {
            switch(operation)
            {
                case ModifierOperation.Add:
                case ModifierOperation.Subtract:
                    return 0f;
                case ModifierOperation.Multiply:
                case ModifierOperation.Divide:
                    return 1f;
                default:
                    throw new Exception("Unknown operation \"" + operation + "\" to provide default modifier for");
            }
        }

        public static string OperationSymbol(this ModifierOperation operation)
        {
            switch(operation)
            {
                case ModifierOperation.Add: return "+";
                case ModifierOperation.Subtract: return "-";
                case ModifierOperation.Multiply: return "x";
                case ModifierOperation.Divide: return "/";
                case ModifierOperation.Set: return "=";
                default:
                    throw new Exception("Unknown operation \"" + operation + "\" to provide symbol for");
            }
        }

        public static Func<float, float, float> AggregationOperation(this ModifierOperation operation)
        {
            switch(operation)
            {
                case ModifierOperation.Add:
                case ModifierOperation.Subtract:
                    return (x, y) => x + y;
                case ModifierOperation.Multiply:
                    return (x, y) => x * y;
                case ModifierOperation.Divide:
                    return (x, y) => x / y;
                case ModifierOperation.Set:
                    return (x, y) => y;
                default:
                    throw new Exception("Unknown operation \"" + operation + "\" to aggregate roll modifiers with");
            }
        }
        public static float Aggregate(this ModifierOperation type, float arg1, float arg2)
        {
            Func<float, float, float> operation = type.AggregationOperation();
            return operation(arg1, arg2);
        }
    }
}
