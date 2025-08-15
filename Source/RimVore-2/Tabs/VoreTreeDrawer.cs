//#define UseDebug
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimVore2.Tabs
{
    class VoreTreeDrawer
    {
        public Pawn Pawn { get; private set; }
        public bool IsPred => Pawn.PawnData()?.VoreTracker != null && Pawn.PawnData()?.VoreTracker.DescendentCount() != 0;
        public bool IsPrey => Pawn.GetVoreRecord() != null;
        public bool AtMaxDepth => Depth >= MaxDrawDepth;
        public bool DrawPawnPortait => true;// Pawn.IsHumanoid();//Todo experiment with bigger creatures

#if UseDebug
        //Debug data
        private static int DebugIdCount = 0;
        private int DebugId = 0;
        private Rect? DebugLastUsedRect = null;
        private Rect? DebugLastRecordRect = null;
#endif

        private const float MinPortaitSize = 50;
        private const int MaxDrawDepth = 3;
        private const int IconSize = 20;
        private const float RecursionIndention = 25;


        private int Depth = 0;
        private List<VoreTreeDrawer> PreyInfo;
        private bool DisplayChildren = false;
        private float PortaitSize = MinPortaitSize;

        private RenderTexture ProfilePicture => PortraitsCache.Get(Pawn, new Vector2(PortaitSize, PortaitSize), Rot4.South);

        public VoreTreeDrawer(Pawn Pawn, int Depth = 0)
        {
            this.Pawn = Pawn;
            this.Depth = Depth;
#if UseDebug
            DebugId = DebugIdCount++;
#endif
        }

        //Expand/Contract all children
        public void SetAllDrawChildrenValue(bool DisplayChildren, int depth = 0)
        {
            this.DisplayChildren = DisplayChildren;
            if(depth >= MaxDrawDepth)
            {
                return;
            }
            this.CalculatePreyInfo();
            if(PreyInfo == null)
            {
                return;
            }
            foreach(VoreTreeDrawer drawer in PreyInfo)
            {
                drawer.SetAllDrawChildrenValue(DisplayChildren, depth + 1);
            }
        }


        public Rect Draw(Rect ContainerRect)
        {
            Rect UsedRect = new Rect(ContainerRect);
            DrawDebugAreas();

            Rect RecordRect = DrawRecord(ContainerRect);
            Rect ChildrenRect = new Rect(RecordRect.x, RecordRect.y + RecordRect.height, ContainerRect.width, 0);
            ChildrenRect = DrawChildren(ChildrenRect);

            UsedRect.height = RecordRect.height + ChildrenRect.height;
            UsedRect.width = Math.Max(ChildrenRect.width, RecordRect.width);

            if(Prefs.DevMode)
            {
#if UseDebug
                DebugLastUsedRect = UsedRect;
                DebugLastRecordRect = RecordRect;
#endif
            }

            return UsedRect;
        }

        private void CalculatePreyInfo()
        {
            if(!IsPred) return;
            if(PreyInfo == null) PreyInfo = new List<VoreTreeDrawer>();
            PreyInfo.Clear();
            foreach(VoreTrackerRecord records in Pawn.PawnData()?.VoreTracker.VoreTrackerRecords)
            {
                PreyInfo.Add(new VoreTreeDrawer(records.Prey, Depth + 1));
            }
        }
        private Rect DrawRecord(Rect container)
        {
            Rect rect = new Rect(container);
            rect.height = PortaitSize;

            float InfoStart = rect.x + PortaitSize;
            DrawPortrait();
            DrawPredExpander();
            float LabelLength = DrawLabel();
            float ButtonLength = DrawActionButtons();

            rect.width = Math.Max(LabelLength, ButtonLength);
            return rect;

            float DrawActionButtons()
            {
                if(!IsPrey) return 0;
                float IconY = rect.height - IconSize + rect.y;
                return StandardVoreButtonsHelper.DrawButtons(Pawn, InfoStart, IconY, IconSize).width;
            }
            float DrawLabel()
            {
                Rect labelRect = new Rect(InfoStart, rect.y, rect.width, 0);
                float NameWidth = 0;
                if(IsPrey)
                {
                    Vector2 TextSize = Text.CalcSize(Pawn.GetVoreRecord().DisplayLabel);
                    labelRect.height = TextSize.y;
                    NameWidth = TextSize.x;
                    GUI.Label(labelRect, Pawn.GetVoreRecord().DisplayLabel);
                }
                else
                {
                    string name = Pawn.Name?.ToStringFull;
                    if(name == null) name = Pawn.def.label;
                    Vector2 TextSize = Text.CalcSize(name);
                    labelRect.height = TextSize.y;
                    NameWidth = TextSize.x;
                    GUI.Label(labelRect, name);
                }
                return NameWidth;
            }
            void DrawPredExpander()
            {
                if(!IsPred) return;
                //Refresh children info if dirty
                if(DisplayChildren && PreyInfo.Count != Pawn.PawnData().VoreTracker.VoreTrackerRecords.Count)
                {
                    CalculatePreyInfo();
                }
                //Draw hidden count if at max depth and return
                if(AtMaxDepth)
                {
                    VoreTabHelper.DrawIcon(
                        PortaitSize - IconSize + rect.x
                        , rect.height - IconSize + rect.y
                        , IconSize
                        , UITextures
.HiddenButtonTexture
                        , () => { }
                        , "RV2_Tab_HiddenPreyCount".Translate(Pawn.PawnData().VoreTracker.DescendentCount())
                    );
                    return;
                }
                //Draw expansion button
                VoreTabHelper.DrawIcon(
                        PortaitSize - IconSize + rect.x
                        , rect.height - IconSize + rect.y
                        , IconSize
                        , DisplayChildren ? UITextures.CollapseButton : UITextures.RevealButton
                        , () =>
                        {
                            DisplayChildren = !DisplayChildren;
                            if(DisplayChildren)
                            {
                                CalculatePreyInfo();
                            }
                        }
                        , "RV2_Tab_ExpandOrCollapse".Translate()
                    );
            }
            void DrawPortrait()
            {
                if(!DrawPawnPortait) return;
                Rect PortraitRect = new Rect(rect.x, rect.y, PortaitSize, PortaitSize);
                GUI.DrawTexture(PortraitRect, ProfilePicture);
            }
        }
        private Rect DrawChildren(Rect ContainerRect)
        {
            if(!DisplayChildren || AtMaxDepth || PreyInfo == null || !PreyInfo.Any())
            {
                return new Rect(0, 0, 0, 0);
            }

            Rect ChildrenRect = new Rect(ContainerRect);
            ChildrenRect.height = 0;
            ChildrenRect.width = 0;
            foreach(VoreTreeDrawer info in PreyInfo)
            {
                float NewRecordStartY = ChildrenRect.height + ChildrenRect.y;
                float NewRecordStartX = ContainerRect.x + RecursionIndention;

                Rect childRect = new Rect(NewRecordStartX, NewRecordStartY, ContainerRect.width, 0);
                childRect = info.Draw(childRect);
                ChildrenRect.height += childRect.height;
                ChildrenRect.width = Math.Max(childRect.width, ChildrenRect.width);
            }

            return ChildrenRect;
        }
        private void DrawDebugAreas()
        {
#if UseDebug
            if(!Prefs.DevMode) return;
            if(DebugLastRecordRect == null) return;
            Color color = new Color((DebugId * 32 % 255) / 255f, ((DebugId * 32 + DebugId * 7) % 255) / 255f, ((DebugId * 32 + DebugId * 11) % 255) / 255f);
            Widgets.DrawRectFast(DebugLastUsedRect.Value, color: Color.white);
            Widgets.DrawRectFast(DebugLastUsedRect.Value.ContractedBy(1f, 1f), color);
            Widgets.DrawRectFast(DebugLastRecordRect.Value, color: Color.black);
            Widgets.DrawRectFast(DebugLastRecordRect.Value.ContractedBy(1f, 1f), color: color);
#endif
        }

    }
}
