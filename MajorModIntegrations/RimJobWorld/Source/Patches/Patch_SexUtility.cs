using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using rjw;
using RimVore2;
using Verse;
using Verse.AI;

namespace RV2_RJW
{
    [HarmonyPatch(typeof(SexUtility), "CumFilthGenerator")]
    public static class Patch_SexUtility_CumFilthGenerator
    {
        private static HediffDef cumReservesHediffDef = HediffDef.Named("RV2_CumReserves");
        private static readonly ThingDef maleCum = ThingDef.Named("FilthCum");
        private static readonly ThingDef femaleCum = ThingDef.Named("FilthGirlCum");
        private static float cumReservesToApplyToGround = 4f;

        private static void AddCumReservesIfAvailable(Pawn pawn)
        {
            try
            {
                Hediff cumReserves = pawn?.health?.hediffSet?.hediffs?
                    .FirstOrDefault(hed => hed.def == cumReservesHediffDef);
                if(cumReserves == null)
                {
                    return;
                }
                if(RV2Log.ShouldLog(false, "RJW"))
                    RV2Log.Message($"Pawn {pawn.LabelShort} has cum reserves at {cumReserves.Severity} severity, using those to create some cum on the ground", "RJW");
                List<Hediff> parts = pawn.GetGenitalsList();

                if(Genital_Helper.has_vagina(pawn, parts))
                    FilthMaker.TryMakeFilth(pawn.PositionHeld, pawn.MapHeld, femaleCum, pawn.LabelIndefinite(), (int)cumReservesToApplyToGround / 2);

                if(Genital_Helper.has_penis_fertile(pawn, parts))
                    FilthMaker.TryMakeFilth(pawn.PositionHeld, pawn.MapHeld, maleCum, pawn.LabelIndefinite(), (int)cumReservesToApplyToGround);
            }
            catch (Exception e)
            {
                Log.Warning("RimVore-2: Something went wrong when checking for cum reserves to apply during RJW cumOn method: " + e);
                return;
            }
        }
    }

    [HarmonyPatch(typeof(SexUtility), "Aftersex")]
    public static class Patch_SexUtility_Aftersex
    {
        [HarmonyPostfix]
        private static void DoPostSexVoreProposal(SexProps props)
        {
            try
            {
                string reason = null;
                bool canPropose = props.partner != null
                    && props.partner != props.pawn  // self vore is a no-no. selfcest can happen tho (apparently)
                    && props.pawn.WantsToProposeTo(props.partner, out reason);

                if(!canPropose)
                {
                    if(reason != null && RV2Log.ShouldLog(false, "RJW"))
                        RV2Log.Message($"{props.pawn.LabelShort} does not want to propose to {props.partner.LabelShort}: {reason}", "RJW");
                    return;
                }
                float proposalChance = props.isRape ? RV2_RJW_Settings.rjw.PostRapeProposalChance : RV2_RJW_Settings.rjw.PostSexProposalChance;
                QuirkManager quirks = props.pawn.QuirkManager();
                if(quirks != null)
                {
                    proposalChance = quirks.ModifyValue("PostSexProposalChance", proposalChance);
                }
                if(RV2Log.ShouldLog(false, "RJW"))
                    RV2Log.Message("Post sex proposal roll, chance: " + proposalChance, "RJW");
                if(!Rand.Chance(proposalChance))
                {
                    return;
                }

                // I spent like 20 minutes figuring the bool one-liner but decided it wasn't readable enough, so there goes that time :)
                // props.isRape && !(isAnimal && !bestIsRape)
                bool isRape = props.isRape;
                if(RimVore2.VoreKeywordUtility.IsAnimal(props.partner) && !RV2_RJW_Settings.rjw.BestialityIsRape)
                {
                    // RJW always flags bestiality as rape, which means post-bestiality proposals are always forced and skip the fancy animal handling skill logic from RV2
                    isRape = false;
                }
                bool isForced = isRape ? RV2_RJW_Settings.rjw.PostRapeProposalsAreForced : RV2_RJW_Settings.rjw.PostSexProposalsAreForced;
                if(RV2Log.ShouldLog(false, "RJW"))
                    RV2Log.Message("Creating proposal for {props.pawn} and {props.partner}", "RJW");
                VoreInteractionRequest request = new VoreInteractionRequest(props.pawn, props.partner, VoreRole.Invalid, true);
                VoreInteraction interaction = VoreInteractionManager.Retrieve(request);
                VorePathDef pathDef = interaction.PreferredPath;
                if(pathDef == null)
                {
                    if(RV2Log.ShouldLog(false, "RJW"))
                        RV2Log.Message("no preferred path, not creating proposal", "RJW");
                    return;
                }
                if(RV2Log.ShouldLog(false, "RJW"))
                    RV2Log.Message("Picked path for vore proposal: " + pathDef.defName, "RJW");
                VoreProposal proposal = new VoreProposal_TwoWay(interaction.Predator, interaction.Prey, props.pawn, props.partner, pathDef);
                VoreJob job = VoreJobMaker.MakeJob(VoreJobDefOf.RV2_ProposeVore, props.pawn, props.partner);
                job.Proposal = proposal;
                job.VorePath = pathDef;
                job.IsForced = isForced;
                props.pawn.jobs.jobQueue.EnqueueFirst(job);
            }
            catch(Exception e)
            {
                Log.Warning("RV-2: Something went wrong during post-sex proposals: " + e);
            }
        }
    }
}