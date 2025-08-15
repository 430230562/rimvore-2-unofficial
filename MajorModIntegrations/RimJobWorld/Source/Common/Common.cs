using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RV2_RJW
{
    public static class Common
    {
        public static readonly List<RJW_SexToVoreLookupDef> SexToVoreLookups = DefDatabase<RJW_SexToVoreLookupDef>.AllDefsListForReading;
        public static readonly RJW_SexTypeGiverDictionaryDef SexTypeGiverDictionary = DefDatabase<RJW_SexTypeGiverDictionaryDef>.GetNamed("Default");

        public static readonly RecordDef voreTransferReceiverRecord = DefDatabase<RecordDef>.GetNamed("RV2_RJW_VoreTransfer_Receiver");
        public static readonly RecordDef voreTransferGiverRecord = DefDatabase<RecordDef>.GetNamed("RV2_RJW_VoreTransfer_Giver");
    }
}
