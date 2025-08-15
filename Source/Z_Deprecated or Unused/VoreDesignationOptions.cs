//using Verse;
//using System.Collections.Generic;
//using System.Linq;

//namespace RimVore2
//{
//    public class VoreDesignationOptions : IExposable
//    {
//        public bool OverridesOthers = false;
//        public bool UserDesignated = false;
//        public bool CanBePredator = false;
//        public bool IsPredatorAuto = false;
//        public bool CanBeEndoPrey = false;
//        public bool IsEndoPreyAuto = false;
//        public bool CanBeFatalPrey = false;
//        public bool IsFatalPreyAuto = false;
//        public bool IgnoreMinimumAge = false;

//        public void ExposeData()
//        {
//            Scribe_Values.Look(ref OverridesOthers, "OverridesOthers", false);
//            Scribe_Values.Look(ref UserDesignated, "UserDesignated", false);
//            Scribe_Values.Look(ref CanBePredator, "CanBePredator", false);
//            Scribe_Values.Look(ref IsPredatorAuto, "IsPredatorAuto", false);
//            Scribe_Values.Look(ref CanBeEndoPrey, "CanBeEndoPrey", false);
//            Scribe_Values.Look(ref IsEndoPreyAuto, "IsEndoPreyAuto", false);
//            Scribe_Values.Look(ref CanBeFatalPrey, "CanBeFatalPrey", false);
//            Scribe_Values.Look(ref IsFatalPreyAuto, "IsFatalPreyAuto", false);
//            Scribe_Values.Look(ref IgnoreMinimumAge, "IgnoreMinimumAge", false);
//        }
//    }
//}
