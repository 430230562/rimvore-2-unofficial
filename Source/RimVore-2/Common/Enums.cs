using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RimVore2
{
    public enum ModifierOperation
    {
        Invalid,
        Add,
        Subtract,
        Multiply,
        Divide,
        Set
    }

    public enum DefaultStripSetting
    {
        Random,
        NeverStrip,
        AlwaysStrip
    }

    public enum AliasType
    {
        Invalid,
        BodyPart,
        Hediff
    }

    public enum RelationKind
    {
        Invalid,
        Colonist,
        Guest,
        TempColonist,
        Slave,
        Prisoner,
        Raider,
        Trader,
        Visitor,
        Mechanoid,
        Animal,
        ColonyAnimal,
        WildAnimal,
        WildMan,
        Factionless
    }

    public enum QuirkRarity
    {
        Invalid = -1,    // only to be used as default value
        ForcedOnly = 0,
        Guaranteed = 100,   // only to be used for RollForEach, with PickOne it's not guaranteed because 100 will be used as weight
        Abundant = 80,
        Common = 40,
        Uncommon = 20,
        Rare = 10,
        VeryRare = 5
    }

    public enum QuirkPoolType
    {
        Invalid,
        PickOne,
        RollForEach
    }

    public enum RollInstructionTiming
    {
        onStart,
        onCycle,
        onEnd
    }

    public enum VoreRole
    {
        Invalid,
        Predator,
        Prey,
        Feeder
    }

    public enum RaceType
    {
        Invalid,
        Humanoid,
        Animal,
        Insectoid,
        Mechanoid
    }

    public enum ForcedState
    {
        Willing,
        ForcedByPrey,
        ForcedByPredator,
        ForcedByFeeder
    }

    public enum IdentifierType
    {
        Everyone,
        Relation,
        Race,
        Animal,
        FoodTypeFlag,
        Gender
    }

    public enum RuleTargetRole
    {
        All,
        Predator,
        Prey,
        Feeder
    }

    // must be kept in Sync with Settings_Rules.ruleStateIcons !
    public enum RuleState
    {
        On,
        Off,
        Copy
    }

    public enum SexualPart
    {
        Penis,
        Vagina,
        Breasts
    }

    public enum NotificationType
    {
        None,
        MessageNeutral,
        MessageThreatSmall,
        MessageThreatBig,
        Letter,
        LetterThreatSmall,
        LetterThreatBig
    }

    public enum ProposalStatus
    {
        Pending,
        Accepted,
        Denied,
        Forced
    }

    public enum GrappleRole
    {
        Invalid,
        Attacker,
        Defender
    }

    public enum StruggleForceMode
    {
        Invalid,
        WhenForced,
        WhenNotForced,
        Never,
        Always
    }
}
