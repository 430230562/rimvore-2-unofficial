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
    public class RitualBehaviorWorker_VoreSpeech : RitualBehaviorWorker
    {
        public RitualBehaviorWorker_VoreSpeech() : base() { }
        public RitualBehaviorWorker_VoreSpeech(RitualBehaviorDef def) : base(def) { }

        protected override void PostExecute(TargetInfo target, Pawn organizer, Precept_Ritual ritual, RitualObligation obligation, RitualRoleAssignments assignments)
        {
            Pawn arg = assignments.AssignedPawns("initiator").FirstOrDefault<Pawn>();
            if(arg == null)
            {
                return;
            }
            Find.LetterStack.ReceiveLetter(this.def.letterTitle.Formatted(ritual.Named("RITUAL")), this.def.letterText.Formatted(arg.Named("INITIATOR"), ritual.Named("RITUAL"), ritual.ideo.MemberNamePlural.Named("IDEOMEMBERS")) + "\n\n" + ritual.outcomeEffect.ExtraAlertParagraph(ritual), LetterDefOf.PositiveEvent, target, null, null, null, null);
        }
    }
}