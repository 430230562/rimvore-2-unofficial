using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimVore2
{
    public class ThoughtWorker_Precept_RecentKeyword : ThoughtWorker_Precept, IPreceptCompDescriptionArgs
    {
        PreceptThoughtWorker_ListeningKeyword Extension => def?.GetModExtension<PreceptThoughtWorker_ListeningKeyword>();

        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            if(Extension?.keyword == null)
            {
                RV2Log.Error("ThoughtWorker_Precept_RecentKeyword does not have a keyword modExtensin: " + def.defName);
                return false;
            }
            IdeologyPawnData data = p.PawnData()?.Ideology;
            if(data == null)
            {
                return false;
            }
            return data.KeywordExpired(Extension.keyword, Extension.maxTimeSinceLast);
        }
        public IEnumerable<NamedArgument> GetDescriptionArgs()
        {
            string value = Extension?.maxTimeSinceLast.ToString();
            if(value == null)
            {
                value = "?";
            }
            yield return value.Named("VORERELATEDINTERVAL");
            yield break;
        }
    }
}