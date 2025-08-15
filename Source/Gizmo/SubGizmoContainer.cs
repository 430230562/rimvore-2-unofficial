using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;

namespace RimVore2
{
    public class SubGizmoContainer : Command
    {
        private const float GizmoPadding = 1f;
        private const float IconPadding = 3f;
        private const float IconSize = 32f;
        private IconGrid iconGrid;
        private int previousIconCount = -1;
        private List<SubGizmo> allSubGizmos = new List<SubGizmo>();

        private readonly Pawn pawn;

        public void CalculateIconGrid(List<SubGizmo> subGizmos, float topLeftX = 0, float topLeftY = 0)
        {
            int iconCount = subGizmos.Count;
            // probably saves some performance, not entirely sure though
            if(previousIconCount == iconCount)
            {
                iconGrid.ResetGridPosition();
                return;
            }
            previousIconCount = iconCount;
            int rowCount = 2;
            // e.g. 7 icons means 4 icons per row for 2 rows
            int columnCount = (int)Mathf.Ceil((float)iconCount / rowCount);
            iconGrid = new IconGrid(columnCount, rowCount, topLeftX, topLeftY, IconSize, IconPadding);
        }

        private void InitIcons()
        {
            allSubGizmos.Clear();
            foreach(RV2DesignationDef designation in RV2_Common.VoreDesignations)
            {
                allSubGizmos.Add(new DesignationGizmo(pawn, designation));
            }
            allSubGizmos.Add(new QuirkButton(pawn));
            allSubGizmos.Add(new ToggleGrappleButton(pawn));
            allSubGizmos.Add(new UseGrappleButton(pawn));
        }

        private List<SubGizmo> GetVisibleIcons()
        {
            return allSubGizmos
                .Where(icon => icon.IsVisible())
                .ToList();
        }

        public override bool Visible => GetVisibleIcons().Count > 0;

        public override float GetWidth(float maxWidth)
        {
            if(iconGrid == null)
            {
                // icon grid being NULL means the gizmos topLeft has not been passed to GizmoOnGUI yet
                return 0;
            }
            float width = Mathf.Min(maxWidth, iconGrid.size.x);
            if(RV2Log.ShouldLog(true, "UI"))
                RV2Log.Message("GetWidth returns " + iconGrid.size.x + " with maxWidth " + maxWidth, true, "UI");
            return width;
        }

        public SubGizmoContainer(Pawn pawn)
        {
            this.pawn = pawn;
            defaultLabel = "SubGizmoContainer";
            defaultDesc = "SubGizmoContainer";
            InitIcons();
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            List<SubGizmo> gizmos = GetVisibleIcons();
            CalculateIconGrid(gizmos);
            // topLeft needs to consider the padding to the outermost box
            topLeft += new Vector2(GizmoPadding, GizmoPadding);
            iconGrid.SetStartTopLeft(topLeft);
            Rect innerGizmoRect = new Rect(topLeft, iconGrid.size);
            Rect outerGizmoRect = innerGizmoRect.ExpandedBy(GizmoPadding);
            //gizmoRect = gizmoRect.ContractedBy(GizmoPadding);
            //iconGrid.startX = innerGizmoRect.x;
            //iconGrid.startY = innerGizmoRect.y;
            Widgets.DrawWindowBackground(outerGizmoRect);
            for(int i = 0; i < gizmos.Count; i++)
            {
                SubGizmo icon = gizmos[i];
                Rect iconRect = iconGrid.GetCurrentGridRect(true);
                if(icon.Draw(iconRect))
                {
                    //lastClicked = icon;
                    return new GizmoResult(GizmoState.Interacted, Event.current);
                }
            }
            return new GizmoResult(GizmoState.Clear);
        }
    }
}