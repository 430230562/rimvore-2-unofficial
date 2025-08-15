using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimVore2
{
    public class VoreTargetRequest
    {
        // single
        bool? isColonist = null;
        bool? isPrisoner = null;
        bool? isSlave = null;
        bool? isAnimal = null;
        bool? isHumanoid = null;
        bool? isDowned = null;
        bool? canFightBack = null;
        Func<Pawn, bool> Validator = null;
        List<QuirkDef> initiatorQuirks = null;
        // pair
        bool? isSameFaction = null;
        bool? isHostedByInitiatorFaction = null;
        bool? isHostile = null;
        bool? canBeVored = null;
        bool? canBeFatalVored = null;
        bool? canBeEndoVored = null;
        bool? canDoVore = null;
        bool? canDoFatalVore = null;
        bool? canDoEndoVore = null;
        Func<Pawn, Pawn, bool> PairValidator = null;

        public VoreTargetRequest() { }

        public VoreTargetRequest(
            bool? isColonist = null,
            bool? isPrisoner = null,
            bool? isSlave = null,
            bool? isAnimal = null,
            bool? isHumanoid = null,
            bool? isDowned = null,
            bool? canFightBack = null,
            Func<Pawn, bool> validator = null,
            List<QuirkDef> quirks = null,
            bool? isSameFaction = null,
            bool? isHostedByInitiatorFaction = null,
            bool? isHostile = null,
            bool? canBeVored = null,
            bool? canBeFatalVored = null,
            bool? canBeEndoVored = null,
            bool? canDoVore = null,
            bool? canDoFatalVore = null,
            bool? canDoEndoVore = null,
            Func<Pawn, Pawn, bool> pairValidator = null)
        {
            this.isColonist = isColonist;
            this.isPrisoner = isPrisoner;
            this.isSlave = isSlave;
            this.isAnimal = isAnimal;
            this.isHumanoid = isHumanoid;
            this.isDowned = isDowned;
            this.canFightBack = canFightBack;
            Validator = validator;
            this.initiatorQuirks = quirks;

            this.isSameFaction = isSameFaction;
            this.isHostedByInitiatorFaction = isHostedByInitiatorFaction;
            this.isHostile = isHostile;
            this.canBeVored = canBeVored;
            this.canBeFatalVored = canBeFatalVored;
            this.canBeEndoVored = canBeEndoVored;
            this.canDoVore = canDoVore;
            this.canDoFatalVore = canDoFatalVore;
            this.canDoEndoVore = canDoEndoVore;
            PairValidator = pairValidator;
        }

        public virtual bool IsValid(Pawn pawn, out string reason)
        {
            if(isColonist != null && pawn.IsColonist != isColonist)
            {
                reason = "is not colonist";
                return false;
            }
            if(isPrisoner != null && pawn.IsPrisoner != isPrisoner)
            {
                reason = "is not prisoner";
                return false;
            }
            if(isSlave != null && pawn.IsSlave != isSlave)
            {
                reason = "is not slave";
                return false;
            }
            if(isAnimal != null && pawn.IsAnimal() != isAnimal)
            {
                reason = "is not animal";
                return false;
            }
            if(isHumanoid != null && pawn.IsHumanoid() != isHumanoid)
            {
                reason = "is not humanoid";
                return false;
            }
            if(isDowned != null && pawn.Downed != isDowned)
            {
                reason = "is not downed";
                return false;
            }
            if(canFightBack != null && pawn.skills.GetSkill(SkillDefOf.Melee).TotallyDisabled != canFightBack)
            {
                reason = "is not colonist";
                return false;
            }
            if(Validator != null && !Validator(pawn))
            {
                reason = "validator returned false";
                return false;
            }
            reason = null;
            return true;
        }

        public virtual bool IsValid(Pawn initiator, Pawn target, out string reason)
        {
            if(!initiator.CanReach(target, PathEndMode.ClosestTouch, Danger.Some))
            {
                reason = "can't reach";
                return false;
            }
            if(!IsValid(target, out reason))
            {
                return false;
            }
            if(isSameFaction != null && (initiator.Faction == target.Faction) != isSameFaction)
            {
                reason = "is not same faction";
                return false;
            }
            if(isHostedByInitiatorFaction != null && initiator.Faction == target.HostFaction != isHostedByInitiatorFaction)
            {
                reason = "is not hosted by initiator faction";
                return false;
            }
            if(isHostile != null && target.HostileTo(initiator.Faction) != isHostile)
            {
                reason = "is not hostile";
                return false;
            }
            if(canBeVored != null && initiator.CanVore(target, out _) != canBeVored)
            {
                reason = "can not be vored";
                return false;
            }
            if(canBeFatalVored != null && initiator.CanFatalVore(target, out _, true) != canBeFatalVored)
            {
                reason = "can not be fatal-vored";
                return false;
            }
            if(canBeEndoVored != null && initiator.CanEndoVore(target, out _, true) != canBeEndoVored)
            {
                reason = "can not be endo-vored";
                return false;
            }
            if(canDoVore != null && target.CanVore(initiator, out _) != canDoVore)
            {
                reason = "can not vore initiator";
                return false;
            }
            if(canDoFatalVore != null && target.CanFatalVore(initiator, out _, true) != canDoFatalVore)
            {
                reason = "can not fatal vore initiator";
                return false;
            }
            if(canDoEndoVore != null && target.CanEndoVore(initiator, out _, true) != canDoEndoVore)
            {
                reason = "can not endo vore initiator";
                return false;
            }
            if(PairValidator != null && !PairValidator(initiator, target))
            {
                reason = "PairValidator failed";
                return false;
            }

            if(!initiatorQuirks.NullOrEmpty())
            {
                QuirkManager quirks = initiator.QuirkManager();
                if(quirks == null)
                {
                    reason = "initiatorQuirks missing";
                    return false;
                }
                foreach(QuirkDef quirk in initiatorQuirks)
                {
                    if(!quirks.HasQuirk(quirk))
                    {
                        reason = "initiatorQuirks missing";
                        return false;
                    }
                }
            }
            return true;
        }

        public override string ToString()
        {
            return $@"isColonist ? {isColonist}
isPrisoner ? {isPrisoner}
isSlave ? {isSlave}
isAnimal ? {isAnimal}
isHumanoid ? {isHumanoid}
isDowned ? {isDowned}
canFightBack ? {canFightBack}
initiatorQuirks: {(initiatorQuirks == null ? "NONE" : String.Join(", ", initiatorQuirks.Select(q => q.defName)))}
isSameFaction ? {isSameFaction}
isHostedByInitiatorFaction ? {isHostedByInitiatorFaction}
isHostile ? {isHostile}
canBeVored ? {canBeVored}
canBeFatalVored ? {canBeFatalVored}
canBeEndoVored ? {canBeEndoVored}
canDoVore ? {canDoVore}
canDoFatalVore ? {canDoFatalVore}
canDoEndoVore ? {canDoEndoVore}
";
        }
    }
}
