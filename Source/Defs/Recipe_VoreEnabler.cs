using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace RimVore2
{
    public class RecipeWorker_VoreEnabler : RecipeWorker
    {
        IEnumerable<QuirkDef> QuirksToApply => recipe.addsHediff?.comps? // get all comps from added hediff
            .FindAll(comp => comp is HediffCompProperties_QuirkForcer)  // limit to QuirkForcer comps
            .Cast<HediffCompProperties_QuirkForcer>()   // cast to QuirkForcer to access quirks
            .SelectMany(comp => comp.quirks);   // collect all quirks from all QuirkForcers

        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            if(!base.AvailableOnNow(thing, part))
            {
                return false;
            }
            if(!(thing is Pawn pawn))
            {
                return false;
            }
            QuirkManager pawnQuirks = pawn.QuirkManager();
            if(pawnQuirks == null)
            {
                return false;
            }
            if(QuirksToApply.EnumerableNullOrEmpty())
            {
                Log.Error("RecipeWorker_VoreEnabler does not have quirks to add, config error?");
                return false;
            }
            bool allQuirksApplicable = QuirksToApply.All(quirk => pawnQuirks.CanPostInitAddQuirk(quirk, out _));
            return allQuirksApplicable;
        }

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            base.ApplyOnPawn(pawn, part, billDoer, ingredients, bill);
            QuirkManager pawnQuirks = pawn.QuirkManager();
            foreach(QuirkDef quirk in QuirksToApply)
            {
                if(!pawnQuirks.TryPostInitAddQuirk(quirk, out string reason))
                {
                    string notificationMessage = "RV2_QuirkInvalid_CouldNotAddWithReason".Translate(quirk.label, reason);
                    NotificationUtility.DoNotification(NotificationType.MessageNeutral, notificationMessage);
                }
            }
        }
    }
}
