using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;
using HarmonyLib;

namespace RimVore2
{
    public class VoreGoalDef : Def, IPreferrable
    {
        public List<RV2DesignationDef> requiredDesignations = new List<RV2DesignationDef>();
        public bool IsLethal => requiredDesignations.Any(designation => designation.lethal);
        public List<TargetedRequirements> requirements;
        public TaleDef goalFinishTale;
        public RecordDef goalFinishRecordPredator;
        public RecordDef goalFinishRecordPrey;
        public List<RulePackDef> relatedRulePacks;
        public bool validForRituals = true;

        //Tab UI information
        public string IconPath;
        private Texture2D icon;
        public Texture2D Icon
        {
            get
            {
                if(icon == null)
                {
                    if(!IconPath.NullOrEmpty())
                    {
                        icon = ContentFinder<Texture2D>.Get(IconPath);

                    }
                    else
                    {
                        if(IsLethal)
                            icon = UITextures.SkullButtonTexture;
                        else
                            icon = UITextures.HeartButtonTexture;
                    }
                }
                return icon;
            }
        }

        public bool IsValid(Pawn predator, Pawn prey, out string reason, bool ignoreDesignations = false)
        {
            //Log.Message("Checking voreGoalDef for validity");
            if(!predator.CanBePredator(out reason))
                return false;
            if(!prey.CanBePrey(out reason))
                return false;
            QuirkManager predatorQuirks = predator.QuirkManager(false);
            if(IsLethal)
            {
                if(!RV2Mod.Settings.features.FatalVoreEnabled)
                {
                    reason = "RV2_VoreInvalidReasons_FatalDisabled".Translate();
                    return false;
                }
                if(predatorQuirks != null && predatorQuirks.HasSpecialFlag("EndoPredatorOnly"))
                {
                    reason = "RV2_VoreInvalidReasons_QuirkEndoOnlyPredator".Translate();
                    return false;
                }
            }
            if(!IsLethal)
            {
                if(!RV2Mod.Settings.features.EndoVoreEnabled)
                {
                    reason = "RV2_VoreInvalidReasons_EndoDisabled".Translate();
                    return false;
                }
                if(predatorQuirks != null && predatorQuirks.HasSpecialFlag("FatalPredatorOnly"))
                {
                    reason = "RV2_VoreInvalidReasons_QuirkFatalOnlyPredator".Translate();
                    return false;
                }
            }
            
            // specific interactions are allowed to ignore the designations, e.g. vore hunting animals may fatally vore pawns that have fatal vore designation disabled or are not designated as predators themself
            if(!ignoreDesignations && !AreDesignationsValid(predator, prey, out reason))
            {
                return false;
            }
            if(!AreVoreEnablersValid(predator, prey, out reason))
                return false;

            if(!requirements.NullOrEmpty())
            {
                foreach(TargetedRequirements requirement in requirements)
                {
                    if(!requirement.FulfillsRequirements(predator, prey, out reason))
                        return false;
                }
            }

            reason = null;
            return true;
        }

        private bool AreDesignationsValid(Pawn predator, Pawn prey, out string reason)
        {
            reason = null;
            // predator is always required for vore, but the user doesn't need to set that in the Defs
            if(!requiredDesignations.Contains(RV2DesignationDefOf.predator))
            {
                requiredDesignations.Add(RV2DesignationDefOf.predator);
            }
            bool allDesignationsValid = true;


            if(RV2Log.ShouldLog(true, "Designations")) 
                RV2Log.Message($"{predator.LabelShort}, {prey.LabelShort}: Checking designations: {string.Join(", ", requiredDesignations.ConvertAll(d => d.defName))}", true, "Designations");
            foreach(RV2DesignationDef designation in requiredDesignations)
            {
                bool isDesignationValid = designation.IsEnabledFor(predator, prey, out string subReason);
                if(RV2Log.ShouldLog(true, "Designations"))
                    RV2Log.Message($"{predator.LabelShort}, {prey.LabelShort}: Designation {designation.defName} is valid ? {isDesignationValid}", true, "Designations");
                allDesignationsValid &= isDesignationValid;
                if(subReason != null)
                {
                    // if reason not set yet, set it, otherwise, add to it
                    reason = reason == null ? subReason : reason + " + " + subReason;
                }
            }
            if(RV2Log.ShouldLog(true, "Designations"))
                RV2Log.Message($"{predator.LabelShort}, {prey.LabelShort}: Goal {defName} is valid ? {allDesignationsValid}", true, "Designations");
            return allDesignationsValid;
        }

        private Dictionary<RaceType, Dictionary<VoreTargetSelectorRequest, string>> preyVoreEnablerRequests;
        private Dictionary<RaceType, Dictionary<VoreTargetSelectorRequest, string>> PreyVoreEnablerRequests
        {
            get
            {
                if(preyVoreEnablerRequests == null) {
                    preyVoreEnablerRequests = VoreValidator.PrecomputePreyRequestFailureReasons(race => new List<VoreTargetSelectorRequest>()
                    {
                        new VoreTargetSelectorRequest(true)
                        {
                            voreGoal = this
                        },
                        new VoreTargetSelectorRequest(true)
                        {
                            role = VoreRole.Prey,
                            raceType = race,
                            voreGoal = this
                        }
                    });
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
            if(role == VoreRole.Prey || role == VoreRole.Predator)
            {
                RuleTargetRole identifierRole = role == VoreRole.Predator ? RuleTargetRole.Predator : RuleTargetRole.Prey;

                List<RV2DesignationDef> applicableDesignations = requiredDesignations
                    .FindAll(d => d.AppliesToRole(identifierRole));
                foreach(RV2DesignationDef designation in applicableDesignations)
                {
                    if(!designation.IsEnabledFor(pawn, out _))
                    {
                        return -1f;
                    }
                }
            }

            return pawn.PreferenceFor(this, role, modifierOperation);
        }

        public string GetName() => defName;

        public bool IsObsessed(Pawn pawn, VoreRole role)
        {
            return GetPreference(pawn, role) >= QuirkUtility.ObsessedPreferenceValue;
        }

        public void IncrementRecords(Pawn predator, Pawn prey)
        {
            if(goalFinishRecordPredator != null)
            {
                predator.records?.Increment(goalFinishRecordPredator);
            }
            if(goalFinishRecordPrey != null)
            {
                prey.records?.Increment(goalFinishRecordPrey);
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