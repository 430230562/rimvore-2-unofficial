using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class VoreMentalStateDef : MentalStateDef
    {
        public VoreTargetRequest request;
        public string targetChangeMessageKey;
        public MentalStateDef fallbackMentalState;
        public VoreRole initiatorRole = VoreRole.Predator;
        public int targetCountToVore = 1;
        public List<VoreGoalDef> goalWhitelist;
        public List<VoreGoalDef> goalBlacklist;
        public List<VoreTypeDef> typeWhitelist;
        public List<VoreTypeDef> typeBlacklist;
        public List<VorePathDef> pathWhitelist;
        public List<VorePathDef> pathBlacklist;
        public List<RV2DesignationDef> designationWhitelist;
        public List<RV2DesignationDef> designationBlacklist;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if(stateClass == typeof(MentalState_VoreTargeter) && request == null)
            {
                yield return "Required field \"request\" is not set";
            }
            if(fallbackMentalState == null)
            {
                yield return "Required field \"fallbackMentalState\" is not set";
            }
        }

        public override string ToString()
        {
            return $@"{base.ToString()}
fallbackMentalState: {fallbackMentalState}
initiatorRole: {initiatorRole}
targetCountToVore: {targetCountToVore}
goalWhitelist: {(goalWhitelist == null ? "NONE" : String.Join(", ", goalWhitelist.Select(g => g.defName)))}
goalBlacklist: {(goalBlacklist == null ? "NONE" : String.Join(", ", goalBlacklist.Select(g => g.defName)))}
typeWhitelist: {(typeWhitelist == null ? "NONE" : String.Join(", ", typeWhitelist.Select(g => g.defName)))}
typeBlacklist : {(typeBlacklist == null ? "NONE" : String.Join(", ", typeBlacklist.Select(g => g.defName)))}
pathWhitelist: {(pathWhitelist == null ? "NONE" : String.Join(", ", pathWhitelist.Select(g => g.defName)))}
pathBlacklist : {(pathBlacklist == null ? "NONE" : String.Join(", ", pathBlacklist.Select(g => g.defName)))}
designationWhitelist: {(designationWhitelist == null ? "NONE" : String.Join(", ", designationWhitelist.Select(g => g.defName)))}
designationBlacklist: {(designationBlacklist == null ? "NONE" : String.Join(", ", designationBlacklist.Select(g => g.defName)))}
request: {request}";
        }
    }
}
