//using RimWorld;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Verse;

//namespace RimVore2
//{
//    public class StatWorker_Quirk : StatWorker
//    {
//        public override bool IsDisabledFor(Thing thing)
//        {
//            if(base.IsDisabledFor(thing))
//            {
//                return true;
//            }
//            if(!(thing is Pawn pawn))
//            {
//                return true;
//            }
//            if(pawn.QuirkManager(false) == null)
//            {
//                return true;
//            }
//            return false;
//        }
//        public override bool ShouldShowFor(StatRequest req)
//        {
//            bool isVisible = base.ShouldShowFor(req)
//                && req.Thing is Pawn
//                && !IsDisabledFor(req.Thing);
//            Log.Message("should show stat for " + req.Thing + "? : " + isVisible);
//            return isVisible;
//        }

//        public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
//        {
//            float statValue = base.GetValueUnfinalized(req, applyPostProcess);
//            Pawn pawn = (Pawn)req.Thing;
//            QuirkManager quirks = pawn.QuirkManager();
//            if(quirks.TryGetStatValue(base.stat, ref statValue))
//            {
//                return statValue;
//            }
//            return base.stat.defaultBaseValue;
//        }

//        public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
//        {
//            string description = base.GetExplanationUnfinalized(req, numberSense);
//            Pawn pawn = (Pawn)req.Thing;
//            QuirkManager quirks = pawn.QuirkManager();
//            description += "\n" + quirks.GetStatDescription(base.stat);
//            return description;
//        }
//    }
//}