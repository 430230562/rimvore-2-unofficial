using RimVore2.Tabs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimVore2
{
    class Tabs_PawnVore : ITab
    {
        public override bool IsVisible => RV2Mod.Settings.features.ShowVoreTab && SelPawn.IsActivePredator();

        private static readonly Vector2 MaxSize = new Vector2(500f, 450f);
        private Vector2 scrollPosition;
        private float ScrollHeight = 0;

        private List<VoreTreeDrawer> CachedTree = new List<VoreTreeDrawer>();
        private Pawn lastPawn;//Used to check if you click another pawn

        private bool IsDirty => lastPawn != SelPawn || SelPawn.PawnData()?.VoreTracker.VoreTrackerRecords.Count != CachedTree.Count;

        public Tabs_PawnVore()
        {
            size = new Vector2(MaxSize.x, 0);//Initilize as 0 as Draw will set the height
            labelKey = "RV2_PawnVoreTab";
        }
        public override void OnOpen()
        {
            base.OnOpen();
            RefreshTree();
        }
        protected override void FillTab()
        {
            //Check if looking at different pawn or if the pawn has released/vored a new pawn
            if(IsDirty)
            {
                RefreshTree();
            }
            lastPawn = SelPawn;

            VoreTabHelper.ResetGUI();
            try
            {
                //Offsetting from left a bit, giving some space up top and to the right to avoid the close X
                Rect contentRect = new Rect(5, 25, size.x - 25, size.y - 30);
                Rect InnerRect = new Rect(contentRect);
                InnerRect.height = ScrollHeight;
                InnerRect.width -= 25;
                Widgets.BeginScrollView(contentRect, ref scrollPosition, InnerRect);
                InnerRect.height = 0;

                DrawTree(ref InnerRect);

                ScrollHeight = InnerRect.height;
                Widgets.EndScrollView();
            }
            catch(Exception e)
            {
                RV2Log.Error($"Uncaught exception while drawing pawn tab for {this.SelPawn}. \n Exception: {e}");
            }
            VoreTabHelper.ResetGUI();
            size.y = Math.Min(MaxSize.y, ScrollHeight + 30);
        }
        public void RefreshTree()
        {
            //todo keep displaychildren settings/keep still valid ones
            CachedTree.Clear();
            foreach(VoreTrackerRecord record in SelPawn.PawnData().VoreTracker.VoreTrackerRecords)
            {
                VoreTreeDrawer Tree = new VoreTreeDrawer(record.Prey);
                Tree.SetAllDrawChildrenValue(true);//Have the tree defaulting have all descendents open
                CachedTree.Add(Tree);
            }
        }
        private void DrawTree(ref Rect rect)
        {
            foreach(VoreTreeDrawer drawer in CachedTree)
            {
                float NewRecordY = rect.y + rect.height;
                Rect Rect = new Rect(rect.x, NewRecordY, rect.width, 0);
                rect.height += drawer.Draw(Rect).height;
            }
        }
    }
}
