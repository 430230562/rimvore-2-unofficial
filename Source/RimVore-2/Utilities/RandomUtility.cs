using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimVore2
{
    public static class RandomUtility
    {
        private static Random random = new Random();

        public static float GetRandomFloat()
        {
            return (float)random.NextDouble();
        }

        /// <summary>
        /// Take a value and add / subtract a rolled value
        /// </summary>
        /// <param name="value">Value to add / subtract variance to / from</param>
        /// <param name="variance">Fraction max</param>
        /// <returns></returns>
        public static float ApplyVariance(float value, float variance)
        {
            float varianceValue = (RandomUtility.GetRandomFloat() * 2 - 1) * (value * variance);
            return value + varianceValue;
        }

        /// <summary>
        /// When provided with a sequence and an accessor to the weight value, will return a random element in the sequence, factoring in weight
        /// This can be called with negative values, in which case it will offset all values by the largest negative value (all values will become positive during picking)
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="sequence">Sequence to select a random value from</param>
        /// <param name="weightSelector">Accessor for weight value in object</param>
        /// <returns>Element randomly chosen by weight from collection</returns>
        /// <remarks>shamelessly copied from https://stackoverflow.com/questions/56692/random-weighted-choice </remarks>
        public static T RandomElementByWeight<T>(this IEnumerable<T> sequence, Func<T, float> weightSelector)
        {
            IEnumerable<T> items = sequence.ToList();

            float totalWeight = items.Sum(x => weightSelector(x));
            float randomWeightedIndex = GetRandomFloat() * totalWeight;
            float itemWeightedIndex = 0f;
            foreach(T item in items)
            {
                itemWeightedIndex += weightSelector(item);
                if(randomWeightedIndex < itemWeightedIndex)
                    return item;
            }
            Log.Warning("RandomElementByWeight<T>() called, but Enumeration was empty, returning default");
            return default(T);
        }

    }
}
