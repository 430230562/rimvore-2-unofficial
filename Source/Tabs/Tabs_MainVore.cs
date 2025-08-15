using HarmonyLib;
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
    class Tabs_MainVore : MainTabWindow
    {
        public override Vector2 RequestedTabSize => new Vector2(1050, 500);

        private List<VoreTab> VoreTabs;
        private VoreTab CurrentTab;

        private static readonly float TabHeight = 32;
        private static readonly float ContentTopPadding = 2;
        protected virtual float TableExtraBottomSpace => 53f;


        public Tabs_MainVore() : base()
        {
            GenerateTabs();
            CurrentTab = VoreTabs[0];
            CurrentTab.OnSwitchedTo();
        }
        public override void PreOpen()
        {
            base.PreOpen();
            CurrentTab = VoreTabs[0];
            CurrentTab.OnSwitchedTo();
        }
        public override void DoWindowContents(Rect rect)
        {
            //Tabs selection
            List<TabRecord> tabList = VoreTabs
                .Select<VoreTab, TabRecord>(v => new TabRecord(v.Label,
                    () =>
                    {
                        CurrentTab = v;
                        v.OnSwitchedTo();
                    },
                    () => CurrentTab == v)
                ).ToList();
            rect.y += TabHeight;
            TabDrawer.DrawTabs(rect, tabList);
            Widgets.DrawLineHorizontal(rect.x, rect.y, rect.width);

            //Padding
            rect.y += ContentTopPadding;

            //Tab content
            Rect ContentRect = new Rect(rect.x, rect.y, rect.width, rect.height - TabHeight);
            CurrentTab.DrawContents(ContentRect);
        }

        protected void GenerateTabs()
        {
            OverviewTab overviewtab = new OverviewTab(this.Margin, this.TableExtraBottomSpace);
            InteractiveTab InteractiveTab = new InteractiveTab();

            VoreTabs = new List<VoreTab>
            {
                overviewtab,
                InteractiveTab
            };
        }

        protected bool IsTabOpen(VoreTab tab)
        {
            return CurrentTab == tab;
        }
    }

    public abstract class VoreTab
    {
        public String Label;
        public VoreTab(string Label)
        {
            this.Label = Label;
        }
        public abstract void DrawContents(Rect rect);
        public abstract void OnSwitchedTo();
    }
}
