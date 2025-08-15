using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimVore2
{
    public static class VorePathDurationUtility
    {
        public static void CalculateAllPathDurations()
        {
            foreach(VorePathDef path in RV2_Common.VorePaths)
            {
                try
                {
                    CalculatePathDuration(path);
                }
                catch(Exception e)
                {
                    Log.Warning("Exception when trying to guess path duration for " + path.defName + ": " + e);
                }
            }
        }

        private static void CalculatePathDuration(VorePathDef path)
        {
            Func<float, string> PresentDuration = (float val) => val == float.MaxValue ? "Infinite" : Mathf.Round(val).ToString();

            List<string> stageExplanations = new List<string>();
            float pathDuration = 0f;
            for(int i = 0; i < path.stages.Count; i++)
            {
                VoreStageDef stage = path.stages[i];
                float stageDuration = stage.AbstractDuration();
                if(i == path.stages.Count - 1 && stageDuration >= 100)
                {
                    // with how stages work the last stage is the "end" and the 100 tick duration is for emergency eject
                    stageExplanations.Add($"{stage.defName}: exit, ignored ({PresentDuration(stageDuration)}");
                    continue;
                }
                stageExplanations.Add($"{stage.defName}: {PresentDuration(stageDuration)}");
                pathDuration += stageDuration;
            }
            Log.Message($"Path {path.defName} has a guessed duration of {PresentDuration(pathDuration)} rare ticks: {string.Join(", ", stageExplanations)}");
        }
    }
}
