using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimVore2
{
    public class RuleTargetComponentNode_Leaf : RuleTargetComponentNode
    {
        public RuleTargetComponent component;

        public RuleTargetComponentNode_Leaf() : base() { }
        public RuleTargetComponentNode_Leaf(RuleTargetComponentNode parent) : base(parent) { }

        public override string Label => component.Label;

        public override bool AppliesTo(Pawn pawn, RuleTargetRole role)
        {
            if(!component.AppliesToRole(role))
                return false;
            // the quick access needs to draw buttons dependent on role in general, not pawn specific, so we may have a null pawn here
            if(pawn == null)
            {
                return true;
            }
            return component.AppliesToPawn(pawn);
        }
        bool heightStale = true;
        float height = 9999f;
        public override void Draw(Listing_Standard list, RuleTargetComponentTree tree)
        {
            Rect inRect = list.GetRect(height);
            Rect controlRect = new Rect(inRect.x, inRect.y, controlWidth, inRect.height);
            DrawEntryControls(controlRect, tree);
            float targetWidth = (list.ColumnWidth - controlWidth) * targetWidthRatio;
            float explanationWidth = (list.ColumnWidth - controlWidth) * (1 - targetWidthRatio);
            Rect targetRect = new Rect(controlRect.xMax, inRect.y, targetWidth, inRect.height);
            Rect explanationRect = new Rect(targetRect.xMax, inRect.y, explanationWidth, inRect.height);
            Listing_Standard innerList = new Listing_Standard()
            {
                maxOneColumn = true
            };
            innerList.Begin(targetRect);
            base.Draw(innerList, tree);
            Action<string> buttonSetter = (string creatorKey) =>
            {
                component = RuleTargetComponentFactory.CreateComponent(creatorKey);
                RuleTarget.staleOnNextCycle = true;
                heightStale = true;
                height = 9999;
            };
            innerList.CreateLabelledDropDown(RuleTargetComponentFactory.Keys, component.ButtonTranslationKey, (string s) => s.Translate(), buttonSetter, null, "RV2_Settings_Rules_TargetHeader".Translate());
            component.DrawInteractible(innerList);
            if(heightStale)
            {
                height = innerList.CurHeight;
                RuleTarget.staleOnNextCycle = true;
                heightStale = false;
            }
            innerList.End();

            //Widgets.DrawLineVertical(controlRect.xMax, controlRect.y, controlRect.height);
            DrawExplanation(explanationRect);
        }

        const float targetConfirmationWidth = 72f;
        const float targetConfirmationSize = 24f;
        Vector2 explanationScrollPosition;
        //const float targetConfirmationVerticalLineX = 8f;
        private void DrawExplanation(Rect inRect)
        {
            //Widgets.DrawLineVertical(inRect.x + targetConfirmationVerticalLineX, inRect.y, inRect.height);
            Pawn pawn = RuleTarget.PickedPawn;
            if(pawn == null)
            {
                UIUtility.LabelInCenter(inRect, "RV2_Settings_Rule_RuleExplanation_NoPawnSelected".Translate());
                return;
            }

            string explanation = component.PawnExplanation(pawn);
            float explanationHeight = Text.CalcHeight(explanation, inRect.width - UIUtility.DefaultSliderWidth);
            float requiredHeight = Mathf.Max(explanationHeight, targetConfirmationSize);
            Rect outerRect = inRect;
            UIUtility.MakeAndBeginScrollView(outerRect, requiredHeight, ref explanationScrollPosition, out Listing_Standard list);
            Rect innerRect = list.GetRect(requiredHeight);
            UIUtility.SplitRectVertically(innerRect, out Rect leftRect, out Rect rightRect, targetConfirmationWidth);
            bool isPawnTargeted = AppliesTo(pawn, RuleTargetRole.All);
            Texture2D targetTexture = isPawnTargeted ? UITextures.CheckOnTexture : UITextures.CheckOffTexture;
            string confirmationTooltip = isPawnTargeted ? "RV2_Settings_Rule_RuleExplanation_DoesApply" : "RV2_Settings_Rule_RuleExplanation_DoesNotApply";
            confirmationTooltip = confirmationTooltip.Translate(pawn.LabelShortCap);
            UIUtility.TextureInCenter(leftRect, targetTexture, out Rect textureRect, targetConfirmationSize);
            TooltipHandler.TipRegion(textureRect, confirmationTooltip);
            Widgets.Label(rightRect, explanation);

            list.End();
            Widgets.EndScrollView();
        }

        const float controlButtonSize = 20f;
        private void DrawEntryControls(Rect rect, RuleTargetComponentTree tree)
        {
            if(ParentBranch == null)
            {
                // a leaf without a parent is only possible if the leaf is the root, so no removal or re-ordering is available
                return;
            }
            List<Rect> rows = UIUtility.CreateRows(rect, 3, out _, 4f);
            // UP and DOWN are somewhat misleading here, in the TREE we are moving UP, but in the visual representation we are moving DOWN
            if(ParentBranch.CanMoveDown(this))
            {
                UIUtility.TextureInCenter(rows[0], UITextures.MoveUpButtonTexture, out Rect textureRect, controlButtonSize);
                if(Widgets.ButtonInvisible(textureRect))
                    ParentBranch.MoveDown(tree, this);
            }
            if(tree.CanRemoveEntries)
            {
                UIUtility.TextureInCenter(rows[1], UITextures.CheckOffTexture, out Rect textureRect, controlButtonSize);
                if(Widgets.ButtonInvisible(textureRect))
                {
                    tree.Remove(this);
                    RuleTarget.treeRequiredHeightStale = true;
                }
            }
            if(ParentBranch.CanMoveUp(this))
            {
                UIUtility.TextureInCenter(rows[2], UITextures.MoveDownButtonTexture, out Rect textureRect, controlButtonSize);
                if(Widgets.ButtonInvisible(textureRect))
                    ParentBranch.MoveUp(tree, this);
            }
        }

        public override RuleTargetComponentNode CloneFor(RuleTargetComponentNode parent)
        {
            return new RuleTargetComponentNode_Leaf(parent)
            {
                component = (RuleTargetComponent)component.Clone()
            };
        }

        public override void DefsLoaded()
        {
            component.DefsLoaded();
        }
        public override string ToString()
        {
            return $"{base.ToString()} - leaf {ruleTargetId} parent: {ParentBranch?.SimpleToString()} - component: {component}";
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref component, "component", new object[0]);
        }

        public override IEnumerable<RuleTargetStaleTrigger> GetStaleTriggers()
        {
            yield return component.MakeStaleTrigger();
        }
    }
}
