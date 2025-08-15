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
    public class VoreProposal_Feeder_Predator : VoreProposal
    {
        public VoreProposal_Feeder_Predator(Pawn initiator, Pawn primaryTarget) : base(initiator, primaryTarget) { }

        public VoreProposal_Feeder_Predator() : base() { }

        public override VoreRole RoleOf(Pawn pawn)
        {
            if(pawn == Initiator)
            {
                return VoreRole.Feeder;
            }
            if(pawn == PrimaryTarget)
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
                    notificationText = "RV2_Message_VoreProposalForced_Feeder_Predator";
                    break;
                case ProposalStatus.Accepted:
                    notificationText = "RV2_Message_VoreProposalAccepted_Feeder_Predator";
                    break;
                case ProposalStatus.Denied:
                    notificationText = "RV2_Message_VoreProposalDenied_Feeder_Predator";
                    break;
                default:
                    RV2Log.Error("Invalid proposal status: " + status);
                    return;
            }
            notificationText = notificationText.Translate(Initiator.LabelShortCap.Named("FEEDER"), PrimaryTarget.LabelShortCap.Named("PREDATOR"));

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
