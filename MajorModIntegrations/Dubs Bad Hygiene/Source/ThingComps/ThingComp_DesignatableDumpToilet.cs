using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RV2_DBH
{
    public class ThingCompProperties_DesignatableDumpToilet : CompProperties
    {
        public ThingCompProperties_DesignatableDumpToilet() : base()
        {
            compClass = typeof(ThingComp_DesignatableDumpToilet);
        }
    }

    public class ThingComp_DesignatableDumpToilet : ThingComp
    {
        public bool isEnabled = true;

        public override void CompTick()
        {
            base.CompTick();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach(Gizmo baseGizmo in base.CompGetGizmosExtra())
            {
                yield return baseGizmo;
            }
            Gizmo gizmo = new Command_Toggle()
            {
                defaultLabel = "RV2_DBH_ToiletAllowedForDumpingLabel".Translate(),
                icon = Common.ToiletDumpDesignationTexture,
                toggleAction = () =>
                {
                    isEnabled = !isEnabled;
                },
                isActive = () => isEnabled,
            };
            yield return gizmo;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref isEnabled, nameof(isEnabled));
        }
    }
}
