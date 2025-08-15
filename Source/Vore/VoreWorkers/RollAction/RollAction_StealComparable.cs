using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace RimVore2
{
    public abstract class RollAction_StealComparable<T> : RollAction_Steal
    {
        protected bool useFixed = true;
        protected bool useRandom = false;
        protected bool useTakersBest = false;
        protected bool useTakersWorst = false;
        protected bool useGiversBest = false;
        protected bool useGiversWorst = false;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref useFixed, "useFixed");
            Scribe_Values.Look(ref useRandom, "useRandom");
            Scribe_Values.Look(ref useTakersBest, "useTakersBest");
            Scribe_Values.Look(ref useTakersWorst, "useTakersWorst");
            Scribe_Values.Look(ref useGiversBest, "useGiversBest");
            Scribe_Values.Look(ref useGiversWorst, "useGiversWorst");
        }

        public abstract IEnumerable<T> SelectionRetrieval(Pawn pawn);
        public abstract float ValueRetrieval(Pawn pawn, T obj);
        public abstract T FixedSelector { get; }

        public virtual T Choose()
        {
            if(useFixed)
            {
                return FixedSelector;
            }
            IEnumerable<T> giverSelection = SelectionRetrieval(GiverPawn);
            IEnumerable<T> takerSelection = SelectionRetrieval(TakerPawn);
            // intersect allows us to reduce the list to common objects for both pawns
            IEnumerable<T> selection = giverSelection.Intersect(takerSelection);
            if(selection.EnumerableNullOrEmpty())
            {
                return default(T);
            }
            if(RV2Log.ShouldLog(true, "OngoingVore"))
                RV2Log.Message($"Selection of {typeof(T)} available for {TakerPawn.LabelShort} & {GiverPawn.LabelShort}: {string.Join(", ", selection)}", false, "OngoingVore");
            // we can already decide if we have a random one to use
            if(useRandom)
            {
                selection.RandomElement();
            }
            // remove any objects that are only on one side
            giverSelection = giverSelection.Intersect(selection);
            takerSelection = takerSelection.Intersect(selection);
            if(RV2Log.ShouldLog(true, "OngoingVore"))
            {
                RV2Log.Message($"giver selection: {string.Join(", ", giverSelection.Select(s => s + ": " + ValueRetrieval(GiverPawn, s)))}", false, "OngoingVore");
                RV2Log.Message($"taker selection: {string.Join(", ", takerSelection.Select(s => s + ": " + ValueRetrieval(TakerPawn, s)))}", false, "OngoingVore");
            }
            if(useTakersBest)
            {
                return takerSelection.MaxBy(entry => ValueRetrieval(TakerPawn, entry));
            }
            if(useTakersWorst)
            {
                return takerSelection.MinBy(entry => ValueRetrieval(TakerPawn, entry));
            }
            if(useGiversBest)
            {
                return takerSelection.MaxBy(entry => ValueRetrieval(GiverPawn, entry));
            }
            if(useGiversWorst)
            {
                return takerSelection.MinBy(entry => ValueRetrieval(GiverPawn, entry));
            }
            RV2Log.Warning("Reached the end of Choose() logic, this should never happen");
            return default(T);
        }
    }
}
