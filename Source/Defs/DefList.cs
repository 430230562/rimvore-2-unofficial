using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace RimVore2
{
    // TODO_Maybe might wanna make this abstract with a type in the def itself and all that, but I don't wanna bother with just one single type of list
    public class DefList_PawnRelationDef : Def
    {
        public List<PawnRelationDef> relations;
    }
}
