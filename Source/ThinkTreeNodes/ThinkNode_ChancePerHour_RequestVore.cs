//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Verse;
//using Verse.AI;

//namespace RimVore2
//{
//    public class ThinkNode_ChancePerHour_RequestVore : ThinkNode_ChancePerHour
//    {
//        protected override float MtbHours(Pawn pawn)
//        {
//            float meanTimeBetween = RV2Mod.settings.fineTuning.MTBVoreProposalCreation;
//            QuirkManager quirkManager = pawn.QuirkManager(false);
//            if(quirkManager != null)
//            {
//                meanTimeBetween = quirkManager.ModifyValue("voreRequestMtb", meanTimeBetween);
//            }
//            return meanTimeBetween;
//        }
//    }
//}
