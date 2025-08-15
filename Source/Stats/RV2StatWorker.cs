using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class RV2StatWorker : StatWorker
    {
        RV2StatDef RV2Stat => (RV2StatDef)stat;

        public override bool IsDisabledFor(Thing thing)
        {
            if(base.IsDisabledFor(thing))
            {
                return true;
            }
            if(!(thing is Pawn pawn))
            {
                return true;
            }
            //if(pawn.QuirkManager(false) == null)
            //{
            //    return true;
            //}
            return false;
        }

        public override bool ShouldShowFor(StatRequest req)
        {
            bool isVisible = base.ShouldShowFor(req)
                && req.Thing is Pawn
                && !IsDisabledFor(req.Thing);
            //Log.Message("should show stat for " + req.Thing + "? : " + isVisible);
            return isVisible;
        }

        public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
        {
            float statValue = base.GetValueUnfinalized(req, applyPostProcess);
            Pawn pawn = (Pawn)req.Thing;
            if(pawn.QuirkManager(false) == null)
            {
                return statValue;
            }
            IEnumerable<QuirkFactor> quirkFactors = RV2Stat.quirkFactors;
            if(!quirkFactors.EnumerableNullOrEmpty())
            {
                foreach(QuirkFactor quirkFactor in quirkFactors)
                {
                    statValue = quirkFactor.ModifyValue(pawn, statValue);
                    //statValue *= quirkFactor.GetFactor(pawn, ModifierOperation.Multiply);
                }
            }
            return statValue;
        }

        public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
        {
            string description = base.GetExplanationUnfinalized(req, numberSense);
            if(!(req.Thing is Pawn pawn))
            {
                return description;
            }
            if(RV2Stat.quirkFactors.NullOrEmpty())
            {
                return description;
            }
            if(pawn.QuirkManager(false) == null)
            {
                return description;
            }
            IEnumerable<QuirkFactor> quirkFactors = RV2Stat.quirkFactors
                .Where(quirkFactor => quirkFactor.CanGetFactor(pawn));
            if(!quirkFactors.EnumerableNullOrEmpty())
            {
                description += "\nQuirks:\n";
                foreach(QuirkFactor quirkFactor in quirkFactors)
                {
                    description += quirkFactor.GetDescription(pawn);
                }
            }
            if(description == null)
            {
                description = "ERR";
            }
            return description;
        }
    }
}
