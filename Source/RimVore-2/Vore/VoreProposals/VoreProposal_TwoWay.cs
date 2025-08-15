using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;

namespace RimVore2
{
    public class VoreProposal_TwoWay : VoreProposal
    {
        private Pawn predator;
        private Pawn prey;
        public VorePathDef VorePath;

        public bool InitiatorIsPredator => predator == Initiator;

        public VoreProposal_TwoWay(Pawn predator, Pawn prey, Pawn initiator, Pawn primaryTarget, VorePathDef path) : base(initiator, primaryTarget)
        {
            this.predator = predator;
            this.prey = prey;
            this.VorePath = path;
        }

        public VoreProposal_TwoWay() : base() { }

        protected override bool RollSuccess()
        {
            if(status == ProposalStatus.Forced)
            {
                return true;
            }
            float chanceToAccept = PreferenceUtility.GetChanceToAcceptProposal(this);
            if(ModsConfig.IdeologyActive)
            {
                bool isRitualRelated = predator.GetLord()?.LordJob is LordJob_Ritual || prey.GetLord()?.LordJob is LordJob_Ritual;
                if(isRitualRelated)
                {
                    chanceToAccept *= RV2Mod.Settings.ideology.VoreFeastProposalAcceptanceModifier;
                }
            }
            if(RV2Log.ShouldLog(true, "Preferences"))
                RV2Log.Message($"Chance to accept: {Math.Round(chanceToAccept * 100)}%", false, "Preferences");
            return Rand.Chance(chanceToAccept);
        }

        protected override void DoNotification()
        {
            string notificationText;
            switch(status)
            {
                case ProposalStatus.Forced:
                    notificationText = "RV2_Message_VoreProposalForced";
                    break;
                case ProposalStatus.Accepted:
                    notificationText = "RV2_Message_VoreProposalAccepted";
                    break;
                case ProposalStatus.Denied:
                    notificationText = "RV2_Message_VoreProposalDenied";
                    break;
                default:
                    RV2Log.Error("Invalid proposal status: " + status);
                    return;
            }
            notificationText = notificationText.Translate(Initiator.LabelShortCap.Named("INITIATOR"), PrimaryTarget.LabelShortCap.Named("TARGET"));
            if(IsPassed)    // we don't care about the path description if the proposal was denied
                notificationText += " => " + VorePath.actionDescription.Formatted(predator.LabelShortCap.Named("PREDATOR"), prey.LabelShortCap.Named("PREY"));

            NotificationType notificationType = NotificationUtility.ProposalNotificationType(IsPassed, VorePath);

            NotificationUtility.DoNotification(notificationType, notificationText, targets: new LookTargets(ParticipatingPawns()));
        }

        protected override void Denied()
        {
            base.Denied();

            Initiator.jobs.EndCurrentJob(Verse.AI.JobCondition.Succeeded);

            float voreFightingChance = RV2Mod.Settings.fineTuning.ForcedVoreChanceOnFailedProposal;
            QuirkManager initiatorQuirks = Initiator.QuirkManager();
            if(initiatorQuirks != null)
            {
                voreFightingChance = initiatorQuirks.ModifyValue("FailedProposalForceVoreChance", voreFightingChance);
            }
            bool canFightForVore = RV2Mod.Settings.rules.CanForceFailedProposal(Initiator, PrimaryTarget) && !PrimaryTarget.Downed;
            if(canFightForVore && Rand.Chance(voreFightingChance))
            {
                StartVoreFightMentalState();
            }
            VoreThoughtUtility.NotifyDeniedProposal(Initiator, PrimaryTarget, RoleOf(Initiator), VorePath.voreType, VorePath.voreGoal);
        }

        private void StartVoreFightMentalState()
        {
            MentalStateDef attackerMentalStateDef = RV2_Common.VoreFighting_Attacker;
            MentalStateDef defenderMentalStateDef = RV2_Common.VoreFighting_Defender;
            Initiator.mindState.mentalStateHandler.TryStartMentalState(attackerMentalStateDef);
            if(Initiator.MentalStateDef != attackerMentalStateDef)
            {
                Log.Warning($"Pawn {Initiator.LabelShort} did not start attacker mental state, cancelling vore-fighting");
                return;
            }
            MentalState_ForcedVoreFight_Attacker initiatorMentalState = (MentalState_ForcedVoreFight_Attacker)Initiator.MentalState;
            initiatorMentalState.proposal = this;
            initiatorMentalState.otherPawn = PrimaryTarget;
            PrimaryTarget.mindState.mentalStateHandler.TryStartMentalState(defenderMentalStateDef);
            if(PrimaryTarget.MentalStateDef != defenderMentalStateDef)
            {
                Log.Warning($"Pawn {PrimaryTarget.LabelShort} did not start defender mental state, cancelling vore-fighting");
                Initiator.ClearMind();
                return;
            }
            MentalState_ForcedVoreFight_Defender targetMentalState = (MentalState_ForcedVoreFight_Defender)PrimaryTarget.MentalState;
            targetMentalState.otherPawn = Initiator;
        }

        protected override void Accepted()
        {
            base.Accepted();
            Initiator?.skills?.Learn(SkillDefOf.Social, 500f);
        }

        public override VoreRole RoleOf(Pawn pawn)
        {
            if(pawn == predator)
            {
                return VoreRole.Predator;
            }
            if(pawn == prey)
            {
                return VoreRole.Prey;
            }
            return VoreRole.Invalid;
        }
        public override Pawn RoleFor(VoreRole role)
        {
            switch(role)
            {
                case VoreRole.Predator:
                    return predator;
                case VoreRole.Prey:
                    return prey;
                default:
                    return null;
            }
        }

        public override void ExposeData()
        {
            Scribe_References.Look(ref predator, "Predator");
            Scribe_References.Look(ref prey, "Prey");
            Scribe_Defs.Look(ref VorePath, "VorePath");
            Scribe_Values.Look(ref status, "status");
        }
    }
}
