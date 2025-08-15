using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimVore2
{
    /// <summary>
    /// Manhunter idle is what we want to target, it's way weirder than all the other patches in this mod, but basically:
    ///     Check if the original job is null
    ///     There are 3 reasons for it to be null
    ///         - manhunter has no attack
    ///         - attackable pawn was found, but no path exists
    ///         - fallback value for the job
    ///     The first reason we can just re-check, but the second reason may be too "expensive" to check again, so just run the injection instead
    /// Feel free to suggest changes or different approaches, I am aware this patch sucks, but I hate transpiling
    /// </summary>
    [HarmonyPatch(typeof(JobGiver_Manhunter), "TryGiveJob")]
    public static class Patch_JobGiver_Manhunter
    {
        [HarmonyPostfix]
        private static void InterceptIdleAndVoreDownedPawns(ref Job __result, ref Pawn pawn)
        {
            Job backupResult = __result;
            try
            {
                Job job = __result;
                // we try to hook into the idle or "nothing" part of the giver
                if(job != null)
                {
                    if(job.def != JobDefOf.Goto)
                    {
                        return;
                    }
                }
                __result = TryVoreDownedPawns(pawn);
            }
            catch(Exception e)
            {
                __result = backupResult;
                Log.Warning("RimVore-2: Something went wrong when trying to intercept manhunter idle job: " + e);
                return;
            }
        }

        public static Job TryVoreDownedPawns(Pawn manhunter)
        {
            if(!RV2Mod.Settings.fineTuning.ManhuntersVoreDownedPawns)
            {
                return null;
            }

            // recheck attack-ability
            if(manhunter.TryGetAttackVerb(null, false) == null)
            {
                return null;
            }

            Pawn predator = manhunter;
            if(RV2Log.ShouldLog(true, "Jobs"))
                RV2Log.Message("Intercepting idle job for manhunter " + predator.LabelShort, false, "Jobs");
            List<Pawn> downedPawns = predator.Map.mapPawns.SpawnedDownedPawns
                .FindAll(p => predator.CanReach(p, PathEndMode.ClosestTouch, Danger.Deadly));
            if(downedPawns.NullOrEmpty())
            {
                return null;
            }
            Pawn prey = downedPawns.RandomElement();

            List<VorePathDef> pathWhitelist = VoreOptionUtility.ConditionalPathWhitelistForPredatorAnimals(predator);

            VoreInteractionRequest request = new VoreInteractionRequest(predator, prey, VoreRole.Predator, isForAuto: true, shouldIgnoreDesignations: RV2Mod.Settings.features.IgnoreDesignationsVoreHuntingAnimals, pathWhitelist: pathWhitelist);
            VoreInteraction interaction = VoreInteractionManager.Retrieve(request);
            if(RV2Log.ShouldLog(true, "Jobs"))
                RV2Log.Message($"Interaction for manhunter: {interaction}", false, "Jobs");
            VorePathDef path = interaction.ValidPaths.RandomElementWithFallback();
            if(path == null)
            {
                return null;
            }
            VoreJob voreJob = VoreJobMaker.MakeJob(VoreJobDefOf.RV2_VoreInitAsPredator, predator, prey);
            voreJob.VorePath = path;
            voreJob.IsForced = true;

            if(RV2Log.ShouldLog(true, "Jobs"))
                RV2Log.Message("Successfully injected vore job", false, "Jobs");
            return voreJob;
        }
    }
}