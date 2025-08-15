using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimVore2.Tabs
{
    public static class StandardVoreButtonsHelper
    {
        public static List<VoreButton> VoreButtons = new List<VoreButton>();
        private static void InitVoreButtonsList()
        {
            InitButtons();
            VoreButtons.OrderBy(b => b.GetOrder());
        }
        //This function is seperated from InitVoreButtonList so that submods can add button without effecting ordering logic
        private static void InitButtons()
        {
            VoreButtons.Add(new InspectButton());
            VoreButtons.Add(new TypeButton());
            VoreButtons.Add(new GoalButton());
            VoreButtons.Add(new EjectButton());
            VoreButtons.Add(new ManualPassButton());
            VoreButtons.Add(new StruggleButton());
        }
        public static Rect DrawButtons(Pawn Pawn, float StartingX, float StartingY, float IconSize)
        {
            Rect rect = new Rect();
            rect.x = StartingX;
            rect.y = StartingY;
            float curIconX = 0;
            VoreTrackerRecord record = Pawn.GetVoreRecord();
            if(VoreButtons.NullOrEmpty())
            {
                InitVoreButtonsList();
            }
            foreach(VoreButton btn in VoreButtons)
            {
                curIconX += btn.Draw(StartingX + curIconX, StartingY, IconSize, record);
            }

            rect.width = curIconX;
            rect.height = IconSize;

            return rect;
        }
    }
    public abstract class VoreButton
    {
        public static bool CanPlayerGiveCommand(Pawn pawn)
        {
            return !pawn.Dead && pawn.IsColonist && !pawn.InMentalState && !pawn.Downed && !pawn.IsBurning();
        }

        public float Draw(float X, float Y, float IconSize, VoreTrackerRecord record)
        {
            return VoreTabHelper.DrawIconConditionally(X, Y, IconSize, GetIcon(record), () => OnClick(record), GetToolTip(record), () => ConditionalCheck(record));
        }
        public abstract int GetOrder();
        public abstract string GetToolTip(VoreTrackerRecord record);
        public abstract Texture2D GetIcon(VoreTrackerRecord record);
        public abstract void OnClick(VoreTrackerRecord record);
        public abstract bool ConditionalCheck(VoreTrackerRecord record);
    }
    public class InspectButton : VoreButton
    {
        public override bool ConditionalCheck(VoreTrackerRecord record)
        {
            return true;
        }

        public override Texture2D GetIcon(VoreTrackerRecord record)
        {
            return TexButton.Search;
        }

        public override int GetOrder()
        {
            return 9;
        }

        public override string GetToolTip(VoreTrackerRecord record)
        {
            return "RV2_Tab_Inspect".Translate();
        }

        public override void OnClick(VoreTrackerRecord record)
        {
            Thing jumpTarget = record.Prey;
            if(jumpTarget == null)
            {
                return;
            }
            if (record.Prey.Dead)
            {
                jumpTarget = record.Prey.Corpse;
            }
            CameraJumper.TryJump(jumpTarget);
            Find.Selector.ClearSelection();
            Find.Selector.Select(jumpTarget);
        }
    }

    public class TypeButton : VoreButton
    {
        public override bool ConditionalCheck(VoreTrackerRecord record)
        {
            return true;
        }

        public override Texture2D GetIcon(VoreTrackerRecord record)
        {
            return record.VoreType.Icon;
        }

        public override int GetOrder()
        {
            return 10;
        }

        public override string GetToolTip(VoreTrackerRecord record)
        {
            return record.VoreType.label;
        }

        public override void OnClick(VoreTrackerRecord record)
        {
        }
    }
    public class GoalButton : VoreButton
    {
        public override bool ConditionalCheck(VoreTrackerRecord record)
        {
            return true;
        }

        public override Texture2D GetIcon(VoreTrackerRecord record)
        {
            return record.VoreGoal.Icon;
        }
        public override int GetOrder()
        {
            return 20;
        }
        public override string GetToolTip(VoreTrackerRecord record)
        {
            return record.VoreGoal.label;
        }

        public override void OnClick(VoreTrackerRecord record)
        {
            //todo goal switching
        }
    }
    public class EjectButton : VoreButton
    {
        public override bool ConditionalCheck(VoreTrackerRecord record)
        {
            return CanPlayerGiveCommand(record.Predator) && record.CanEject; ;
        }

        public override Texture2D GetIcon(VoreTrackerRecord record)
        {
            return UITextures.EjectButton;
        }
        public override int GetOrder()
        {
            return 30;
        }
        public override string GetToolTip(VoreTrackerRecord record)
        {
            return "RV2_RMB_EjectPrey_Self".Translate();
        }

        public override void OnClick(VoreTrackerRecord record)
        {
            Job ejectJob = JobMaker.MakeJob(VoreJobDefOf.RV2_EjectPreySelf, record.Prey);
            record.Predator.jobs.TryTakeOrderedJob(ejectJob);
        }
    }
    public class ManualPassButton : VoreButton
    {
        public override bool ConditionalCheck(VoreTrackerRecord record)
        {
            //Copied from RMB menu
            return CanPlayerGiveCommand(record.Predator)//Can control predator
                && record.IsManuallyPassed == false   // don't allow option if record is already manually passed
                && record.CurrentVoreStage.def.passConditions   // take all pass conditions of the current stage
                .Any(condition => condition is StagePassCondition_Manual);  // and check if any of them is a manual pass
        }

        public override Texture2D GetIcon(VoreTrackerRecord record)
        {
            return UITextures.ManualPassButton;
        }
        public override int GetOrder()
        {
            return 40;
        }
        public override string GetToolTip(VoreTrackerRecord record)
        {
            return "RV2_RMB_ManualPass".Translate();
        }

        public override void OnClick(VoreTrackerRecord record)
        {
            record.IsManuallyPassed = true;
        }
    }
    public class StruggleButton : VoreButton
    {
        public override bool ConditionalCheck(VoreTrackerRecord record)
        {
            return CanPlayerGiveCommand(record.Prey);
        }

        public override Texture2D GetIcon(VoreTrackerRecord record)
        {
            if(record.StruggleManager.ShouldStruggle)
            {
                return UITextures.IsStrugglingButton;
            }
            return UITextures.NotStrugglingButton;
        }
        public override int GetOrder()
        {
            return 50;
        }
        public override string GetToolTip(VoreTrackerRecord record)
        {
            return "RV2_Toggle_Struggle".Translate();
        }

        public override void OnClick(VoreTrackerRecord record)
        {
            record.StruggleManager.shouldStruggle = !record.StruggleManager.ShouldStruggle;
        }
    }
}
