using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    /// <notes>
    /// The tree is built with the leaf nodes on the "left" side, which should make traversal more performant because the right deeper side is only traversed if the combinator passes for the shallow side
    /// 
    /// </notes>
    public class RuleTargetComponentTree : ICloneable, IExposable
    {
        RuleTargetComponentNode root;

        public RuleTargetComponentTree()
        { 
            if(Scribe.mode == LoadSaveMode.Inactive)
            {
                InitRoot();
            }
        }
        public RuleTargetComponentTree(RuleTargetComponentNode root)
        {
            this.root = root;
        }

        private void InitRoot()
        {
            if(root == null)
            {
                root = new RuleTargetComponentNode_Leaf(null)
                {
                    component = new RuleTargetComponent_Everyone(RuleTargetRole.All)
                };
            }
        }

        public IEnumerable<RuleTargetStaleTrigger> GetStaleTriggers()
        {
            return root.GetStaleTriggers();
        }

        public bool AppliesTo(Pawn pawn, RuleTargetRole role)
        {
            return root.AppliesTo(pawn, role);
        }

        public bool CanRemoveEntries => root.CanRemoveEntries;
        public string Label => root.Label;

        public void Draw(Listing_Standard list)
        {
            RuleTargetComponentNode currentBranch = root;
            int loopPrevention = 100;
            while(currentBranch.InnerBranch != null && --loopPrevention > 0)
                currentBranch = currentBranch.InnerBranch;
            if(loopPrevention == 0)
            {
                Log.Error("Hit endless loop on tree traversal for drawing!");
                return;
            }
            currentBranch.Draw(list, this);
        }

        /// <summary>
        /// Create a new branch and leaf, add the leaf to the branch, take the root and add it as the second child of the new branch, finally set new branch as new root
        /// </summary>
        /// <returns>Newly created component</returns>
        public void AddNew()
        {
            RuleTargetComponentNode_Branch newBranch = new RuleTargetComponentNode_Branch(null)
            {
                combinator = TargetComponentCombination.Or
            };
            RuleTargetComponentNode_Leaf newLeaf = new RuleTargetComponentNode_Leaf(newBranch)
            {
                component = new RuleTargetComponent_Everyone(RuleTargetRole.All)
            };
            newBranch.AddChild(newLeaf);
            newBranch.AddChild(root);
            root = newBranch;
        }

        public void Swap(RuleTargetComponentNode_Leaf leaf, RuleTargetComponentNode_Leaf otherLeaf)
        {
            //Log.Message($"Swapping {leaf} \n^\n |\nv\n {otherLeaf}");
            leaf.ParentBranch.RemoveChild(leaf);
            otherLeaf.ParentBranch.RemoveChild(otherLeaf);

            RuleTargetComponentNode_Branch cachedBranch = leaf.ParentBranch;
            otherLeaf.ParentBranch.AddChild(leaf);
            cachedBranch.AddChild(otherLeaf);
        }

        public void Remove(RuleTargetComponentNode_Leaf leaf)
        {
            if(root == leaf)
            {
                Log.Warning("Tried to remove root leaf!");
                return;
            }
            RuleTargetComponentNode_Branch branch = leaf.ParentBranch;
            if(branch.ParentBranch == null)
            {
                // we are looking at the root branch
                // set the opposite node (leaf or branch, doesn't matter) as new root
                root = branch.OtherNode(leaf);
                // and then unlink the new root from the old root
                root.SetParent(null);
                return;
            }
            // otherwise normal handling, take the branch of the leaf and cut it out of the tree
            // we do this by replacing "this" branch in the parent branch with the inner branch
            RuleTargetComponentNode_Branch upperBranch = branch.ParentBranch;
            upperBranch.RemoveChild(branch);
            upperBranch.AddChild(branch.OtherNode(leaf));
        }

        public object Clone()
        {
            return new RuleTargetComponentTree()
            {
                root = root.CloneFor(null)
            };
        }

        public void DefsLoaded()
        {
            root.DefsLoaded();
        }

        public override string ToString()
        {
            return $"{base.ToString()} - root: \n{root}";
        }

        public void ExposeData()
        {
            Scribe_Deep.Look(ref root, "root", new object[0]);

            if(Scribe.mode == LoadSaveMode.LoadingVars)
            {
                InitRoot();
            }
        }
    }
}
