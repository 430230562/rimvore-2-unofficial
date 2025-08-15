using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class SettingsContainer_Quirks : SettingsContainer
    {
        public SettingsContainer_Quirks() { }

        StringResolvable<QuirkDef, bool> enabledQuirks = new StringResolvable<QuirkDef, bool>(LookMode.Value);
        StringResolvable<QuirkPoolDef, bool> enabledPools = new StringResolvable<QuirkPoolDef, bool>(LookMode.Value);
        StringResolvable<QuirkDef, QuirkRarity> quirkRarityOverrides = new StringResolvable<QuirkDef, QuirkRarity>(LookMode.Value);

        public Dictionary<string, QuirkPreset> QuirkPresets = new Dictionary<string, QuirkPreset>();

        public float GetChanceForWeight(QuirkDef quirk, bool considerOverrides = true)
        {
            if(considerOverrides)
            {
                int totalWeight = GetEnabledQuirks(quirk.GetPool()).Sum(q => (int)GetRarity(q));
                if(totalWeight <= 0)
                {
                    return 0;
                }
                return (float)GetRarity(quirk) / totalWeight;
            }
            else
            {
                int totalWeight = GetEnabledQuirks(quirk.GetPool()).Sum(q => (int)q.rarity);
                if(totalWeight <= 0)
                {
                    return 0;
                }
                return (float)quirk.rarity / totalWeight;
            }
        }
        #region quirk based settings
        public void SetQuirkEnabled(QuirkDef quirk, bool value)
        {
            if(GetRarity(quirk) == QuirkRarity.Invalid)
            {
                enabledQuirks.SetOrAdd(quirk, false);
                return;
            }
            enabledQuirks.SetOrAdd(quirk, value);
        }
        public bool IsQuirkEnabled(QuirkDef quirk)
        {
            if(enabledQuirks.ContainsKey(quirk))
            {
                return enabledQuirks[quirk];
            }
            RV2Log.Warning("QuirkPool " + quirk.defName + " was not present in enabledPools dictionary. Possible Sync issue!");
            return false;
        }
        public bool IsQuirkEnabledAndValid(QuirkDef quirk)
        {
            if(IsQuirkEnabled(quirk) == false)
            {
                return false;
            }
            return GetRarity(quirk) != QuirkRarity.Invalid;
        }
        public QuirkRarity GetRarity(QuirkDef quirk)
        {
            if(quirkRarityOverrides.ContainsKey(quirk))
            {
                return quirkRarityOverrides[quirk];
            }
            return quirk.rarity;
        }
        public void SetRarityOverride(QuirkDef quirk, QuirkRarity rarity)
        {
            quirkRarityOverrides.SetOrAdd(quirk, rarity);
            if(rarity != QuirkRarity.Invalid && !IsQuirkEnabled(quirk))
            {
                SetQuirkEnabled(quirk, true);
            }
        }
        public void RemoveAllRarityOverrides()
        {
            quirkRarityOverrides.Clear();
        }
        #endregion

        #region pool related settings
        public List<QuirkDef> GetEnabledQuirks(QuirkPoolDef pool)
        {
            return pool.quirks.FindAll(quirk => IsQuirkEnabled(quirk));
        }

        public void SetPoolEnabled(QuirkPoolDef pool, bool value)
        {
            enabledPools.SetOrAdd(pool, value);
        }
        public bool IsPoolEnabled(QuirkPoolDef pool)
        {
            if(enabledPools.ContainsKey(pool))
            {
                return enabledPools[pool];
            }
            RV2Log.Warning("QuirkPool " + pool.defName + " was not present in enabledPools dictionary. Possible Sync issue!");
            return false;
        }
        public bool IsPoolEnabledAndValid(QuirkPoolDef pool)
        {
            if(!IsPoolEnabled(pool))
            {
                return false;
            }
            return pool.quirks.Any(quirk => IsQuirkEnabledAndValid(quirk));
        }
        public void SetAllPoolsAndQuirks(bool value)
        {
            foreach(QuirkPoolDef pool in enabledPools.Keys)
            {
                SetPoolAndAllQuirks(pool, value);
            }
        }
        public void SetPoolAndAllQuirks(QuirkPoolDef pool, bool value)
        {
            SetPoolEnabled(pool, value);
            foreach(QuirkDef quirk in pool.quirks)
            {
                SetQuirkEnabled(quirk, value);
            }
        }
        #endregion

        public void RemoveAllRarityOverrides(QuirkPoolDef pool)
        {
            foreach(QuirkDef quirk in pool.quirks)
            {
                if(quirkRarityOverrides.ContainsKey(quirk))
                {
                    quirkRarityOverrides.Remove(quirk);
                    SetQuirkEnabled(quirk, true);
                }
            }
        }
        public void OverrideAllRarities(QuirkPoolDef pool, QuirkRarity rarity)
        {
            foreach(QuirkDef quirk in pool.quirks)
            {
                quirkRarityOverrides.SetOrAdd(quirk, rarity);
                // trying to set to true with rarity = Invalid will auto-set to false
                SetQuirkEnabled(quirk, true);
            }
            bool canPoolBeEnabled = pool.quirks.Any(quirk => IsQuirkEnabled(quirk));
            SetPoolEnabled(pool, canPoolBeEnabled);
        }

        public override void Reset()
        {
            enabledQuirks.Clear();
            enabledPools.Clear();
            quirkRarityOverrides.Clear();
            enabledQuirks.Sync(DefDatabase<QuirkDef>.AllDefsListForReading, true);
            enabledPools.Sync(DefDatabase<QuirkPoolDef>.AllDefsListForReading, true);
        }

        public override void ExposeData()
        {
            Scribe_Deep.Look(ref enabledQuirks, "enabledQuirks", new object[0]);
            Scribe_Deep.Look(ref enabledPools, "enabledPools", new object[0]);
            Scribe_Deep.Look(ref quirkRarityOverrides, "quirkRarityOverrides", new object[0]);
            Scribe_Collections.Look(ref QuirkPresets, "QuirkPresets", LookMode.Value, LookMode.Deep);
            // additional safety during loading, a single scribing failure will result in any StringResolvable to be NULL permanently. May need to find a solution
            if(Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if(enabledQuirks == null) enabledQuirks = new StringResolvable<QuirkDef, bool>(LookMode.Value);
                if(enabledPools == null) enabledPools = new StringResolvable<QuirkPoolDef, bool>(LookMode.Value);
                if(quirkRarityOverrides == null) quirkRarityOverrides = new StringResolvable<QuirkDef, QuirkRarity>(LookMode.Value);
                if(QuirkPresets == null) QuirkPresets = new Dictionary<string, QuirkPreset>();
            }
        }

        public override void DefsLoaded()
        {
            if(Scribe.mode == LoadSaveMode.Saving || Scribe.mode == LoadSaveMode.LoadingVars)
            {
                EnsureSmartSettingDefinition();
            }
            enabledQuirks.Sync(DefDatabase<QuirkDef>.AllDefsListForReading, true);
            enabledPools.Sync(DefDatabase<QuirkPoolDef>.AllDefsListForReading, true);
        }

        public override void EnsureSmartSettingDefinition()
        {
            // nothing to do here for the quirk settings, they are their own beas
        }
    }
}
