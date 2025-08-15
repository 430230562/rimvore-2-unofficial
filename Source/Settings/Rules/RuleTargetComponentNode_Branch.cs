using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimVore2
{
    public enum TargetComponentCombination
    {
        Or,
        And
    }
    public class RuleTargetComponentNode_Branch : RuleTargetComponentNode
    {
        public TargetComponentCombination combinator;

        public override string Label
        {
            get
            {
                if(children.Count == 1)
                    return children[0].Label;
                return $"{children[0].Label} {combinator} {children[1].Label}";
            }
        }

        public RuleTargetComponentNode_Branch() : base() { }
        public RuleTargetComponentNode_Branch(RuleTargetComponentNode parent) : base(parent) { }

        public override bool AppliesTo(Pawn pawn, RuleTargetRole role)
        {
            switch(combinator)
            {
                // tree is build with "shallow" side in index 0, the leaf should always be on the "left" side and thus checked first
                // so if the leaf provides a clear condition for the combinator, it skips the deep traversal further down the "deep" side at index 1
                case TargetComponentCombination.And:
                    return children.All(child => child.AppliesTo(pawn, role));
                case TargetComponentCombination.Or:
                    return children.Any(child => child.AppliesTo(pawn, role));
                default:
                    throw new Exception("Unexpected TargetComponentCombination");
            }
        }


        public override void Draw(Listing_Standard list, RuleTargetComponentTree tree)
        {
            // this should only happen once at the bottom of the tree
            if(children.Count == 2 && children[1] is RuleTargetComponentNode_Leaf leaf)
            {
                leaf.Draw(list, tree);
                list.GapLine();
            }
            base.Draw(list, tree);
            DrawSelf(list);
            list.GapLine();
            children[0].Draw(list, tree);
            if(parent != null)
            {
                list.GapLine();
                parent.Draw(list, tree);
            }
        }

        static Func<TargetComponentCombination, string> targetComponentCombinationPresenter = (TargetComponentCombination combination) => $"RV2_TargetComponentCombination_{combination}".Translate();
        private void DrawSelf(Listing_Standard list)
        {
            Action<TargetComponentCombination> valueSetter = (TargetComponentCombination newValue) =>
            {
                combinator = newValue;
            };
            float previousWidth = list.ColumnWidth;
            list.ColumnWidth = (list.ColumnWidth - controlWidth) * targetWidthRatio + controlWidth; // a little funky, but this sets the width to the same one that the leaf uses
            list.EnumLabeled("RV2_Settings_Rule_RuleTargetComponent_Combinator".Translate(), combinator, valueSetter, targetComponentCombinationPresenter);
            list.ColumnWidth = previousWidth;
        }

        // UP and DOWN are somewhat misleading here, in the TREE we are moving UP, but in the visual representation we are moving DOWN
        public bool CanMoveUp(RuleTargetComponentNode_Leaf leaf)
        {
            if(children.Count == 1)
                return false;

            // leaf is on the last branch and on the right side, we can just swap the children to move it up
            if(children[1] == leaf)
                return true;
            // otherwise we just need to make sure that we have a parent branch
            return ParentBranch != null;
        }
        public bool CanMoveDown(RuleTargetComponentNode_Leaf leaf)
        {
            if(children.Count == 1)
                return false;

            // we can move down if the leaf is on the shallow left side
            // the leaf being on the "deep" right side would mean that we are in the last branch and thus can not move down
            return children[0] == leaf;
        }
        // UP and DOWN are somewhat misleading here, in the TREE we are moving UP, but in the visual representation we are moving DOWN
        public void MoveUp(RuleTargetComponentTree tree, RuleTargetComponentNode_Leaf leaf)
        {
            if(children[1] == leaf)
            {
                SwapChildren();
                return;
            }
            RuleTargetComponentNode_Leaf otherLeaf = ParentBranch.OtherNode(this) as RuleTargetComponentNode_Leaf;
            tree.Swap(leaf, otherLeaf);
        }
        public void MoveDown(RuleTargetComponentTree tree, RuleTargetComponentNode_Leaf leaf)
        {
            if(children[1] is RuleTargetComponentNode_Leaf)
            {
                SwapChildren();
                return;
            }
            RuleTargetComponentNode_Leaf otherLeaf = InnerBranch.children[0] as RuleTargetComponentNode_Leaf;
            tree.Swap(leaf, otherLeaf);
        }

        public override RuleTargetComponentNode CloneFor(RuleTargetComponentNode parent)
        {
            RuleTargetComponentNode_Branch newBranch = new RuleTargetComponentNode_Branch(parent)
            {
                combinator = combinator
            };
            newBranch.children = children
                .Select(child => child.CloneFor(newBranch))
                .ToList();
            return newBranch;
        }

        public override void DefsLoaded()
        {
            foreach(RuleTargetComponentNode child in children)
            {
                child.DefsLoaded();
            }
        }
        public override string ToString()
        {
            string log = $"{base.ToString()} - branch {ruleTargetId} parent: {ParentBranch?.SimpleToString()}\nchild1: {children[0]}";
            if(children.Count == 2)
                log += $"{(children.Count == 2 ? children[1].ToString() : "NONE")}";
            return log;
        }

        public override IEnumerable<RuleTargetStaleTrigger> GetStaleTriggers()
        {
            return children.SelectMany(child => child.GetStaleTriggers());
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref combinator, "combinator");
        }
    }
}
