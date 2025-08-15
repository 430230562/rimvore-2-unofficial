using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public abstract class QuirkPicker
    {
        public abstract List<QuirkDef> GetQuirks();
        public abstract IEnumerable<string> ConfigErrors();
    }
    public class QuirkPicker_All : QuirkPicker
    {
        public List<QuirkDef> quirks;

        public override List<QuirkDef> GetQuirks()
        {
            return quirks;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            if(quirks.NullOrEmpty()) yield return "No quirks set";
        }

    }
    public class QuirkPicker_RollAll : QuirkPicker
    {
        public List<RollableQuirk> quirks;

        public override List<QuirkDef> GetQuirks()
        {
            List<QuirkDef> resultQuirks = new List<QuirkDef>();
            foreach(RollableQuirk rollableQuirk in quirks)
            {
                if(Rand.Chance(rollableQuirk.chanceOrWeight))
                {
                    resultQuirks.Add(rollableQuirk.quirk);
                }
            }
            return resultQuirks;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            if(quirks.EnumerableNullOrEmpty())
            {
                yield return "required list \"quirks\" is null or empty";
                yield break;
            }
            foreach(RollableQuirk rQuirk in quirks)
            {
                foreach(string error in rQuirk.ConfigErrors())
                {
                    yield return error;
                }
            }
        }
    }
    public class QuirkPicker_RollOne : QuirkPicker
    {
        public List<RollableQuirk> quirks;

        public override List<QuirkDef> GetQuirks()
        {
            QuirkDef pickedQuirk = RandomUtility.RandomElementByWeight(quirks, (RollableQuirk rq) => rq.chanceOrWeight).quirk;
            return new List<QuirkDef>() { pickedQuirk };
        }

        public override IEnumerable<string> ConfigErrors()
        {
            if(quirks.EnumerableNullOrEmpty())
            {
                yield return "required list \"quirks\" is null or empty";
                yield break;
            }
            foreach(RollableQuirk rQuirk in quirks)
            {
                foreach(string error in rQuirk.ConfigErrors())
                {
                    yield return error;
                }
            }
        }
    }

    public class RollableQuirk
    {
        public readonly float chanceOrWeight;
        public readonly QuirkDef quirk;

        public IEnumerable<string> ConfigErrors()
        {
            if(chanceOrWeight <= 0) yield return "Chance/weight to roll must be larger than 0";
            if(quirk == null) yield return "Required field \"quirk\" not set";
        }
    }
}
/*<forcedQuirks>
  <li Class="All">
      <quirks>
        <li>PurePredator</li>
        <li>MetalMuncher</li>
      <quirks>
  </li>
  <li Class="RollAll">
      <quirks>
        <li>
          <chanceOrWeight></chanceOrWeight>
          <quirk></quirk>
        </li>
        <li>
          <chanceOrWeight></chanceOrWeight>
          <quirk></quirk>
        </li>
      <quirks>
  </li>
  <li Class="RollOne">
      <quirks>
        <li>
          <chanceOrWeight></chanceOrWeight>
          <quirk></quirk>
        </li>
        <li>
          <chanceOrWeight></chanceOrWeight>
          <quirk></quirk>
        </li>
      <quirks>
  </li>
</forcedQuirks>
*/
