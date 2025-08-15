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
    public class SettingsTab_Quirks : SettingsTab
    {
        public SettingsTab_Quirks(string label, Action clickedAction, bool selected) : base(label, clickedAction, selected) { }
        public SettingsTab_Quirks(string label, Action clickedAction, Func<bool> selected) : base(label, clickedAction, selected) { }

        public override SettingsContainer AssociatedContainer => RV2Mod.Settings.quirks;
        private SettingsContainer_Quirks Quirks => (SettingsContainer_Quirks)AssociatedContainer;

        private bool poolsHeightStale = true;
        private int poolsListHash = 0;
        private bool quirkHeightStale = true;
        private bool infoHeightStale = true;
        private float poolHeight = 0f;
        private float quirkHeight = 0f;
        private float infoHeight = 0f;
        private Vector2 poolsScrollPosition;
        private Vector2 quirkScrollPosition;
        private Vector2 infoScrollPosition;

        private QuirkPoolDef currentPool;
        private QuirkDef currentQuirk;
        private bool poolListActive = true; // used by hotkeys to determine which list should be moved around in

        public override void FillRect(Rect inRect)
        {
            int columnCount = 3;
            List<Rect> columns = UIUtility.CreateColumns(inRect, columnCount, out _, 0, 0, 12f);
            DoQuirkPoolList(columns[0]);
            DoQuirkList(columns[1]);
            DoQuirkInfo(columns[2]);
            HandleKeyEvents();
        }

        private void HandleKeyEvents()
        {
            int offset = 0;
            if(RV2KeyBindingDefOf.RV2_QuirkMenuPrevious.KeyDownEvent)
            {
                offset = -1;
            }
            if(RV2KeyBindingDefOf.RV2_QuirkMenuNext.KeyDownEvent)
            {
                offset = 1;
            }
            if(offset != 0)
            {
                // if user last clicked onto the pools or hasn't clicked anything and presses the hotkey, we change the pool list "cursor"
                if(poolListActive)
                {
                    List<QuirkPoolDef> sortedQuirkList = SortedQuirkPools.ToList();
                    if(currentPool == null)
                    {
                        SetCurrentPool(sortedQuirkList.First());
                        return;
                    }
                    int currentIndex = sortedQuirkList.IndexOf(currentPool);
                    currentIndex += offset;
                    if(currentIndex >= 0 && currentIndex < sortedQuirkList.Count)
                        SetCurrentPool(sortedQuirkList[currentIndex]);
                }
                // in this case the user clicked into the quirk list inside of a pool, so move that one around instead
                else
                {
                    if(currentQuirk == null)
                    {
                        currentQuirk = currentPool.quirks.First();
                        return;
                    }
                    int currentIndex = currentPool.quirks.IndexOf(currentQuirk);
                    currentIndex += offset;
                    if(currentIndex >= 0 && currentIndex < currentPool.quirks.Count)
                        SetCurrentQuirk(currentPool.quirks[currentIndex]);
                }
            }
        }

        readonly QuickSearchWidget quirkPoolFilter = new QuickSearchWidget();
        IEnumerable<QuirkPoolDef> SortedQuirkPools => RV2_Common.SortedQuirkPools
            .Where(pool => quirkPoolFilter.filter.Matches(pool.label))
            .OrderBy(pool => pool);

        private void DoQuirkPoolList(Rect inRect)
        {
            UIUtility.MakeAndBeginScrollView(inRect, poolHeight, ref poolsScrollPosition, out Listing_Standard list);

            if(list.ButtonText("RV2_Settings_Quirks_ResetQuirks".Translate()))
            {
                Quirks.Reset();
            }
            if(list.ButtonText("RV2_Settings_ResetAllRaritiesPoolList".Translate()))
            {
                Quirks.RemoveAllRarityOverrides();
            }
            List<string> enableDisableLabels = new List<string>() { "RV2_Settings_EnableAll".Translate(), "RV2_Settings_DisableAll".Translate() };
            list.ButtonRow(enableDisableLabels, out int enableDisableClicked, PairAlignment.Equal);
            if(enableDisableClicked != -1)
            {
                switch(enableDisableClicked)
                {
                    case 0:
                        Quirks.SetAllPoolsAndQuirks(true);
                        break;
                    case 1:
                        Quirks.SetAllPoolsAndQuirks(false);
                        break;
                }
            }
            list.GapLine();

            DoPoolFilterWidget(list);
            // update the scroll view that the list entries have changed and the scroll view needs to be recalculated
            int newPoolListHash = SortedQuirkPools.GetHashCode();
            if(newPoolListHash != poolsListHash)
            {
                poolsListHash = newPoolListHash;
                poolsHeightStale = true;
            }
            foreach(QuirkPoolDef pool in SortedQuirkPools)
            {
                bool poolEnabled = Quirks.IsPoolEnabledAndValid(pool);
                PrepareOnOffButton(poolEnabled, out string onOffLabel, out float onOffWidth);
                List<string> rowLabels = new List<string>() { pool.label, onOffLabel };
                // we want the on-off button to always have the same size, looks more uniform
                List<float> fixedSizes = new List<float>() { -1, onOffWidth };
                bool isCurrentlySelectedPool = pool == currentPool;
                bool customButton(Rect rect, string label) => UIUtility.ToggleButton(rect, label, poolEnabled, isCurrentlySelectedPool);
                list.ButtonRow(rowLabels, out int indexClicked, PairAlignment.Proportional, fixedSizes, null, customButton);
                if(indexClicked != -1)
                {
                    switch(indexClicked)
                    {
                        case 0:
                            SetCurrentPool(pool);
                            break;
                        case 1:
                            SetCurrentPool(pool);
                            Quirks.SetPoolAndAllQuirks(pool, !Quirks.IsPoolEnabled(pool));
                            break;
                    }
                }
            }

            list.EndScrollView(ref poolHeight, ref poolsHeightStale);
        }

        private void DoPoolFilterWidget(Listing_Standard list)
        {
            Rect rowRect = list.GetRect(Text.LineHeight);
            quirkPoolFilter.OnGUI(rowRect);
        }

        private void PrepareOnOffButton(bool enabled, out string label, out float width)
        {
            string onLabel = "RV2_Settings_On".Translate();
            string offLabel = "RV2_Settings_Off".Translate();
            width = Math.Max(Text.CalcSize(onLabel).x, Text.CalcSize(offLabel).x) + 10f;

            if(enabled)
            {
                label = onLabel;
            }
            else
            {
                label = offLabel;
            }
        }
        private void DoQuirkList(Rect inRect)
        {
            if(currentPool == null)
            {
                string displayLabel = "RV2_Settings_Quirks_PoolInfo_NoPoolSelected".Translate();
                UIUtility.LabelInCenter(inRect, displayLabel, GameFont.Medium);
                return;
            }

            Rect outerRect = inRect;
            UIUtility.MakeAndBeginScrollView(outerRect, quirkHeight, ref quirkScrollPosition, out Listing_Standard list);

            ShowQuirkList(list);

            list.EndScrollView(ref quirkHeight, ref quirkHeightStale);
        }

        Func<QuirkRarity, string> quirkRarityPresenter = (QuirkRarity rarity) => $"RV2_QuirkRarity_{rarity}".Translate();
        private void ShowQuirkList(Listing_Standard list)
        {
            list.LeftRightLabels("RV2_Settings_Quirks_PoolInfo_PoolName".Translate(), currentPool.label);

            list.LeftRightLabels("RV2_Settings_Quirks_PoolInfo_PoolType".Translate(), currentPool.poolType.ToString());
            list.LeftRightLabels("RV2_Settings_Quirks_PoolInfo_GenerationOrder".Translate(), currentPool.generationOrder.ToString());
            if(currentPool.description != null)
            {
                list.Label("RV2_Settings_Quirks_PoolInfo_Description".Translate());
                list.Label(currentPool.description);
            }
            list.GapLine();
            list.Label("RV2_Settings_Quirks_PoolInfo_Quirks".Translate());
            if(list.ButtonText("RV2_Settings_ResetAllRaritiesPool".Translate()))
            {
                Quirks.RemoveAllRarityOverrides(currentPool);
            }
            List<string> enableDisableLabels = new List<string>() { "RV2_Settings_EnableAll".Translate(), "RV2_Settings_DisableAll".Translate() };
            list.ButtonRow(enableDisableLabels, out int enableDisableClicked, PairAlignment.Equal);
            if(enableDisableClicked != -1)
            {
                switch(enableDisableClicked)
                {
                    case 0:
                        Quirks.SetPoolAndAllQuirks(currentPool, true);
                        break;
                    case 1:
                        Quirks.SetPoolAndAllQuirks(currentPool, false);
                        break;
                }
            }
            Action<QuirkRarity> overrideAllRaritiesAction = (QuirkRarity newRarity) => Quirks.OverrideAllRarities(currentPool, newRarity);
            QuirkRarity firstRarity = currentPool.quirks.First().GetRarity();
            bool poolHasUnanimousRarities = currentPool.quirks.All(q => q.GetRarity() == firstRarity);
            QuirkRarity poolRarityPresenter = poolHasUnanimousRarities ? firstRarity : QuirkRarity.Common;
            list.EnumLabeled("RV2_Settings_SetAllRaritiesPool".Translate(), poolRarityPresenter, overrideAllRaritiesAction, valuePresenter: quirkRarityPresenter, tooltip: "RV2_Settings_SetAllRaritiesPool_Tip".Translate());
            list.GapLine();
            foreach(QuirkDef quirk in currentPool.quirks)
            {
                bool quirkEnabled = Quirks.IsQuirkEnabled(quirk);
                PrepareOnOffButton(quirkEnabled, out string onOffLabel, out float onOffWidth);
                List<string> rowLabels = new List<string>() { quirk.label, onOffLabel };
                // we want the on-off button to always have the same size, looks more uniform
                List<float> fixedSizes = new List<float>() { -1, onOffWidth };
                bool isCurrentlySelectedQuirk = quirk == currentQuirk;
                bool customButton(Rect rect, string label) => UIUtility.ToggleButton(rect, label, quirkEnabled, isCurrentlySelectedQuirk);
                list.ButtonRow(rowLabels, out int indexClicked, PairAlignment.Proportional, fixedSizes, null, customButton);
                if(indexClicked != -1)
                {
                    switch(indexClicked)
                    {
                        case 0:
                            SetCurrentQuirk(quirk);
                            break;
                        case 1:
                            SetCurrentQuirk(quirk);
                            Quirks.SetQuirkEnabled(quirk, !Quirks.IsQuirkEnabled(quirk));
                            break;
                    }
                }
            }
            list.GapLine();
            DoConstraints(list, currentPool);
        }
        private void DoQuirkInfo(Rect inRect)
        {
            if(currentQuirk == null)
            {
                string displayLabel = "RV2_Settings_Quirks_QuirkInfo_NoQuirkSelected".Translate();
                UIUtility.LabelInCenter(inRect, displayLabel, GameFont.Medium);
                return;
            }

            Rect outerRect = inRect;
            UIUtility.MakeAndBeginScrollView(outerRect, infoHeight, ref infoScrollPosition, out Listing_Standard list);

            ShowQuirkInfo(list);

            list.EndScrollView(ref infoHeight, ref infoHeightStale);
        }
        private void ShowQuirkInfo(Listing_Standard list)
        {
            void rarityAction(QuirkRarity r) => Quirks.SetRarityOverride(currentQuirk, r);
            bool currentQuirkHasRarityOverride = currentQuirk.rarity != Quirks.GetRarity(currentQuirk);
            string buttonTooltip = currentQuirkHasRarityOverride ? "RV2_Settings_Quirks_PoolInfo_Rarity_TipOverriden".Translate() : "RV2_Settings_Quirks_PoolInfo_Rarity_TipDefault".Translate();
            bool customButton(Rect localRect, string localLabel) => UIUtility.ToggleButton(localRect, localLabel, !currentQuirkHasRarityOverride, false);
            float chance;
            if(!Quirks.IsQuirkEnabled(currentQuirk))
            {
                chance = 0;
            }
            else
            {
                switch(currentPool.poolType)
                {
                    case QuirkPoolType.RollForEach:
                        chance = (float)Quirks.GetRarity(currentQuirk);
                        break;
                    case QuirkPoolType.PickOne:
                        chance = (float)Math.Round(Quirks.GetChanceForWeight(currentQuirk) * 100);
                        break;
                    default:
                        chance = 0f;
                        break;
                }
            }
            QuirkRarity rarity = Quirks.GetRarity(currentQuirk);
            string rarityLabel = "RV2_Settings_Quirks_PoolInfo_Rarity".Translate();
            list.CreateLabelledDropDownForEnum<QuirkRarity>(rarityLabel, rarityAction, rarity, customButton, null, buttonTooltip);
            list.LeftRightLabels("RV2_Settings_Quirks_PoolInfo_Chance".Translate(), chance + "%");
            if(Quirks.GetRarity(currentQuirk) == QuirkRarity.Invalid)
            {
                Quirks.SetQuirkEnabled(currentQuirk, false);
            }
            list.GapLine();
            list.Label(currentQuirk.description);
            list.GapLine();
            DoConstraints(list, currentQuirk);
        }

        private void DoConstraints(Listing_Standard list, QuirkPoolDef pool)
        {
            DoConstraints(list, pool.requiredQuirks, pool.blockingQuirks, pool.requiredTraits, pool.blockingTraits, pool.requiredKeywords, pool.blockingKeywords, false);
        }

        private void DoConstraints(Listing_Standard list, QuirkDef quirk)
        {
            DoConstraints(list, quirk.requiredQuirks, quirk.blockingQuirks, quirk.requiredTraits, quirk.blockingTraits, quirk.requiredKeywords, quirk.blockingKeywords, false);
        }

        private void DoConstraints(Listing_Standard list, List<QuirkDef> requiredQuirks, List<QuirkDef> blockingQuirks, List<TraitDef> requiredTraits, List<TraitDef> blockingTraits, List<string> requiredKeywords, List<string> blockingKeywords, bool showEmpty = true)
        {
            DoConstraint(list, "RV2_Settings_Quirks_RequiredQuirks".Translate(), requiredQuirks?.ConvertAll(quirk => quirk.label), showEmpty);
            DoConstraint(list, "RV2_Settings_Quirks_BlockingQuirks".Translate(), blockingQuirks?.ConvertAll(quirk => quirk.label), showEmpty);
            DoConstraint(list, "RV2_Settings_Quirks_RequiredTraits".Translate(), requiredTraits?.ConvertAll(trait => trait.label), showEmpty);
            DoConstraint(list, "RV2_Settings_Quirks_BlockingTraits".Translate(), blockingTraits?.ConvertAll(trait => trait.label), showEmpty);
            DoConstraint(list, "RV2_Settings_Quirks_RequiredKeywords".Translate(), requiredKeywords, showEmpty);
            DoConstraint(list, "RV2_Settings_Quirks_BlockingKeywords".Translate(), blockingKeywords, showEmpty);
        }

        private void DoConstraint(Listing_Standard list, string label, List<string> entries, bool showEmpty = true)
        {
            if(!showEmpty && entries.NullOrEmpty())
            {
                return;
            }
            list.Label(label);
            list.Indent();
            if(entries.NullOrEmpty())
            {
                list.Label("RV2_Settings_Quirks_ListNone".Translate());
            }
            else
            {
                foreach(string entry in entries)
                {
                    list.Label(entry);
                }
            }
            list.Outdent();
        }

        private void SetCurrentPool(QuirkPoolDef pool)
        {
            currentPool = pool;
            quirkHeightStale = true;
            infoHeightStale = true;
            currentQuirk = null;
            poolListActive = true;
        }
        private void SetCurrentQuirk(QuirkDef quirk)
        {
            currentQuirk = quirk;
            infoHeightStale = true;
            poolListActive = false;
        }
    }
}
