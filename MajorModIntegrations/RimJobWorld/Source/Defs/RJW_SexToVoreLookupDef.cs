using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using rjw;
using RimVore2;
using rjw.Modules.Interactions.Enums;
using Verse;

namespace RV2_RJW
{
    public class RJW_SexToVoreLookupDef : Def
    {
        VoreTypeDef giverVoreType;
        List<xxx.rjwSextype> ejectSexTypes = new List<xxx.rjwSextype>();
        List<TransferSexType> transferSexTypes = new List<TransferSexType>();

        public bool AppliesTo(VorePathDef path)
        {
            return path.voreType == giverVoreType;
        }

        public bool ShouldEject(SexProps sexProps)
        {
            return ejectSexTypes.Contains(sexProps.sexType);
        }

        public bool ShouldTransfer(SexProps sexProps, out VoreTypeDef transferType)
        {
            TransferSexType targetTransfer = transferSexTypes
                .Find(transfer => transfer.sexTypes.Contains(sexProps.sexType));
            if(targetTransfer == null)
            {
                transferType = null;
                return false;
            }
            transferType = targetTransfer.targetVoreType;
            return true;
        }
        private class TransferSexType
        {
            public List<xxx.rjwSextype> sexTypes = new List<xxx.rjwSextype>();
            public VoreTypeDef targetVoreType;
        }
    }

}
