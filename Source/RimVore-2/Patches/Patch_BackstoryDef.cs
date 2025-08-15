using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    /// <summary>
    /// Prevents a possible race condition of defdatabase access during loading of backstory defs
    /// Called from <see cref="RV2Mod.InitializeEarlyPatches"/>
    /// </summary>
    public static class Patch_BackstoryDef
    {
        static object defDatabaseLock = new object();


        public static void LockDefDatabaseIfUninitialized(List<WorkTypeDef> ___cachedDisabledWorkTypes)
        {
            if (___cachedDisabledWorkTypes == null)
            {
                Monitor.Enter(defDatabaseLock);
            }
        }

        public static void UnlockDefDatabase()
        {
            if (Monitor.IsEntered(defDatabaseLock))
            {
                Monitor.Exit(defDatabaseLock);
            }
        }
    }
}