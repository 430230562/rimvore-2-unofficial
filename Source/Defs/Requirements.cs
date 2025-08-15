using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class TargetedRequirements : IExposable
    {
        public VoreRole target;
        public List<Requirement> requirements;

        public bool FulfillsRequirements(Pawn predator, Pawn prey, out string reason)
        {
            // can be called with null prey in case of predators available type / goal / path check, skip prey conditionals in that case
            if(target == VoreRole.Prey && prey == null)
            {
                reason = null;
                return true;
            }
            switch(target)
            {
                case VoreRole.Predator:
                    return PawnFulfillsRequirements(predator, out reason);
                case VoreRole.Prey:
                    return PawnFulfillsRequirements(prey, out reason);
                default:
                    throw new NotImplementedException();
            }
        }

        private bool PawnFulfillsRequirements(Pawn pawn, out string reason)
        {
            List<string> reasons = new List<string>();
            foreach(Requirement requirement in requirements)
            {
                if(!requirement.FulfillsRequirements(pawn, out string subReason))
                {
                    reasons.Add(subReason);
                }
            }
            if(reasons.Count > 0)
            {
                reason = string.Join(", ", reasons);
                return false;
            }
            reason = null;
            return true;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref target, "targetPawnRole");
            Scribe_Collections.Look(ref requirements, "requirements", LookMode.Deep, new object[0]);
        }

        public IEnumerable<string> ConfigErrors()
        {
            if(target == VoreRole.Invalid)
            {
                yield return "required field \"target\" not set";
            }
            if(requirements.NullOrEmpty())
            {
                yield return "required list \"requirements\" not set";
            }
            else
            {
                foreach(Requirement requirement in requirements)
                {
                    foreach(string error in requirement.ConfigErrors())
                    {
                        yield return error;
                    }
                }
            }
        }
    }

    public abstract class Requirement : IExposable
    {
        public abstract bool FulfillsRequirements(Pawn pawn, out string reason);
        public abstract void ExposeData();
        public abstract IEnumerable<string> ConfigErrors();
    }

    public abstract class Requirement_Def<T> : Requirement, IExposable where T : Def
    {
        public T def;
        public string type;
        public string DefLabel => def.label ?? def.defName;

        public bool FulfillsRequirements(Pawn pawn, bool isValid, out string reason)
        {
            if(!isValid)
            {
                reason = "RV2_RequirementInvalidReasons_NoDef".Translate(type, DefLabel);
                return false;
            }
            reason = null;
            return true;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            if(def == null)
            {
                yield return "required field \"def\" not set";
            }
        }

        public override void ExposeData()
        {
            // I wish this worked :(
            // Scribe_Defs.Look<T>(ref def, "def");
        }
    }

    public abstract class Requirement_DefWithValue<T> : Requirement_Def<T> where T : Def
    {
        public float minValue = float.MinValue;
        public float maxValue = float.MaxValue;

        public bool FulfillsRequirements(Pawn pawn, bool isValid, float curValue, out string reason)
        {
            // ?? means "if label exists, use it, otherwise use defName"
            string typeName = def.label ?? def.defName;
            if(!base.FulfillsRequirements(pawn, isValid, out reason))
            {
                return false;
            }
            // no reason to check if min and max are set by user thanks to their MinValue / MaxValue
            if(curValue < minValue)
            {
                reason = "RV2_RequirementInvalidReasons_DefTooLow".Translate(type, typeName, minValue);
                return false;
            }
            if(curValue > maxValue)
            {
                reason = "RV2_RequirementInvalidReasons_DefTooHigh".Translate(type, typeName, maxValue);
                return false;
            }
            reason = null;
            return true;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref minValue, "minValue");
            Scribe_Values.Look(ref maxValue, "maxValue");
        }
    }

    public class Requirement_Need : Requirement_DefWithValue<NeedDef>
    {
        public Requirement_Need()
        {
            type = "Need";
        }

        public override bool FulfillsRequirements(Pawn pawn, out string reason)
        {
            Need need = pawn.needs?.TryGetNeed((NeedDef)def);
            bool hasNeed = need != null;
            float value = hasNeed ? need.CurLevel : 0;
            return base.FulfillsRequirements(pawn, hasNeed, value, out reason);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref def, "def");
        }
    }

    public class Requirement_Capacity : Requirement_DefWithValue<PawnCapacityDef>
    {
        public Requirement_Capacity()
        {
            type = "Capacity";
        }

        public override bool FulfillsRequirements(Pawn pawn, out string reason)
        {
            PawnCapacityDef capacityDef = (PawnCapacityDef)def;
            bool hasCapacity = pawn.health?.capacities?.CapableOf(capacityDef) == true;
            float value = hasCapacity ? pawn.health.capacities.GetLevel(capacityDef) : 0;
            return base.FulfillsRequirements(pawn, hasCapacity, value, out reason);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref def, "def");
        }
    }

    public class Requirement_Stat : Requirement_DefWithValue<StatDef>
    {
        public Requirement_Stat()
        {
            type = "Stat";
        }

        public override bool FulfillsRequirements(Pawn pawn, out string reason)
        {
            StatDef statDef = (StatDef)def;
            bool hasStat = statDef.Worker?.IsDisabledFor(pawn) == false;
            float value = hasStat ? pawn.GetStatValue(statDef) : 0;
            return base.FulfillsRequirements(pawn, hasStat, value, out reason);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref def, "def");
        }
    }

    public class Requirement_Hediff : Requirement_DefWithValue<HediffDef>
    {
        public Requirement_Hediff()
        {
            type = "Hediff";
        }

        public override bool FulfillsRequirements(Pawn pawn, out string reason)
        {
            HediffDef hediffDef = (HediffDef)def;
            bool hasHediff = pawn.health?.hediffSet?.HasHediff(hediffDef) == true;
            if(hasHediff)
            {
                // pawn may have multiple hediffs with the hediffDef
                IEnumerable<Hediff> hediffs = pawn.health.hediffSet.hediffs.Where(hediff => hediff.def == hediffDef);
                // let the min / max evaluation run for all hediffs, if any hediff succeeds, this requirement is passed
                bool anyHediffValid = hediffs.Any(hediff => base.FulfillsRequirements(pawn, hasHediff, hediff.Severity, out _));
                if(!anyHediffValid)
                {
                    reason = "RV2_RequirementInvalidReasons_NoHediffInRange".Translate(LogUtility.PresentFloatRange(minValue, maxValue));
                    return false;
                }
                reason = null;
                return true;
            }
            string hediffName = hediffDef.label ?? hediffDef.defName;
            reason = "RV2_RequirementInvalidReasons_NoDef".Translate("HediffDef", hediffName);
            return false;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref def, "def");
        }
    }

    public class Requirement_Skill : Requirement_DefWithValue<SkillDef>
    {
        public Requirement_Skill()
        {
            type = "Skill";
        }

        public override bool FulfillsRequirements(Pawn pawn, out string reason)
        {
            SkillDef skillDef = (SkillDef)def;
            SkillRecord skill = pawn.skills?.skills?.SingleOrDefault(s => s.def == skillDef);
            bool hasSkill = skill != null && !skill.TotallyDisabled;
            float value = hasSkill ? pawn.skills.GetSkill(skillDef).Level : 0;
            return base.FulfillsRequirements(pawn, hasSkill, value, out reason);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref def, "def");
        }
    }

    public class Requirement_Ability : Requirement_Def<AbilityDef>
    {
        public Requirement_Ability()
        {
            type = "Ability";
        }

        public override bool FulfillsRequirements(Pawn pawn, out string reason)
        {
            AbilityDef abilityDef = (AbilityDef)def;
            bool hasAbility = pawn.abilities.abilities.Any(ability => ability.def == abilityDef);
            return base.FulfillsRequirements(pawn, hasAbility, out reason);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref def, "def");
        }
    }

    public class Requirement_RaceThingDef : Requirement_Def<ThingDef>
    {
        public Requirement_RaceThingDef()
        {
            type = "RaceThingDef";
        }

        public override bool FulfillsRequirements(Pawn pawn, out string reason)
        {
            ThingDef raceDef = (ThingDef)def;
            bool isRaceDef = pawn.def == raceDef;
            return base.FulfillsRequirements(pawn, isRaceDef, out reason);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref def, "def");
        }
    }

    public class Requirement_PawnKindDef : Requirement_Def<PawnKindDef>
    {
        public Requirement_PawnKindDef()
        {
            type = "PawnKindDef";
        }

        public override bool FulfillsRequirements(Pawn pawn, out string reason)
        {
            PawnKindDef kindDef = (PawnKindDef)def;
            bool isKindDef = pawn.kindDef == kindDef;
            return base.FulfillsRequirements(pawn, isKindDef, out reason);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref def, "def");
        }
    }

    public class Requirement_Equipment : Requirement_Def<ThingDef>
    {
        public Requirement_Equipment()
        {
            type = "Equipment";
        }
        public override bool FulfillsRequirements(Pawn pawn, out string reason)
        {
            ThingDef thingDef = (ThingDef)def;
            bool hasThing = false;
            // check worn apparel
            if(pawn.apparel?.WornApparel?.Any(apparel => apparel.def == thingDef) == true)
            {
                hasThing = true;
            }
            // check equipment (weapons)
            if(pawn.equipment.AllEquipmentListForReading.Any(equipment => equipment.def == thingDef))
            {
                hasThing = true;
            }
            return base.FulfillsRequirements(pawn, hasThing, out reason);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref def, "def");
        }
    }

    // ----------------- RV2 specific ----------------
    public class Requirement_Quirk : Requirement
    {
        public string specialFlag;

        public Requirement_Quirk() { }

        public override bool FulfillsRequirements(Pawn pawn, out string reason)
        {
            QuirkManager quirks = pawn.QuirkManager();
            if(quirks == null)
            {
                reason = "RV2_RequirementInvalidReasons_NoQuirks".Translate();
                return false;
            }
            if(!quirks.HasSpecialFlag(specialFlag))
            {
                reason = "RV2_RequirementInvalidReasons_NoDef".Translate("quirk special flag", specialFlag);
                return false;
            }
            reason = null;
            return true;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref specialFlag, "specialFlag");
        }

        public override IEnumerable<string> ConfigErrors()
        {
            if(specialFlag == null)
            {
                yield return "required field \"specialFlag\" not set";
            }
        }
    }

    public class Requirement_RaceType : Requirement
    {
        public RaceType raceType;

        public Requirement_RaceType() { }

        public override bool FulfillsRequirements(Pawn pawn, out string reason)
        {
            RaceType pawnRaceType = pawn.GetRaceType();
            bool hasValidRace = pawnRaceType == raceType;
            if(!hasValidRace)
            {
                reason = "RV2_RequirementInvalidReasons_InvalidRace".Translate(pawnRaceType.ToString(), raceType.ToString());
                return false;
            }
            reason = null;
            return true;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref raceType, "raceType");
        }

        public override IEnumerable<string> ConfigErrors()
        {
            if(raceType == RaceType.Invalid)
            {
                yield return "required field \"raceType\" not set";
            }
        }
    }

    public class Requirement_ColonyRelation : Requirement
    {
        public RelationKind relation;

        public Requirement_ColonyRelation() { }

        public override bool FulfillsRequirements(Pawn pawn, out string reason)
        {
            List<RelationKind> pawnRelations = pawn.GetRelationKinds();
            bool pawnHasRelation = pawnRelations.Contains(relation);
            if(!pawnHasRelation)
            {
                string relationsString = string.Join(", ", pawnRelations.ConvertAll(relation => relation.ToString()));
                reason = "RV2_RequirementInvalidReasons_InvalidRelation".Translate(relationsString, relation.ToString());
                return false;
            }
            reason = null;
            return true;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref relation, "relation");
        }

        public override IEnumerable<string> ConfigErrors()
        {
            if(relation == RelationKind.Invalid)
            {
                yield return "required field \"relation\" not set";
            }
        }
    }

    public class Requirement_BodyPartAlias : Requirement
    {
        public string bodyPartAlias;

        public override bool FulfillsRequirements(Pawn pawn, out string reason)
        {
            BodyPartRecord bodyPart = pawn.GetBodyPartByName(bodyPartAlias);
            if(bodyPart == null)
            {
                reason = "RV2_RequirementInvalidReasons_MissingBodyPart".Translate(bodyPartAlias);
                return false;
            }
            reason = null;
            return true;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref bodyPartAlias, "bodyPartAlias");
        }

        public override IEnumerable<string> ConfigErrors()
        {
            if(bodyPartAlias == null)
            {
                yield return "required field \"bodyPartAlias\" not set";
            }
        }
    }

    public class Requirement_HediffAlias : Requirement
    {
        public string bodyPartAlias;
        public string hediffAlias;
        public float minSeverity = float.MinValue;
        public float maxSeverity = float.MaxValue;

        public override bool FulfillsRequirements(Pawn pawn, out string reason)
        {
            bool needToCheckBodyPart = bodyPartAlias != null;
            List<BodyPartRecord> bodyParts = null;
            // if the hediff must exist on a part, get that part by alias first
            if(needToCheckBodyPart)
            {
                // get all parts where the alias matches
                bodyParts = pawn.GetBodyPartsByName(bodyPartAlias);
                if(bodyParts.NullOrEmpty())
                {
                    // body part was not found, thus we can not find our hediff anymore
                    reason = "RV2_RequirementInvalidReasons_MissingBodyPart".Translate(bodyPartAlias);
                    return false;
                }
            }
            // get all hediffs 
            IEnumerable<Hediff> hediffs = pawn.health?.hediffSet.hediffs;
            if(hediffs.EnumerableNullOrEmpty())
            {
                // if pawn has no hediffs, we can not find out hediff
                reason = "RV2_RequirementInvalidReasons_MissingHediff".Translate(hediffAlias);
                return false;
            }
            // if hediffs are limited to a body part, filter them to our selected list of body parts
            if(needToCheckBodyPart)
            {
                hediffs = hediffs.Where(hediff => bodyParts.Contains(hediff.Part));
            }

            List<string> hediffAliases = hediffAlias.GetAliases("Hediff");
            bool hasHediff = false; // will be used to delivery a concise failure reason
            foreach(Hediff hediff in hediffs)
            {
                // check if this is one of the hediffs we are looking for
                if(hediff.def.defName.ContainsAnyAsSubstring(hediffAliases))
                {
                    hasHediff = true;
                    //Log.Message("pawn has hediff alias " + hediffAlias + " with severity " + hediff.Severity + " minSev: " + minSeverity + " maxSev: " + maxSeverity);
                    // check if the hediff has a valid severity
                    if(hediff.Severity > maxSeverity)
                    {
                        continue;
                    }
                    if(hediff.Severity < minSeverity)
                    {
                        continue;
                    }
                    // if we did not run into any issues, we found our hediff
                    reason = null;
                    return true;
                }
            }
            // if we checked all hediffs and didn't find our targets, the requirement failed, delivery a reason
            if(hasHediff)
            {
                // the pawn HAS the hediff, but not with a valid severity
                reason = "RV2_RequirementInvalidReasons_NoHediffInSeverityRange".Translate(LogUtility.PresentFloatRange(minSeverity, maxSeverity));
            }
            else
            {
                reason = "RV2_RequirementInvalidReasons_MissingHediff".Translate(hediffAlias);
            }
            return false;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref bodyPartAlias, "bodyPartAlias");
            Scribe_Values.Look(ref hediffAlias, "hediffAlias");
        }

        public override IEnumerable<string> ConfigErrors()
        {
            if(hediffAlias == null)
            {
                yield return "required field \"hediffAlias\" not set";
            }
        }
    }

    /*public class Requirement_Alias : Requirement_DefWithValue<AliasDef>
    {
        AliasType aliasType;

        public Requirement_Alias()
        {
            type = aliasType.ToString();
        }

        public override bool FulfillsRequirements(Pawn pawn, out string reason)
        {
            switch (aliasType)
            {
                case AliasType.BodyPart:
                    Func<string, bool> partValidator = (string partAlias) => 
                        pawn.health.hediffSet.GetNotMissingParts()  // get all parts the pawn currently has
                            .Any(existingPart => existingPart.def.defName.ToLower().Contains(partAlias.ToLower())); // check if any part is the one we are looking for
                    return AliasFulfillsRequirements(pawn, partValidator, out reason);
                case AliasType.Hediff:
                    Func<string, bool> hediffValidator = (string hediffAlias) =>
                        pawn.health.hediffSet.hediffs.Any(hed => hed.def.defName.ToLower().Contains(hediffAlias.ToLower()));
                    return AliasFulfillsRequirements(pawn, hediffValidator, out reason);
                default:
                    throw new NotImplementedException();
            }
        }

        private bool AliasFulfillsRequirements(Pawn pawn, Func<string, bool> validator, out string reason)
        {
            AliasDef aliasDef = (AliasDef)def;

            bool hasAnyValidAlias = aliasDef.aliases.Any(alias => validator(alias));
            if(!hasAnyValidAlias)
            {
                string aliasesString = string.Join(", ", aliasDef.aliases);
                reason = "RV2_RequirementInvalidReasons_MissingAlias".Translate(aliasesString);
                return false;
            }
            reason = null;
            return true;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref def, "def");
            Scribe_Values.Look(ref aliasType, "aliasType");
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if(aliasType == AliasType.Invalid)
            {
                yield return "required field \"aliasType\" not set";
            }
        }
    }*/
}
