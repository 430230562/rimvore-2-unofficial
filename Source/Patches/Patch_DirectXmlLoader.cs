using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimVore2
{
    public class Patch_DirectXmlLoader
    {
        const string isNotDateAttributeName = "LoadIfNotDate";
        const string isDateAttributeName = "LoadIfDate";

        [HarmonyPrefix]
        private static bool InterruptDateTaggedNodes(XmlNode node, ref Def __result)
        {
            Def backupResult = __result;
            try
            {
                if(node?.Attributes == null || node.Attributes.Count <= 0)
                    return true;
                
                XmlAttribute isNotDate = node.Attributes[isNotDateAttributeName];
                XmlAttribute isDate = node.Attributes[isDateAttributeName];

                if(isNotDate == null && isDate == null)
                    return true;
                if(isNotDate != null && isDate != null)
                {
                    Log.Error("Both IS and IS NOT date attributes were set, ignoring both and loading node normally");
                    return true;
                }
                bool shouldLoadNode = true;
                if(isNotDate != null)
                {
                    shouldLoadNode = !IsDate(isNotDate.Value);
                }
                if(isDate != null)
                {
                    shouldLoadNode = IsDate(isDate.Value);
                }
                if(shouldLoadNode)
                    return true;

                // actually unload the node if we reach this point
                __result = null;
                return false;

            }
            catch(Exception e)
            {
                __result = backupResult;
                Log.Warning("RimVore-2: Something went wrong when trying to unload date restricted XML nodes: " + e);
                return true;
            }
        }

        private static bool IsDate(string dateString)
        {
            int checkDay;
            int checkMonth;
            try
            {
                DateTime checkDate = DateTime.Parse(dateString).Date;
                checkDay = checkDate.Day;
                checkMonth = checkDate.Month;
            }
            catch(Exception e)
            {
                Log.Error($"Could not parse date: {dateString} - assuming today is not that date. {e}");
                return false;
            }

            DateTime currentDate = DateTime.Now.Date;
            int currentDay = currentDate.Day;
            int currentMonth = currentDate.Month;
            return currentDay == checkDay && currentMonth == checkMonth;
        }
    }
}