using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimVore2
{
    public static class ScribeUtilities
    {
        public static void ScribeVariableDictionary<K, V>(ref Dictionary<K, V> options, string key, LookMode lookMode1 = LookMode.Value, LookMode lookMode2 = LookMode.Value)
        {
            if(Scribe.mode == LoadSaveMode.Saving)
            {
                if(!options.EnumerableNullOrEmpty())
                {
                    options.RemoveAll(kvp => kvp.Value == null);
                }
            }
            Scribe_Collections.Look(ref options, key, lookMode1, lookMode2);
            if(Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if(options == null)
                {
                    options = new Dictionary<K, V>();
                }
            }
        }

        public static void SyncKeys<K, V>(ref Dictionary<K, V> dictionary, List<K> providedKeys, V defaultValue = default(V), Func<K, V> defaultValueFunc = null)
        {
            if(dictionary == null)
            {
                dictionary = new Dictionary<K, V>();
            }
            if(providedKeys.NullOrEmpty())
            {
                RV2Log.Warning("No keys provided to sync with");
                return;
            }

            List<K> enabledKeys = dictionary.Keys?.ToList();
            List<K> keysToAdd;
            // if we have no active keys, don't bother to intersect and remove keys
            if(!enabledKeys.NullOrEmpty())
            {
                // find keys to leave alone (exist in both lists)
                List<K> overlappingKeys = enabledKeys.Intersect(providedKeys).ToList();
                // every currently active key not found in the overlap must be removed
                List<K> keysToRemove = enabledKeys.FindAll(key => !overlappingKeys.Contains(key));
                // every new key not currently active must be added
                keysToAdd = providedKeys.FindAll(key => !overlappingKeys.Contains(key));

                // all keys that are not in the provided list of keys must have been removed or renamed to something else, prune those keys
                if(!keysToRemove.NullOrEmpty())
                {
                    if(RV2Log.ShouldLog(false, "SyncKeys"))
                        RV2Log.Message($"The given dictionary contains keys that are not in the provided keys: {string.Join(", ", keysToRemove)} ", false, "SyncKeys");
                    foreach(K key in keysToRemove)
                    {
                        dictionary.Remove(key);
                    }
                }
            }
            // in case we had no enabled keys, all provided keys need to be added
            else
            {
                keysToAdd = providedKeys;
            }
            if(!keysToAdd.NullOrEmpty())
            {
                if(RV2Log.ShouldLog(false, "SyncKeys"))
                    RV2Log.Message($"The following keys were not found in the given dictionary: {string.Join(", ", keysToAdd)} ", false, "SyncKeys");
                foreach(K key in keysToAdd)
                {
                    V value;
                    if(defaultValueFunc != null)
                    {
                        value = defaultValueFunc(key);
                    }
                    else
                    {
                        value = defaultValue;
                    }
                    if(key == null)
                    {
                        continue;
                    }
                    if(RV2Log.ShouldLog(true, "Debug"))
                        RV2Log.Message($"SyncKeys could not find key {key} in dictionary and added the key from the provided keys with default value {value}", false, "Debug");
                    if(dictionary.ContainsKey(key))
                    {
                        Log.Warning("For some reason the SyncKeys tried to add a key that already exists! Skipping the key insertion!");
                        continue;
                    }
                    dictionary.Add(key, value);
                }
            }
        }
    }
}
