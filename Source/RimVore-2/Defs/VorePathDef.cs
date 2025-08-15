using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimVore2
{
    public class VorePathDef : Def
    {
        public string RMBLabel;
        public VoreTypeDef voreType;
        public VoreGoalDef voreGoal;

        public TaleDef initTale;
        public TaleDef exitTale;
        public string actionDescription;
        public VoreProduct voreProduct;
        public PostVoreMemoryDef postVoreMemories;
        public List<VoreStageDef> stages;
        public List<TargetedRequirements> requirements;
        public bool feedsPredator = false;
        public int defaultRequiredStruggles = -1;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if(RMBLabel == null)
            {
                yield return "required field \"RMBLabel\" is not set";
            }
            if(stages.NullOrEmpty())
            {
                yield return "required list \"stages\" is empty";
            }
            if(voreType == null)
            {
                yield return "required field \"voreType\" is not set";
            }
            if(voreGoal == null)
            {
                yield return "required field \"voreGoal\" is not set";
            }
            if(requirements != null)
            {
                foreach(TargetedRequirements requirement in requirements)
                {
                    foreach(string error in requirement.ConfigErrors())
                    {
                        yield return error;
                    }
                }
            }
            if(voreProduct != null)
            {
                foreach(string error in voreProduct.ConfigErrors())
                {
                    yield return error;
                }
            }
        }

        public List<ThingDef> ValidVoreProductContainers => voreProduct?.selectableContainers?.FindAll(container => container.GetModExtension<VoreContainerExtension>()?.IsValid() == true);

        public ThingDef FirstValidContainer => ValidVoreProductContainers.NullOrEmpty() ? null : ValidVoreProductContainers.First();

        public override string ToString()
        {
            return defName + " VoreType: " + voreType +
                "|VoreGoal: " + voreGoal +
                "|Path: " + string.Join(", ", stages);
        }

        public bool IsValid(Pawn predator, Pawn prey, out string reason, bool isForAuto = false, bool ignoreDesignations = false, bool ignoreRules = false)
        {
            //Log.Message("Checking vorePathDef for validity");
            // if the prey is null, do not call any checking methods with the prey. The prey can be null if this method is called to retrieve all generally valid paths for a predator
            bool preyCanBeChecked = prey != null;
            // check settings
            if(!ignoreRules && !RV2Mod.Settings.rules.VorePathEnabled(predator, RuleTargetRole.Predator, this, isForAuto))
            {
                reason = "RV2_VoreInvalidReasons_PredatorRuleBlockingPath".Translate();
                return false;
            }
            if(!AreVoreEnablersValid(predator, prey, out reason))
            {
                return false;
            }
            // valid between pred and prey
            if(preyCanBeChecked)
            {
                if(!RV2Mod.Settings.rules.VorePathEnabled(prey, RuleTargetRole.Prey, this, isForAuto))
                {
                    reason = "RV2_VoreInvalidReasons_PreyRuleBlockingPath".Translate();
                    return false;
                }
                if(!voreGoal.IsValid(predator, prey, out reason, ignoreDesignations))
                {
                    return false;
                }
                if(!voreType.IsValid(predator, prey, out reason))
                {
                    return false;
                }
                if(!predator.CanVore(prey, out reason))
                {
                    return false;
                }
            }
            if(!AreStagesValid(predator, prey, out reason))
            {
                return false;
            }
            if(!requirements.NullOrEmpty())
            {
                foreach(TargetedRequirements requirement in requirements)
                {
                    // if any requirement is not passed, path is not valid
                    if(!requirement.FulfillsRequirements(predator, prey, out reason))
                    {
                        return false;
                    }
                }
            }
            reason = null;
            return true;
        }

        private Dictionary<VoreTargetSelectorRequest, string> predatorVoreEnablerRequests;
        private Dictionary<VoreTargetSelectorRequest, string> PredatorVoreEnablerRequests
        {
            get
            {
                if(predatorVoreEnablerRequests == null) {
                    predatorVoreEnablerRequests = VoreValidator.PrecomputeRequestFailureReasons(new List<VoreTargetSelectorRequest>()
                        {
                            new VoreTargetSelectorRequest(true) {
                                voreGoal = this.voreGoal,
                                voreType = this.voreType
                            }
                        }
                    );
                }
                return predatorVoreEnablerRequests;
            }
        }

        private Dictionary<RaceType, Dictionary<VoreTargetSelectorRequest, string>> preyVoreEnablerRequests;
        private Dictionary<RaceType, Dictionary<VoreTargetSelectorRequest, string>> PreyVoreEnablerRequests
        {
            get
            {
                if(preyVoreEnablerRequests == null) {
                    preyVoreEnablerRequests = VoreValidator.PrecomputePreyRequestFailureReasons((race => new List<VoreTargetSelectorRequest>()
                        {
                            new VoreTargetSelectorRequest(true) {
                                voreGoal = this.voreGoal,
                                voreType = this.voreType
                            },
                            new VoreTargetSelectorRequest(true) {
                                role = VoreRole.Prey,
                                raceType = race,
                                voreGoal = this.voreGoal,
                                voreType = this.voreType
                            },
                            new VoreTargetSelectorRequest(true) {
                                role = VoreRole.Prey,
                                raceType = race
                            }
                        })
                    );
                }
                return preyVoreEnablerRequests;
            }
        }

        private Dictionary<VoreTargetSelectorRequest, string> TargetSelectors(Pawn prey)
        {
            if(prey != null)
            {
                return PreyVoreEnablerRequests[prey.GetRaceType()];
            }
            return PredatorVoreEnablerRequests;
        }

        private bool AreVoreEnablersValid(Pawn predator, Pawn prey, out string reason)
        {
            if(!RV2Mod.Settings.features.VoreQuirksEnabled)
            {
                reason = null;
                return true;
            }
            Dictionary<VoreTargetSelectorRequest, string> requests = TargetSelectors(prey);
            if(!VoreValidator.PredatorHasVoreEnablerQuirks(predator, requests, out reason))
            {
                return false;
            }
            reason = null;
            return true;
        }

        public bool AreStagesValid(Pawn predator, Pawn prey, out string reason)
        {
            foreach(VoreStageDef stage in stages)
            {
                if(!stage.requirements.NullOrEmpty())
                {
                    foreach(TargetedRequirements requirement in stage.requirements)
                    {
                        if(!requirement.FulfillsRequirements(predator, prey, out reason))
                        {
                            return false;
                        }
                    }
                }
            }
            reason = null;
            return true;
        }
    }

    public class PostVoreMemoryDef
    {
        public ThoughtDef predatorPostVoreSuccess;
        public ThoughtDef predatorPostVoreInterrupted;
        public ThoughtDef preyPostVoreSuccess;
        public ThoughtDef preyPostVoreInterrupted;
    }
}