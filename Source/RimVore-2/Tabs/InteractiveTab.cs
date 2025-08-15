using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimVore2.Tabs
{
    class InteractiveTab : VoreTab
    {
        public static IEnumerable<Pawn> AllTopMapPreds => AllPreds.Where(p => p.Map == Find.CurrentMap);
        public static IEnumerable<Pawn> AllTopPreds => AllPreds.Where(p => p.GetVoreRecord() == null);
        public static IEnumerable<Pawn> AllPreds => GlobalVoreTrackerUtility.ActivePredators.Where(p => !p.IsKidnapped());

        private Dictionary<Pawn, VoreTreeDrawer> CachedDisplayInfo = new Dictionary<Pawn, VoreTreeDrawer>();

        //Source of information for pawns
        private readonly Dictionary<string, Func<IEnumerable<Pawn>>> PawnSources = new Dictionary<string, Func<IEnumerable<Pawn>>>();
        private string PawnSource; //Current selection

        private static readonly float SourceButtonHeight = 25;
        private static readonly float SourceButtonWidth = 100;
        private static readonly float SourceButtonBottomPadding = 5;
        private static readonly float ScrollbarWidthAdjuster = 25;

        private Vector2 scrollPosition;
        private float PredUsedHeight = 0;

        //Dirty checks
        private int PreviousCount = -1;
        private bool IsDirty = false;

        public InteractiveTab() : base("RV2_Interactive_Tab".Translate())
        {
            RegisterPawnSource("RV2_Pawn_Source_All", () => AllTopPreds);
            RegisterPawnSource("RV2_Pawn_Source_Map", () => AllTopMapPreds);

            PawnSource = PawnSources.First().Key;
        }

        public void RegisterPawnSource(string key, Func<IEnumerable<Pawn>> source)
        {
            //This doesn't handle the case of trying to add 2 with the same name
            PawnSources.Add(key, source);
        }
        public override void OnSwitchedTo()
        {
            RefreshTopPredVoreInfo();
        }

        public override void DrawContents(Rect rect)
        {
            if(IsDirty || PreviousCount != GlobalVoreTrackerUtility.ActivePreyWithRecord.Count())
            {
                RefreshTopPredVoreInfo();
            }

            DrawPawnSourceButton(rect);

            DrawTree(rect);
        }

        private void DrawTree(Rect rect)
        {
            Rect scrollRect = new Rect(rect.x, rect.y + SourceButtonHeight + SourceButtonBottomPadding, rect.width, rect.height - SourceButtonHeight - SourceButtonBottomPadding);

            Rect InnerRect = new Rect(scrollRect);
            InnerRect.height = PredUsedHeight;
            InnerRect.width -= ScrollbarWidthAdjuster;
            Widgets.BeginScrollView(scrollRect, ref scrollPosition, InnerRect);
            try
            {
                try
                {
                    InnerRect.height = 0;
                    foreach(Pawn pawn in GetPawns())
                    {
                        if(!CachedDisplayInfo.ContainsKey(pawn))
                        {
                            IsDirty = true;
                            continue;
                        }
                        VoreTreeDrawer drawer = CachedDisplayInfo[pawn];
                        float NewRecordY = scrollRect.y + InnerRect.height;
                        Rect RecordRect = new Rect(scrollRect.x, NewRecordY, scrollRect.width, 0);
                        InnerRect.height += drawer.Draw(RecordRect).height;
                    }
                    PredUsedHeight = InnerRect.height;
                }
                catch(Exception e)
                {
                    Widgets.Label(InnerRect, $"Error in draw current list {e.Message}\n{e.StackTrace}");
                }

            }
            catch(Exception e)
            {
                RV2Log.Error(e.ToString());
            }
            Widgets.EndScrollView();
        }
        private void DrawPawnSourceButton(Rect rect)
        {
            Rect sourceRect = new Rect(rect.x, rect.y, SourceButtonWidth, SourceButtonHeight);
            if(Widgets.ButtonText(sourceRect, PawnSource.Translate()))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach(string key in PawnSources.Keys)
                {
                    list.Add(new FloatMenuOption(key.Translate(), delegate
                    {
                        PawnSource = key;
                        IsDirty = true;
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }
        }

        private void RefreshTopPredVoreInfo()
        {
            IsDirty = false;
            CachedDisplayInfo.Clear();
            foreach(Pawn pawn in GetPawns())
            {
                CachedDisplayInfo.Add(pawn, new VoreTreeDrawer(pawn));
            }
            PreviousCount = GlobalVoreTrackerUtility.ActivePreyWithRecord.Count();
        }

        private IEnumerable<Pawn> GetPawns()
        {
            return PawnSources[PawnSource]();
        }
    }
}
