using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace RimVore2
{
    public class RollAction_StealTrait : RollAction_Steal
    {
        public override bool TryAction(VoreTrackerRecord record, float rollStrength)
        {
            base.TryAction(record, rollStrength);
            if(TakerPawn.Dead)
            {
                return false;
            }
            List<Trait> takerTraits = TakerPawn.story?.traits?.allTraits;
            List<Trait> giverTraits = GiverPawn.story?.traits?.allTraits
                .Where(trait => !trait.def.HasModExtension<TraitExtension_NotStealableFlag>())
                .ToList();
            if(takerTraits == null || giverTraits.NullOrEmpty())    // taker just needs to be capable of having traits, giver must have at least one trait
            {
                if(RV2Log.ShouldLog(true, "PostVore"))
                    RV2Log.Message("Either participant has no traits, can't steal trait", false, "PostVore");
                // either pawn doesn't have traits, nothing to do
                return false;
            }
            if(RV2Log.ShouldLog(true, "PostVore"))
            {
                RV2Log.Message($"Giver traits: {string.Join(", ", giverTraits.Select(t => t.def.defName))}", false, "PostVore");
                RV2Log.Message($"Taker traits: {string.Join(", ", takerTraits.Select(t => t.def.defName))}", false, "PostVore");
            }
            if(takerTraits.Count >= RV2Mod.Settings.cheats.MaxPawnTraits)
            {
                if(RV2Log.ShouldLog(true, "PostVore"))
                    RV2Log.Message("Can't steal more traits, already at allowed maximum traits", false, "PostVore");
                return false;
            }
            // TODO: traits with degrees increased / decreased through stealing
            IEnumerable<Trait> potentialTraits = giverTraits
                .Where(giverTrait => takerTraits
                    .All(takerTrait => takerTrait.def != giverTrait.def)
                );
            if(potentialTraits.EnumerableNullOrEmpty())
            {
                return false;
            }
            Trait traitToSteal = potentialTraits.RandomElement();
            if(RV2Log.ShouldLog(true, "PostVore"))
                RV2Log.Message($"Stealing {traitToSteal.def.defName} trait from selection: {string.Join(", ", potentialTraits.Select(t => t.def.defName))}", false, "PostVore");
            // I think we can get away without re-instancing a new trait
            PreyPawn.story.traits.RemoveTrait(traitToSteal);
            PredatorPawn.story.traits.GainTrait(traitToSteal);
            return true;
        }
    }
}
