using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimVore2
{
    [HarmonyPatch(typeof(Precept), "GetTip")]
    public class PatchTemplate
    {
        [HarmonyPostfix]
        private static void AppendQuirkCompDescriptions(ref string __result, Precept __instance)
        {
            string backupResult = __result;
            try
            {
                List<PreceptComp_Quirk_AddQuirks> addQuirkComps = new List<PreceptComp_Quirk_AddQuirks>();
                List<PreceptComp_Quirk_RemoveQuirks> removeQuirkComps = new List<PreceptComp_Quirk_RemoveQuirks>();
                List<PreceptComp_Quirk_EnsureOneOf> ensureQuirkComps = new List<PreceptComp_Quirk_EnsureOneOf>();
                foreach(PreceptComp comp in __instance.def.comps)
                {
                    if(comp is PreceptComp_Quirk_AddQuirks addComp)
                    {
                        addQuirkComps.Add(addComp);
                    }
                    if(comp is PreceptComp_Quirk_RemoveQuirks removeComp)
                    {
                        removeQuirkComps.Add(removeComp);
                    }
                    if(comp is PreceptComp_Quirk_EnsureOneOf ensureComp)
                    {
                        ensureQuirkComps.Add(ensureComp);
                    }
                }
                if(addQuirkComps.NullOrEmpty() && removeQuirkComps.NullOrEmpty() && ensureQuirkComps.NullOrEmpty())
                {
                    if(RV2Log.ShouldLog(false, "IdeoQuirks"))
                        RV2Log.Message("No quirk comps found for precept " + __instance.def.defName, true, "IdeoQuirks");
                    return;
                }
                string coloredHeader = "RV2_IdeologyDescription_QuirksHeader".Translate().Colorize(ColoredText.TipSectionTitleColor);

                StringBuilder quirkDesc = new StringBuilder();
                quirkDesc.Append(__result);
                quirkDesc.Append("\n\n" + coloredHeader);
                if(!addQuirkComps.NullOrEmpty())
                {
                    quirkDesc.Append("\n  - " + "RV2_IdeologyDescription_PreceptComp_AddQuirks".Translate());
                    quirkDesc.Append("\n" + String.Join("\n", addQuirkComps.Select(comp => string.Join("\n", comp.GetDescriptions()))));
                }
                if(!removeQuirkComps.NullOrEmpty())
                {
                    quirkDesc.Append("\n  - " + "RV2_IdeologyDescription_PreceptComp_RemoveQuirks".Translate());
                    quirkDesc.Append("\n" + String.Join("\n", removeQuirkComps.Select(comp => string.Join("\n", comp.GetDescriptions()))));
                }
                if(!ensureQuirkComps.NullOrEmpty())
                {
                    quirkDesc.Append("\n  - " + "RV2_IdeologyDescription_PreceptComp_EnsureOneOf".Translate());
                    quirkDesc.Append("\n" + String.Join("\n", ensureQuirkComps.Select(comp => string.Join("\n", comp.GetDescriptions()))));
                }
                __result = quirkDesc.ToString();
            }
            catch(Exception e)
            {
                __result = backupResult;
                Log.Warning("RimVore-2: Something went wrong when determining the descriptions provided by RV2 Quirk Comps: " + e);
                return;
            }
        }
    }
}