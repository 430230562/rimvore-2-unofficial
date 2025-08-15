using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class VoreRulePreset : IExposable
    {
        public List<RuleEntry> rules;
        public VoreRulePreset() { }

        /// <note>
        /// We must clone each element, not doing so will lead to the loadIDs to be deep-scribed twice, causing fatal issues with scribing
        /// </note>
        public VoreRulePreset(List<RuleEntry> rules)
        {
            this.rules = rules
                .Select(rule => (RuleEntry)rule.Clone())
                .ToList();
        }

        /// <summary>
        /// Clones the list of rules in the preset into the active rules list
        /// </summary>
        /// <note>
        /// We must clone each element, not doing so will lead to the loadIDs to be deep-scribed twice, causing fatal issues with scribing
        /// </note>
        public void ApplyPreset()
        {
            RV2Mod.Settings.rules.Rules = this.rules
                .Select(presetRule => (RuleEntry)presetRule.Clone())
                .ToList();
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref rules, "rules", LookMode.Deep);
            //ScribeUtilities.ScribeVariableDictionary(ref rules, "rules", LookMode.Deep, LookMode.Deep);
        }
    }
}
