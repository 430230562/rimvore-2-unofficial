using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using System.Text;
using System.Threading.Tasks;

namespace RimVore2
{
    [StaticConstructorOnStartup]
    public static class RV2Log
    {
        public static int LogMessagesToCache = -1; // set to -1 to cache all (not sure if that's a good idea)
        public static HashSet<int> usedKeys = new HashSet<int>();
        public static HashSet<int> UsedKeys
        {
            get
            {
                // do we only log a certain amount of messages and is the count of used keys above our limit
                // no need to check for key existence, HashSet simply returns false if key could not be found
                bool needToRemoveFirst = LogMessagesToCache >= 0 && usedKeys.Count > LogMessagesToCache;
                if(needToRemoveFirst)
                {
                    usedKeys.Remove(usedKeys.First());
                }
                return usedKeys;
            }
        }

        public static bool ShouldLog(bool verbose = false, string category = null)
        {
            if(!RV2Mod.Settings.debug.Logging)
            {
                return false;
            }
            if(verbose && !RV2Mod.Settings.debug.VerboseLogging)
            {
                return false;
            }
            if(category != null && !RV2Mod.Settings.debug.AllowedToLog(category))
            {
                return false;
            }
            return true;
        }

        public static void Message(string text, string category)
        {
            Message(text, false, category);
        }
        public static void Message(string text, bool once = false, string category = null, int hashOverride = 0)
        {
            DoLog(text, once, category, hashOverride, LogLevel.Message);
        }

        public static void Warning(string text, string category)
        {
            Warning(text, false, category);
        }
        public static void Warning(string text, bool once = false, string category = null, int hashOverride = 0)
        {
            Action<string> logAction = (string s) => Log.Warning(s);
            DoLog(text, once, category, hashOverride, LogLevel.Warning);
        }

        public static void Error(string text, string category)
        {
            Error(text, false, category);
        }
        public static void Error(string text, bool once = false, string category = null, int hashOverride = 0)
        {
            Action<string> logAction = (string s) => Log.Warning(s);
            DoLog(text, once, category, hashOverride, LogLevel.Error);
        }

        public static void DoLog(string text, bool once, string category, int hash, LogLevel logLevel)
        {
            if(category != null)
            {
                // prevent log message if category is filtered out by debug settings
                if(!RV2Mod.Settings.debug.AllowedToLog(category))
                {
                    return;
                }

                text = "RV2_" + category + ": " + text;
            }

            Action<string> logAction = (string s) => Log.Message(s);
            switch(logLevel)
            {
                case LogLevel.Warning:
                    logAction = (string s) => Log.Warning(s);
                    break;
                case LogLevel.Error:
                    logAction = (string s) => Log.Error(s);
                    break;
            }
            if(once)
            {
                if(hash == 0)
                {
                    hash = text.GetHashCode();
                }

                LogOnce(text, hash, logAction);
            }
            else
            {
                logAction(text);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="text"></param>
        private static void LogOnce(string text, int hash = 0, Action<string> logAction = null)
        {
            if(logAction == null)
            {
                logAction = (string s) => Log.Message(s);
            }
            if(hash == 0)
            {
                hash = text.GetHashCode();
            }
            if(UsedKeys.Contains(hash))
            {
                return;
            }
            UsedKeys.Add(hash);
            /**/
            logAction(text);
        }

    }

    public enum LogLevel
    {
        Message,
        Warning,
        Error
    }
}
