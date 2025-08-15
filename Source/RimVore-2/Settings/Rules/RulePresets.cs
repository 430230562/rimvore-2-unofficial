using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public static class RulePresets
    {
        /// <summary>
        /// Scribing is fucking dogshit, I can't use a LookMode to scribe a collection, so I have to encapsulate this dict and use LookMode.Deep
        /// </summary>

        public static KeyValuePair<string, VoreRulePreset> NabbersChoice()
        {
            List<RuleEntry> rules = new List<RuleEntry>();

            // default constructor creates for everyone
            RuleTarget targetEveryone = new RuleTarget();
            VoreRule ruleDisableFatal = new VoreRule(RuleState.On) { };
            ruleDisableFatal.DesignationStates[RV2DesignationDefOf.fatal.defName] = RuleState.Off;
            rules.Add(new RuleEntry(targetEveryone, ruleDisableFatal));
            // ----------------------------------------------------------------- 
            // all animals
            RuleTarget targetAllAnimals = new RuleTarget(
                new RuleTargetComponentTree(
                    new RuleTargetComponentNode_Leaf()
                    {
                        component = new RuleTargetComponent_ColonyRelation(RuleTargetRole.All, RelationKind.Animal)
                    }
                )
            )
            {
                customName = "Animals ignore age and only vore orally",
            };
            VoreRule ruleIgnoreAgeAndOralOnly = new VoreRule(RuleState.Copy)
            {
                ConsiderMinimumAge = RuleState.Off
            };
            ruleIgnoreAgeAndOralOnly.DesignationStates[RV2DesignationDefOf.fatal.defName] = RuleState.On;
            foreach(VorePathRule pathRule in ruleIgnoreAgeAndOralOnly.AllPathRules())
            {
                if(pathRule.VorePath.voreType != VoreTypeDefOf.Oral)
                {
                    pathRule.Enabled = false;
                }
            }
            rules.Add(new RuleEntry(targetAllAnimals, ruleIgnoreAgeAndOralOnly));
            // -----------------------------------------------------------------
            RuleTarget targetCarnivorousAnimals = RuleTarget.ForCarnivorousAnimals(RuleTargetRole.Predator);
            targetCarnivorousAnimals.customName = "Carnivorous Animals may fatally vore others";
            VoreRule ruleFatalDesignationEnabled = new VoreRule() { };
            ruleFatalDesignationEnabled.DesignationStates[RV2DesignationDefOf.fatal.defName] = RuleState.On;
            rules.Add(new RuleEntry(targetCarnivorousAnimals, ruleFatalDesignationEnabled));
            // -----------------------------------------------------------------
            RuleTarget targetVisitorsOrTraders = RuleTarget.ForVisitorsOrTraders(RuleTargetRole.All);
            targetVisitorsOrTraders.customName = "No vore for visitors";
            VoreRule ruleDesignationsDisabled = new VoreRule(RuleState.Copy) { };
            ruleDesignationsDisabled.DesignationStates[RV2DesignationDefOf.predator.defName] = RuleState.Off;
            ruleDesignationsDisabled.DesignationStates[RV2DesignationDefOf.endo.defName] = RuleState.Off;
            ruleDesignationsDisabled.DesignationStates[RV2DesignationDefOf.fatal.defName] = RuleState.Off;
            rules.Add(new RuleEntry(targetVisitorsOrTraders, ruleDesignationsDisabled));
            // ----------------------------------------------------------------- 
            RuleTarget targetPrisonersOrSlaves = RuleTarget.ForPrisonersOrSlaves(RuleTargetRole.Prey);
            targetPrisonersOrSlaves.customName = "Prisoners and slaves may be fatally vored";
            // clone the rule so that changes don't affect both rules
            VoreRule ruleFatalDesignationEnabledClone = (VoreRule)ruleFatalDesignationEnabled.Clone();
            rules.Add(new RuleEntry(targetPrisonersOrSlaves, ruleFatalDesignationEnabledClone));


            return new KeyValuePair<string, VoreRulePreset>("Nabbers' Recommendation", new VoreRulePreset(rules));
        }
    }
}
