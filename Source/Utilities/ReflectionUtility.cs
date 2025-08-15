using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public static class ReflectionUtility
    {
        public static void StartUp()
        {
            ModDirectory = LoadedModManager.RunningMods.Single(m => m.PackageId == "nabber.rimvore2").RootDir;
        }

        public static string ModDirectory;

        /// <summary>
        /// Use reflection to retrieve all fields of type T in a given DefOf
        /// </summary>
        /// <typeparam name="T">The type of field to retrieve</typeparam>
        /// <param name="defOf">The DefOf class to retrieve from</param>
        /// <returns>Enumeration containing all fields of type T</returns>
        /// <exception cref="ArgumentException">The given object does not have the DefOf attribute</exception>
        public static IEnumerable<T> AllDefOfEntries<T>(Type type)
        {
            if(!type.HasAttribute<DefOf>())
            {
                throw new ArgumentException("Tried to retrieve all entries of a non-DefOf attributed class");
            }
            return type.GetFields()
                .Where(f => f.FieldType == typeof(T))
                .Select(f => f.GetValue(type))
                .Cast<T>();
        }
    }
}
