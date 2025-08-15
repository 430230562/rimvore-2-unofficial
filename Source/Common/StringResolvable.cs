using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    /// <summary>
    /// This class allows scribing Def values without causing game-launch exceptions due to unloaded defs, especially useful in settings
    /// </summary>
    /// <typeparam name="T1">The Def type to scribe</typeparam>
    /// <typeparam name="T2">The type of value to scribe</typeparam>
    public class StringResolvable<T1, T2> : IDictionary<T1, T2>, IExposable where T1 : Def
    {
        LookMode lookMode;
        Dictionary<string, T2> internalDict;
        T1 Resolve(string key) => DefDatabase<T1>.GetNamed(key);

        /// <summary>
        /// Do not use this for initialization, use <see cref="StringResolvable(LookMode)"/> 
        /// </summary>
        [Obsolete]
        public StringResolvable() { }

        public StringResolvable(LookMode lookMode)
        {
            this.lookMode = lookMode;
            internalDict = new Dictionary<string, T2>();
        }

        public T2 this[T1 key]
        {
            get => internalDict.TryGetValue(key.defName, default(T2));
            set => internalDict.SetOrAdd(key.defName, value);
        }
        public T2 this[string key]
        {
            get => internalDict.TryGetValue(key, default(T2));
            set => internalDict.SetOrAdd(key, value);
        }

        public ICollection<T1> Keys => internalDict.Keys.Select(key => Resolve(key)).ToList();
        public ICollection<string> UnresolvedKeys => internalDict.Keys;

        public ICollection<T2> Values => internalDict.Values;

        public int Count => internalDict.Count;

        public bool IsReadOnly => false;

        public void Add(T1 key, T2 value)
        {
            internalDict.Add(key.defName, value);
        }

        public void Add(KeyValuePair<T1, T2> item)
        {
            internalDict.Add(new KeyValuePair<string, T2>(item.Key.defName, item.Value));
        }

        public void SetOrAdd(T1 key, T2 value)
        {
            internalDict.SetOrAdd(key.defName, value);
        }

        public void Sync(List<T1> keys, T2 defaultValue = default(T2), Func<string, T2> defaultValueFunc = null)
        {
            Sync(keys.ConvertAll(k => k.defName), defaultValue, defaultValueFunc);
        }
        public void Sync(List<string> keys, T2 defaultValue = default(T2), Func<string, T2> defaultValueFunc = null)
        {
            ScribeUtilities.SyncKeys(ref internalDict, keys, defaultValue, defaultValueFunc);
        }

        public void Clear()
        {
            internalDict.Clear();
        }

        public bool Contains(KeyValuePair<T1, T2> item)
        {
            return internalDict.Contains(new KeyValuePair<string, T2>(item.Key.defName, item.Value));
        }

        public bool ContainsKey(T1 key)
        {
            return internalDict.ContainsKey(key.defName);
        }

        public void CopyTo(KeyValuePair<T1, T2>[] array, int arrayIndex)
        {
            if(array == null)
            {
                throw new ArgumentNullException("array");
            }
            if(arrayIndex < 0 || arrayIndex > array.Length)
            {
                throw new ArgumentOutOfRangeException("arrayIndex", arrayIndex, "Index was out of range. Must be non-negative and less than the size of the collection.");
            }
            if(array.Length - arrayIndex < this.Count)
            {
                throw new ArgumentException("Destination array is not long enough to copy all the items in the collection. Check array index and length.");
            }

            int num = internalDict.Count;
            IEnumerator<KeyValuePair<T1, T2>> enumerator = this.GetEnumerator();
            for(int i = 0; i < num; i++)
            {
                array[arrayIndex++] = enumerator.Current;
                enumerator.MoveNext();
            }
        }

        public IEnumerator<KeyValuePair<T1, T2>> GetEnumerator()
        {
            return internalDict
                .Select(kvp => new KeyValuePair<T1, T2>(Resolve(kvp.Key), kvp.Value))
                .GetEnumerator();
        }

        public bool Remove(T1 key)
        {
            return internalDict.Remove(key.defName);
        }

        public bool Remove(KeyValuePair<T1, T2> item)
        {
            return internalDict.Remove(item.Key.defName);
        }

        public bool TryGetValue(T1 key, out T2 value)
        {
            return internalDict.TryGetValue(key.defName, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return internalDict
                .Select(kvp => new KeyValuePair<T1, T2>(Resolve(kvp.Key), kvp.Value))
                .GetEnumerator();
        }
        public void ExposeData()
        {
            Scribe_Values.Look(ref lookMode, "lookMode");
            Scribe_Collections.Look(ref internalDict, "internalDict", lookMode);
        }
    }
}
