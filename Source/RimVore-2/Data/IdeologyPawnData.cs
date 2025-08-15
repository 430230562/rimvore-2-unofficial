using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class IdeologyPawnData : IExposable
    {
        private const int ticksInADay = 60000;

        private Dictionary<string, int> KeywordTicks = new Dictionary<string, int>();

        public bool KeywordExpired(string keyword, int daysNeeded)
        {
            if(!KeywordTicks.ContainsKey(keyword))
            {
                KeywordTicks.Add(keyword, GenTicks.TicksGame);
            }
            return GenTicks.TicksGame - KeywordTicks.TryGetValue(keyword) > daysNeeded * ticksInADay;
        }

        public void UpdateKeyword(string keyword)
        {
            KeywordTicks.SetOrAdd(keyword, GenTicks.TicksGame);
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref KeywordTicks, "KeywordTicks", LookMode.Value);
            if(Scribe.mode == LoadSaveMode.LoadingVars && KeywordTicks == null)
            {
                KeywordTicks = new Dictionary<string, int>();
            }
        }
    }
}
