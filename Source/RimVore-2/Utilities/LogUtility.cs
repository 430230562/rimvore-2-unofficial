using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public static class LogUtility
    {
        public static string ToString<K, V>(this Dictionary<K, V> dictionary)
        {
            List<string> stringifiedEntries = new List<string>();
            foreach(KeyValuePair<K, V> kvp in dictionary)
            {
                string entry = kvp.Key.ToString() + " : " + kvp.Value.ToString();
                stringifiedEntries.Add(entry);
            }
            return "key type: " +
                typeof(K).ToString() +
                " | value type: " +
                typeof(V).ToString() +
                "\n" +
                string.Join("\n", stringifiedEntries);
        }

        public static string PresentFloatRange(float value1, float value2)
        {
            return PresentFloat(value1) + " - " + PresentFloat(value2);
        }
        public static string PresentFloat(float value, int digits = 2)
        {
            if(value == float.MaxValue) return "∞";
            else if(value == float.MinValue) return "-∞";
            return value.ToString();
        }
        public static string PresentFloatRange(FloatRange floatRange)
        {
            return PresentFloatRange(floatRange.min, floatRange.max);
        }

        public static void LogConfigErrors<T>(IEnumerable<string> errors)
        {
            foreach(string text in errors)
            {
                Log.Error(string.Concat(new object[]
                {
                    "Config error in ",
                    typeof(T).ToString(),
                    ": ",
                    text
                }));
            }
        }

        const int ticksPerIngameHour = 2500;
        const int secondsInMinute = 60;
        const int minutesInHour = 60;
        const int hoursInDay = 24;
        const int daysInWeek = 7;

        public static string PresentIngameTime(int ticks)
        {
            int hours = ticks / ticksPerIngameHour;
            int days = hours / hoursInDay;
            hours -= days * hoursInDay;
            int weeks = days / daysInWeek;
            days -= weeks * daysInWeek;
            List<string> strings = new List<string>();
            if(weeks > 0)
                strings.Add("RV2_Time_Weeks".Translate(weeks));
            if(days > 0)
                strings.Add("RV2_Time_Days".Translate(days));
            if(hours > 0)
                strings.Add("RV2_Time_Hours".Translate(hours));
            return string.Join(", ", strings);
        }

        public static string PresentRealTime(int ticks)
        {
            int seconds = ticks / GenTicks.TicksPerRealSecond;
            int minutes = seconds / secondsInMinute;
            seconds -= minutes * secondsInMinute;
            int hours = minutes / minutesInHour;
            minutes -= hours * minutesInHour;
            int days = hours / hoursInDay;
            hours -= days * hoursInDay;
            List<string> strings = new List<string>();
            if(days > 0)
                strings.Add("RV2_Time_Days".Translate(days));
            if(hours > 0)
                strings.Add("RV2_Time_Hours".Translate(hours));
            if(minutes > 0)
                strings.Add("RV2_Time_Minutes".Translate(minutes));
            if(seconds > 0)
                strings.Add("RV2_Time_Seconds".Translate(seconds));
            return string.Join(", ", strings);
        }

        public static string QuantifyThings(IEnumerable<Thing> things)
        {
            if(things.EnumerableNullOrEmpty())
            {
                return "RV2_ThingList_Empty".Translate();
            }
            Dictionary<ThingDef, int> thingCounts = new Dictionary<ThingDef, int>();
            foreach(Thing thing in things)
            {
                if(thingCounts.ContainsKey(thing.def))
                {
                    thingCounts[thing.def] += thing.stackCount;
                }
                else
                {
                    thingCounts.Add(thing.def, thing.stackCount);
                }
            }
            IEnumerable<string> quantifiedThings = thingCounts
                .OrderBy(t => t.Value)
                .Select(kvp => kvp.Value + "x " + kvp.Key.label);
            return string.Join(", ", quantifiedThings);
        }
    }
}
