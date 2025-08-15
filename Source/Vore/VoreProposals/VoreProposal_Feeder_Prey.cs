using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace RimVore2
{
    public class VoreProposal_Feeder_Prey : VoreProposal
    {
        public Pawn Predator;
        public VorePathDef VorePath;
        public VoreProposal_Feeder_Prey(Pawn initiator, Pawn predator, Pawn primaryTarget, VorePathDef vorePath) : base(initiator, primaryTarget)
        {
            this.Predator = predator;
            VorePath = vorePath;
        }

        public VoreProposal_Feeder_Prey() : base() { }

        public override VoreRole RoleOf(Pawn pawn)
        {
            if(pawn == Initiator)
            {
                return VoreRole.Feeder;
            }
            if(pawn == PrimaryTarget)
            {
                return VoreRole.Prey;
            }
            if(pawn == Predator)
            {
                return VoreRole.Predator;
            }
            return VoreRole.Invalid;
        }
        public override Pawn RoleFor(VoreRole role)
        {
            switch(role)
            {
                case VoreRole.Feeder:
                    return Initiator;
                case VoreRole.Predator:
                    return Predator;
                case VoreRole.Prey:
                    return PrimaryTarget;
                default:
                    return null;
            }
        }

        protected override void DoNotification()
        {
            string notificationText;
            switch(status)
            {
                case ProposalStatus.Forced:
                    notificationText = "RV2_Message_VoreProposalForced_Feeder_Prey";
                    break;
                case ProposalStatus.Accepted:
                    notificationText = "RV2_Message_VoreProposalAccepted_Feeder_Prey";
                    break;
                case ProposalStatus.Denied:
                    notificationText = "RV2_Message_VoreProposalDenied_Feeder_Prey";
                    break;
                default:
                    RV2Log.Error("Invalid proposal status: " + status);
                    return;
            }
            notificationText = notificationText.Translate(Initiator.LabelShortCap.Named("FEEDER"), PrimaryTarget.LabelShortCap.Named("PREY"), Predator.LabelShortCap.Named("PREDATOR"));

            if(IsPassed)    // we don't care about the path description if the proposal was denied
            {
                notificationText += " => " + VorePath.actionDescription.Formatted(Predator.LabelShortCap.Named("PREDATOR"), PrimaryTarget.LabelShortCap.Named("PREY"));
            }


            NotificationType notificationType = NotificationUtility.ProposalNotificationType(IsPassed);

            NotificationUtility.DoNotification(notificationType, notificationText, targets: new LookTargets(ParticipatingPawns()));
        }

        protected override bool RollSuccess()
        {
            if(status == ProposalStatus.Forced)
            {
                return true;
            }
            float chanceToAccept = PreferenceUtility.GetChanceToAcceptProposal(this);

            if(RV2Log.ShouldLog(true, "Preferences"))
                RV2Log.Message($"Chance to accept feeder proposal: {Math.Round(chanceToAccept * 100)}%", false, "Preferences");
            return Rand.Chance(chanceToAccept);
        }
    }
}
