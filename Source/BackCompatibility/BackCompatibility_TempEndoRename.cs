using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;

namespace RimVore2
{
    public class BackCompatibilityConverter_TempEndoRename : BackCompatibilityConverter
    {
        public override bool AppliesToVersion(int majorVer, int minorVer)
        {
            // TODO can't check our own version here, just always try to convert and disable this in a later release
            return true;
        }

        public override string BackCompatibleDefName(Type defType, string defName, bool forDefInjections = false, XmlNode node = null)
        {
            if(defType == typeof(HediffDef))
            {
                if(defName == "RV2_GoalEnabler_Tempendo")
                    return "RV2_GoalEnabler_TempEndo";
            }
            if(defType == typeof(QuirkDef))
            {
                if(defName == "Enablers_Core_Goal_Tempendo")
                    return "Enablers_Core_Goal_TempEndo";
            }
            if(defType == typeof(RecipeDef))
            {
                if(defName == "RV2_MakeGoalEnabler_Tempendo")
                    return "RV2_MakeGoalEnabler_TempEndo";
            }
            if(defType == typeof(ThingDef))
            {
                if(defName == "RV2_GoalEnabler_Tempendo")
                    return "RV2_GoalEnabler_TempEndo";
            }
            return null;
        }

        public override Type GetBackCompatibleType(Type baseType, string providedClassName, XmlNode node)
        {
            return null;
        }

        public override void PostExposeData(object obj)
        {
            return;
        }
    }
}
