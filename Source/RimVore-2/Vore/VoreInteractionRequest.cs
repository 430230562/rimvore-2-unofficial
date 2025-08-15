using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public struct VoreInteractionRequest : IEquatable<VoreInteractionRequest>
    {
        Pawn initiator;
        Pawn target;
        VoreRole initiatorRole;
        bool isForAuto;
        bool isForProposal;
        bool shouldIgnoreDesignations;
        List<VoreRole> roleWhitelist;
        List<VoreRole> roleBlacklist;
        List<VoreTypeDef> typeWhitelist;
        List<VoreTypeDef> typeBlacklist;
        List<VoreGoalDef> goalWhitelist;
        List<VoreGoalDef> goalBlacklist;
        List<VorePathDef> pathWhitelist;
        List<VorePathDef> pathBlacklist;
        List<RV2DesignationDef> designationWhitelist;
        List<RV2DesignationDef> designationBlacklist;

        public Pawn Initiator => initiator;
        public Pawn Target => target;
        public VoreRole InitiatorRole { get => initiatorRole; set => initiatorRole = value; }
        public bool IsForAuto => isForAuto;
        public bool IsForProposal => isForProposal;
        public bool ShouldIgnoreDesignations => shouldIgnoreDesignations;
        public List<VoreRole> RoleWhitelist => roleWhitelist;
        public List<VoreRole> RoleBlacklist => roleBlacklist;
        public List<VoreTypeDef> TypeWhitelist => typeWhitelist;
        public List<VoreTypeDef> TypeBlacklist => typeBlacklist;
        public List<VoreGoalDef> GoalWhitelist => goalWhitelist;
        public List<VoreGoalDef> GoalBlacklist => goalBlacklist;
        public List<VorePathDef> PathWhitelist => pathWhitelist;
        public List<VorePathDef> PathBlacklist => pathBlacklist;
        public List<RV2DesignationDef> DesignationWhitelist => designationWhitelist;
        public List<RV2DesignationDef> DesignationBlacklist => designationBlacklist;

        public VoreInteractionRequest(
            Pawn initiator = null,
            Pawn target = null,
            VoreRole initiatorRole = VoreRole.Invalid,
            bool isForAuto = false,
            bool isForProposal = false,
            bool shouldIgnoreDesignations = false,
            List<VoreRole> roleWhitelist = null,
            List<VoreRole> roleBlacklist = null,
            List<VoreTypeDef> typeWhitelist = null,
            List<VoreTypeDef> typeBlacklist = null,
            List<VoreGoalDef> goalWhitelist = null,
            List<VoreGoalDef> goalBlacklist = null,
            List<VorePathDef> pathWhitelist = null,
            List<VorePathDef> pathBlacklist = null,
            List<RV2DesignationDef> designationWhitelist = null,
            List<RV2DesignationDef> designationBlacklist = null
        )
        {
            this.initiator = initiator;
            this.target = target;
            this.initiatorRole = initiatorRole;
            this.isForAuto = isForAuto;
            this.isForProposal = isForProposal;
            this.shouldIgnoreDesignations = shouldIgnoreDesignations;
            this.roleWhitelist = roleWhitelist;
            this.roleBlacklist = roleBlacklist;
            this.typeWhitelist = typeWhitelist;
            this.typeBlacklist = typeBlacklist;
            this.goalWhitelist = goalWhitelist;
            this.goalBlacklist = goalBlacklist;
            this.pathWhitelist = pathWhitelist;
            this.pathBlacklist = pathBlacklist;
            this.designationWhitelist = designationWhitelist;
            this.designationBlacklist = designationBlacklist;
        }

        public bool Equals(VoreInteractionRequest other)
        {
            if(!ParticipantsEquals(other))
                return false;
            if(this.IsForAuto != other.IsForAuto)
                return false;
            if(this.isForProposal != other.isForProposal)
                return false;
            if(this.shouldIgnoreDesignations != other.shouldIgnoreDesignations)
                return false;

            if(!ListsEqual(this.roleWhitelist, other.roleWhitelist))
                return false;
            if(!ListsEqual(this.roleBlacklist, other.roleBlacklist))
                return false;
            if(!ListsEqual(this.typeWhitelist, other.typeWhitelist))
                return false;
            if(!ListsEqual(this.typeBlacklist, other.typeBlacklist))
                return false;
            if(!ListsEqual(this.goalWhitelist, other.goalWhitelist))
                return false;
            if(!ListsEqual(this.goalBlacklist, other.goalBlacklist))
                return false;
            if(!ListsEqual(this.pathWhitelist, other.pathWhitelist))
                return false;
            if(!ListsEqual(this.pathBlacklist, other.pathBlacklist))
                return false;
            if(!ListsEqual(this.designationWhitelist, other.designationWhitelist))
                return false;
            if(!ListsEqual(this.designationBlacklist, other.designationBlacklist))
                return false;

            return true;
        }

        private bool ListsEqual<T>(List<T> list1, List<T> list2)
        {
            // if both are null or empty, they are equal
            if(list1.NullOrEmpty() && list2.NullOrEmpty())
                return true;
            // otherwise if either is null or empty but not the other, it has to be inequal
            if(list1.NullOrEmpty() || list2.NullOrEmpty())
                return false;
            // and SequenceEqual only works if both lists are not null or empty
            return list1.SequenceEqual(list2);
        }

        private bool ParticipantsEquals(VoreInteractionRequest other)
        {
            // invalid means unknown, so never assume they are equal
            if(other.InitiatorRole == VoreRole.Invalid || this.InitiatorRole == VoreRole.Invalid)
                return false;

            Pawn pawn1;
            Pawn pawn2;
            // if we compare prey to pred requests, swap the pawns around to make the request match properly - prey intitiator == predator target
            if(other.InitiatorRole == this.InitiatorRole)
            {
                pawn1 = initiator;
                pawn2 = target;
            }
            else
            {
                pawn1 = target;
                pawn2 = initiator;
            }
            return pawn1 == other.initiator
                && pawn2 == other.target;
        }
    }
}
