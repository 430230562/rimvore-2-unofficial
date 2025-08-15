using HarmonyLib;
using RimVore2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RV2_DBH
{
    public static class Common
    {
        public static Texture2D ToiletDumpDesignationTexture = ContentFinder<Texture2D>.Get("Widgets/toiletDumpingDesignation");
        public static Type ClosestSanitationType = AccessTools.TypeByName("ClosestSanitation");

        public static bool ValidForToiletDisposal(VoreTrackerRecord record)
        {
            return record.VoreContainer?.VoreProductContainer != null
                && !record.VoreGoal.HasModExtension<InvalidForToiletDisposalFlag>()
                && record.HasReachedEnd;
        }
    }
}
