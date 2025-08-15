using Verse;
using System.Collections.Generic;
using System;
using UnityEngine;
using RimWorld;
using System.Linq;

namespace RimVore2
{
    public class Hediff_ContainingPrey : HediffWithComps
    {
        public List<VoreTrackerRecord> ConnectedVoreRecords = new List<VoreTrackerRecord>();

        private string label;
        public override string Label
        {
            get
            {
                if(label == null)
                {
                    UpdateLabel();
                }
                return label;
            }
        }

        public Hediff_ContainingPrey() { }

        // overriding severity like this will automatically change the "ShouldRemove" field for Hediffs, which means we never have to manually remove the hediff, just let the game handle it
        public override float Severity
        {
            get
            {
                if(ConnectedVoreRecords.EnumerableNullOrEmpty())
                {
                    severityInt = 0;
                }
                else
                {
                    severityInt = ConnectedVoreRecords.Count() * 0.1f;
                }

                return severityInt;
            }
        }

        public override bool Visible => Severity > 0;

        public override void Tick()
        {
            base.Tick();
            if(base.ageTicks % RV2Mod.Settings.debug.HediffLabelRefreshInterval == 0)
            {
                UpdateLabel();
            }
        }

        public void UpdateLabel()
        {
            label = string.Join("\n", ConnectedVoreRecords
                .Select(record =>
                {
                    string labelString = base.def.label + ": " + record.GetPreyName();
                    string partGoal = record.CurrentVoreStage.def.partGoal?.label;
                    if(partGoal != null && partGoal != "")
                    {
                        labelString = "[" + partGoal + "] " + labelString;
                    }
                    float partProgress = record.CurrentVoreStage.PercentageProgress;
                    if(partProgress != -1)
                    {
                        partProgress = Mathf.Clamp(partProgress, 0f, 1f);
                        partProgress = Mathf.Round(partProgress * 100);
                        labelString += " " + partProgress + "%";
                    }
                    return labelString;
                })
            );
        }
    }
}
