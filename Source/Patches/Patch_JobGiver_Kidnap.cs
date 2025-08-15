using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using HarmonyLib;
using Verse.AI;

namespace RimVore2
{
    /// <summary>
    /// Commented variant can be found in Patch_JobGiver_GetFood
    /// This is a simpler version of the hunting vore job, kidnapping only applies to already downed pawns
    /// targetA = prey
    /// targetB = map exit location
    /// </summary>
    [HarmonyPatch(typeof(JobGiver_Kidnap), "TryGiveJob")]
    public static class Patch_JobGiver_Kidnap
    {
        [HarmonyPostfix]
        private static void InterceptKidnapping(ref Job __result, Pawn pawn)
        {
            Job backupResult = __result;
            if(RV2Log.ShouldLog(true, "Jobs"))
                RV2Log.Message("Intercepted job for kidnapping: " + __result?.def?.defName, false, "Jobs");
            try
            {
                Job job = __result;
                if(job == null)
                {
                    if(RV2Log.ShouldLog(true, "Jobs"))
                        RV2Log.Message("Kidnap job replacement, job was null", false, "Jobs");
                    return;
                }
                if(job.def != JobDefOf.Kidnap)
                {
                    if(RV2Log.ShouldLog(true, "Jobs"))
                        RV2Log.Message("Kidnap job replacement, job was not JobDefOf.Kidnap", false, "Jobs");
                    return;
                }
                if(job.targetA == null || job.targetB == null)
                {
                    if(RV2Log.ShouldLog(true, "Jobs"))
                        RV2Log.Message("Kidnap job replacement, one of the targets was null", false, "Jobs");
                    return;
                }
                Pawn predator = pawn;
                Pawn prey = job.targetA.Pawn;
                bool shouldIgnoreDesignations = RV2Mod.Settings.features.IgnoreDesignationsKidnappingRaiders;

                if(prey == null)
                {
                    if(RV2Log.ShouldLog(true, "Jobs"))
                        RV2Log.Message("Kidnapping job does not target pawn", false, "Jobs");
                    return;
                }
                if(!shouldIgnoreDesignations && !predator.CanEndoVore(prey, out string reason, true))
                {
                    if(RV2Log.ShouldLog(true, "Jobs"))
                        RV2Log.Message($"Would have replaced kidnapping job with store vore job, but predator {predator.LabelShort} can't vore: {reason}", false, "Jobs");
                    return;
                }
                if(Rand.Chance(RV2Mod.Settings.fineTuning.RaiderVorenappingChance))
                {
                    VorePathDef pathDef = DetermineKidnapPathDef(predator, prey.CarriedBy, shouldIgnoreDesignations);
                    if(pathDef == null)
                    {
                        // maybe the predator is missing quirks to store vore naturally, let's give the predator those quirks
                        QuirkUtility.ForceVorenappingQuirks(predator);
                        pathDef = DetermineKidnapPathDef(predator, prey, shouldIgnoreDesignations);
                        // if the path is still null, we just can't kidnap vore
                        if(pathDef == null)
                        {
                            if(RV2Log.ShouldLog(false, "Jobs"))
                                RV2Log.Message($"Would have replaced kidnapping job with store vore job, but predator {predator.LabelShort} doesn't have any valid paths for store vore", false, "Jobs");
                            return;
                        }
                    }
                    if(RV2Log.ShouldLog(false, "Jobs"))
                        RV2Log.Message($"Replacing kidnapping job, picked {pathDef.label} as path for vore", false, "Jobs");
                    VoreJob jobReplacement = VoreJobMaker.MakeJob(VoreJobDefOf.RV2_KidnapVorePrey, pawn, job.targetA, job.targetB, job.targetC);
                    jobReplacement.count = 1;
                    jobReplacement.VorePath = pathDef;
                    jobReplacement.IsForced = true;
                    jobReplacement.IsKidnapping = true;
                    __result = jobReplacement;
                    return;
                }
            }
            catch(Exception e)
            {
                Log.Warning("RimVore-2: Something went wrong when trying to intercept the raider kidnapping job: " + e);
                __result = backupResult;
            }
        }

        private static VorePathDef DetermineKidnapPathDef(Pawn predator, Pawn prey, bool shouldIgnoreDesignations)
        {
            List<VoreGoalDef> goalWhitelist = new List<VoreGoalDef>()
            {
                VoreGoalDefOf.Store
            };
            VoreInteractionRequest request = new VoreInteractionRequest(predator, prey, VoreRole.Predator, isForAuto: true, shouldIgnoreDesignations: shouldIgnoreDesignations, goalWhitelist: goalWhitelist);
            VoreInteraction interaction = VoreInteractionManager.Retrieve(request);
            if(RV2Log.ShouldLog(true, "Jobs"))
                RV2Log.Message($"Interaction for kidnapping: {interaction}", false, "Jobs");
            /*IEnumerable<VorePathDef> validPaths = interaction
                .TypesForGoal(goal)
                .Where(typeValidator => typeValidator.Value == null)
                .Select(typeValidator => typeValidator.Key)
                .Select(type => interaction.GetPathDef(type, goal));*/
            VorePathDef pickedPath = interaction.ValidPaths.RandomElementWithFallback();
            return pickedPath;
        }
    }
}