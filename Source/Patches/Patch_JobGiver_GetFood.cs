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
    [HarmonyPatch(typeof(JobGiver_GetFood), "TryGiveJob")]
    public static class Patch_JobGiver_GetFood
    {
        [HarmonyPostfix]
        private static void InterceptPredatorHunting(ref Job __result, Pawn pawn)
        {
            Job backupResult = __result;
            try
            {
                Job job = __result;
                if(job == null)
                {
                    // no idea why this would happen
                    return;
                }
                if(job.def != JobDefOf.PredatorHunt)
                {
                    // if the created job isn't a hunting animal, we don't care to replace it
                    return;
                }
                if(job.targetA == null)
                {
                    // no idea why this sometimes happens on a hunting job that should always require a target...
                    return;
                }
                Pawn predator = pawn;
                Pawn prey = __result.targetA.Pawn;
                bool shouldIgnoreDesignations = RV2Mod.Settings.features.IgnoreDesignationsVoreHuntingAnimals;

                List<VorePathDef> pathWhitelist = VoreOptionUtility.ConditionalPathWhitelistForPredatorAnimals(pawn);
                
                VoreInteractionRequest request = new VoreInteractionRequest(predator, prey, VoreRole.Predator, isForAuto: true, shouldIgnoreDesignations: shouldIgnoreDesignations, pathWhitelist: pathWhitelist);
                VoreInteraction interaction = VoreInteractionManager.Retrieve(request);
                if(RV2Log.ShouldLog(true, "Jobs"))
                    RV2Log.Message(interaction.ToString(), false, "Jobs");
                IEnumerable<VorePathDef> validPaths = interaction.ValidPaths;

                if(validPaths.EnumerableNullOrEmpty())
                {
                    if(RV2Log.ShouldLog(false, "Jobs"))
                        RV2Log.Message($"Would have replaced hunting job with vore job, but predator {predator.LabelShort} doesn't have any valid paths for digest vore that would feed them", "Jobs");
                    return;
                }
                if(Rand.Chance(RV2Mod.Settings.fineTuning.HuntingAnimalsVoreChance))
                {
                    VorePathDef pathDef = validPaths.RandomElement();
                    if(RV2Log.ShouldLog(true, "Jobs"))
                        RV2Log.Message($"Replacing hunting job, picked {pathDef.label} as path for vore", false, "Jobs");
                    VoreJob jobReplacement = VoreJobMaker.MakeJob(VoreJobDefOf.RV2_HuntVorePrey, pawn, job.targetA, job.targetB, job.targetC);
                    jobReplacement.VorePath = pathDef;
                    jobReplacement.killIncappedTarget = false;
                    __result = jobReplacement;
                    return;
                }
            }
            catch(Exception e)
            {
                Log.Warning("RimVore-2: Something went wrong when trying to intercept the predator hunting job: " + e);
                __result = backupResult;
            }
        }
    }
}