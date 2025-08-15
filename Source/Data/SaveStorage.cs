//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace RimVore2
//{
//    class SaveStorage : ModBase
//    {
//        public override string ModIdentifier => "RV2";

//        //public static DataStore DataStore;

//        // don't let harmony run amok and patch files at this point
//        protected override bool HarmonyAutoPatch { get => false; }

//        public override void WorldLoaded()
//        {
//            base.WorldLoaded();
//            //DataStore = Find.World.GetComponent<DataStore>();
//            //VoreFeedUtility.LoadAllReservedPrey(Find.CurrentMap?.mapPawns?.AllPawns);
//            DebugTools.Reset.ResyncAllHediffs();    // redirect to debugtools because I am L A Z Y
//        }

//        public override void DefsLoaded()
//        {
//            base.DefsLoaded();
//            SettingsUpdater.UpdateSettings();
//            RV2Mod.Mod.DefsLoaded();
//            ConfigUtility.PresentAdditionalConfigErrors();
//            ConfigUtility.PresentAdditionalConfigMessages();
//            BackstoryUtility.UpdateAllBackstoryDescriptions();
//            //VorePathDurationUtility.CalculateAllPathDurations();
//            new Targeter_ForcePause();
//        }
//    }
//}
