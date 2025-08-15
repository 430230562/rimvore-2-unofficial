using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class HediffComp_QuirkForcer : HediffComp
    {
        private QuirkManager PawnQuirks => Pawn.QuirkManager(false);

        public List<QuirkDef> ForcedQuirks => ((HediffCompProperties_QuirkForcer)props).quirks;

        public override void CompPostMake()
        {
            // if pawn does not have quirks yet (freshly spawned), do not add quirks
            // quirks will be set if this hediff was applied via surgery
            if(PawnQuirks != null)
            {
                foreach(QuirkDef quirk in ForcedQuirks)
                {
                    if(!PawnQuirks.TryPostInitAddQuirk(quirk, out string reason))
                    {
                        string notificationText = "RV2_QuirkInvalid_CouldNotAddWithReason".Translate(quirk.label, reason);
                        NotificationUtility.DoNotification(NotificationType.MessageNeutral, notificationText, null);
                    }
                    else
                    {
                        if(RV2Log.ShouldLog(false, "Quirks"))
                            RV2Log.Message($"Added quirk {quirk.label} from hediff {parent.Label}", "Quirks");
                    }
                }
            }
        }
    }

    public class HediffCompProperties_QuirkForcer : HediffCompProperties
    {
        public List<QuirkDef> quirks = new List<QuirkDef>();

        public HediffCompProperties_QuirkForcer()
        {
            base.compClass = typeof(HediffComp_QuirkForcer);
        }

        public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
        {
            foreach(string error in base.ConfigErrors(parentDef))
            {
                yield return error;
            }
            if(quirks.NullOrEmpty())
            {
                yield return "required list \"quirks\" null or empty";
            }
        }
    }
}
