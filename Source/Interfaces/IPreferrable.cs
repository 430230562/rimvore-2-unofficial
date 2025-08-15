using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    interface IPreferrable
    {
        float GetPreference(Pawn pawn, VoreRole role, ModifierOperation modifierOperation = ModifierOperation.Add);
        string GetName();
    }
}
