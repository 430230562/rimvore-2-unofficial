using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class QuirkFactor_ValueModifier : QuirkFactor, IExposable
    {
        string modifierName;

        public override bool CanGetFactor(Pawn pawn)
        {
            QuirkManager pawnQuirks = pawn.QuirkManager(false);
            if(pawnQuirks == null)
            {
                return false;
            }
            if(!pawnQuirks.HasValueModifier(modifierName))
            {
                return false;
            }
            return true;
        }

        public override float GetFactor(Pawn pawn, ModifierOperation operation)
        {
            QuirkManager quirks = pawn.QuirkManager(false);
            if(quirks == null)
            {
                return 1;
            }
            quirks.TryGetValueModifier(modifierName, operation, out float factor);
            //Log.Message("Retrieving modifier " + modifierName + " for " + pawn.LabelShort + ": " + factor);
            return factor;
        }

        public override float ModifyValue(Pawn pawn, float value)
        {
            QuirkManager quirks = pawn.QuirkManager(false);
            if(quirks == null)
            {
                return value;
            }
            return quirks.ModifyValue(modifierName, value);
        }

        public override string GetDescription(Pawn pawn)
        {
            IEnumerable<Quirk> factorQuirks = pawn.QuirkManager()?.ActiveQuirks
                .Where(quirk => quirk.def.HasValueModifiersFor(modifierName));
            if(factorQuirks.EnumerableNullOrEmpty())
            {
                return "ERR: NO QUIRKS";
            }
            List<string> descriptions = new List<string>();
            List<ModifierOperation> modifierOperations = GeneralUtility.GetValues<ModifierOperation>();
            foreach(Quirk factorQuirk in factorQuirks)
            {
                foreach(ModifierOperation operation in modifierOperations)
                {
                    if(factorQuirk.def.TryGetValueModifierFor(modifierName, operation, out float factor))
                    {
                        descriptions.Add(
                            "    " +
                            factorQuirk.def.label +
                            ": " +
                            operation.OperationSymbol() +
                            " " +
                            factor.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Absolute)
                        );
                    }
                }
            }
            return string.Join("\n", descriptions);
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref modifierName, "modifierName");
        }

        public override IEnumerable<string> ConfigErrors()
        {
            if(modifierName == null)
            {
                yield return "Required field \"modifierName\" is not set";
            }
        }
    }
}
