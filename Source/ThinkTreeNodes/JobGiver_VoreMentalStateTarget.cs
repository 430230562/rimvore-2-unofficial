using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace RimVore2
{
    public class JobGiver_VoreMentalStateTarget : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            //Log.Message("JobGiver VoreMentalState enter");
            if(!(pawn.MentalState is MentalState_VoreTargeter mentalState))
            {
                if(RV2Log.ShouldLog(false, "MentalStates"))
                    RV2Log.Message("Mental state is not VoreTargeter", "MentalStates");
                return null;
            }
            Pawn target = mentalState.CurrentTarget;
            if(target == null)
            {
                if(RV2Log.ShouldLog(false, "MentalStates"))
                    RV2Log.Message("No target provided by mental state", "MentalStates");
                return null;
            }
            // if the target has been vored, we need to wait until we recalculate
            if(!target.Spawned)
            {
                return null;
            }
            List<VoreGoalDef> goalWhitelist = mentalState.VoreMentalStateDef.goalWhitelist;
            List<VoreGoalDef> goalBlacklist = mentalState.VoreMentalStateDef.goalBlacklist;
            List<VoreTypeDef> typeWhitelist = mentalState.VoreMentalStateDef.typeWhitelist;
            List<VoreTypeDef> typeBlacklist = mentalState.VoreMentalStateDef.typeBlacklist;
            List<VorePathDef> pathWhitelist = mentalState.VoreMentalStateDef.pathWhitelist;
            List<VorePathDef> pathBlacklist = mentalState.VoreMentalStateDef.pathBlacklist;
            List<RV2DesignationDef> designationWhitelist = mentalState.VoreMentalStateDef.designationWhitelist;
            List<RV2DesignationDef> designationBlacklist = mentalState.VoreMentalStateDef.designationBlacklist;
            VoreRole initiatorRole = mentalState.InitiatorRole;
            bool calculateAsAutoVore = RV2Mod.Settings.fineTuning.MentalBreaksUseAutoVoreRules;
            VoreInteractionRequest request = new VoreInteractionRequest(pawn, target, initiatorRole, isForAuto: calculateAsAutoVore, isForProposal: false, RV2Mod.Settings.features.IgnoreDesignationsMentalState, null, null, typeWhitelist, typeBlacklist, goalWhitelist, goalBlacklist, pathWhitelist, pathBlacklist, designationWhitelist, designationBlacklist);
            VoreInteraction interaction = VoreInteractionManager.Retrieve(request);
            if(RV2Log.ShouldLog(false, "MentalStates"))
                RV2Log.Message(interaction.ToString(), "MentalStates");
            VorePathDef path = interaction.PreferredPath ?? interaction.ValidPaths.RandomElementWithFallback();
            if(path == null)
            {
                if(RV2Log.ShouldLog(false, "MentalStates"))
                    RV2Log.Message("No path available for interaction, falling back to base game mental state", "MentalStates");
                mentalState.CallFallbackMentalState();
                return null;
            }
            JobDef voreJobDef = initiatorRole.GetInitJobDefFor();
            VoreJob job = VoreJobMaker.MakeJob(voreJobDef, pawn, target);
            job.targetA = target;
            job.VorePath = path;
            if(RV2Log.ShouldLog(false, "MentalStates"))
                RV2Log.Message($"Giving job {job}", "MentalStates");
            return job;
        }
    }
}
