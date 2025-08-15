using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimVore2
{
    public class Window_RuleEditor : Window
    {
        private readonly VoreRule workingRule;
        private readonly RuleTarget workingTarget;
        private readonly bool isFirstRule = false;
        public static int activeIndex;

        public Window_RuleEditor()
        {
            absorbInputAroundWindow = true;
            focusWhenOpened = true;
            draggable = true;
            resizeable = true;
            forcePause = true;
            onlyOneOfTypeAllowed = true;
            UIUtility.OverwriteResizer(this, 400, 400);
        }

        public Window_RuleEditor(int ruleEntryIndex) : this()
        {
            activeIndex = ruleEntryIndex;
            RuleEntry activeEntry = RV2Mod.Settings.rules.Rules[ruleEntryIndex];
            workingRule = (VoreRule)activeEntry.Rule.Clone();
            workingTarget = (RuleTarget)activeEntry.Target.Clone();
            isFirstRule = ruleEntryIndex == 0;
            RuleTarget.treeRequiredHeightStale = true;
        }

        public override void PreClose()
        {
            base.PreClose();
            if(RV2Mod.Mod != null)
            {
                RV2Mod.Mod.WriteSettings();
            }
        }
        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(UI.screenWidth * 0.4f, UI.screenHeight * 0.6f);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            float exitButtonHeight = 40f;
            Listing_Standard list = new Listing_Standard()
            {
                ColumnWidth = inRect.width,
                maxOneColumn = true
            };
            list.Begin(inRect);
            if(!isFirstRule)
            {
                workingTarget.DrawInteractible(list);
                //list.GapLine();
            }
            Rect controlRect = list.GetRect(inRect.height - list.CurHeight - exitButtonHeight);
            workingRule.DoControls(controlRect, workingTarget.TargetRole, isFirstRule);
            Rect exitButtonRect = list.GetRect(exitButtonHeight);
            UIUtility.DoSaveCancelButtons(exitButtonRect, Save, Cancel);
            list.End();
        }

        private void Save()
        {
            RuleEntry newEntry = new RuleEntry(workingTarget, workingRule);
            RV2Mod.Settings.rules.Rules[activeIndex] = newEntry;
            RV2Mod.Settings.rules.NotifyStale();
            Close();

            //Log.Message("Saved rule editor, active target: " + target);
        }

        private void Cancel()
        {
            Close();
        }
    }
}