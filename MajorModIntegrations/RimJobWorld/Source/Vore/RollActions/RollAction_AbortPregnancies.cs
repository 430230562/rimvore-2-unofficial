
using RimVore2;
using rjw;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RV2_RJW
{
    public class RollAction_AbortPregnancies : RollAction
    {
        public override bool TryAction(VoreTrackerRecord record, float rollStrength)
        {
            if(!ModAdapter.IsRJWLoaded)
            {
                return false;
            }
            if(!RV2_RJW_Settings.rjw.VaginalVoreAbortsPregnancies)
                return false;
            base.TryAction(record, rollStrength);
            Pawn mother = record.Predator;
            IEnumerable<Hediff_BasePregnancy> pregnancies = mother.health?.hediffSet?.hediffs
                .Where(hed => hed is Hediff_BasePregnancy)
                .Cast<Hediff_BasePregnancy>();
            int pregnancyCount = pregnancies.Count();
            if(pregnancies.EnumerableNullOrEmpty())
            {
                if(RV2Log.ShouldLog(false, "RJW"))
                    RV2Log.Message("No pregnancies to abort", "RJW");
                return false;
            }
            pregnancies
                .ToList()   // copy to prevent ExceptionModifiedException
                .ForEach(pregnancy => pregnancy.Miscarry()); 
            if(RV2Log.ShouldLog(false, "RJW"))
                RV2Log.Message("Aborted " + pregnancyCount + " pregnancies", "RJW");
            return true;
        }
    }
}