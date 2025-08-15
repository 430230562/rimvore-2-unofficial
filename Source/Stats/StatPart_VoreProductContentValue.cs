using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class StatPart_VoreProductContentValue : StatPart
    {
        /// <remarks>
        /// First line, no indents: label and value
        /// (Each thing in container)
        /// \t  {label}: {value}
        /// \t\t    full value explanation double-indented
        /// </remarks>
        public override string ExplanationPart(StatRequest req)
        {
            IEnumerable<Thing> contents = GetContentsOfContainer(req);
            if(contents.EnumerableNullOrEmpty())
            {
                return null;
            }
            List<string> contentExplanations = new List<string>();
            foreach(Thing thing in contents)
            {
                float marketValue = thing.GetStatValue(StatDefOf.MarketValue);
                //string subExplanation = StatDefOf.MarketValue.Worker.GetExplanationFull(StatRequest.For(thing), StatDefOf.MarketValue.toStringNumberSense, marketValue);

                string thingExplanation = $"    {thing.LabelCap}: {marketValue.ToStringByStyle(StatDefOf.MarketValue.toStringStyle)}";
                //thingExplanation += "\t\t" + subExplanation.Replace("\n", "\n\t\t");
                contentExplanations.Add(thingExplanation);
            }
            string fullExplanation = $"{"RV2_StatsReport_VoreProductContainerContents".Translate()}: {req.Thing.GetStatValue(StatDefOf.MarketValue).ToStringByStyle(StatDefOf.MarketValue.toStringStyle)}";
            fullExplanation += "\n" + string.Join("\n", contentExplanations);
            return fullExplanation;
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            IEnumerable<Thing> contents = GetContentsOfContainer(req);
            if(contents.EnumerableNullOrEmpty())
            {
                return;
            }

            val += contents.Sum(thing => thing.GetStatValue(StatDefOf.MarketValue));
        }

        private IEnumerable<Thing> GetContentsOfContainer(StatRequest req)
        {
            if(!(req.Thing is VoreProductContainer container))
            {
                return new List<Thing>();
            }
            return container.GetDirectlyHeldThings();
        }
    }
}
