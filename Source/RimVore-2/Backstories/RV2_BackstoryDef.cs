using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
#if v1_3
using AlienRace;
#endif
using HarmonyLib;

namespace RimVore2
{
    public class RV2_BackstoryDef : BackstoryDef
    {
        public List<QuirkPicker> forcedQuirkPickers;
        public List<QuirkPicker> blockedQuirkPickers;
        public float commonality = 1;
        public List<HediffDef> forcedHediffs = new List<HediffDef>();

        public List<QuirkDef> ForcedQuirks => DetermineForcedQuirks();
        public List<QuirkDef> BlockedQuirks => DetermineBlockedQuirks();

        public List<SexualPart> forcedSexualParts;

        //public List<string> blockingKeywords;
        //public List<string> requiredKeywords;

        public List<QuirkDef> DetermineForcedQuirks()
        {
            return DetermineQuirks(forcedQuirkPickers);
        }
        public List<QuirkDef> DetermineBlockedQuirks()
        {
            return DetermineQuirks(blockedQuirkPickers);
        }
        private List<QuirkDef> DetermineQuirks(List<QuirkPicker> pickers)
        {
            if(pickers.NullOrEmpty())
            {
                return new List<QuirkDef>();
            }
            return pickers.SelectMany(picker => picker.GetQuirks()).ToList();
        }

        public void UpdateDescription(bool? nullableScatEnabled, bool? nullableBonesEnabled)
        {
            bool scatEnabled = nullableScatEnabled ?? RV2Mod.Settings.features.ScatEnabled;
            bool bonesEnabled = nullableBonesEnabled ?? RV2Mod.Settings.features.BonesEnabled;

            ScatOrBonesBackstoryDescription descriptionExtension = this.GetModExtension<ScatOrBonesBackstoryDescription>();
            if(descriptionExtension == null)
            {
#if v1_3
                // UpdateTranslateableFields will set the Backstory.baseDesc back to the Def provided base description
                // call UpdateTranslateableFields via reflection due to Internal tag
                AccessTools.Method(typeof(BackstoryDef), "UpdateTranslateableFields")?.Invoke(null, new object[] { this });
#endif
                return;
            }
            string newBaseDesc;
            if(scatEnabled && bonesEnabled)
            {
                newBaseDesc = descriptionExtension.descriptionForScatAndBones;
            }
            else if(scatEnabled)
            {
                newBaseDesc = descriptionExtension.descriptionForScat;
            }
            else if(bonesEnabled)
            {
                newBaseDesc = descriptionExtension.descriptionForBones;
            }
            else
            {
                // user has neither bones nor scat enabled
                return;
            }
            if(RV2Log.ShouldLog(false, "Backstories"))
                RV2Log.Message($"Updating description for {base.defName}", "Backstories");
#if v1_3
            base.backstory.baseDesc = newBaseDesc;
#else
            base.baseDesc = newBaseDesc;
#endif
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if(forcedQuirkPickers != null)
            {
                if(forcedQuirkPickers.Count == 0)
                {
                    yield return "list \"forcedQuirkPickers\" is provided, but empty";
                }
                foreach(QuirkPicker picker in forcedQuirkPickers)
                {
                    foreach(string error in picker.ConfigErrors())
                    {
                        yield return error;
                    }
                }
            }
            if(blockedQuirkPickers != null)
            {
                if(blockedQuirkPickers.Count == 0)
                {
                    yield return "list \"blockedQuirkPickers\" is provided, but empty";
                }
                foreach(QuirkPicker picker in blockedQuirkPickers)
                {
                    foreach(string error in picker.ConfigErrors())
                    {
                        yield return error;
                    }
                }
            }
        }
        public void ApplyForcedGenitals(Pawn pawn)
        {
            if(forcedSexualParts.NullOrEmpty())
            {
                return;
            }
            foreach(SexualPart sexPart in forcedSexualParts)
            {
                ModAdapter.Genitals.AddSexualPart(pawn, sexPart);
            }
        }
    }

}
