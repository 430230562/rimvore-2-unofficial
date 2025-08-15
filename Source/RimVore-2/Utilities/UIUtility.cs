using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimVore2
{
    // this is a StaticContructorOnStartup because the game yelled at me. I guess it really wants its textures loaded immediately
    [StaticConstructorOnStartup]
    public static class UIUtility
    {
        public const float DefaultSliderWidth = 20f;   // will be subtracted from width for inner windows in scroll views
        public const float DefaultOuterPaddingH = 0f;  // outer horizontal border to inbound rectangle
        public const float DefaultOuterPaddingV = 30f; // outer vertical border to inbound rectangle
        public const float DefaultPaddingBetweenColumns = 24f; // border between columns
        public const float ImageButtonWithLabelSize = 15f;
        public const float GapBetweenButtonsAndLabel = 5f;
        public const string ButtonStringPadding = "  ";


        public static Func<float, string> PercentagePresenter = delegate (float value)
        {
            return Math.Round(value * 100).ToString() + "%";
        };
        public static Func<float, string> ChancePresenter = delegate (float value)
        {
            if(value == 0) return "RV2_Settings_Disabled".Translate();
            else if(value == 1) return "RV2_Settings_Guaranteed".Translate();
            return Math.Round(value * 100).ToString() + "%";
        };
        public static Func<float, string> TemperaturePresenter = delegate (float value)
        {
            return value.ToStringTemperature();
        };

        // use the string as a char array, then iterate through every element and append a space before each uppercase character. This should be sufficient formatting for *most* enum labels
        public static Func<string, string> EnumPresenter = delegate (string input)
        {
            return InsertSpaceBeforeEachUpperCase(input);
        };
        private static string InsertSpaceBeforeEachUpperCase(string input)
        {
            string output = "";
            foreach(char character in input)
            {
                if(char.IsUpper(character))
                    output += ' ';
                output += character;
            }
            return output.Trim();   // strings starting with upper case would 
        }

        // basic getters for Defs
        public static Func<T, string> DefLabelGetter<T>() where T : Def => (T def) => def.LabelCap;
        public static Func<T, string> DefTooltipGetter<T>() where T : Def => (T def) => def.description;

        /// <summary>
        /// Divide inRect into columns
        /// </summary>
        /// <param name="inRect">Rect to divide</param>
        /// <param name="columnCount">Amount of columns to result in</param>
        /// <param name="columnWidth">Outgoing width of each column</param>
        /// <param name="outerPaddingH">Horizontal padding to the inRect to columns</param>
        /// <param name="OuterPaddingV">Vertical padding to the inRect to columns</param>
        /// <param name="paddingBetweenColumns">Horizontal padding between columns</param>
        /// <returns></returns>
        public static List<Rect> CreateColumns(Rect inRect, int columnCount, out float columnWidth, float outerPaddingH = DefaultOuterPaddingH, float OuterPaddingV = DefaultOuterPaddingV, float paddingBetweenColumns = DefaultPaddingBetweenColumns)
        {
            float x = inRect.x + outerPaddingH;
            float y = inRect.y + OuterPaddingV;
            //float columnWidth = ((inRect.width - 2 * outerPaddingHorizontal) - innerPaddingHorizontal / 2) / columns;
            columnWidth = ((inRect.width - 2 * outerPaddingH) - paddingBetweenColumns * columnCount) / columnCount;
            // Log.Message("calculated column width: " + columnWidth + " for inRect width " + inRect.width);
            float height = inRect.height - OuterPaddingV * 2;
            List<Rect> columnList = new List<Rect>();
            for(int i = 0; i < columnCount; i++)
            {
                columnList.Add(new Rect
                (
                    x + (columnWidth + paddingBetweenColumns) * i,
                    y,
                    columnWidth,
                    height
                ));
                // Widgets.DrawRectFast(columnList.Last(), i % 2 == 1 ? Color.red : Color.blue);
            }
            return columnList;
        }

        public static List<Rect> CreateRows(Rect inRect, int rowCount, out float rowHeight, float paddingBetweenRows = 0f)
        {
            rowHeight = (inRect.height - paddingBetweenRows * (rowCount - 1)) / rowCount;
            List<Rect> rowList = new List<Rect>();
            for(int i = 0; i < rowCount; i++)
            {
                rowList.Add(new Rect(
                    inRect.x,
                    inRect.y + (rowHeight + paddingBetweenRows) * i,
                    inRect.width,
                    rowHeight
                ));
            }
            return rowList;
        }

        public static Rect CreateInnerScrollRect(Rect outerRect, float height, float sliderWidth = DefaultSliderWidth)
        {
            return new Rect
            (
                0,
                0,
                outerRect.width - sliderWidth,
                height
            );
        }

        public static void SetHeightIfStale(ref bool isStale, ref float heightToSet, float height)
        {
            if(isStale)
            {
                heightToSet = height;
                isStale = false;
            }
        }

        [Obsolete("Move to EnumLabeled")]
        public static void CreateLabelledDropDownForEnum<T>(this Listing_Standard list, string label, Action<T> action, T currentOption, Func<Rect, string, bool> customButton = null, string labelTooltip = null, string globalButtonTooltip = null, Dictionary<T, string> optionLabels = null, Dictionary<T, string> optionTooltips = null, List<T> valueBlacklist = null, List<T> valueWhitelist = null) where T : struct, IConvertible
        {
            if(!typeof(T).IsEnum)
            {
                throw new ArgumentException("Tried to create drop down for enum that is not an enum!");
            }
            List<T> values = Enum.GetValues(typeof(T)).Cast<T>().ToList();
            string buttonLabel = currentOption.ToString();
            // if we have no option labels
            if(optionLabels != null)
            {
                // if the current option is missing in the label dictionary, use the enums name instead
                buttonLabel = optionLabels.TryGetValue(currentOption, currentOption.ToString());
            }

            float requiredHeight = Text.CalcHeight(label, list.ColumnWidth);
            Rect rowRect = list.GetRect(requiredHeight);
            if(Mouse.IsOver(rowRect))
            {
                Widgets.DrawHighlight(rowRect);
            }
            float labelWidth = Text.CalcSize(label).x;
            // add a few spaces on both ends so the button doesn't look too tight
            float buttonWidth = Text.CalcSize(ButtonStringPadding + buttonLabel + ButtonStringPadding).x;

            string buttonTooltip = globalButtonTooltip;

            if(optionTooltips != null)
            {
                buttonTooltip = optionTooltips.TryGetValue(currentOption, null);
            }

            SplitRectVertically(rowRect, out Rect labelRect, out Rect buttonRect, labelWidth, buttonWidth, labelTooltip, buttonTooltip);

            Widgets.Label(labelRect, label);
            // if no custom button provided, use Widgets.ButtonText()
            if(customButton == null)
            {
                customButton = (Rect localRect, string localLabel) => Widgets.ButtonText(localRect, localLabel);
            }
            if(customButton(buttonRect, buttonLabel))
            {
                List<FloatMenuOption> menuOptions = new List<FloatMenuOption>();
                foreach(T value in values)
                {
                    if(valueBlacklist?.Contains(value) == true)
                    {
                        continue;
                    }
                    if(valueWhitelist?.Contains(value) == false)
                    {
                        continue;
                    }
                    string optionLabel = value.ToString();
                    if(optionLabels != null)
                    {
                        optionLabel = optionLabels.TryGetValue(value, optionLabel);
                    }
                    menuOptions.Add(new FloatMenuOption(optionLabel, () => action(value)));
                }
                Find.WindowStack.Add(new FloatMenu(menuOptions));
            }
        }

        public static void CreateLabelledDropDownForDef<T>(this Listing_Standard list, string label, Dictionary<T, Action<string>> options, T currentOption, string labelTooltip = null, List<string> optionTooltips = null) where T : Def
        {
            if(options == null || options.Keys.Count == 0)
            {
                Log.Error("options is null");
                return;
            }
            if(!options.ContainsKey(currentOption))
            {
                Log.Error("currentOption does not exist in options");
                return;
            }

            float requiredHeight = Text.CalcHeight(label, list.ColumnWidth);
            Rect rowRect = list.GetRect(requiredHeight);
            if(Mouse.IsOver(rowRect))
            {
                Widgets.DrawHighlight(rowRect);
            }
            float labelWidth = Text.CalcSize(label).x;
            // add a few spaces on both ends so the button doesn't look too tight
            float buttonWidth = Text.CalcSize(ButtonStringPadding + currentOption.label + ButtonStringPadding).x;
            string buttonTooltip = null;
            // if tooltips are not set, let's try to use the description of the Def
            if(optionTooltips == null)
            {
                buttonTooltip = currentOption.description;
            }
            // otherwise use the current index of the tooltips
            else
            {
                int currentOptionIndex = options.Keys.FirstIndexOf(key => key == currentOption);
                buttonTooltip = optionTooltips[currentOptionIndex];
            }

            SplitRectVertically(rowRect, out Rect labelRect, out Rect buttonRect, labelWidth, buttonWidth, labelTooltip, buttonTooltip);

            Widgets.Label(labelRect, label);

            if(Widgets.ButtonText(buttonRect, currentOption.label))
            {
                List<FloatMenuOption> menuOptions = new List<FloatMenuOption>();
                foreach(KeyValuePair<T, Action<string>> option in options)
                {
                    menuOptions.Add(new FloatMenuOption(option.Key.label, () => option.Value(option.Key.defName)));
                }
                Find.WindowStack.Add(new FloatMenu(menuOptions));
            }
        }

        public static void CreateLabelledDropDown<T>(this Listing_Standard list, IEnumerable<T> options, T currentOption, Func<T, string> optionLabelGetter, Action<T> optionAction, Func<Rect, string, bool> button = null, string label = null, string labelTooltip = null, Func<T, string> optionTooltipGetter = null)
        {
            if(options.EnumerableNullOrEmpty())
            {
                RV2Log.Warning("No options provided!", true);
                return;
            }
            if(list == null || optionLabelGetter == null || optionAction == null)
            {
                RV2Log.Warning("No listing, option label getter or option action provided!", true);
                return;
            }
            if(currentOption == null)
            {
                if(RV2Log.ShouldLog(true, "Settings"))
                    RV2Log.Message("No current option, setting to first available", false, "Settings");
                currentOption = options.First();
                optionAction(currentOption);
            }
            if(button == null)
            {
                button = (Rect localRect, string localLabel) => Widgets.ButtonText(localRect, localLabel);
            }
            Rect buttonRect;
            string buttonLabel = optionLabelGetter(currentOption);
            string buttonTooltip = optionTooltipGetter != null ? optionTooltipGetter(currentOption) : null;
            float buttonWidth = Text.CalcSize(ButtonStringPadding + buttonLabel + ButtonStringPadding).x;
            float requiredHeight;
            if(label != null)
            {
                requiredHeight = Text.CalcHeight(label + buttonLabel, list.ColumnWidth);
                Rect rowRect = list.GetRect(requiredHeight);
                if(Mouse.IsOver(rowRect))
                {
                    Widgets.DrawHighlight(rowRect);
                }
                float labelWidth = Text.CalcSize(label).x;
                SplitRectVertically(rowRect, out Rect labelRect, out buttonRect, labelWidth, buttonWidth, labelTooltip);
                Widgets.Label(labelRect, label);
            }
            else
            {
                requiredHeight = Text.CalcHeight(optionLabelGetter(currentOption), list.ColumnWidth);
                buttonRect = list.GetRect(requiredHeight);
                if(Mouse.IsOver(buttonRect))
                {
                    Widgets.DrawHighlight(buttonRect);
                }
            }
            bool buttonPressed = button(buttonRect, buttonLabel);
            if(buttonTooltip != null)
            {
                TooltipHandler.TipRegion(buttonRect, buttonTooltip);
            }
            if(buttonPressed)
            {
                List<FloatMenuOption> floatOptions = new List<FloatMenuOption>();
                foreach(T option in options)
                {
                    string optionLabel = optionLabelGetter(option);
                    floatOptions.Add(new FloatMenuOption(optionLabel, () => optionAction(option)));
                }
                Find.WindowStack.Add(new FloatMenu(floatOptions));
            }
        }

        /// <summary>
        /// Divide a inRect into two rects, set both widths to -1 to split in the middle
        /// </summary>
        /// <param name="inRect">Rect to divide</param>
        /// <param name="leftWidth">Width of the left rect, can be set to -1 to use the remaining space of rightRect</param>
        /// <param name="rightWidth">Width of the right rect, can be set to -1 to use the remaining space of leftRect</param>
        /// <param name="leftRect">Resulting left rect</param>
        /// <param name="rightRect">Resulting left rect</param>
        /// <param name="leftTooltip">Tooltip for left rect</param>
        /// <param name="rightTooltip">Tooltip for left rect</param>
        /// <param name="paddingBetween">Padding between rects</param>
        public static void SplitRectVertically(Rect inRect, out Rect leftRect, out Rect rightRect, float leftWidth = -1f, float rightWidth = -1f, string leftTooltip = null, string rightTooltip = null, float paddingBetween = 0f)
        {
            bool isTooTight = leftWidth + rightWidth + paddingBetween > inRect.width;
            if(isTooTight)
            {
                string errorMessage = "Tried to split vertically when left and right width with padding exceeds total width";
                if(RV2Log.ShouldLog(true, "UI"))
                    RV2Log.Message(errorMessage, true, "UI", inRect.x.GetHashCode() + inRect.y.GetHashCode() * errorMessage.GetHashCode());
                // if the labels fit without padding, discard the padding
                if(leftWidth + rightWidth <= inRect.width)
                {
                    paddingBetween = 0f;
                }
                // otherwise just trigger the calculation to split in the middle
                else
                {
                    leftWidth = -1;
                    rightWidth = -1;
                }
            }
            if(leftWidth == -1f && rightWidth == -1)
            {
                leftWidth = (inRect.width - paddingBetween) / 2;
                rightWidth = leftWidth;
            }
            else if(leftWidth == -1)
            {
                leftWidth = inRect.width - (rightWidth + paddingBetween);
            }
            else if(rightWidth == -1)
            {
                rightWidth = inRect.width - (leftWidth + paddingBetween);
            }
            leftRect = new Rect(inRect.x, inRect.y, leftWidth, inRect.height);
            float rightX = Math.Max(inRect.x + leftRect.width + paddingBetween, inRect.x + inRect.width - rightWidth);
            rightRect = new Rect(rightX, leftRect.y, rightWidth, leftRect.height);

            if(leftTooltip != null)
            {
                TooltipHandler.TipRegion(leftRect, leftTooltip);
            }
            if(rightTooltip != null)
            {
                TooltipHandler.TipRegion(rightRect, rightTooltip);
            }
        }

        public static void LeftRightLabels(this Listing_Standard list, string leftLabel, string rightLabel, string leftTip = null, string rightTip = null)
        {
            Vector2 leftSize = Text.CalcSize(leftLabel);
            Vector2 rightSize = Text.CalcSize(rightLabel);
            float leftWidth = leftSize.x;
            float rightWidth = rightSize.x;
            // if both labels together take too much space, take the smaller one (that hopefully fits) and then subtract the total width with the smaller one, that's the space available for the other label
            if(list.ColumnWidth < leftSize.x + rightSize.x)
            {
                if(leftSize.x < rightSize.x)
                {
                    leftWidth = leftSize.x;
                    rightWidth = list.ColumnWidth - leftWidth;
                }
                else
                {
                    rightWidth = leftSize.x;
                    leftWidth = list.ColumnWidth - rightWidth;
                }
            }
            float requiredHeight = Text.CalcHeight(leftLabel + rightLabel, leftWidth + rightWidth);

            Rect rowRect = list.GetRect(requiredHeight);
            if(Mouse.IsOver(rowRect))
            {
                Widgets.DrawHighlight(rowRect);
            }
            SplitRectVertically(rowRect, out Rect leftRect, out Rect rightRect, leftWidth, rightWidth, leftTip, rightTip);
            Widgets.Label(leftRect, leftLabel);
            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.UpperRight;
            Widgets.Label(rightRect, rightLabel);
            Text.Anchor = anchor;
        }

        public static void LabelWithTooltip(Rect rect, string label, string tooltip = null)
        {
            Widgets.Label(rect, label);
            if(tooltip != null)
            {
                TooltipHandler.TipRegion(rect, tooltip);
            }
        }

        public static void LabelWithTooltip(this Listing_Standard list, string label, string tooltip = null)
        {
            float requiredHeight = Text.CalcHeight(label, list.ColumnWidth);
            Rect rect = list.GetRect(requiredHeight);
            LabelWithTooltip(rect, label, tooltip);
        }

        /// <summary>
        /// make a label prefaced with a dynamic amount of ImageButtons 
        /// </summary>
        /// <param name="list">The Listing_Standard to add to</param>
        /// <param name="label">The label to display after the buttons</param>
        /// <param name="buttonTextures">List of textures for each ImageButton to display</param>
        /// <param name="buttonsClicked">Each buttonTextures entry will have a bool entry here to reference</param>
        public static void ButtonImagesWithLabel(this Listing_Standard list, string label, List<Texture2D> buttonTextures, out List<bool> buttonsClicked, string labelTooltip = null, List<string> buttonTooltips = null, float buttonSize = ImageButtonWithLabelSize, float paddingBetweenButtons = 5f)
        {
            Rect rowRect = list.GetRect(Text.CalcHeight(label, list.ColumnWidth));
            if(Mouse.IsOver(rowRect))
            {
                Widgets.DrawHighlight(rowRect);
            }
            list.Gap(list.verticalSpacing);
            float currentX = rowRect.x;
            buttonsClicked = new List<bool>();
            bool buttonsHaveTooltips;
            if(buttonTooltips == null)
            {
                buttonsHaveTooltips = false;
            }
            else if(buttonTextures.Count == buttonTooltips.Count)
            {
                buttonsHaveTooltips = true;
            }
            else
            {
                RV2Log.Warning("buttonTooltips is not empty, but the number of tooltips doesn't match the number of buttonTextures, disabling button tooltips");
                buttonsHaveTooltips = false;
            }
            for(int i = 0; i < buttonTextures.Count; i++)
            {
                Rect imageButtonRect = new Rect(currentX, rowRect.y + (rowRect.height - list.verticalSpacing) / 2 - buttonSize / 2, buttonSize, buttonSize);
                bool buttonClicked = Widgets.ButtonImage(imageButtonRect, buttonTextures[i], Color.white, Color.blue);
                if(buttonsHaveTooltips)
                {
                    TooltipHandler.TipRegion(imageButtonRect, buttonTooltips[i]);
                }
                buttonsClicked.Add(buttonClicked);
                currentX += buttonSize + paddingBetweenButtons;
            }
            currentX += GapBetweenButtonsAndLabel;
            Rect labelRect = new Rect(currentX, rowRect.y, rowRect.width - currentX, rowRect.height);
            Widgets.Label(labelRect, label);
            if(labelTooltip != null)
            {
                TooltipHandler.TipRegion(labelRect, labelTooltip);
            }
        }

        public static void ButtonImages(Rect inRect, List<Texture2D> buttons, List<Action> buttonClickActions, List<string> buttonTooltips = null, Func<Rect, Texture2D, bool> customButton = null, float buttonSize = ImageButtonWithLabelSize, float paddingBetweenButtons = 5f)
        {
            if(buttons.NullOrEmpty())
            {
                return;
            }
            if(Mouse.IsOver(inRect))
            {
                Widgets.DrawHighlight(inRect);
            }
            if(customButton == null)
            {
                customButton = (Rect buttonRect, Texture2D buttonTexture) => Widgets.ButtonImage(buttonRect, buttonTexture);
            }
            float currentX = inRect.x;
            float currentY = inRect.y + inRect.height / 2 - buttonSize / 2;
            for(int i = 0; i < buttons.Count; i++)
            {
                Rect buttonRect = new Rect(currentX, currentY, buttonSize, buttonSize);
                if(customButton(buttonRect, buttons[i]))
                {
                    if(buttonClickActions != null && buttonClickActions.Count - 1 >= i)
                    {
                        SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera(null);
                        buttonClickActions[i]();
                    }
                }
                // if tooltips are set and there is a retrievable tooltip for the current index
                if(buttonTooltips != null && buttonTooltips.Count - 1 >= i)
                {
                    TooltipHandler.TipRegion(buttonRect, buttonTooltips[i]);
                }
                currentX += buttonSize + paddingBetweenButtons;
            }
        }

        /// <summary>
        /// Convenience method to use ButtonImagesWithLabel() with a single ButtonImage
        /// </summary>
        public static void ButtonImageWithLabel(this Listing_Standard list, string label, Texture2D buttonTexture, out bool buttonClicked, string labelTooltip = null, string buttonTooltip = null, float buttonSize = ImageButtonWithLabelSize)
        {
            List<string> buttonTooltips;
            if(buttonTooltip == null)
            {
                buttonTooltips = null;
            }
            else
            {
                buttonTooltips = new List<string>() { buttonTooltip };
            }
            List<Texture2D> textures = new List<Texture2D>() { buttonTexture };
            ButtonImagesWithLabel(list, label, textures, out List<bool> buttonsClicked, labelTooltip, buttonTooltips, buttonSize);
            if(buttonsClicked[0])
            {
                buttonClicked = true;
            }
            else
            {
                buttonClicked = false;
            }
        }

        /// <summary>
        /// Convenience method to get scroll views going faster
        /// </summary>
        /// <param name="list">Listing_Standard to provide to</param>
        /// <param name="inRect">Outer view rect the scrollview is visible in</param>
        /// <param name="requiredHeight">Static reference to a (usually) cached height that is refreshed at the end of the scroll view</param>
        /// <param name="scrollPosition">Static reference to scroll position Vector2</param>
        /// <param name="innerRect">The resulting inner rect</param>
        public static void MakeAndBeginScrollView(Rect inRect, float requiredHeight, ref Vector2 scrollPosition, out Listing_Standard list)
        {
            //list.Label("before scroll, creating for height " + requiredHeight);
            Rect innerRect = CreateInnerScrollRect(inRect, requiredHeight);
            Widgets.BeginScrollView(inRect, ref scrollPosition, innerRect);
            Rect extraRect = innerRect.AtZero();
            extraRect.height = 999999f;
            list = new Listing_Standard()
            {
                ColumnWidth = innerRect.width,
                maxOneColumn = true
            };
            list.Begin(extraRect);
            //list.Label("inside scroll");
        }
        /// <summary>
        /// Convenience method to finish scroll views faster
        /// </summary>
        /// <param name="list">Listing_Standard to provide to</param>
        /// <param name="requiredHeight">Static reference to a (usually) cached height that is refreshed at the end of the scroll view</param>
        /// <param name="requiredHeightStale">Reference to stale state of height, will be set to false</param>
        /// <param name="innerRect">The inner scroll rect</param>
        public static void EndScrollView(this Listing_Standard list, ref float requiredHeight, ref bool requiredHeightStale)
        {
            if(requiredHeightStale)
            {
                requiredHeight = list.CurHeight;
                requiredHeightStale = false;
            }
            list.End();
            //list.Label("before scroll end");
            Widgets.EndScrollView();
            //list.Label("after scroll end");
        }

        public static void LabelInCenter(Rect inRect, string label, GameFont font = GameFont.Small)
        {
            GameFont originalFont = Text.Font;
            Text.Font = font;
            Vector2 labelSize = Text.CalcSize(label);
            float width = Mathf.Min(labelSize.x, inRect.width);
            float height = Text.CalcHeight(label, width);

            float startX, startY;
            if(labelSize.x > inRect.width)
                startX = inRect.x;
            else
                startX = inRect.x + inRect.width / 2 - width / 2;

            if(labelSize.y > inRect.height)
                startY = inRect.y;
            else
                startY = inRect.y + inRect.height / 2 - height / 2;

            Rect labelRect = new Rect(startX, startY, width, height);
            Widgets.Label(labelRect, label);
            Text.Font = originalFont;
        }

        public static void TextureInCenter(Rect inRect, Texture texture, out Rect textureRect, float size = -1, float padding = 0, Color? color = null)
        {
            inRect = inRect.ContractedBy(padding);
            if(size < 0)
                size = Math.Min(inRect.width, inRect.height);
            float startX = inRect.x
                + inRect.width / 2
                - size / 2;
            float startY = inRect.y
                + inRect.height / 2
                - size / 2;
            textureRect = new Rect(startX, startY, size, size);


            if(color != null)
                GUI.DrawTexture(textureRect, texture, 0, true, 0, color.Value, 0, 0);
            else
                GUI.DrawTexture(textureRect, texture);
        }

        public static void HeaderLabel(this Listing_Standard list, string label, bool drawGapLine = true)
        {
            GameFont originalFont = Text.Font;
            Text.Font = GameFont.Medium;
            list.Label(label);
            if(drawGapLine)
            {
                list.GapLine();
            }
            Text.Font = originalFont;
        }

        /// <summary>
        /// Fill a row with multiple columns of buttons
        /// </summary>
        /// <param name="list">The Listing_Standard to provide to</param>
        /// <param name="labels">The labels of each button</param>
        /// <param name="indexClicked">The outbound index of the button that has been clicked</param>
        /// <param name="alignment">The type of width separation to give each button their width</param>
        /// <param name="fixedWidths">Overrides for the width of specified indices. -1 means no fixed width for index</param>
        /// <param name="tooltips">List of tooltips that are displayed when the button of its corresponding index is clicked</param>
        /// <param name="customButton">Custom button implementations, default Widgets.ButtonText()</param>
        public static void ButtonRow(this Listing_Standard list, List<string> labels, out int indexClicked, PairAlignment alignment, List<float> fixedWidths = null, List<string> tooltips = null, Func<Rect, string, bool> customButton = null)
        {
            indexClicked = -1;
            float requiredHeight = Text.CalcHeight(string.Join(ButtonStringPadding + ButtonStringPadding, labels), list.ColumnWidth);
            Rect rowRect = list.GetRect(requiredHeight);
            List<Rect> rects = CalculateRectsForAlignments(rowRect, labels, alignment, fixedWidths);
            for(int i = 0; i < rects.Count; i++)
            {
                if(customButton == null)
                {
                    customButton = (Rect rect, string label) => Widgets.ButtonText(rect, label);
                }
                if(customButton(rects[i], labels[i]))
                {
                    indexClicked = i;
                }
                if(!tooltips.NullOrEmpty() && tooltips.Count >= i)
                {
                    TooltipHandler.TipRegion(rects[i], tooltips[i]);
                }
            }
        }

        /// <summary>
        /// Create a button that reflects its own active state by being normally colored or greyed out
        /// </summary>
        /// <param name="rect">Rect to fill with button</param>
        /// <param name="label">Button label</param>
        /// <param name="isActive">Current button state</param>
        /// <returns></returns>
        public static bool ToggleButton(Rect rect, string label, bool isActive, bool isCurrentlySelected, bool doMouseoverSound = true)
        {
            Color color = GUI.color;
            TextAnchor anchor = Text.Anchor;

            Texture2D atlas = ContentFinder<Texture2D>.Get("UI/Widgets/ButtonBG", true);
            if(Mouse.IsOver(rect))
            {
                if(Mouse.IsOver(rect))
                {
                    atlas = ContentFinder<Texture2D>.Get("UI/Widgets/ButtonBGMouseover", true);
                    if(Input.GetMouseButton(0))
                    {
                        atlas = ContentFinder<Texture2D>.Get("UI/Widgets/ButtonBGClick", true);
                    }
                }
                Widgets.DrawAtlas(rect, atlas);
            }
            if(isActive && isCurrentlySelected)
            {
                GUI.color = Color.yellow;
            }
            else if(isActive)
            {
                GUI.color = color;
            }
            else if(!isActive && isCurrentlySelected)
            {
                GUI.color = Color.cyan;
            }
            else if(!isActive)
            {
                GUI.color = Color.grey;
            }

            Widgets.DrawAtlas(rect, atlas);
            GUI.color = color;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect, label);
            Text.Anchor = anchor;
            return Widgets.ButtonInvisible(rect, doMouseoverSound);
        }

        /// <summary>
        /// Split inRect into rectangles with different shared widths
        /// </summary>
        /// <param name="inRect">Rect to divide</param>
        /// <param name="labels">Labels to calculate sizes for</param>
        /// <param name="alignment">Which alignment type to use to calculate rect width</param>
        /// <param name="fixedWidths">Optional fixed widths for the specified index (must match Count of labels if not null)</param>
        /// <returns></returns>
        public static List<Rect> CalculateRectsForAlignments(Rect inRect, List<string> labels, PairAlignment alignment, List<float> fixedWidths = null)
        {
            float totalWidth = inRect.width;
            if(fixedWidths == null)
            {
                // allows us to use fixedWidths without null checking
                fixedWidths = new List<float>(labels.Count);
                for(int i = 0; i < labels.Count; i++)
                {
                    fixedWidths.Add(-1);
                }
            }
            if(fixedWidths.Count != labels.Count)
            {
                throw new ArgumentException("Labels count does not match provided fixed width count. Aborting.");
            }
            List<float> labelWidths = labels.ConvertAll(label => Text.CalcSize(label).x);
            for(int i = 0; i < labels.Count; i++)
            {
                if(fixedWidths[i] != -1)
                {
                    // set the label width to be "discarded", this means the fixedWidth will instead be used
                    labelWidths[i] = -1;
                }
            }

            // sum up all widths that are fixed
            float totalFixedWidth = fixedWidths.FindAll(w => w != -1).Sum();

            // dynamic is for all labels that are not fixed
            int dynamicCount = labels.Count - fixedWidths.FindAll(w => w != -1).Count;
            float totalDynamicWidth = totalWidth - totalFixedWidth;
            float totalRequiredDynamicWidth = labelWidths.FindAll(w => w != -1).Sum();

            float spaceBetween;
            Func<float, float> labelCalc;
            switch(alignment)
            {
                case PairAlignment.Spaced:
                    // give every label as much space as it needs, the space between them will be empty -> (totalWidth - requiredWith) / requiredGaps
                    spaceBetween = (totalDynamicWidth - totalRequiredDynamicWidth) / (labels.Count - 1);
                    labelCalc = (float width) => width;
                    break;
                case PairAlignment.Equal:
                    // all labels share the same amount of dynamic space, no gaps
                    spaceBetween = 0;
                    labelCalc = (float width) => totalWidth / dynamicCount;
                    break;
                case PairAlignment.Proportional:
                    // all labels are weighted and then take their proportion of the total width, no gaps
                    spaceBetween = 0;
                    labelCalc = (float width) => totalDynamicWidth * (width / totalRequiredDynamicWidth);
                    break;
                default:
                    // will give every label exactly the size it needs with no space between, this will effetively left-align all labels and leave free space on the right
                    spaceBetween = 0;
                    labelCalc = (float width) => width;
                    break;
            }

            List<Rect> rects = new List<Rect>();
            float curX = inRect.x;
            for(int i = 0; i < labels.Count; i++)
            {
                // use fixed width if label width discarded, otherwise recalculate label width and use it
                float width = labelWidths[i] == -1 ? fixedWidths[i] : labelCalc(labelWidths[i]);
                rects.Add(new Rect(curX, inRect.y, width, inRect.height));
                curX += width + spaceBetween;
            }


            return rects;
        }

        public static void LabeledCheckbox<T>(this Listing_Standard list, string label, ref T currentValue, List<Texture2D> checkboxIcons, string tooltip = null, float checkboxSize = 20f, float padding = 5f) where T : struct, IComparable, IFormattable, IConvertible
        {
            if(!typeof(T).IsEnum)
            {
                RV2Log.Warning("LabeledCheckbox<T>() called with type that is not an enum!");
                return;
            }
            float labelWidth = list.ColumnWidth - (checkboxSize + padding);
            float labelHeight = Text.CalcHeight(label, labelWidth);
            Rect rowRect = list.GetRect(labelHeight);
            if(Mouse.IsOver(rowRect))
            {
                Widgets.DrawHighlight(rowRect);
            }
            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            Rect labelRect = new Rect(rowRect.x, rowRect.y, labelWidth, labelHeight);
            Rect checkboxRect = new Rect(labelRect.width + padding, labelRect.y + rowRect.height / 2 - checkboxSize / 2, checkboxSize, checkboxSize);
            Widgets.Label(labelRect, label);
            Text.Anchor = anchor;
            if(tooltip != null)
            {
                if(Mouse.IsOver(rowRect))
                {
                    Widgets.DrawHighlight(rowRect);
                }
                TooltipHandler.TipRegion(rowRect, tooltip);
            }
            Checkbox(checkboxRect, ref currentValue, checkboxIcons);
        }

        public static void Checkbox<T>(Rect inRect, ref T currentValue, List<Texture2D> checkboxIcons, string tooltip = null) where T : struct, IComparable, IFormattable, IConvertible
        {
            if(!typeof(T).IsEnum)
            {
                RV2Log.Warning("Checkbox<T>() called with type that is not an enum!");
                return;
            }
            if(Widgets.ButtonImage(inRect, checkboxIcons.ElementAt(currentValue.Index())))
            {
                SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera(null);
                currentValue = currentValue.Next();
            }
            if(tooltip != null)
            {
                TooltipHandler.TipRegion(inRect, tooltip);
            }
        }

        public static FloatMenuOption DisabledOption(string label)
        {
            return new FloatMenuOption(label, () => { })
            {
                Disabled = true
            };
        }

        public static void SliderLabeled(this Listing_Standard list, string label, ref float currentValue, float minValue = 0, float maxValue = 9999, string labelTooltip = null, Func<float, string> valuePresenter = null)
        {
            label += ": ";
            if(valuePresenter == null)
            {
                label += currentValue;
            }
            else
            {
                label += valuePresenter(currentValue);
            }
            float startY = list.CurHeight;
            list.Label(label, -1, labelTooltip);
            currentValue = list.Slider(currentValue, minValue, maxValue);
            float endY = list.CurHeight;
            Rect SliderAndLabelRect = new Rect(0, startY, list.ColumnWidth, endY - startY);
            if(Mouse.IsOver(SliderAndLabelRect))
            {
                Widgets.DrawHighlight(SliderAndLabelRect);
            }
        }
        public static void SliderLabeled(this Listing_Standard list, string label, ref int currentValue, int minValue = 0, int maxValue = 9999, string labelTooltip = null, Func<float, string> valuePresenter = null)
        {
            float currentValueFloat = (float)currentValue;
            SliderLabeled(list, label, ref currentValueFloat, (float)minValue, (float)maxValue, labelTooltip, valuePresenter);
            currentValue = (int)Math.Round(currentValueFloat);
        }

        public static void FancySlider(Rect rect, string label, ref float currentValue, float minValue = 0, float maxValue = 999, string format = "0.00", string extraLabel = "")
        {
            string currentValueText = currentValue.ToString(format) + extraLabel;
            float textBoxWidth = 80f;
            float upperHeight = rect.height - Text.CalcHeight(label, rect.width - textBoxWidth);
            Rect upperRect = new Rect(rect.x, rect.y, rect.width, upperHeight);
            Rect lowerRect = new Rect(rect.x, rect.y + upperHeight, rect.width, rect.height - upperHeight);

            UIUtility.SplitRectVertically(upperRect, out Rect labelRect, out Rect textBoxRect, -1, textBoxWidth);
            Widgets.Label(labelRect, label);
            string inputText = Widgets.TextField(textBoxRect, currentValueText);
            if(extraLabel != "")
            {
                inputText = inputText.Replace(extraLabel, "");  // remove the extra string to parse back to float
            }
            float.TryParse(inputText, out currentValue);
            currentValue = Mathf.Clamp(currentValue, minValue, maxValue);

            string minValueText = minValue.ToString(format) + extraLabel;
            string maxValueText = maxValue.ToString(format) + extraLabel;
            float minValueWidth = Text.CalcSize(minValueText).x;
            float maxValueWidth = Text.CalcSize(maxValueText).x;
            float sliderWidth = lowerRect.width - minValueWidth - maxValueWidth;
            Rect minValueRect = new Rect(rect.x, lowerRect.y, minValueWidth, lowerRect.height);
            float sliderY = lowerRect.y + ((Text.LineHeight - GUI.skin.horizontalSliderThumb.fixedHeight) / 2); // need a slight y offset, or the slider will be misaligned to the labels
            Rect sliderRect = new Rect(minValueRect.x + minValueWidth, sliderY, sliderWidth, lowerRect.height);
            Rect maxValueRect = new Rect(sliderRect.x + sliderWidth, lowerRect.y, maxValueWidth, lowerRect.height);
            Widgets.Label(minValueRect, minValueText);
            Widgets.Label(maxValueRect, maxValueText);
            currentValue = Widgets.HorizontalSlider(sliderRect, currentValue, minValue, maxValue);
        }

        /// <remarks>
        /// There is fallback logic for valueSetting being null, but utilizing it will leave out the ability to translate the ENUM to a key, it is thus recommended to provide a value presenter
        /// </remarks>
        public static void EnumLabeled<T>(this Listing_Standard list, string label, T currentValue, Action<T> valueSetter, Func<T, string> valuePresenter, string tooltip = null, List<T> valueWhitelist = null, List<T> valueBlacklist = null) where T : struct, IConvertible
        {
            if(!typeof(T).IsEnum)
            {
                throw new ArgumentException("Tried to create drop down for non-enum!");
            }
            IEnumerable<T> values = Enum.GetValues(typeof(T)).Cast<T>();
            if(!valueWhitelist.NullOrEmpty())
            {
                values = values.Where(value => valueWhitelist.Contains(value));
            }
            if(!valueBlacklist.NullOrEmpty())
            {
                values = values.Where(value => !valueBlacklist.Contains(value));
            }
            if(values.EnumerableNullOrEmpty())
            {
                return;
            }
            string buttonLabel = PresentValue(currentValue);
            buttonLabel = ButtonStringPadding + buttonLabel + ButtonStringPadding;
            float requiredHeight = Text.CalcHeight(label + buttonLabel, list.ColumnWidth);
            Rect rowRect = list.GetRect(requiredHeight);
            if(Mouse.IsOver(rowRect))
            {
                Widgets.DrawHighlight(rowRect);
            }
            if(tooltip != null)
            {
                TooltipHandler.TipRegion(rowRect, tooltip);
            }
            float labelWidth = Text.CalcSize(label).x;
            float buttonWidth = Text.CalcSize(buttonLabel).x;
            SplitRectVertically(rowRect, out Rect labelRect, out Rect buttonRect, labelWidth, buttonWidth);
            Widgets.Label(labelRect, label);

            if(Widgets.ButtonText(buttonRect, buttonLabel))
            {
                List<FloatMenuOption> menuOptions = new List<FloatMenuOption>();
                foreach(T value in values)
                {
                    menuOptions.Add(new FloatMenuOption(PresentValue(value), () => valueSetter(value)));
                }
                Find.WindowStack.Add(new FloatMenu(menuOptions));
            }

            string PresentValue(T value)
            {
                if(valuePresenter != null)
                    return valuePresenter(value);
                // if no presenter provided, use the custom one that spaces each capitalized substring
                return UIUtility.EnumPresenter(value.ToString());
            }
        }

        public static string DoLabelledTextField(this Listing_Standard list, string label, string initialFieldContent, string labelTooltip = null, string textFieldTooltip = null)
        {
            Vector2 labelSize = Text.CalcSize(label);
            Rect rowRect = list.GetRect(labelSize.y);
            return DoLabelledTextField(rowRect, label, initialFieldContent, labelTooltip, textFieldTooltip);
        }

        public static string DoLabelledTextField(Rect inRect, string label, string initialFieldContent, string labelTooltip = null, string textFieldTooltip = null)
        {
            if(Mouse.IsOver(inRect))
            {
                Widgets.DrawHighlight(inRect);
            }
            Vector2 labelSize = Text.CalcSize(label);
            UIUtility.SplitRectVertically(inRect, out Rect labelRect, out Rect fieldRect, labelSize.x, -1, labelTooltip, textFieldTooltip, 15f);
            Widgets.Label(labelRect, label);
            return Widgets.TextField(fieldRect, initialFieldContent);
        }

        public static bool DoSaveCancelButtons(Rect inRect, Action saveAction, Action cancelAction)
        {
            string saveLabel = "RV2_Settings_Rules_Save".Translate();
            string buttonSpace = "    ";
            Vector2 saveButtonSize = Text.CalcSize(buttonSpace + saveLabel + buttonSpace);
            string cancelLabel = "RV2_Settings_Rules_Cancel".Translate();
            Vector2 cancelButtonSize = Text.CalcSize(buttonSpace + cancelLabel + buttonSpace);
            float totalWidth = saveButtonSize.x + cancelButtonSize.x;
            float startX = inRect.width / 2 - totalWidth / 2;
            Rect saveRect = new Rect(startX, inRect.y, saveButtonSize.x, inRect.height);
            Rect cancelRect = new Rect(startX + saveButtonSize.x, inRect.y, cancelButtonSize.x, inRect.height);
            if(Widgets.ButtonText(saveRect, "RV2_Settings_Rules_Save".Translate()))
            {
                saveAction();
                return true;
            }
            if(Widgets.ButtonText(cancelRect, "RV2_Settings_Rules_Cancel".Translate()))
            {
                cancelAction();
                return true;
            }
            return false;
        }

        public static void Indent(this Listing_Standard list, bool affectColumnWidth, float width = 12f)
        {
            list.Indent(width);
            if(affectColumnWidth)
            {
                list.ColumnWidth -= width;
            }
        }
        public static void Outdent(this Listing_Standard list, bool affectColumnWidth, float width = 12f)
        {
            list.Outdent(width);
            if(affectColumnWidth)
            {
                list.ColumnWidth += width;
            }
        }

        public static Rect ContractedBy(this Rect rect, float top = 0, float right = 0, float bottom = 0, float left = 0)
        {
            rect.y += top;
            rect.height -= top;

            rect.width -= right;

            rect.height -= bottom;

            rect.x += left;
            rect.width -= left;

            return rect;
        }

        public static Rect CollapsibleRect(this Listing_Standard list, ref bool collapsed, float expandedHeight, float collapsedHeight = 0f)
        {
            float contentHeight = collapsed ? collapsedHeight : expandedHeight;
            Rect contentRect = list.GetRect(contentHeight);
            string buttonLabel = collapsed ? "RV2_Settings_Expand" : "RV2_Settings_Collapse";
            buttonLabel = buttonLabel.Translate();
            if(list.ButtonText(buttonLabel))
                collapsed = !collapsed;
            return contentRect;
        }
        public static void OverwriteResizer(this Window window, float minWidth = 400f, float minHeight = 400f)
        {
            try
            {
                WindowResizer newResizer = new WindowResizer()
                {
                    minWindowSize = new Vector2(minWidth, minHeight)
                };
                typeof(Window).GetField("resizer", AccessTools.all).SetValue(window, newResizer);
            }
            catch(Exception e)
            {
                RV2Log.Error("Could not overwrite resizer field, reflection probably messed up due to an update: " + e, true);
            }
        }

        public static RenderTexture PortraitTexture(Pawn pawn, float size)
        {
            return PortraitsCache.Get(pawn, new Vector2(size, size), Rot4.South, compensateForUIScale: false);
        }
    }

    public class IconGrid
    {
        readonly int columns;
        readonly int rows;
        int curColumn = 0;
        int curRow = 0;
        public float startX;
        public float startY;
        readonly float iconSize;
        readonly float iconPadding;
        public Vector2 size;
        public IconGrid(int columns, int rows, float startX, float startY, float iconSize, float iconPadding)
        {
            this.columns = columns;
            this.rows = rows;
            this.startX = startX;
            this.startY = startY;
            this.iconSize = iconSize;
            this.iconPadding = iconPadding;
            CalculateSize();
            if(RV2Log.ShouldLog(false, "UI"))
                RV2Log.Message($"Initialized icon grid: {this}", true, "UI");
        }

        public override string ToString()
        {
            return $"Columns: {columns}, rows: {rows}, startX: {startX}, startY: {startY}, iconSize: {iconSize}, iconPadding: {iconPadding} |-> total size: {size}";
        }

        public void CalculateSize()
        {
            // simply calculate the index after the last one, which factors in the padding
            // other calculation would be to take the actual last index and add its own size GetX(columns-1)+iconPadding+iconSize
            float maxX = GetX(columns) - startX;
            float maxY = GetY(rows) - startY;
            size = new Vector2(maxX, maxY);
        }

        public void SetStartTopLeft(Vector2 topLeft)
        {
            startX = topLeft.x;
            startY = topLeft.y;
            CalculateSize();
        }

        public float GetX(int index)
        {
            return startX + GetCoordinate(index);
        }
        public float GetY(int index)
        {
            return startY + GetCoordinate(index);
        }

        public float GetCoordinate(int index)
        {
            return iconPadding + index * (iconSize + iconPadding);
        }

        public Vector2Int GetCurrentGridPosition(bool increaseIndex = false)
        {
            Vector2Int vector = new Vector2Int(curColumn, curRow);
            if(increaseIndex)
            {
                IncreaseGridPosition();
            }
            //RV2Log.Message("current grid position: " + vector);
            return vector;
        }

        public Rect GetCurrentGridRect(bool increaseIndex = false)
        {
            Vector2Int pos = GetCurrentGridPosition(increaseIndex);
            return GetRect(pos);
        }

        public Vector2 GetTopLeft(Vector2Int gridPosition)
        {
            float x = GetX(gridPosition.x);
            float y = GetY(gridPosition.y);
            //RV2Log.Message("grid position " + gridPosition + " x: " + x + " y: " + y);
            return new Vector2(x, y);
        }


        public Rect GetRect(Vector2Int gridPosition)
        {
            Vector2 topLeft = GetTopLeft(gridPosition);
            return new Rect(topLeft, new Vector2(iconSize, iconSize));
        }

        public void IncreaseGridPosition()
        {
            // if we are currently at the end of the row, set back to column 1 and increase row count
            if(curColumn == columns - 1)
            {
                curColumn = 0;
                curRow++;
            }
            else
            {
                curColumn++;
            }
            if(curColumn >= columns)
            {
                RV2Log.Warning("Trying to draw icons in a row that is out of bounds!");
            }
        }
        public void ResetGridPosition()
        {
            curColumn = 0;
            curRow = 0;
        }
    }

    public enum PairAlignment
    {
        Spaced, // all elements use the exact width they require, this will produce gaps
        Equal, // all elements share the same width
        Proportional  // all elements receive width proportional to their length
    }
}
