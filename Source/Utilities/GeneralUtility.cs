using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimVore2
{
    public static class GeneralUtility
    {
        public static IEnumerable<T> Move<T>(this IEnumerable<T> entries, int oldIndex, int newIndex)
        {
            int entryCount = entries.Count();
            if(oldIndex >= entryCount || newIndex >= entryCount || oldIndex < 0 || newIndex < 0)
            {
                Log.Warning($"index out of bounds: oldIndex: {oldIndex} newIndex: {newIndex} count: {entryCount}");
                return entries;
            }
            List<T> newEntries = new List<T>(entries);
            T removedEntry = newEntries.ElementAt(oldIndex);
            newEntries.RemoveAt(oldIndex);
            newEntries.Insert(newIndex, removedEntry);
            return newEntries.AsEnumerable();
        }
        public static void ForEach<T>(this IEnumerable<T> entries, Action<T> action)
        {
            if(entries.EnumerableNullOrEmpty() || action == null)
            {
                return;
            }
            foreach(T entry in entries)
            {
                action(entry);
            }
        }

        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source)
        {
            return source.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
        public static void Add<TKey, TValue>(this Dictionary<TKey, TValue> dict, KeyValuePair<TKey, TValue> entry)
        {
            dict.Add(entry.Key, entry.Value);
        }

        public static void AddDistinctRange<T>(this List<T> list, IEnumerable<T> entries)
        {
            foreach(T entry in entries)
            {
                if(!list.Contains(entry))
                {
                    list.Add(entry);
                }
            }
        }

        public static T Next<T>(this T enumValue, bool forward = true) where T : struct, IComparable, IFormattable, IConvertible
        {
            if(!typeof(T).IsEnum)
            {
                Log.Warning("Next<T>() called with type that is not an enum!");
                return enumValue;
            }
            List<T> values = GetValues<T>();
            int index = values.IndexOf(enumValue);
            if(forward)
            {
                index++;
            }
            else
            {
                index--;
            }
            index = index % values.Count;
            return values[index];
        }
        public static int Index<T>(this T enumValue) where T : struct, IComparable, IFormattable, IConvertible
        {
            if(!typeof(T).IsEnum)
            {
                Log.Warning("Index<T>() called with type that is not an enum!");
                return -1;
            }
            List<T> values = GetValues<T>();
            return values.IndexOf(enumValue);
        }
        public static List<T> GetValues<T>() where T : struct, IComparable, IFormattable, IConvertible
        {
            if(!typeof(T).IsEnum)
            {
                Log.Warning("GetValues<T>() called with type that is not an enum!");
                return null;
            }
            return Enum.GetValues(typeof(T))
                .Cast<T>()
                .ToList();
        }
        public static int Count<T>() where T : struct, IComparable, IFormattable, IConvertible
        {
            if(!typeof(T).IsEnum)
            {
                Log.Warning("Count<T>() called with type that is not an enum!");
                return -1;
            }
            return GetValues<T>().Count;
        }

        public static float LimitClamp(this float value, float minValue, float maxValue)
        {
            if(maxValue < minValue)
            {
                float tmp = minValue;
                minValue = maxValue;
                maxValue = tmp;
            }
            if(value < minValue)
                return minValue;
            if(value > maxValue)
                return maxValue;
            return value;
        }

        public static void SetOrAdd<K, V>(this SortedDictionary<K, V> dict, K key, V value)
        {
            if(dict.ContainsKey(key))
                dict[key] = value;
            else
                dict.Add(key, value);
        }

        public static bool IsInRange(this IntRange range, int value)
        {
            return value > range.min && value < range.max;
        }
        public static bool IsInRange(this FloatRange range, float value)
        {
            return value > range.min && value < range.max;
        }

        public static bool ContainsSubstring(this List<string> list, string entry)
        {
            entry = entry.ToLower();
            return list.Any(listEntry => listEntry.ToLower().Contains(entry));
        }

        public static bool ContainsAnyAsSubstring(this string entry, List<string> list)
        {
            entry = entry.ToLower();
            return list.Any(listEntry => entry.Contains(listEntry.ToLower()));
        }

        public static GrappleRole BoolToGrappleRole(bool isAttacker)
        {
            return isAttacker ? GrappleRole.Attacker : GrappleRole.Defender;
        }

        public static TargetingParameters Clone(this TargetingParameters original)
        {
            if(original == null)
                return null;

            return new TargetingParameters()
            {
                canTargetAnimals = original.canTargetAnimals,
                canTargetBuildings = original.canTargetBuildings,
                canTargetFires = original.canTargetFires,
                canTargetHumans = original.canTargetHumans,
                canTargetItems = original.canTargetItems,
                canTargetLocations = original.canTargetLocations,
                canTargetMechs = original.canTargetMechs,
                canTargetPawns = original.canTargetPawns,
                canTargetSelf = original.canTargetSelf,
                mapObjectTargetsMustBeAutoAttackable = original.mapObjectTargetsMustBeAutoAttackable,
                mustBeSelectable = original.mustBeSelectable,
                neverTargetDoors = original.neverTargetDoors,
                neverTargetHostileFaction = original.neverTargetHostileFaction,
                neverTargetIncapacitated = original.neverTargetIncapacitated,
                onlyTargetColonists = original.onlyTargetColonists,
                onlyTargetControlledPawns = original.onlyTargetControlledPawns,
                onlyTargetDamagedThings = original.onlyTargetDamagedThings,
                onlyTargetFactions = original.onlyTargetFactions == null ? null : new List<Faction>(original.onlyTargetFactions),
                onlyTargetFlammables = original.onlyTargetFlammables,
                onlyTargetIncapacitatedPawns = original.onlyTargetIncapacitatedPawns,
                onlyTargetPrisonersOfColony = original.onlyTargetPrisonersOfColony,
                onlyTargetPsychicSensitive = original.onlyTargetPsychicSensitive,
                onlyTargetThingsAffectingRegions = original.onlyTargetThingsAffectingRegions,
                targetSpecificThing = original.targetSpecificThing,
                thingCategory = original.thingCategory,
                validator = original.validator == null ? null : (Predicate<TargetInfo>)original.validator.Clone(),
                canTargetPlants = original.canTargetPlants,
                onlyTargetAnimaTrees = original.onlyTargetAnimaTrees,
                onlyTargetColonistsOrPrisoners = original.onlyTargetColonistsOrPrisoners,
                onlyTargetColonistsOrPrisonersOrSlaves = original.onlyTargetColonistsOrPrisonersOrSlaves,
                onlyTargetColonistsOrPrisonersOrSlavesAllowMinorMentalBreaks = original.onlyTargetColonistsOrPrisonersOrSlavesAllowMinorMentalBreaks,
                onlyTargetSameIdeo = original.onlyTargetSameIdeo,
            };
        }

        public static void NukeSettings()
        {
            Log.Warning("Calling Reset on RV2 settings");
            RV2Mod.Settings.Reset();
            RV2Mod.Settings.Write();
        }
    }
}
