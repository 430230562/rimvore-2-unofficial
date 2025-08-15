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
    public class RitualBehaviorWorker_VoreFeast : RitualBehaviorWorker
    {
        public RitualBehaviorWorker_VoreFeast() : base() { }
        public RitualBehaviorWorker_VoreFeast(RitualBehaviorDef def) : base(def) { }

        protected override void PostExecute(TargetInfo target, Pawn organizer, Precept_Ritual ritual, RitualObligation obligation, RitualRoleAssignments assignments)
        {
            Pawn arg = assignments.AssignedPawns("speaker").FirstOrDefault<Pawn>();
            if(arg == null)
            {
                return;
            }
            Find.LetterStack.ReceiveLetter(this.def.letterTitle.Formatted(ritual.Named("RITUAL")), this.def.letterText.Formatted(arg.Named("SPEAKER"), ritual.Named("RITUAL"), ritual.ideo.MemberNamePlural.Named("IDEOMEMBERS")) + "\n\n" + ritual.outcomeEffect.ExtraAlertParagraph(ritual), LetterDefOf.PositiveEvent, target, null, null, null, null);
        }

        protected override LordJob CreateLordJob(TargetInfo target, Pawn organizer, Precept_Ritual ritual, RitualObligation obligation, RitualRoleAssignments assignments)
        {
            LordJob lordJob = base.CreateLordJob(target, organizer, ritual, obligation, assignments);
            if(lordJob is LordJob_Ritual ritualLordJob)
            {
                ritualLordJob.pawnsDeathIgnored.AddRange(assignments.Participants);
                return ritualLordJob;
            }
            return lordJob;
        }

        public override void PostCleanup(LordJob_Ritual ritual)
        {
            base.PostCleanup(ritual);
            JobGiver_RequestVore_VoreFeast.ClearAttemptedVoreCounts(ritual);
        }
    }
}