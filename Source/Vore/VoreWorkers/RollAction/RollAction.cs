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
    public abstract class RollAction : IExposable
    {
        protected VoreRole target = VoreRole.Invalid;
        protected bool invert = false;
#pragma warning disable IDE0044 // Add readonly modifier
        private bool canBlockNextActions = false;
#pragma warning restore IDE0044 // Add readonly modifier
        public virtual bool CanBlockNextActions => canBlockNextActions;
        protected VoreTrackerRecord record;

        protected Pawn TargetPawn => record.GetPawnByRole(target);
        protected Pawn OtherPawn => record.GetPawnByRole(target) == PredatorPawn ? PreyPawn : PredatorPawn;
        protected Pawn PredatorPawn => record.Predator;
        protected Pawn PreyPawn => record.Prey;

        public RollAction() { }

        // return false if the action wants to block further actions
        public virtual bool TryAction(VoreTrackerRecord record, float rollStrength)
        {
            if(RV2Log.ShouldLog(true, "OngoingVore"))
            {
                RV2Log.Message($"Running RollAction {this.GetType().Name}", "OngoingVore");
            }
            this.record = record;
            return true;
        }
        public virtual void ExposeData() { }
        public virtual IEnumerable<string> ConfigErrors()
        {
            yield break;
        }
    }

    /* I don't know about these... went with implementing them directly in the PreVore and PostVore hooks */
    //public class RollAction_IncrementRecord_Type : RollAction_IncrementRecord
    //{
    //    protected override RecordDef RecordDef
    //    {
    //        get
    //        {
    //            switch (target)
    //            {
    //                case VoreRole.Predator:
    //                    return record.VoreType.initiationRecordPredator;
    //                case VoreRole.Prey:
    //                    return record.VoreType.initiationRecordPrey;
    //                default:
    //                    return null;
    //            }
    //        }
    //    }
    //}

    //public class RollAction_IncrementRecord_Goal : RollAction_IncrementRecord
    //{
    //    protected override RecordDef RecordDef
    //    {
    //        get
    //        {
    //            switch (target)
    //            {
    //                case VoreRole.Predator:
    //                    return record.VoreGoal.goalFinishRecordPredator;
    //                case VoreRole.Prey:
    //                    return record.VoreGoal.goalFinishRecordPrey;
    //                default:
    //                    return null;
    //            }
    //        }
    //    }
    //}
}