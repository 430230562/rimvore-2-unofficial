using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimVore2
{
    public class UseGrappleButton : SubGizmo
    {
        public UseGrappleButton(Pawn pawn) : base(pawn) { }

        protected override Texture2D CurrentTexture => ContentFinder<Texture2D>.Get("Widget/grappleAttacker");

        public override bool IsVisible()
        {
            bool settingsAllow = RV2Mod.Settings.features.GrapplingEnabled && RV2Mod.Settings.combat.UseGrappleGizmoEnabled;
            if(!settingsAllow)
            {
                return false;
            }
            // never show this gizmo for pawns outside of player control
            return pawn.IsColonistPlayerControlled || pawn.IsColonyMechPlayerControlled;
            // colony animals don't get this gizmo either
        }

        protected override string CurrentTip
        {
            get
            {
                string baseTip = "RV2_Widget_Grapple".Translate();
                if(UseGrappleGizmoAvailable(pawn, out string reason))
                {
                    return baseTip;
                }
                else
                {
                    return $"{"RV2_Widget_Grapple_UnavailablePrefix".Translate()}{baseTip} \n\n {reason}";
                }
            }
        }

        public override void Action()
        {
            Find.Targeter.BeginTargeting(GrappleTragetingParameters(), (LocalTargetInfo target) =>
            {

                Pawn targetPawn = target.Pawn;
                Action grappleAction = UseGrappleAction(pawn, targetPawn, out string reason);
                if(grappleAction != null)
                {
                    grappleAction();
                }
                else
                {
                    Messages.Message(reason, targetPawn, MessageTypeDefOf.RejectInput, false);
                }
            });
        }
        private static Action UseGrappleAction(Pawn pawn, LocalTargetInfo target, out string failureReason)
        {
            if(!UseGrappleGizmoAvailable(pawn, out failureReason))
            {
                return null;
            }
            if(target.IsValid && !pawn.CanReach(target, PathEndMode.Touch, Danger.Deadly, false, false, TraverseMode.ByPawn))
            {
                failureReason = "NoPath".Translate();
                return null;
            }
            Pawn targetPawn = target.Pawn;
            if(pawn == targetPawn)
            {
                failureReason = "CannotAttackSelf".Translate();
                return null;
            }
            bool inSameFaction = pawn.Faction == targetPawn.Faction
                || pawn.InSameExtraFaction(targetPawn, ExtraFactionType.HomeFaction)
                || pawn.InSameExtraFaction(targetPawn, ExtraFactionType.MiniFaction);

            bool isHostile = targetPawn.HostileTo(pawn.Faction);
            if(inSameFaction && !isHostile)
            {
                failureReason = "CannotAttackSameFactionMember".Translate();
                return null;
            }

            failureReason = null;
            return () =>
            {
                Job gotoJob = JobMaker.MakeJob(JobDefOf.Goto, targetPawn);
                pawn.jobs.TryTakeOrderedJob(gotoJob, JobTag.DraftedOrder);

                Job grappleJob = JobMaker.MakeJob(VoreJobDefOf.RV2_VoreGrapple, targetPawn);
                pawn.jobs.jobQueue.EnqueueFirst(grappleJob, JobTag.DraftedOrder);
            };
        }

        public override bool Draw(Rect iconRect)
        {
            bool isAvailable = UseGrappleGizmoAvailable(pawn, out _);
            TooltipHandler.TipRegion(iconRect, CurrentTip);
            Color previousColor = GUI.color;
            if(!isAvailable)
            {
                GUI.color = Color.grey;
            }
            GUI.DrawTexture(iconRect, CurrentTexture);
            GUI.color = previousColor;

            if(isAvailable && Widgets.ButtonInvisible(iconRect, false))
            {
                Action();
                return true;
            }
            return false;
        }

        private static bool UseGrappleGizmoAvailable(Pawn pawn, out string reason)
        {
            if(!RV2Mod.Settings.combat.UseGrappleGizmoEnabled)
            {
                reason = "RV2_UseGrappleGizmoInvalid_DisabledInSettings".Translate();
                return false;
            }
            if(!pawn.Drafted)
            {
                reason = "IsNotDraftedLower".Translate(pawn.LabelShortCap, pawn);
                return false;
            }
            if(!pawn.IsColonistPlayerControlled)
            {
                reason = "CannotOrderNonControlledLower".Translate();
                return false;
            }
            if(pawn.WorkTagIsDisabled(WorkTags.Violent))
            {
                reason = "IsIncapableOfViolenceLower".Translate(pawn.LabelShortCap, pawn);
                return false;
            }

            reason = null;
            return true;
        }

        private TargetingParameters GrappleTragetingParameters()
        {
            Predicate<TargetInfo> validator = (TargetInfo target) =>
            {
                Pawn targetPawn = target.Thing as Pawn;
                if(targetPawn == null)
                    return false;
                if(!CombatUtility.CanGrapple(pawn, out _, targetPawn))
                    return false;
                return true;
            };
            return new TargetingParameters()
            {
                canTargetBuildings = false,
                validator = validator
            };
        }
    }
}
