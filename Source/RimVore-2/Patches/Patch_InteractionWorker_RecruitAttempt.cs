using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimVore2
{
    /// <summary>
    /// on failed tame interaction, have a chance to vore the tamer. Fatal is predator, endo if not predator
    /// </summary>
    [HarmonyPatch(typeof(InteractionWorker_RecruitAttempt), "Interacted")]
    public static class Patch_InteractionWorker_RecruitAttempt
    {
        [HarmonyPostfix]
        public static void VoreOnFailedTameAttempt(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks)
        {
            if(!recipient.IsAnimal())
                return;
            // we can analyze the extraSentencePacks to figure out if the recruitment was successful
            if(!extraSentencePacks.Contains(RulePackDefOf.Sentence_RecruitAttemptRejected))
            {
                if(RV2Log.ShouldLog(false, "TamingVore"))
                    RV2Log.Message("Taming attempt successful, no chance of vore", "TamingVore");
                return;
            }
            float baseFailedTameVoreChance = RV2Mod.Settings.fineTuning.FailedTameVoreChance;
            if(baseFailedTameVoreChance == 0)
                return;
            float voreChance;
            // the failed taming may have started a manhunter mental state, we simply force-vore the other pawn
            // if an unknown mental state was produced, the VoreInteraction could only be valid if it was appended with the VoreMentalState extension, so no need to check for a specific mental state
            if(recipient.mindState.mentalStateHandler.InMentalState)
            {
                if(RV2Log.ShouldLog(false, "TamingVore"))
                    RV2Log.Message("Animal in mental state, vore chance 100%", "TamingVore");
                voreChance = 1f;
            }
            else
            {
                int animalSkillLevel = initiator.skills.GetSkill(SkillDefOf.Animals).Level;
                // 0 skill -> 1.5x chance, 20 skill -> 0.5x chance
                voreChance = baseFailedTameVoreChance - (animalSkillLevel - 10) * baseFailedTameVoreChance / 20;
            }
            if(RV2Log.ShouldLog(false, "TamingVore"))
                RV2Log.Message($"Vore chance: {voreChance}", "TamingVore");
            if(!Rand.Chance(voreChance))
            {
                if(RV2Log.ShouldLog(false, "TamingVore"))
                    RV2Log.Message("Failed vore chance roll, no vore", "TamingVore");
                return;
            }
            List<VorePathDef> pathWhitelist = VoreOptionUtility.ConditionalPathWhitelistForPredatorAnimals(recipient);
            VoreInteractionRequest request = new VoreInteractionRequest(recipient, initiator, VoreRole.Predator, pathWhitelist: pathWhitelist);
            VoreInteraction interaction = VoreInteractionManager.Retrieve(request);
            if(!interaction.IsValid)
            {
                if(RV2Log.ShouldLog(false, "TamingVore"))
                    RV2Log.Message("Failed taming attempt, but vore interaction is not valid, can't vore", "TamingVore");
                return;
            }
            VoreJob job = VoreJobMaker.MakeJob(VoreJobDefOf.RV2_VoreInitAsPredator, recipient, initiator);
            job.VorePath = interaction.ValidPaths.RandomElement();
            job.IsForced = initiator.PreferenceFor(VoreRole.Prey) <= 4;
            if(RV2Log.ShouldLog(false, "TamingVore"))
                RV2Log.Message($"Doing vore path {job.VorePath.defName}, forced ? {job.IsForced}", "TamingVore");
            recipient.jobs.jobQueue.EnqueueFirst(job);
            NotificationUtility.DoNotification(RV2Mod.Settings.fineTuning.FailedTameVoreNotification, "RV2_Message_FailedTameVore_Description".Translate(initiator.Named("TAMER"), recipient.Named("TAMEE")), "RV2_Message_FailedTameVore_Title".Translate(), new LookTargets(recipient, initiator));
        }
    }
}
