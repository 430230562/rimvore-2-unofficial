using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public abstract class RitualRole_Composite : RitualRole
    {
        public RitualRole_Composite() : base()
        {
            subRoles = PopulateSubRoles();
        }

        List<RitualRole> subRoles = new List<RitualRole>();
        const string aggregateString = " / ";

        public abstract List<RitualRole> PopulateSubRoles();

        public override bool AppliesToPawn(Pawn p, out string reason, TargetInfo selectedTarget, LordJob_Ritual ritual = null, RitualRoleAssignments assignments = null, Precept_Ritual precept = null, bool skipReason = false)
        {
            bool anyApplies = false;
            reason = null;
            foreach(RitualRole role in subRoles)
            {
                bool roleApplies = role.AppliesToPawn(p, out string tmpReason, p, ritual, assignments, precept, skipReason);
                anyApplies |= roleApplies;
                if(tmpReason != null)
                {
                    if(reason == null)
                        reason = tmpReason;
                    else
                        reason += aggregateString + tmpReason;
                }
            }
            if(anyApplies)
                reason = null;
            return anyApplies;
        }

        public override bool AppliesToRole(Precept_Role role, out string reason, Precept_Ritual ritual = null, Pawn pawn = null, bool skipReason = false)
        {
            bool anyApplies = false;
            reason = null;
            foreach(RitualRole subRole in subRoles)
            {
                bool roleApplies = subRole.AppliesToRole(role, out string tmpReason, ritual, pawn, skipReason);
                anyApplies |= roleApplies;
                if(tmpReason != null)
                {
                    if(reason == null)
                        reason = tmpReason;
                    else
                        reason += aggregateString + tmpReason;
                }
            }
            if(anyApplies)
                reason = null;
            return anyApplies;
        }

        public override string ExtraInfoForDialog(IEnumerable<Pawn> selected)
        {
            string fullInfo = null;
            foreach(RitualRole subRole in subRoles)
            {
                string tmpInfo = subRole.ExtraInfoForDialog(selected);
                if(tmpInfo != null)
                {
                    if(fullInfo == null)
                        fullInfo = tmpInfo;
                    else
                        fullInfo += aggregateString + tmpInfo;
                }
            }
            return fullInfo;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref subRoles, "subRoles", LookMode.Deep);
        }
    }

    public class RitualRole_Composite_ColonistOrPrisonerOrSlave : RitualRole_Composite
    {
        public RitualRole_Composite_ColonistOrPrisonerOrSlave() : base() { }

        public WorkTypeDef requiredWorkType;
        public bool mustBeCapableToFight;
        public bool disallowWildManPrisoner = true;
        public override List<RitualRole> PopulateSubRoles()
        {
            return new List<RitualRole>()
            {
                new RitualRoleColonist()
                {
                    requiredWorkType = requiredWorkType
                },
                new RitualRolePrisonerOrSlave()
                {
                    mustBeCapableToFight = mustBeCapableToFight,
                    disallowWildManPrisoner = disallowWildManPrisoner
                }
            };
        }
    }
    public class RitualRole_Composite_ColonistOrColonyAnimalOrPrisonerOrSlave : RitualRole_Composite_ColonistOrPrisonerOrSlave
    {
        public override bool Animal => true;

        public RitualRole_Composite_ColonistOrColonyAnimalOrPrisonerOrSlave() : base() { }

        public override List<RitualRole> PopulateSubRoles()
        {
            List<RitualRole> roles = base.PopulateSubRoles();
            roles.Add(new RitualRoleColonyAnimal());
            return roles;
        }
    }
    public class RitualRole_Composite_TagColonistOrColonyAnimalOrPrisonerOrSlave : RitualRole_Composite_ColonistOrColonyAnimalOrPrisonerOrSlave
    {
        public RitualRole_Composite_TagColonistOrColonyAnimalOrPrisonerOrSlave() : base() { }

        public string tag;

        public override List<RitualRole> PopulateSubRoles()
        {
            List<RitualRole> roles = base.PopulateSubRoles();
            roles.Add(new RitualRoleTag()
            {
                tag = tag
            });
            return roles;
        }
    }
}
