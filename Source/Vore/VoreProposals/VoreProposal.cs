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
    public abstract class VoreProposal : IExposable
    {
        public Pawn Initiator;
        public Pawn PrimaryTarget;

        protected ProposalStatus status = ProposalStatus.Pending;
        public bool IsPassed => status == ProposalStatus.Forced || status == ProposalStatus.Accepted;
        public bool IsForced => status == ProposalStatus.Forced;

        public VoreProposal(Pawn initiator, Pawn primaryTarget)
        {
            this.Initiator = initiator;
            this.PrimaryTarget = primaryTarget;
            if(PreferenceUtility.CanBeForced(primaryTarget))
            {
                this.status = ProposalStatus.Forced;
            }
        }
        public VoreProposal() { }

        protected abstract bool RollSuccess();
        protected abstract void DoNotification();
        public abstract VoreRole RoleOf(Pawn pawn);
        public abstract Pawn RoleFor(VoreRole role);
        protected virtual void Accepted()
        {
            if(status != ProposalStatus.Forced)
            {
                status = ProposalStatus.Accepted;
            }
        }
        protected virtual void Denied()
        {
            status = ProposalStatus.Denied;
        }
        protected virtual IEnumerable<Pawn> ParticipatingPawns()
        {
            yield return Initiator;
            yield return PrimaryTarget;
        }
        protected virtual bool ShouldNotifyPlayer()
        {
            return ParticipatingPawns().Any(p => p.Faction != null && p.Faction.IsPlayer);
        }
        protected virtual void DoInteraction()
        {
            List<RulePackDef> extraSentences = new List<RulePackDef>();
            switch(status)
            {
                case ProposalStatus.Accepted:
                    extraSentences.Add(VoreRulePackDefOf.RV2_Proposal_Accepted);
                    break;
                case ProposalStatus.Denied:
                    extraSentences.Add(VoreRulePackDefOf.RV2_Proposal_Denied);
                    break;
                case ProposalStatus.Forced:
                    extraSentences.Add(VoreRulePackDefOf.RV2_Proposal_Forced);
                    break;
            }
            PlayLogEntry_Interaction logEntry = new PlayLogEntry_Interaction(VoreInteractionDefOf.RV2_Proposal, Initiator, PrimaryTarget, extraSentences);
            Find.PlayLog.Add(logEntry);
        }

        public bool TryProposal()
        {
            if(RollSuccess())
            {
                Accepted();
            }
            else
            {
                Denied();
            }
            if (ShouldNotifyPlayer())
            {
                DoNotification();
            }
            DoInteraction();
            return IsPassed;
        }

        public virtual void ExposeData()
        {
            Scribe_References.Look(ref Initiator, "Initiator");
            Scribe_References.Look(ref PrimaryTarget, "Target");
            Scribe_Values.Look(ref status, "status");
        }
    }
}
