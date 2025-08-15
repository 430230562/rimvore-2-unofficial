using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimVore2
{
    public class VoreTypeDef : Def, IPreferrable
    {
        public List<TargetedRequirements> requirements;
        public RecordDef initiationRecordPredator;
        public RecordDef initiationRecordPrey;
        public List<RulePackDef> relatedRulePacks;
        public HistoryEventDef postVoreEvent;

        //Tab UI information
        public string IconPath;
        private Texture2D icon;
        public Texture2D Icon
        {
            get
            {
                if(icon == null)
                {
                    if(IconPath.NullOrEmpty())
                    {
                        icon = UITextures.InfoButton;
                    }
                    else
                    {
                        icon = ContentFinder<Texture2D>.Get(IconPath);
                    }
                }
                return icon;
            }
        }

        public bool IsValid(Pawn predator, Pawn prey, out string reason)
        {
            //Log.Message("Checking voreTypeDef for validity");
            if(!AreVoreEnablersValid(predator, prey, out reason))
            {
                return false;
            }
            if(!requirements.NullOrEmpty())
            {
                foreach(TargetedRequirements requirement in requirements)
                {
                    if(!requirement.FulfillsRequirements(predator, prey, out reason))
                    {
                        return false;
                    }
                }
            }

            reason = null;
            return true;
        }

        private Dictionary<RaceType, Dictionary<VoreTargetSelectorRequest, string>> preyVoreEnablerRequests;
        private Dictionary<RaceType, Dictionary<VoreTargetSelectorRequest, string>> PreyVoreEnablerRequests
        {
            get
            {
                if(preyVoreEnablerRequests == null) {
                    preyVoreEnablerRequests = VoreValidator.PrecomputePreyRequestFailureReasons((race => new List<VoreTargetSelectorRequest>()
                    {
                        new VoreTargetSelectorRequest(true)
                        {
                            voreType = this
                        },
                        new VoreTargetSelectorRequest(true)
                        {
                            role = VoreRole.Prey,
                            raceType = race,
                            voreType = this
                        }
                    }));
                }
                return preyVoreEnablerRequests;
            }
        }

        private bool AreVoreEnablersValid(Pawn predator, Pawn prey, out string reason)
        {
            if(!RV2Mod.Settings.features.VoreQuirksEnabled)
            {
                reason = null;
                return true;
            }
            Dictionary<VoreTargetSelectorRequest, string> requests = PreyVoreEnablerRequests[prey.GetRaceType()];
            if(!VoreValidator.PredatorHasVoreEnablerQuirks(predator, requests, out reason))
            {
                return false;
            }
            reason = null;
            return true;
        }

        public float GetPreference(Pawn pawn, VoreRole role, ModifierOperation modifierOperation = ModifierOperation.Add)
        {
            return pawn.PreferenceFor(this, role, modifierOperation);
        }
        public string GetName() => defName;

        public bool IsObsessed(Pawn pawn, VoreRole role)
        {
            return GetPreference(pawn, role) >= QuirkUtility.ObsessedPreferenceValue;
        }

        public void IncrementRecords(Pawn predator, Pawn prey)
        {
            if(initiationRecordPredator != null)
            {
                predator.records?.Increment(initiationRecordPredator);
            }
            if(initiationRecordPrey != null)
            {
                prey.records?.Increment(initiationRecordPrey);
            }
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if(!requirements.NullOrEmpty())
            {
                foreach(TargetedRequirements requirement in requirements)
                {
                    foreach(string error in requirement.ConfigErrors())
                    {
                        yield return error;
                    }
                }
            }
        }
    }
}
