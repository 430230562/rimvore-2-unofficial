using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimVore2
{
    public class Window_Quirks : Window
    {
        private QuirkManager quirkManager;
        static private Vector2 quirkScrollPosition;
        private Pawn pawn;
        private readonly List<string> pawnKeywords;
        private readonly List<TraitDef> pawnTraits;

        private Vector2 minSize;

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(1000f, UI.screenHeight * 0.8f);
            }
        }

        Dictionary<string, List<QuirkPoolDef>> categorizedQuirkPools = new Dictionary<string, List<QuirkPoolDef>>();
        Dictionary<string, List<QuirkPoolDef>> CategorizedQuirkPools
        {
            get
            {
                if(categorizedQuirkPools.Count == 0)
                {
                    IEnumerable<QuirkPoolDef> sortedQuirkPools = RV2_Common.SortedQuirkPools
                        .Where(pool => poolFilter.filter.Matches(pool.label))    // only display pools matching pool filter
                        .Where(pool => pool.quirks  // check if the pool contains quirks that can be displayed
                            .Any(quirk => (
                                    quirkFilter.filter.Matches(quirk.label) // either the label matches the filter
                                    || quirkFilter.filter.Matches(quirk.GetRarity().ToString()) // or the rarity matches the filter
                                ) && (
                                    !quirk.hidden && quirkManager.HasQuirk(quirk)  // and the pawn has the visible quirk
                                    || Prefs.DevMode   // or we are in dev mode
                                )
                            )
                        );
                    //Log.Message("pools: " + String.Join(", ", sortedQuirkPools.Select(p => p.defName)));
                    foreach(QuirkPoolDef pool in sortedQuirkPools)
                    {
                        string categoryName = "";
                        if(pool.category != null)
                        {
                            categoryName = pool.category;
                        }
                        // if the category is already a tracked list, add our pool to the existing list
                        if(categorizedQuirkPools.ContainsKey(categoryName))
                        {
                            categorizedQuirkPools[categoryName].Add(pool);
                        }
                        // otherwise create the tracked list
                        else
                        {
                            categorizedQuirkPools[categoryName] = new List<QuirkPoolDef>() { pool };
                        }
                    }
                }
                return categorizedQuirkPools;
            }
        }

        public Window_Quirks(Pawn pawn)
        {
            this.pawn = pawn;
            base.soundClose = SoundDefOf.InfoCard_Close;
            base.preventCameraMotion = false;
            base.doCloseX = true;
            base.draggable = true;
            base.onlyOneOfTypeAllowed = true;
            base.closeOnClickedOutside = true;
            base.resizeable = true;

            quirkManager = pawn.QuirkManager();
            if(quirkManager == null)
            {
                throw new Exception("Trying to create quirk window for pawn without quirkmanager!");
            }
            // close existing instances of the window
            foreach(Window window in Find.WindowStack.Windows.Where(window => PawnWindowAlreadyOpen(window)).ToList())
            {
                window.Close(false);
            }
            pawnKeywords = pawn.PawnKeywords(true);
            pawnTraits = pawn.story?.traits?.allTraits?.ConvertAll(trait => trait.def);
        }

        public override void Close(bool doCloseSound = true)
        {
            base.Close();
            VoreInteractionManager.Reset(pawn);
            RV2Mod.Settings.rules.RecacheDesignationsFor(pawn);
        }

        private bool PawnWindowAlreadyOpen(Window window)
        {
            if(window is Window_Quirks quirkWindow)
            {
                // don't remove yourself
                if(window == this)
                {
                    return false;
                }
                // but remove any windows that already have this pawn in them
                return quirkWindow.pawn == pawn;
            }
            return false;
        }

        private float quirkSheetHeight = float.MaxValue;
        private bool quirkSheetHeightStale = true;
        private float headerHeight = 100;
        private bool headerHeightStale = true;
        public override void DoWindowContents(Rect inRect)
        {
            if(minSize == default)
            {
                minSize = new Vector2(inRect.width, inRect.height);
            }
            Listing_Standard list = new Listing_Standard()
            {
                ColumnWidth = inRect.width,
                maxOneColumn = true
            };
            list.Begin(inRect);
            Rect headerRect = list.GetRect(headerHeight);
            List<Rect> columns = UIUtility.CreateColumns(headerRect, 2, out float columnWidth, 0, 0);
            //Log.Message(string.Join(", ", columns.Select(c => c.ToString())));
            ShowLeftHeader(columns[0], out float leftHeight);
            float rightHeight = 0;
            ShowRightHeader(columns[1], out rightHeight);

            if(headerHeightStale)
            {
                headerHeight = Math.Max(leftHeight, rightHeight) + 20;  // without the 20 base game creates a second column which causes the quirk sheet to be drawn offset and thus invisible to see
                headerHeightStale = false;
            }
            Rect quirkRect = list.GetRect(inRect.height - headerHeight);//new Rect(inRect.x, inRect.y + headerHeight, inRect.width, inRect.height - headerHeight);
            try
            {
                DrawQuirkSheet(quirkRect);
            }
            catch(Exception e)
            {
                string message = $"Caught exception:\n\n{e}";
                RV2Log.Error(message, true, "UI");
            }
            list.End();
        }

        private void ShowLeftHeader(Rect inRect, out float requiredHeight)
        {
            //Widgets.DrawRectFast(inRect, Color.blue);
            Listing_Standard list = new Listing_Standard()
            {
                ColumnWidth = inRect.width,
                maxOneColumn = true
            };
            list.Begin(inRect);

            ShowHeaderLabel(list);
            if(Prefs.DevMode)
            {
                ShowRerollButton(list);
            }

            requiredHeight = list.CurHeight;
            list.End();
        }

        public QuickSearchWidget poolFilter = new QuickSearchWidget();
        public QuickSearchWidget quirkFilter = new QuickSearchWidget();
        private void ShowRightHeader(Rect inRect, out float requiredHeight)
        {
            //Widgets.DrawRectFast(inRect, Color.red);
            Listing_Standard list = new Listing_Standard()
            {
                ColumnWidth = inRect.width,
                maxOneColumn = true
            };
            list.Begin(inRect);

            Action onFilterChange = () =>
            {
                CategorizedQuirkPools.Clear();
                quirkSheetHeightStale = true;
                quirkSheetHeight = float.MaxValue;   // the height calculates itself by comparing to the previous maximum, so we need to manually reset it
            };

            Rect poolFilterRect = list.GetRect(Text.LineHeight);
            UIUtility.SplitRectVertically(poolFilterRect, out Rect poolFilterLabelRect, out Rect poolFilterWidgetRect);
            Widgets.Label(poolFilterLabelRect, "RV2_Settings_Quirks_QuirkPoolFilter".Translate());
            poolFilter.OnGUI(poolFilterWidgetRect, onFilterChange);

            Rect quirkFilterRect = list.GetRect(Text.LineHeight);
            UIUtility.SplitRectVertically(quirkFilterRect, out Rect quirkFilterLabelRect, out Rect quirkFilterWidgetRect);
            Widgets.Label(quirkFilterLabelRect, "RV2_Settings_Quirks_QuirkFilter".Translate());
            quirkFilter.OnGUI(quirkFilterWidgetRect, onFilterChange);

            if(Prefs.DevMode)
            {
                Rect presetRect = list.GetRect(Text.LineHeight);
                List<string> presetButtonLabels = new List<string>()
                {
                    "RV2_QuirkWindow_SavePreset".Translate()
                };
                if(RV2Mod.Settings.quirks.QuirkPresets.Any())
                {
                    presetButtonLabels.Add("RV2_QuirkWindow_LoadPreset".Translate());
                    presetButtonLabels.Add("RV2_QuirkWindow_DeletePreset".Translate());
                }
                list.ButtonRow(presetButtonLabels, out int indexClicked, PairAlignment.Equal);
                switch(indexClicked)
                {
                    case 0:
                        SaveQuirkPreset();
                        break;
                    case 1:
                        LoadQuirkPreset();
                        break;
                    case 2:
                        DeleteQuirkPreset();
                        break;
                }
            }

            requiredHeight = list.CurHeight;
            list.End();
        }

        private void SaveQuirkPreset()
        {
            Action<string> save = delegate (string content)
            {
                if(string.IsNullOrEmpty(content))
                {
                    return;
                }
                if(RV2Mod.Settings.quirks.QuirkPresets.ContainsKey(content))
                {
                    return;
                }
                RV2Mod.Settings.quirks.QuirkPresets.Add(content, MakePreset());
                RV2Mod.Settings.Write();
            };
            Action cancel = () => { };
            Window_CustomTextField textFieldWindow = new Window_CustomTextField("", save, cancel);
            Find.WindowStack.Add(textFieldWindow);
        }
        private QuirkPreset MakePreset()
        {
            List<QuirkDef> activeQuirkDefs = quirkManager.ActiveQuirks.Select(q => q.def).ToList();
            return new QuirkPreset(activeQuirkDefs);
        }

        private void LoadQuirkPreset()
        {
            Action<string> load = delegate (string presetName)
            {
                quirkManager.ApplyPreset(RV2Mod.Settings.quirks.QuirkPresets[presetName]);
            };
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            foreach(string presetName in RV2Mod.Settings.quirks.QuirkPresets.Keys.ToList())
            {
                options.Add(new FloatMenuOption(presetName, () => load(presetName)));
            }
            Find.WindowStack.Add(new FloatMenu(options));
        }
        private void DeleteQuirkPreset()
        {
            Action<string> delete = delegate (string presetName)
            {
                RV2Mod.Settings.quirks.QuirkPresets.Remove(presetName);
            };
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            foreach(string presetName in RV2Mod.Settings.quirks.QuirkPresets.Keys.ToList())
            {
                options.Add(new FloatMenuOption(presetName, () => delete(presetName)));
            }
            Find.WindowStack.Add(new FloatMenu(options));
        }

        private void ShowHeaderLabel(Listing_Standard list)
        {
            Text.Font = GameFont.Medium;
            list.Label("RV2_QuirkWindow_Header".Translate() + " - " + pawn.Label);
            Text.Font = GameFont.Small;
            list.Gap();
        }

        private void ShowRerollButton(Listing_Standard list)
        {
            FontStyle originalFontStyle = Text.CurFontStyle.fontStyle;
            int originalFontSize = Text.CurFontStyle.fontSize;
            Text.CurFontStyle.fontStyle = FontStyle.Bold;
            Text.CurFontStyle.fontSize = 24;
            list.ButtonImageWithLabel("RV2_QuirkWindow_RerollButtonLabel".Translate(), UITextures.ResetButtonTexture, out bool rerollClicked, null, null, 24);
            Text.CurFontStyle.fontStyle = originalFontStyle;
            Text.CurFontStyle.fontSize = originalFontSize;
            if(rerollClicked)
            {
                SetStale();
                quirkManager.RerollAll();
            }
        }

        private void DrawQuirkSheet(Rect inRect)
        {
            float columnWidth = 300f;
            int columnCount = CategorizedQuirkPools.Count();
            float requiredSheetWidth = columnCount * columnWidth;
            Rect quirkSheet = new Rect(inRect.x, inRect.y, requiredSheetWidth, quirkSheetHeight);
            Widgets.BeginScrollView(inRect, ref quirkScrollPosition, quirkSheet);
            if(quirkSheetHeightStale && quirkSheetHeight == float.MaxValue)
            {
                quirkSheetHeight = 0;
            }
            List<Rect> columns = UIUtility.CreateColumns(quirkSheet, columnCount, out _, 0, 0, 24);

            int i = 0;
            Dictionary<string, List<QuirkPoolDef>> HardCopyCategorizedQuirkPools = new Dictionary<string, List<QuirkPoolDef>>(CategorizedQuirkPools);   // directly using the CategorizedQuirkPools leads to collection modified exception

            foreach(KeyValuePair<string, List<QuirkPoolDef>> categorizedPool in HardCopyCategorizedQuirkPools)
            {
                Rect column = columns[i++];
                //Color color = GUI.color;
                //GUI.color = color * new Color(1f, 1f, 1f, 0.4f);
                //Widgets.DrawLineVertical(column.xMax, column.yMin, quirkSheetHeight);
                //GUI.color = color;
                Listing_Standard list = new Listing_Standard()
                {
                    ColumnWidth = column.width,
                    maxOneColumn = true
                };
                list.Begin(column);
                if(categorizedPool.Key != "")
                {
                    UIUtility.HeaderLabel(list, categorizedPool.Key);
                }
                else
                {
                    UIUtility.HeaderLabel(list, "RV2_Category_None".Translate());
                }
                float requiredColumnHeight = DrawQuirkColumn(list, categorizedPool.Value);
                if(quirkSheetHeightStale)
                {
                    quirkSheetHeight = Mathf.Max(quirkSheetHeight, requiredColumnHeight);
                }
                list.End();
            }
            quirkSheetHeightStale = false;
            Widgets.EndScrollView();
        }

        private void SetStale()
        {
            cachedQuirkInvalid.Clear();
            categorizedQuirkPools.Clear();
            cachedQuirkDescriptions.Clear();
            cachedQuirkPoolButtonTextures.Clear();
            cachedQuirkPoolButtonTooltips.Clear();
            cachedQuirkButtonTextures.Clear();
            cachedQuirkButtonTooltips.Clear();
        }

        private Dictionary<Quirk, string> cachedQuirkInvalid = new Dictionary<Quirk, string>();
        private bool IsQuirkValid(Quirk quirk, out string reason)
        {
            if(!cachedQuirkInvalid.ContainsKey(quirk))
            {
                quirk.def.IsValid(pawn, out string innerReason, pawnTraits, quirkManager.ActiveQuirks.ConvertAll(q => q.def), pawnKeywords);
                cachedQuirkInvalid.Add(quirk, innerReason);
            }

            reason = cachedQuirkInvalid[quirk];
            return reason == null;
        }

        private Dictionary<Quirk, string> cachedQuirkDescriptions = new Dictionary<Quirk, string>();
        private string GetQuirkDescription(Quirk quirk, Color color)
        {
            if(!cachedQuirkDescriptions.ContainsKey(quirk))
            {
                string description = quirk.def.description.AdjustedFor(pawn) + "\n\nRarity: " + quirk.def.GetRarity().ToString().Colorize(color);
                if(!IsQuirkValid(quirk, out string reason))
                {
                    description = "WARNING\nThis quirk should not have been applied!\nReason: " + reason + "\n\n" + description;
                }
                if(Prefs.DevMode)
                {
                    try
                    {
                        string additionalDescription = quirk.def.AppendDebugInformation(pawn, pawnKeywords);
                        description += additionalDescription;
                    }
                    catch(Exception e)
                    {
                        Log.Error($"Exception trying to determine debug data: {e}");
                    }
                }
                cachedQuirkDescriptions.Add(quirk, description);
            }
            return cachedQuirkDescriptions[quirk];
        }

        /// <summary>
        /// Draw a given list of quirks into the Listing
        /// </summary>
        /// <param name="list">List to use for column listing</param>
        /// <param name="pools">Pools to draw in this column</param>
        /// <returns>The required height needed to draw all elements in the list</returns>
        private float DrawQuirkColumn(Listing_Standard list, List<QuirkPoolDef> pools)
        {
            SortedDictionary<QuirkPoolDef, List<Quirk>> groupedQuirks = quirkManager.GroupedQuirks; // pull cached dictionary
            foreach(QuirkPoolDef pool in pools)
            {
                IEnumerable<Quirk> quirks = groupedQuirks.TryGetValue(pool, new List<Quirk>())
                    .Where(quirk => quirkFilter.filter.Matches(quirk.def.label) || quirkFilter.filter.Matches(quirk.def.GetRarity().ToString()));
                bool isHidden = pool.hidden || quirks.All(quirk => quirk.def.hidden) || quirks.EnumerableNullOrEmpty();
                if(isHidden)
                {
                    if(!Prefs.DevMode)
                    {
                        continue;
                    }
                }
                if(Prefs.DevMode)
                {
                    string poolDescription = pool.description;
                    DoQuirkPoolEntry(list, pool, isHidden, poolDescription, out bool resetClicked);
                    //list.MakeQuirkPoolRow(poolLabel, pool.description, out bool resetClicked);
                    if(resetClicked)
                    {
                        SetStale();
                        quirkManager.RerollPersistentQuirkPool(pool);
                    }
                }
                else
                {
                    list.Label(pool.label, -1, pool.description);
                }
                //list.Label(poolPrefix + pool.label, -1, pool.description);
                list.Indent(24f);
                foreach(Quirk quirk in quirks)
                {
                    // the user might have manually added a quirk that is not valid, this check will yell at them to resolve the collisions
                    string quirkPrefix = "";
                    if(quirk.def.hidden)
                    {
                        if(!Prefs.DevMode)
                        {
                            continue;
                        }
                    }
                    string label = quirkPrefix + quirk.def.label;
                    Color quirkColor = RV2_Common.QuirkRarityColors.TryGetValue(quirk.def.GetRarity(), Color.white);
                    label = label.Colorize(quirkColor);

                    string description = GetQuirkDescription(quirk, quirkColor);
                    Texture2D quirkButtonTexture;
                    if(pool.poolType == QuirkPoolType.PickOne)
                    {
                        quirkButtonTexture = UITextures.ResetButtonTexture;
                    }
                    else
                    {
                        quirkButtonTexture = UITextures.RemoveButtonTexture;
                    }
                    DoQuirkEntry(list, quirk, label, out bool clickedButton, description);
                    //list.ButtonImageWithLabel(label, quirkButtonTexture, out bool quirkButtonClicked, description);
                    if(clickedButton)    // the first button is always the cycle / remove button
                    {
                        SetStale();
                        if(pool.poolType == QuirkPoolType.PickOne)
                        {
                            quirkManager.PickQuirkDialogue(pool);
                        }
                        else
                        {
                            quirkManager.RemovePersistentQuirk(quirk);
                        }
                    }
                }
                //list.Label(label, -1, description);
                bool showAddQuirkButton = Prefs.DevMode    // only show add button if in debug
                    && pool.poolType != QuirkPoolType.PickOne   // and the pool type only allows a single quirk
                    && !quirkManager.HasAllQuirksInPersistentPool(pool);  // and the pawn doesn't already have all quirks the pool has to offer
                if(showAddQuirkButton)
                {
                    Rect buttonRect = list.GetRect(UIUtility.ImageButtonWithLabelSize);
                    buttonRect.width = UIUtility.ImageButtonWithLabelSize;
                    if(Widgets.ButtonImage(buttonRect, UITextures.AddButtonTexture, Color.white, Color.blue))
                    {
                        SetStale();
                        quirkManager.AddQuirkDialogue(pool);
                    }
                }
                list.Outdent(24f);
                list.Gap();
                // return the height used by this column
                /*if(pool != lastPool)
                {
                    list.GapLine();
                }*/
            }
            return list.CurHeight;
        }

        private Dictionary<QuirkPoolDef, List<Texture2D>> cachedQuirkPoolButtonTextures = new Dictionary<QuirkPoolDef, List<Texture2D>>();
        private Dictionary<QuirkPoolDef, List<string>> cachedQuirkPoolButtonTooltips = new Dictionary<QuirkPoolDef, List<string>>();
        private void GetQuirkPoolEntryDecorations(QuirkPoolDef pool, bool isHidden, out List<Texture2D> textures, out List<string> tooltips)
        {
            if(!cachedQuirkPoolButtonTextures.ContainsKey(pool))
            {
                List<Texture2D> buttonTextures = new List<Texture2D>();
                List<string> buttonTooltips = new List<string>();
                buttonTextures.Add(UITextures.ResetButtonTexture);
                buttonTooltips.Add("RV2_QuirkWindow_RerollPool".Translate());
                CircumstancialButtons(false, isHidden, null, out List<Texture2D> circumstancialButtonTextures, out List<string> circumstancialButtonTooltips);
                buttonTextures.AddRange(circumstancialButtonTextures);
                buttonTooltips.AddRange(circumstancialButtonTooltips);
                cachedQuirkPoolButtonTextures.Add(pool, buttonTextures);
                cachedQuirkPoolButtonTooltips.Add(pool, buttonTooltips);
            }
            textures = cachedQuirkPoolButtonTextures[pool];
            tooltips = cachedQuirkPoolButtonTooltips[pool];
        }

        private void DoQuirkPoolEntry(Listing_Standard list, QuirkPoolDef pool, bool isHidden, string description, out bool buttonClicked)
        {
            GetQuirkPoolEntryDecorations(pool, isHidden, out List<Texture2D> buttonTextures, out List<string> buttonTooltips);
            List<bool> buttonsClicked;
            list.ButtonImagesWithLabel(pool.label, buttonTextures, out buttonsClicked, description, buttonTooltips);
            buttonClicked = buttonsClicked.IndexOf(true) == 0;
        }

        private Dictionary<Quirk, List<Texture2D>> cachedQuirkButtonTextures = new Dictionary<Quirk, List<Texture2D>>();
        private Dictionary<Quirk, List<string>> cachedQuirkButtonTooltips = new Dictionary<Quirk, List<string>>();
        private void GetQuirkEntryDecorations(Quirk quirk, out List<Texture2D> textures, out List<string> tooltips)
        {
            if(!cachedQuirkButtonTextures.ContainsKey(quirk))
            {
                List<Texture2D> buttonTextures = new List<Texture2D>();
                List<string> buttonTooltips = new List<string>();

                if(Prefs.DevMode)
                {
                    if(quirk.Pool.poolType == QuirkPoolType.PickOne)
                    {
                        buttonTextures.Add(UITextures.ResetButtonTexture);
                        buttonTooltips.Add("RV2_QuirkWindow_CycleQuirk".Translate());
                    }
                    else
                    {
                        buttonTextures.Add(UITextures.RemoveButtonTexture);
                        buttonTooltips.Add("RV2_QuirkWindow_RemoveQuirk".Translate());
                    }
                }
                bool hasCollision = !IsQuirkValid(quirk, out _);
                CircumstancialButtons(hasCollision, false, quirk, out List<Texture2D> circumstancialButtonTextures, out List<string> circumstancialButtonTooltips);
                buttonTextures.AddRange(circumstancialButtonTextures);
                buttonTooltips.AddRange(circumstancialButtonTooltips);

                cachedQuirkButtonTextures.Add(quirk, buttonTextures);
                cachedQuirkButtonTooltips.Add(quirk, buttonTooltips);
            }
            textures = cachedQuirkButtonTextures[quirk];
            tooltips = cachedQuirkButtonTooltips[quirk];
        }

        private void DoQuirkEntry(Listing_Standard list, Quirk quirk, string label, out bool buttonClicked, string description)
        {
            GetQuirkEntryDecorations(quirk, out List<Texture2D> buttonTextures, out List<string> buttonTooltips);
            List<bool> buttonsClicked;
            list.ButtonImagesWithLabel(label, buttonTextures, out buttonsClicked, description, buttonTooltips);
            buttonClicked = buttonsClicked.IndexOf(true) == 0;
        }

        private void CircumstancialButtons(bool hasCollision, bool isHidden, Quirk quirk, out List<Texture2D> textures, out List<string> tooltips)
        {
            textures = new List<Texture2D>();
            tooltips = new List<string>();

            if(Prefs.DevMode)
            {
                if(hasCollision)
                {
                    textures.Add(UITextures.InvalidButtonTexture);
                    tooltips.Add("RV2_QuirkWindow_InvalidButton".Translate());
                }
                if(isHidden || quirk?.def.hidden == true)  // categories use isHidden, quirks use the def flag
                {
                    textures.Add(UITextures.HiddenButtonTexture);
                    tooltips.Add("RV2_QuirkWindow_HiddenButton".Translate());
                }
            }
            if(quirk is TempQuirk)
            {
                textures.Add(UITextures.TemporaryButtonTexture);
                tooltips.Add("RV2_QuirkWindow_TemporaryButton".Translate());
            }
            if(quirk is IdeologyQuirk ideoQuirk)
            {
                textures.Add(ideoQuirk.ideo.Icon);
                string tip = "RV2_QuirkWindow_IdeologyButton".Translate();
                if(ideoQuirk.replacedQuirk != null)
                {
                    tip += "\n\n" + "RV2_QuirkWindow_IdeologySuppressingQuirk".Translate() + " " + ideoQuirk.replacedQuirk.def.LabelCap;
                }
                tooltips.Add(tip);
            }
        }
    }
}
