using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class FilledDisposalContainerGraphics : DefModExtension
    {
        public Graphic GetGraphic(List<Thing> things)
        {
            GraphicData graphicData = baseGraphic;
            IEnumerable<ThingDef> defs = things
                .Select(t => t.def)
                .Distinct();
            if(!triggers.NullOrEmpty())
            {
                // check all triggers for matching ones
                TriggeringGraphic triggerGraphicData = triggers
                    .Find(trigger => defs.Contains(trigger.triggeringItem));
                if(triggerGraphicData != null)
                {
                    if(RV2Log.ShouldLog(false, "DisposalContainer"))
                        RV2Log.Message($"Found proper trigger for disposal container, triggering item def: {triggerGraphicData.triggeringItem.defName}", "DisposalContainer");
                    graphicData = triggerGraphicData.graphic;
                }
            }
            return graphicData.Graphic;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if(baseGraphic == null)
            {
                yield return "Required field \"baseGraphic\" is not set";
            }
            foreach(TriggeringGraphic trigger in triggers)
            {
                foreach(string error in trigger.ConfigErrors())
                {
                    yield return error;
                }
            }
        }

        GraphicData baseGraphic;
        List<TriggeringGraphic> triggers = new List<TriggeringGraphic>();
    }

    public class TriggeringGraphic
    {
        public ThingDef triggeringItem;
        public GraphicData graphic;

        public IEnumerable<string> ConfigErrors()
        {
            if(triggeringItem == null)
            {
                yield return "Required field \"triggeringItem\" is not set";
            }
            if(graphic == null)
            {
                yield return "Required field \"graphic\" is not set";
            }
        }
    }
}
