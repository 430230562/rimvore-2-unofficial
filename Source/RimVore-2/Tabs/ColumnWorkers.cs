using RimVore2.Tabs;
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
    class PawnColumnWorker_VorePrey : PawnColumnWorker
    {
        private static readonly NumericStringComparer Comparer = new NumericStringComparer();
        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            Rect CellRect = new Rect(rect.x, rect.y, rect.width, Mathf.Min(rect.height, 30f));
            VoreTabHelper.ResetGUI();
            try
            {
                VoreTrackerRecord record = pawn.GetVoreRecord();
                Pawn TopPred = record.TopPredator;

                string name = GetText(pawn);
                string TopName = GetText(TopPred);

                Text.Anchor = TextAnchor.MiddleLeft;
                Text.WordWrap = false;


                Widgets.Label(CellRect, name);

                Text.WordWrap = true;
                Text.Anchor = TextAnchor.UpperLeft;

                float progress = record.CurrentVoreStage.PercentageProgress;
                if(progress < 0)
                {
                    Rect progressRect = new Rect(CellRect);
                    //These numbers are from rimworld worker label for health rect
                    progressRect.xMin -= 4f;
                    progressRect.yMin += 4f;
                    progressRect.yMax -= 6f;
                    Widgets.FillableBar(progressRect, progress, GenMapUI.OverlayHealthTex, BaseContent.ClearTex, false);
                }

                //Tooltip and button jump to top pred on map
                if(TopPred.Map == Find.CurrentMap)
                {
                    if(Widgets.ButtonInvisible(CellRect))
                    {
                        CameraJumper.TryJumpAndSelect(TopPred);
                    }
                    else if(Mouse.IsOver(CellRect))
                    {
                        TooltipHandler.TipRegion(CellRect, "ClickToJumpTo".Translate() + "\n" + TopName);
                    }
                }
            }
            catch(Exception e)
            {
                Widgets.Label(CellRect, $"{e}");
            }
            VoreTabHelper.ResetGUI();
        }
        public override int GetMinWidth(PawnTable table)
        {
            return Mathf.Max(base.GetMinWidth(table), 80);
        }

        public override int GetOptimalWidth(PawnTable table)
        {
            return Mathf.Clamp(165, GetMinWidth(table), GetMaxWidth(table));
        }
        public override int Compare(Pawn a, Pawn b)
        {
            return Comparer.Compare(GetText(a), GetText(b));
        }
        private string GetText(Pawn pawn)
        {
            string name = pawn.Name?.ToStringFull;
            if(name == null)
            {
                name = pawn.def.LabelCap;
            }
            return name;
        }
    }
    class PawnColumnWorker_VorePred : PawnColumnWorker
    {
        private static readonly NumericStringComparer Comparer = new NumericStringComparer();
        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            VoreTabHelper.ResetGUI();
            Rect CellRect = new Rect(rect.x, rect.y, rect.width, Mathf.Min(rect.height, 30f));
            try
            {
                VoreTrackerRecord record = pawn.GetVoreRecord();
                Pawn Pred = record.Predator;
                Pawn TopPred = record.TopPredator;

                string name = GetText(Pred);
                string topName = GetText(TopPred);

                Text.Anchor = TextAnchor.MiddleLeft;
                Text.WordWrap = false;

                Widgets.Label(CellRect, name);

                Text.WordWrap = true;
                Text.Anchor = TextAnchor.UpperLeft;

                //Tooltip and button jump to top pred on map
                if(TopPred.Map == Find.CurrentMap)
                {
                    if(Widgets.ButtonInvisible(CellRect))
                    {
                        CameraJumper.TryJumpAndSelect(TopPred);
                    }
                    else if(Mouse.IsOver(CellRect))
                    {
                        TooltipHandler.TipRegion(CellRect, "ClickToJumpTo".Translate() + "\n" + topName);
                    }
                }
            }
            catch(Exception e)
            {
                Widgets.Label(CellRect, $"{e}");
            }
            VoreTabHelper.ResetGUI();
        }
        public override int GetMinWidth(PawnTable table)
        {
            return Mathf.Max(base.GetMinWidth(table), 80);
        }

        public override int GetOptimalWidth(PawnTable table)
        {
            return Mathf.Clamp(165, GetMinWidth(table), GetMaxWidth(table));
        }
        protected string GetTextFor(Pawn pawn)
        {
            return pawn.LabelCap;
        }
        public override int Compare(Pawn a, Pawn b)
        {
            VoreTrackerRecord ARecord = a.GetVoreRecord();
            Pawn APred = ARecord?.Predator;
            VoreTrackerRecord BRecord = b.GetVoreRecord();
            Pawn BPred = BRecord?.Predator;
            return Comparer.Compare(GetText(APred), GetText(BPred));
        }
        private string GetText(Pawn pawn)
        {
            if(pawn == null)
            {
                return "NULL";
            }
            string name = pawn.Name?.ToStringFull;
            if(name == null)
            {
                name = pawn.def.LabelCap;
            }
            return name;
        }
    }
    class PawnColumnWorker_VoreStageProgress : PawnColumnWorker_Text
    {
        protected override string GetTextFor(Pawn pawn)
        {
            try
            {
                VoreTrackerRecord record = pawn.GetVoreRecord();
                if(record == null)
                {
                    return string.Empty;
                }
                float partProgress = record.CurrentVoreStage.PercentageProgress;
                if(partProgress < 0)
                {
                    return string.Empty;
                }
                string progress = UIUtility.PercentagePresenter(partProgress);
                return $"{progress}";
            }
            catch(Exception e)
            {
                return $"{e}";
            }
        }
    }
    class PawnColumnWorker_VorePart : PawnColumnWorker_Text
    {
        protected override string GetTextFor(Pawn pawn)
        {
            VoreTrackerRecord record = pawn.GetVoreRecord();
            if(record == null)
            {
                return string.Empty;
            }
            return record.CurrentVoreStage.def.DisplayPartName.CapitalizeFirst();
        }
    }
    class PawnColumnWorker_VorePartGoal : PawnColumnWorker_Text
    {
        protected override string GetTextFor(Pawn pawn)
        {
            VoreTrackerRecord record = pawn.GetVoreRecord();
            if(record == null)
            {
                return string.Empty;
            }
            PartGoalDef partGoal = record.CurrentVoreStage.def.partGoal;
            if(partGoal == null)
            {
                return string.Empty;
            }
            return partGoal.LabelCap;
        }
    }
    class PawnColumnWorker_VoreGoal : PawnColumnWorker_Text
    {
        protected override string GetTextFor(Pawn pawn)
        {
            VoreTrackerRecord record = pawn.GetVoreRecord();
            if(record == null)
            {
                return "";
            }
            return record.VoreGoal.LabelCap;
        }

    }
    class PawnColumnWorker_VoreStruggle : PawnColumnWorker_Text
    {
        float StruggleProgressFor(Pawn p) => p.GetVoreRecord()?.StruggleManager?.StruggleProgress ?? 0f;

        protected override string GetTextFor(Pawn pawn)
        {
            try
            {
                StruggleManager manager = pawn.GetVoreRecord()?.StruggleManager;
                if(manager == null)
                {
                    return string.Empty;
                }
                if(!manager.ShouldStruggle)
                {
                    return "-";
                }
                return UIUtility.PercentagePresenter(manager.StruggleProgress);
            }
            catch(Exception e)
            {
                return $"{e}";
            }
        }

        public override int Compare(Pawn a, Pawn b)
        {
            float aStruggle = StruggleProgressFor(a);
            float bStruggle = StruggleProgressFor(b);
            return aStruggle.CompareTo(bStruggle);
        }
    }
    class PawnColumnWorker_VoreButtons : PawnColumnWorker
    {
        protected virtual int Width => def.width;
        private const int IconSize = DefaultCellHeight;
        private float MaxWidth = 75;
        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            try
            {
                Rect ButtonsRect = StandardVoreButtonsHelper.DrawButtons(pawn, rect.x, rect.y, IconSize);
                MaxWidth = Math.Max(MaxWidth, ButtonsRect.width);
            }
            catch(Exception e)
            {
                Widgets.Label(rect, $"{e}");
            }
        }
        public override int GetMinWidth(PawnTable table)
        {
            return (int)Mathf.Max(base.GetMinWidth(table), MaxWidth);
        }
    }
}
