using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace RimVore2
{
    public abstract class QuirkComp : IExposable
    {
        // higher value = more important
        public float priority = 0f;

        public abstract IEnumerable<string> ConfigErrors();

        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref priority, "priority");
        }
    }

    public class QuirkComp_ValueModifier : QuirkComp
    {
        public string modifierName;
        public float modifierValue = float.MinValue;
        public ModifierOperation operation = ModifierOperation.Multiply;

        public bool TryGetModifierValue(string modifierName, out float modifier)
        {
            if(modifierName != this.modifierName)
            {
                modifier = 0;
                return false;
            }
            modifier = modifierValue;
            return true;
        }

        public float Modify(string modifierName, float value)
        {
            if(TryGetModifierValue(modifierName, out float modifier))
            {
                return operation.Aggregate(modifier, value);
            }
            return value;
        }

        public float Modify(float value)
        {
            return operation.Aggregate(value, modifierValue);
        }

        public override IEnumerable<string> ConfigErrors()
        {
            if(modifierName == null)
            {
                yield return "required field \"modifierName\" not set";
            }
            if(modifierValue == float.MinValue)
            {
                yield return "required field \"modifierValue\" not set";
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref modifierName, "modifierName");
            Scribe_Values.Look(ref modifierValue, "modifierValue");
        }

        public override string ToString()
        {
            return "ValueModifier - " + modifierName + ": " + modifierValue;
        }
    }

    public class QuirkComp_ThoughtOverride : QuirkComp
    {
        public ThoughtDef originalThought;
        public ThoughtDef overrideThought;

        public override IEnumerable<string> ConfigErrors()
        {
            if(originalThought == null)
            {
                yield return "Required field \"originalThought\" is null";
            }
            if(overrideThought == null)
            {
                yield return "Required field \"overrideThought\" is null";
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref originalThought, "originalThought");
            Scribe_Defs.Look(ref overrideThought, "overrideThought");
        }

        public override string ToString()
        {
            return "ThoughtOverride - " + originalThought + " -> " + overrideThought;
        }
    }

    public class QuirkComp_HediffOverride : QuirkComp
    {
        public HediffDef originalHediff;
        /// <summary>
        /// Override can be set to NULL, which will block the original hediff entirely
        /// </summary>
        public HediffDef overrideHediff;

        public override IEnumerable<string> ConfigErrors()
        {
            if(originalHediff == null)
            {
                yield return "Required field \"originalThought\" is null";
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref originalHediff, "originalHediff");
            Scribe_Defs.Look(ref originalHediff, "originalHediff");
        }

        public override string ToString()
        {
            return $"HediffOverride - {originalHediff} -> {overrideHediff}";
        }
    }

    public class QuirkComp_SituationalThoughtEnabler : QuirkComp
    {
        public ThoughtDef enabledThought;
        public override IEnumerable<string> ConfigErrors()
        {
            if(enabledThought == null)
                yield return $"Required field \"{nameof(enabledThought)}\" not set";
        }

        public override string ToString()
        {
            return $"SituationalThoughtEnabler - {enabledThought.defName}";
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref enabledThought, "enabledThought");
        }
    }

    public class QuirkComp_PostVoreMemory : QuirkComp
    {
        public List<string> keywords;
        public ThoughtDef memory;

        public override string ToString()
        {
            return "PostVoreMemory - " + string.Join("+", keywords) + " -> " + memory;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            if(keywords == null || keywords.Count == 0)
            {
                yield return "required list \"keywords\" not set or empty";
            }
            if(memory == null)
            {
                yield return "required field \"memory\" not set";
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref keywords, "keywords", LookMode.Value);
            Scribe_Defs.Look(ref memory, "memory");
        }
    }

    public class QuirkComp_VoreEnabler : QuirkComp
    {
        public VoreTargetSelectorRequest selector;

        public override string ToString()
        {
            return "VoreEnabler - " + selector.ToString();
        }

        public override IEnumerable<string> ConfigErrors()
        {
            if(selector == null)
            {
                yield return "required field \"selector\" not set";
            }
            // if race type is set, role must also be set, XOR operation, 
            // if race set, but role not -> fail, 
            // if role set but race not set -> fail
            else if(selector.raceType == default(RaceType) ^ selector.role == default(VoreRole) == true)
            {
                yield return "fields \"raceType\" and \"role\" must both be set if either is used";
            }
            else if(selector.role != VoreRole.Invalid && selector.role != VoreRole.Prey)
            {
                yield return "field \"role\" currently only supports Prey!";
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref selector, "selector");
        }
    }

    public class QuirkComp_SpecialFlag : QuirkComp
    {
        public string flag;

        public override string ToString()
        {
            return "SpecialFlag - " + flag;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            if(flag == null)
            {
                yield return "required field \"flag\" not set";
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref flag, "flag");
        }
    }

    // these effectively just exit to keep the type for each ValueModifier comp "tight" and relevant, so we don't always iterate over a massive List of ValueModifier comps
    /*public class QuirkComp_RollStrengthModifier : QuirkComp_ValueModifier
    {
        public override string ToString()
        {
            return "RollStrengthModifier - " + modifierName + ": " + modifierValue;
        }
    }*/

    public class QuirkComp_VoreTargetSelectorModifier : QuirkComp
    {
        public VoreTargetSelectorRequest selectors;
        public float modifierValue = float.MinValue;

        public bool ModifierValid(VoreTargetSelectorRequest request)
        {
            return selectors.Matching(request);
        }

        public float GetModifierValue(VoreTargetSelectorRequest request)
        {
            if(ModifierValid(request))
            {
                if(RV2Log.ShouldLog(true, "Quirks"))
                    RV2Log.Message("Request " + request.ToString() + " resulted in modifierValue " + modifierValue, false, "Quirks");
                return modifierValue;
            }
            return float.MinValue;
        }

        public override string ToString()
        {
            return "VoreTargetSelectorModifier - modifierValue: " +
                modifierValue +
                " VoreTargetSelectorRequest: " +
                selectors.ToString();
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref selectors, "selectors", new object[0]);
            Scribe_Values.Look(ref modifierValue, "modifierValue");
        }

        public override IEnumerable<string> ConfigErrors()
        {
            if(modifierValue == float.MinValue)
            {
                yield return "Required field \"modifierValue\" is not set";
            }
            foreach(string error in selectors.ConfigErrors())
            {
                yield return error;
            }
        }
    }

    /// <summary>
    /// Copy of QuirkComp_VoreTargetSelectorModifier to make feeder sub roles more interesting
    /// </summary>
    public class QuirkComp_FeederVoreTargetSelectorModifier : QuirkComp_VoreTargetSelectorModifier
    {

    }

    public class QuirkComp_CapacityOffsetModifier : QuirkComp_ValueModifier
    {
        public PawnCapacityDef capacity;

        public override string ToString()
        {
            return "CapacityOffsetModifier - " + capacity.LabelCap + ": " + modifierValue;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            modifierName = "";
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if(capacity == null)
            {
                yield return "Required field \"capacity\" not set";
            }
        }
    }

    public class QuirkComp_DesignationBlock : QuirkComp
    {
        public RV2DesignationDef designation;

        public override string ToString()
        {
            return "DesignationBlock - " + designation.defName;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            if(designation == null)
            {
                yield return "Required field \"desigation\" is not set";
            }
        }
    }

    public class QuirkComp_PostVoreAction : QuirkComp
    {
        List<Roll> rolls = new List<Roll>();
        bool triggerAsRole = false;
        VoreRole roleToTrigger = VoreRole.Invalid;

        public override string ToString()
        {
            return $"PostVoreAction - {(triggerAsRole ? roleToTrigger.ToString() : "")} Rolls: {rolls.Count}:\n{string.Join("\n", rolls.Select(roll => roll.ToString()))}";
        }

        public void DoRolls(VoreRole currentRole, VoreTrackerRecord record)
        {
            if(RV2Log.ShouldLog(true, "PostVore"))
                RV2Log.Message($"({record.Predator.LabelShort}-{record.Prey.LabelShort}) - Post vore actions: {this}", false, "PostVore");
            if(triggerAsRole && currentRole != roleToTrigger)
            {
                if(RV2Log.ShouldLog(true, "PostVore"))
                    RV2Log.Message($"({record.Predator.LabelShort}-{record.Prey.LabelShort}) - No post vore rolls: trigger role {currentRole} does not match required role {roleToTrigger}", true, "PostVore");
                return;
            }

            rolls.ForEach(roll => roll.Work(record));
        }

        public override IEnumerable<string> ConfigErrors()
        {
            if(rolls.NullOrEmpty())
            {
                yield return "Required list \"rolls\" is empty";
            }
            else
            {
                foreach(Roll roll in rolls)
                {
                    foreach(string error in roll.ConfigErrors())
                    {
                        yield return error;
                    }
                }
            }
            if(triggerAsRole && roleToTrigger == VoreRole.Invalid)
            {
                yield return "When using \"triggerAsRole\" a \"roleToTrigger\" must be set";
            }
        }
    }
}
