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
    internal class OverviewTab : VoreTab
    {
        public static IEnumerable<Pawn> AllMapPrey => AllPrey.Where(p => p.GetVoreRecord()?.TopPredator?.Map == Find.CurrentMap);
        public static IEnumerable<Pawn> AllPrey => GlobalVoreTrackerUtility.ActivePreyWithRecord
            .Where(record => !record.Value.TopPredator.IsKidnapped())
            .Select(kvp => kvp.Key);

        //Source of information for pawns
        private readonly Dictionary<string, Func<IEnumerable<Pawn>>> PawnSources = new Dictionary<string, Func<IEnumerable<Pawn>>>();
        private string PawnSource; //Current selection

        private static readonly float TableTopExtraSpace = 0;
        private static readonly float SourceButtonHeight = 25;
        private static readonly float SourceButtonWidth = 100;
        private static readonly float SourceButtonBottonPadding = 5;

        internal float Margin;
        internal float TableExtraBottomSpace;

        private PawnTable PreyTable;
        private Vector2 scrollPosition;

        //Dirty information
        private List<Pawn> PrevPawns;
        private bool PrevDrawErrored = false;

        public OverviewTab(float Margin, float TableExtraBottomSpace) : base("RV2_Overview_Table".Translate())
        {
            this.Margin = Margin;
            this.TableExtraBottomSpace = TableExtraBottomSpace;
            RegisterPawnSource("RV2_Pawn_Source_All", () => AllPrey);
            RegisterPawnSource("RV2_Pawn_Source_Map", () => AllMapPrey);

            PawnSource = PawnSources.First().Key;
        }
        public void RegisterPawnSource(string key, Func<IEnumerable<Pawn>> source)
        {
            //This doesn't handle the case of trying to add 2 with the same name
            PawnSources.Add(key, source);
        }
        public override void OnSwitchedTo()
        {
            RefreshPreyTable();
        }
        public override void DrawContents(Rect rect)
        {
            try
            {
                DrawPawnSource(rect);

                if(CheckIfDirty())
                {
                    RefreshPreyTable();
                    PrevDrawErrored = false;
                }

                Rect scrollRect = new Rect(rect.x, rect.y + SourceButtonHeight + SourceButtonBottonPadding, rect.width, rect.height - (SourceButtonHeight + SourceButtonBottonPadding));
                DrawTable(scrollRect);
            }
            catch(Exception e)
            {
                GUI.Label(rect, e.Message);
                PrevDrawErrored = true;
            }
        }

        private Rect DrawTable(Rect scrollRect)
        {
            Rect InnerRect = new Rect(scrollRect);
            InnerRect.height = PreyTable.Size.y;
            InnerRect.width = PreyTable.Size.x;
            Widgets.BeginScrollView(scrollRect, ref scrollPosition, InnerRect);
            try
            {
                PreyTable.PawnTableOnGUI(new Vector2(InnerRect.x, InnerRect.y));
            }
            catch(Exception e)
            {
                Widgets.Label(InnerRect, $"{e}");
                PrevDrawErrored = true;
            }
            Widgets.EndScrollView();
            return scrollRect;
        }
        private Rect DrawPawnSource(Rect rect)
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
                        RefreshPreyTable();
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }

            return sourceRect;
        }

        private bool CheckIfDirty()
        {
            if(PrevDrawErrored)
            {
                return true;
            }
            if(PreyTable == null)
            {
                return true;
            }
            return !PrevPawns.SetsEqual(GetPawns().ToList());
        }

        private void RefreshPreyTable()
        {
            PawnTableDef Def = TabDefs.VorePreyTableDef;
            Type workerClass = Def.workerClass;
            IEnumerable<Pawn> Pawns = GetPawns();
            PreyTable = (PawnTable)Activator.CreateInstance(
                workerClass,
                Def,
                (Func<IEnumerable<Pawn>>)(() => Pawns),
                UI.screenWidth - (int)(Margin * 2f),
                (int)((float)(UI.screenHeight - 35) - TableExtraBottomSpace - TableTopExtraSpace - Margin * 2f)
            );
            PreyTable.SetDirty();
            PrevPawns = Pawns.ToList();
        }

        private IEnumerable<Pawn> GetPawns()
        {
            return PawnSources[PawnSource]();
        }
    }
}
