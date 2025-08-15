using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimVore2
{



    // TODO: when time allows, this entire tree needs a goal-focused refactor.
    // 1. only branch nodes need children
    // 2. removal and swapping could be optimized
    public abstract class RuleTargetComponentNode : IExposable, ILoadReferenceable
    {
        protected RuleTargetComponentNode parent;
        protected List<RuleTargetComponentNode> children = new List<RuleTargetComponentNode>();
        protected int ruleTargetId;
        protected const float targetWidthRatio = 0.5f;
        protected const float controlWidth = 28f;

        public RuleTargetComponentNode_Branch InnerBranch => children
            .FirstOrDefault(child => child is RuleTargetComponentNode_Branch)
            as RuleTargetComponentNode_Branch;
        public RuleTargetComponentNode_Branch ParentBranch => parent as RuleTargetComponentNode_Branch;
        public RuleTargetComponentNode OtherNode(RuleTargetComponentNode node)
        {
            return children.Find(child => child != node);
        }
        public bool CanRemoveEntries => children.Count > 1;

        protected RuleTargetComponentNode()
        {
            if(Scribe.mode == LoadSaveMode.Inactive)
                ruleTargetId = RV2Mod.Settings.SettingsUniqueIDsManager.GetNextRuleTargetID();
        }
        public RuleTargetComponentNode(RuleTargetComponentNode parent) : this()
        {
            this.parent = parent;
        }
        public RuleTargetComponentNode(RuleTargetComponentNode parent, List<RuleTargetComponentNode> children) : this()
        {
            this.parent = parent;
            this.children = children;
        }

        /// <summary>
        /// Sets parent node and adds current node to parents children
        /// </summary>
        public void SetParent(RuleTargetComponentNode parent)
        {
            this.parent = parent;
        }

        public abstract void DefsLoaded();

        public void AddChild(RuleTargetComponentNode node)
        {
            node.SetParent(this);
            children.Add(node);

            if(children.Count == 2)
            {
                // the tree is built in a way that the "right" side always contains the branches and the left side contains the leaves
                // that way we can cheaply evaluate the current "depth" level as valid or invalid later on with the AND / OR combinators
                bool leftChildIsBranch = children[0] is RuleTargetComponentNode_Branch;
                if(leftChildIsBranch)
                    SwapChildren();
            }
        }
        public void RemoveChild(RuleTargetComponentNode node)
        {
            children.Remove(node);
            //node.SetParent(null);
        }
        public void SwapChildren()
        {
            RuleTargetComponentNode firstChild = children[0];
            children.Remove(firstChild);
            children.Add(firstChild);
        }

        public abstract bool AppliesTo(Pawn pawn, RuleTargetRole role);
        public virtual void Draw(Listing_Standard list, RuleTargetComponentTree tree)
        {
            //if(list.ButtonText(ruleTargetId + " " + this.GetType()))
            //    Log.Message(this.ToString());
        }
        public abstract IEnumerable<RuleTargetStaleTrigger> GetStaleTriggers();
        public abstract string Label { get; }
        public abstract RuleTargetComponentNode CloneFor(RuleTargetComponentNode parent);
        public virtual void ExposeData()
        {
            Scribe_References.Look(ref parent, "parent");
            Scribe_Collections.Look(ref children, "children", LookMode.Deep);
            Scribe_Values.Look(ref ruleTargetId, "ruleTargetId");
        }

        public string SimpleToString()
        {
            return this.GetType() + "_" + ruleTargetId;
        }

        public string GetUniqueLoadID()
        {
            return $"RuleTargetID_{ruleTargetId}";
        }
    }
}
