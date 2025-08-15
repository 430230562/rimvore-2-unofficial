using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimVore2
{
    public class RuleTarget : IExposable, ICloneable
    {
        public string customName;
        RuleTargetComponentTree tree;
        public static Pawn PickedPawn;

        //public RuleTargetComponent ActiveTarget;
        static bool collapsed = false;

        public RuleTarget()
        {
            tree = new RuleTargetComponentTree();
        }
        public RuleTarget(RuleTargetComponentTree tree)
        {
            this.tree = tree;
        }
        public virtual string PresentationLabel => customName.NullOrEmpty() ? tree.Label : customName;

        public RuleTargetRole TargetRole => RuleTargetRole.All;

        public bool AllowsDesignation(RV2DesignationDef designation)
        {
            return tree.AppliesTo(null, designation.assignedTo);
        }

        public bool AppliesTo(Pawn pawn, RuleTargetRole role)
        {
            return tree.AppliesTo(pawn, role);
        }

        public IEnumerable<RuleTargetStaleTrigger> GetStaleTriggers()
        {
            return tree.GetStaleTriggers();
        }

        static Vector2 scrollPosition = Vector2.zero;
        static float treeRequiredHeight = -1;
        public static bool treeRequiredHeightStale = true;
        public static bool staleOnNextCycle = true;
        public void DrawInteractible(Listing_Standard list)
        {
            if(staleOnNextCycle)
            {
                treeRequiredHeightStale = true;
                staleOnNextCycle = false;
            }
            list.HeaderLabel("RV2_Settings_Rules_TargetHeader".Translate());
            customName = list.DoLabelledTextField("RV2_Settings_Rules_Name".Translate(), customName);
            Rect rect = UIUtility.CollapsibleRect(list, ref collapsed, 250f);
            if(collapsed)
            {
                return;
            }
            UIUtility.MakeAndBeginScrollView(rect, treeRequiredHeight, ref scrollPosition, out Listing_Standard innerList);

            DrawPawnPicker(innerList);
            innerList.GapLine();
            tree.Draw(innerList);
            innerList.GapLine();
            if(innerList.ButtonText("RV2_Settings_Rule_RuleTargetComponent_NewComponent".Translate()))
            {
                tree.AddNew();
                staleOnNextCycle = true;
            }
            innerList.Gap();
            DrawPawnSummary(innerList);

            innerList.EndScrollView(ref treeRequiredHeight, ref treeRequiredHeightStale);
        }

        const float pawnPickerHeightActive = 52f;
        const float pawnPickerHeightInactive = 24f;
        static TargetingParameters pickerParameters = new TargetingParameters()
        {
            canTargetBuildings = false,
            mapObjectTargetsMustBeAutoAttackable = false
        };
        private void DrawPawnPicker(Listing_Standard list)
        {
            // can't pick pawn if no map
            if(Find.CurrentMap == null)
            {
                list.Label("RV2_Settings_Rules_Target_PawnPickerNoMapActive".Translate());
                PickedPawn = null;
                return;
            }
            float pawnPickerHeight = PickedPawn == null ? pawnPickerHeightInactive : pawnPickerHeightActive;
            Rect rowRect = list.GetRect(pawnPickerHeight);
            string label = "RV2_Settings_Rules_Target_PawnPickerLabel".Translate();
            float labelWidth = Text.CalcSize(label).x;
            string pickNewLabel = UIUtility.ButtonStringPadding + "RV2_Settings_Rules_Target_PawnPickerPickNew".Translate() + UIUtility.ButtonStringPadding;
            string unselectLabel = UIUtility.ButtonStringPadding + "RV2_Settings_Rules_Target_PawnPickerUnselect".Translate() + UIUtility.ButtonStringPadding;
            float pickNewWidth = Text.CalcSize(pickNewLabel).x;
            float unselectWidth = Text.CalcSize(unselectLabel).x;
            float totalButtonWidth = pickNewWidth + unselectWidth;

            UIUtility.SplitRectVertically(rowRect, out Rect labelRect, out Rect buttonsRect, labelWidth, totalButtonWidth);
            UIUtility.SplitRectVertically(buttonsRect, out Rect pickNewRect, out Rect unselectRect, pickNewWidth, unselectWidth);
            UIUtility.LabelInCenter(labelRect, label);
            if(PickedPawn != null)
            {
                Rect portraitRect = new Rect(labelRect.xMax, labelRect.y, pawnPickerHeight, pawnPickerHeight);
                RenderTexture texture = UIUtility.PortraitTexture(PickedPawn, pawnPickerHeight);
                GUI.DrawTexture(portraitRect, texture);
                TooltipHandler.TipRegion(portraitRect, PickedPawn.Label);
                if(Widgets.ButtonText(unselectRect, unselectLabel))
                {
                    PickedPawn = null;
                    RuleCacheManager.Notify_NewPawnPicked(null);
                }
            }

            if(Widgets.ButtonText(pickNewRect, pickNewLabel))
            {
                Find.WindowStack.TryRemoveAssignableFromType(typeof(Window_RuleEditor));
                Find.WindowStack.TryRemoveAssignableFromType(typeof(Window_Settings));
                Find.WindowStack.TryRemoveAssignableFromType(typeof(Dialog_Options));
                Find.Targeter.BeginTargeting(pickerParameters, (LocalTargetInfo targetInfo) =>
                {
                    PickedPawn = targetInfo.Pawn;
                    RuleCacheManager.Notify_NewPawnPicked(PickedPawn);
                    Find.WindowStack.Add(new Window_Settings());
                    Find.WindowStack.Add(new Window_RuleEditor(Window_RuleEditor.activeIndex));
                });
            }
        }

        const float explanationIconSize = 24f;
        private void DrawPawnSummary(Listing_Standard list)
        {
            if(PickedPawn == null)
                return;
            Rect rowRect = list.GetRect(pawnPickerHeightActive);
            // shrink the width down so that the icons are closer to each other and thus a bit easier on the eyes
            rowRect.width = pawnPickerHeightActive * 4;
            UIUtility.SplitRectVertically(rowRect, out Rect portraitRect, out Rect explanationRect, pawnPickerHeightActive);
            RenderTexture texture = UIUtility.PortraitTexture(PickedPawn, pawnPickerHeightActive);
            GUI.DrawTexture(portraitRect, texture);
            TooltipHandler.TipRegion(portraitRect, PickedPawn.Label);
            UIUtility.SplitRectVertically(explanationRect, out Rect predatorExplanationRect, out Rect preyExplanationRect);
            DrawPawnExplanationForRole(predatorExplanationRect, RuleTargetRole.Predator);
            DrawPawnExplanationForRole(preyExplanationRect, RuleTargetRole.Prey);
        }

        private void DrawPawnExplanationForRole(Rect inRect, RuleTargetRole role)
        {
            Texture2D headerTexture = role == RuleTargetRole.Predator ? UITextures.PredatorIcon : UITextures.PreyIcon;
            bool appliesToPawn = this.AppliesTo(PickedPawn, role);
            Texture2D contentTexture = appliesToPawn ? UITextures.CheckOnTexture : UITextures.CheckOffTexture;
            var rects = UIUtility.CreateRows(inRect, 2, out _);
            UIUtility.TextureInCenter(rects[0], headerTexture, out _, explanationIconSize);
            UIUtility.TextureInCenter(rects[1], contentTexture, out Rect contentTextureRect, explanationIconSize);
            string tip = appliesToPawn ? "RV2_Settings_Rule_RuleExplanation_DoesApply" : "RV2_Settings_Rule_RuleExplanation_DoesNotApply";
            tip = tip.Translate(PickedPawn.LabelShortCap);
            TooltipHandler.TipRegion(contentTextureRect, tip);
        }

        public void DefsLoaded()
        {
            tree.DefsLoaded();
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref customName, "customName");
            Scribe_Deep.Look(ref tree, "tree", new object[0]);
            //Scribe_Deep.Look(ref ActiveTarget, "activeTarget");
        }

        public override string ToString()
        {
            return $"{base.ToString()} - {customName} - {tree}";
        }

        public object Clone()
        {
            return new RuleTarget()
            {
                tree = (RuleTargetComponentTree)tree.Clone(),
                //ActiveTarget = (RuleTargetComponent)this.ActiveTarget.Clone(),
                customName = this.customName
            };
        }

        // -------------------------- convenience methods to simplify rule target creation in other parts of the code --------------------------

        public static RuleTarget ForVisitorsOrTraders(RuleTargetRole role)
        {
            RuleTargetComponentNode_Branch root = new RuleTargetComponentNode_Branch(null)
            {
                combinator = TargetComponentCombination.Or
            };
            root.AddChild(new RuleTargetComponentNode_Leaf(null)
            {
                component = new RuleTargetComponent_ColonyRelation(role, RelationKind.Visitor)
            });
            root.AddChild(new RuleTargetComponentNode_Leaf(null)
            {
                component = new RuleTargetComponent_ColonyRelation(role, RelationKind.Trader)
            });
            return new RuleTarget(
                new RuleTargetComponentTree(root)
            );
        }
        public static RuleTarget ForPrisonersOrSlaves(RuleTargetRole role)
        {
            RuleTargetComponentNode root;

            RuleTargetComponentNode_Leaf prisonerLeaf = new RuleTargetComponentNode_Leaf(null)
            {
                component = new RuleTargetComponent_ColonyRelation(role, RelationKind.Prisoner)
            };
            root = new RuleTargetComponentNode_Branch(null)
            {
                combinator = TargetComponentCombination.Or
            };
            root.AddChild(prisonerLeaf);
            root.AddChild(new RuleTargetComponentNode_Leaf(null)
            {
                component = new RuleTargetComponent_ColonyRelation(role, RelationKind.Slave)
            });
            return new RuleTarget(
                new RuleTargetComponentTree(root)
            );
        }

        public static RuleTarget ForCarnivorousAnimals(RuleTargetRole role)
        {
            RuleTargetComponentNode_Branch root = new RuleTargetComponentNode_Branch(null)
            {
                combinator = TargetComponentCombination.And
            };
            root.AddChild(new RuleTargetComponentNode_Leaf(null)
            {
                component = new RuleTargetComponent_ColonyRelation(role, RelationKind.Animal)
            });
            root.AddChild(new RuleTargetComponentNode_Leaf(null)
            {
                component = new RuleTargetComponent_Diet(role, FoodTypeFlags.Meat)
            });
            return new RuleTarget(
                new RuleTargetComponentTree(root)
            );
        }
    }
}