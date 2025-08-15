using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public static class ConfigUtility
    {
        public static void PresentAdditionalConfigErrors()
        {
            IEnumerable<string> configErrors = AllExtraConfigErrors();
            foreach(string error in configErrors)
            {
                Log.Error(error);
            }
        }
        public static void PresentAdditionalConfigMessages()
        {
            IEnumerable<string> configMessages = AllExtraConfigMessages();
            foreach(string message in configMessages)
            {
                Log.Message(message);
            }
        }

        private static IEnumerable<string> AllExtraConfigErrors()
        {
            foreach(string error in QuirkPoolAssignmentErrors())
            {
                yield return "Config error in QuirkDef: " + error;
            }
            foreach(string error in PreceptThoughtKeywordErrors())
            {
                yield return "Config error in ThoughtDef: " + error;
            }
        }

        private static IEnumerable<string> AllExtraConfigMessages()
        {
            string requiredStrugglesMessage = DefaultRequiredStrugglesMessage();
            if(requiredStrugglesMessage != null)
            {
                yield return "Config issue: " + requiredStrugglesMessage;
            }
        }

        private static IEnumerable<string> QuirkPoolAssignmentErrors()
        {
            List<QuirkDef> quirks = DefDatabase<QuirkDef>.AllDefsListForReading;
            // Log.Message(string.Join(", ", quirks.ConvertAll(q => q.defName)));
            foreach(QuirkDef quirk in quirks)
            {
                if(quirk.GetPool() == null)
                {
                    yield return "QuirkDef " + quirk.defName + " is not assigned to a QuirkPoolDef";
                }
            }
        }

        private static IEnumerable<string> PreceptThoughtKeywordErrors()
        {
            foreach(ThoughtDef thoughtDef in DefDatabase<ThoughtDef>.AllDefsListForReading)
            {
                if(thoughtDef.workerClass != typeof(ThoughtWorker_Precept_RecentKeyword))
                {
                    continue;
                }
                PreceptThoughtWorker_ListeningKeyword extension = thoughtDef.GetModExtension<PreceptThoughtWorker_ListeningKeyword>();
                if(extension == null)
                {
                    yield return thoughtDef.defName + ": No applicable PreceptThoughtWorker_ListeningKeyword extension for ThoughtWorker \"ThoughtWorker_Precept_RecentKeyword\"";
                }
            }
        }

        private static string DefaultRequiredStrugglesMessage()
        {
            List<string> afflictedPaths = new List<string>();
            foreach(VorePathDef path in DefDatabase<VorePathDef>.AllDefsListForReading)
            {
                if(path.defaultRequiredStruggles == -1)
                {
                    afflictedPaths.Add(path.defName);
                }
            }
            if(afflictedPaths.NullOrEmpty())
                return null;
            return $"The following VorePathDefs do not provide a \"defaultRequiredStruggles\" value and will instead use the default provided in the settings: {string.Join(", ", afflictedPaths)}";
        }
    }
}
