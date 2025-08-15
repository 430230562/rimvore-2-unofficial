using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public static class NotificationUtility
    {
        public static void DoNotification(NotificationType type, string text, string label = "Vore", LookTargets targets = null)
        {
            switch(type)
            {
                case NotificationType.Letter:
                    DoLetter(LetterDefOf.NeutralEvent, text, label, targets);
                    break;
                case NotificationType.LetterThreatSmall:
                    DoLetter(LetterDefOf.ThreatSmall, text, label, targets);
                    break;
                case NotificationType.LetterThreatBig:
                    DoLetter(LetterDefOf.ThreatBig, text, label, targets);
                    break;
                case NotificationType.MessageNeutral:
                    DoMessage(MessageTypeDefOf.NeutralEvent, text);
                    break;
                case NotificationType.MessageThreatSmall:
                    DoMessage(MessageTypeDefOf.ThreatSmall, text);
                    break;
                case NotificationType.MessageThreatBig:
                    DoMessage(MessageTypeDefOf.ThreatBig, text);
                    break;
            }
        }
        public static void DoLetter(LetterDef letterDef, string text, string label, LookTargets targets = null)
        {

            Letter letter = LetterMaker.MakeLetter(label, text, letterDef, targets);
            Find.LetterStack.ReceiveLetter(letter);
            //Find.LetterStack.ReceiveLetter("LetterLabelPredatorHuntingColonist".Translate(this.pawn.LabelShort, prey.LabelDefinite(), this.pawn.Named("PREDATOR"), prey.Named("PREY")).CapitalizeFirst(), "LetterPredatorHuntingColonist".Translate(this.pawn.LabelIndefinite(), prey.LabelDefinite(), this.pawn.Named("PREDATOR"), prey.Named("PREY")).CapitalizeFirst(), LetterDefOf.ThreatBig, this.pawn, null, null, null, null);
        }
        public static void DoMessage(MessageTypeDef messageTypeDef, string text)
        {
            Message notificationMessage = new Message(text, messageTypeDef);
            Messages.Message(notificationMessage);
        }

        public static NotificationType ProposalNotificationType(bool isPassed, VorePathDef path = null)
        {
            if(isPassed)
            {
                if(path != null && path.voreGoal.IsLethal)
                    return RV2Mod.Settings.fineTuning.FatalProposalAcceptedNotification;
                else
                    return RV2Mod.Settings.fineTuning.ProposalAcceptedNotification;
            }
            else
            {
                return RV2Mod.Settings.fineTuning.ProposalDeniedNotification;
            }
        }
    }
}
