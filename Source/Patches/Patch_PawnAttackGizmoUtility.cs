//using System;
//using System.Collections.Generic;
//using System.Linq;
//using HarmonyLib;
//using RimWorld;
//using Verse;
//using Verse.AI;

//namespace RimVore2
//{
//    [HarmonyPatch(typeof(PawnAttackGizmoUtility), "GetAttackGizmos")]
//    public class Patch_PawnAttackGizmoUtility
//    {
//        [HarmonyPostfix]
//        private static IEnumerable<Gizmo> AddGrappleGizmos(IEnumerable<Gizmo> __result, Pawn pawn)
//        {
//            foreach(Gizmo gizmo in __result)
//            {
//                yield return gizmo;
//            }
//            if(pawn == null)   // some user had a null pawn here, I am absolutely baffled, but okay
//                yield break;
//            Gizmo useGrappleGizmo = GetUseGrappleGizmo(pawn);
//            if(useGrappleGizmo != null)
//            {
//                yield return useGrappleGizmo;
//            }
//            Gizmo toggleGrappleGizmo = GetToggleGrappleGizmo(pawn);
//            if(toggleGrappleGizmo != null)
//            {
//                yield return toggleGrappleGizmo;
//            }
//        }

//        private static Gizmo GetToggleGrappleGizmo(Pawn pawn)
//        {
//            try
//            {
//                if(!RV2Mod.Settings.combat.ToggleGrappleGizmoEnabled)
//                {
//                    return null;
//                }

//                PawnData pawnData = pawn.PawnData();

//                Command_Action command = new Command_Action()
//                {
//                    action = () =>
//                    {
//                        pawnData.CanUseGrapple = !pawnData.CanUseGrapple;
//                    },
//                    defaultLabel = "RV2_Command_ToggleGrapple".Translate(),
//                    icon = pawnData.CanUseGrapple ? UITextures.ToggleGrappleOn : UITextures.ToggleGrappleOff
//                };
//                command.defaultDesc = pawnData.CanUseGrapple ? "RV2_Command_ToggleGrapple_DescOn" : "RV2_Command_ToggleGrapple_DescOff";
//                command.defaultDesc = command.defaultDesc.Translate();
//                return command;
//            }
//            catch(Exception e)
//            {
//                Log.Warning("RimVore-2: Something went wrong when trying to add the toggle grapple gizmo: " + e);
//                return null;
//            }
//        }

//        private static Gizmo GetUseGrappleGizmo(Pawn pawn)
//        {
//            try
//            {
//                if(!UseGrappleGizmoAvailable(pawn, out _))
//                {
//                    return null;
//                }
//                bool canGrapple = CombatUtility.GetGrappleChance(pawn, true) > 0;
//                if(!canGrapple)
//                {
//                    return null;
//                }
//                Command_Target command = new Command_Target()
//                {
//                    defaultLabel = "RV2_Command_Grapple".Translate(),
//                    defaultDesc = "RV2_Command_Grapple_Desc".Translate(),
//                    targetingParams = GrappleTragetingParameters(pawn),
//                    icon = UITextures.GrappleAttacker
//                };

//                if(!CombatUtility.CanGrapple(pawn, out string reason))
//                {
//                    command.Disable(reason);
//                }
//                command.action = (LocalTargetInfo target) =>
//                {
//                    Pawn targetPawn = target.Pawn;
//                    Action grappleAction = UseGrappleAction(pawn, targetPawn, out reason);
//                    if(grappleAction != null)
//                    {
//                        grappleAction();
//                    }
//                    else
//                    {
//                        Messages.Message(reason, targetPawn, MessageTypeDefOf.RejectInput, false);
//                    }
//                };
//                return command;
//            }
//            catch(Exception e)
//            {
//                Log.Warning("RimVore-2: Something went wrong when trying to add the use grapple gizmo: " + e);
//                return null;
//            }

//        }
//        private static bool UseGrappleGizmoAvailable(Pawn pawn, out string reason)
//        {
//            if(!RV2Mod.Settings.combat.UseGrappleGizmoEnabled)
//            {
//                reason = "RV2_UseGrappleGizmoInvalid_DisabledInSettings".Translate();
//                return false;
//            }
//            if(!pawn.Drafted)
//            {
//                reason = "IsNotDraftedLower".Translate(pawn.LabelShort, pawn);
//                return false;
//            }
//            if(!pawn.IsColonistPlayerControlled)
//            {
//                reason = "CannotOrderNonControlledLower".Translate();
//                return false;
//            }
//            if(pawn.WorkTagIsDisabled(WorkTags.Violent))
//            {
//                reason = "IsIncapableOfViolenceLower".Translate(pawn.LabelShort, pawn);
//                return false;
//            }

//            reason = null;
//            return true;
//        }
//        private static Action UseGrappleAction(Pawn pawn, LocalTargetInfo target, out string failureReason)
//        {
//            if(!UseGrappleGizmoAvailable(pawn, out failureReason))
//            {
//                return null;
//            }
//            if(target.IsValid && !pawn.CanReach(target, PathEndMode.Touch, Danger.Deadly, false, false, TraverseMode.ByPawn))
//            {
//                failureReason = "NoPath".Translate();
//                return null;
//            }
//            Pawn targetPawn = target.Pawn;
//            if(pawn == targetPawn)
//            {
//                failureReason = "CannotAttackSelf".Translate();
//                return null;
//            }
//            bool inSameFaction = pawn.Faction == targetPawn.Faction
//                || pawn.InSameExtraFaction(targetPawn, ExtraFactionType.HomeFaction)
//                || pawn.InSameExtraFaction(targetPawn, ExtraFactionType.MiniFaction);
//            if(inSameFaction)
//            {
//                failureReason = "CannotAttackSameFactionMember".Translate();
//                return null;
//            }

//            failureReason = null;
//            return () =>
//            {
//                Job gotoJob = JobMaker.MakeJob(JobDefOf.Goto, targetPawn);
//                pawn.jobs.TryTakeOrderedJob(gotoJob, JobTag.DraftedOrder);

//                Job grappleJob = JobMaker.MakeJob(VoreJobDefOf.RV2_VoreGrapple, targetPawn);
//                pawn.jobs.jobQueue.EnqueueFirst(grappleJob, JobTag.DraftedOrder);
//            };
//        }
//        private static TargetingParameters GrappleTragetingParameters(Pawn pawn)
//        {
//            Predicate<TargetInfo> validator = (TargetInfo target) =>
//            {
//                Pawn targetPawn = target.Thing as Pawn;
//                if(targetPawn == null)
//                    return false;
//                if(!CombatUtility.CanGrapple(pawn, out _, targetPawn))
//                    return false;
//                return true;
//            };
//            return new TargetingParameters()
//            {
//                canTargetBuildings = false,
//                validator = validator
//            };
//        }
//    }
//}