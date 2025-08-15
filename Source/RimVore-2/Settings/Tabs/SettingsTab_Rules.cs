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
    public class SettingsTab_Rules : SettingsTab
    {
        public SettingsTab_Rules(string label, Action clickedAction, bool selected) : base(label, clickedAction, selected) { }
        public SettingsTab_Rules(string label, Action clickedAction, Func<bool> selected) : base(label, clickedAction, selected) { }


        public override SettingsContainer AssociatedContainer => RV2Mod.Settings.rules;
        public SettingsContainer_Rules Rules => (SettingsContainer_Rules)AssociatedContainer;
        private bool ShouldDisplayPawnExplanation => RuleTarget.PickedPawn != null;

        const float iconSize = 24f;
        private bool heightStale = true;
        private float height = 0f;
        private Vector2 scrollPosition;
        public override void FillRect(Rect inRect)
        {
            UIUtility.MakeAndBeginScrollView(inRect, height, ref scrollPosition, out Listing_Standard list);

            DoSettingsButtons(list);
            list.GapLine();


            DoHeaderRow(list);
            list.GapLine();
            for(int i = 0; i < Rules.Rules.Count; i++)
            {
                RuleEntry entry = Rules.Rules.ElementAt(i);
                DoRuleRow(list, i, entry);
            }

            if(list.ButtonImage(UITextures.AddButtonTexture, UIUtility.ImageButtonWithLabelSize, UIUtility.ImageButtonWithLabelSize))
            {
                Rules.Rules.Add(new RuleEntry(new RuleTarget(), new VoreRule(RuleState.Copy)));
                Rules.NotifyStale();
                height += Text.LineHeight;
            }
            list.EndScrollView(ref height, ref heightStale);
        }

        private void DoSettingsButtons(Listing_Standard list)
        {
            string resetButtonLabel = "RV2_Settings_Rules_Reset".Translate();
            string savePresetLabel = "RV2_Settings_Rules_SavePreset".Translate();
            string loadPresetLabel = "RV2_Settings_Rules_LoadPreset".Translate();
            string removePresetLabel = "RV2_Settings_Rules_RemovePreset".Translate();
            Vector2 resetButtonSize = Text.CalcSize(UIUtility.ButtonStringPadding + resetButtonLabel + UIUtility.ButtonStringPadding);
            Vector2 savePresetSize = Text.CalcSize(UIUtility.ButtonStringPadding + savePresetLabel + UIUtility.ButtonStringPadding);
            Vector2 loadPresetSize = Text.CalcSize(UIUtility.ButtonStringPadding + loadPresetLabel + UIUtility.ButtonStringPadding);
            Vector2 removePresetSize = Text.CalcSize(UIUtility.ButtonStringPadding + removePresetLabel + UIUtility.ButtonStringPadding);
            float maxRequiredHeight = Mathf.Max(resetButtonSize.y, savePresetSize.y, loadPresetSize.y, removePresetSize.y);
            Rect rowRect = list.GetRect(maxRequiredHeight);
            float buttonPadding = 5f;
            Rect resetButtonRect = new Rect(rowRect.x, rowRect.y, resetButtonSize.x, rowRect.height);
            Rect savePresetRect = new Rect(resetButtonRect.x + resetButtonRect.width + buttonPadding, rowRect.y, savePresetSize.x, rowRect.height);
            Rect loadPresetRect = new Rect(savePresetRect.x + savePresetRect.width + buttonPadding, rowRect.y, loadPresetSize.x, rowRect.height);
            Rect removePresetRect = new Rect(loadPresetRect.x + loadPresetRect.width + buttonPadding, rowRect.y, removePresetSize.x, rowRect.height);
            if(Widgets.ButtonText(resetButtonRect, resetButtonLabel))
            {
                Rules.Reset();
            }
            if(Widgets.ButtonText(savePresetRect, savePresetLabel))
            {
                OpenSavePresetMenu();
            }
            if(Rules.Presets.Count >= 1)
            {
                if(Widgets.ButtonText(loadPresetRect, loadPresetLabel))
                {
                    OpenLoadPresetMenu();
                }
                if(Widgets.ButtonText(removePresetRect, removePresetLabel))
                {
                    OpenRemovePresetMenu();
                }
            }
        }
        private void DoHeaderRow(Listing_Standard list)
        {
            int fontSize = Text.CurFontStyle.fontSize;
            Text.CurFontStyle.fontSize = 18;
            int columnCount = ShouldDisplayPawnExplanation ? 4 : 3;
            string nameLabel = "RV2_Settings_Rules_Name".Translate();
            string controlsLabel = "RV2_Settings_Rules_Controls".Translate();
            float columnWidth = list.ColumnWidth / 3;
            float requiredHeight = Mathf.Max(Text.CalcHeight(nameLabel, columnWidth), Text.CalcHeight(controlsLabel, columnWidth));
            Rect rowRect = list.GetRect(requiredHeight);
            List<Rect> columns = UIUtility.CreateColumns(rowRect, columnCount, out _, 0, 0, 5f);
            Widgets.Label(columns[0], nameLabel);
            DoQuickAccessHeader(columns[1]);
            //Widgets.Label(columns[1], quickAccessLabel);
            Widgets.Label(columns[2], controlsLabel);
            Text.CurFontStyle.fontSize = fontSize;
            if(ShouldDisplayPawnExplanation)
            {
                DoPawnExplanationHeader(columns[3]);
            }
        }
        private void DoPawnExplanationHeader(Rect inRect)
        {
            List<Rect> columns = UIUtility.CreateColumns(inRect, 3, out _, 0, 0);
            UIUtility.TextureInCenter(columns[0], UIUtility.PortraitTexture(RuleTarget.PickedPawn, iconSize), out Rect portraitRect, iconSize);
            TooltipHandler.TipRegion(portraitRect, RuleTarget.PickedPawn.Label);
            UIUtility.TextureInCenter(columns[1], UITextures.PredatorIcon, out _, iconSize);
            UIUtility.TextureInCenter(columns[2], UITextures.PreyIcon, out _, iconSize);

        }
        private static void DoQuickAccessHeader(Rect inRect)
        {
            List<Texture2D> buttons = new List<Texture2D>() {
                ContentFinder<Texture2D>.Get("Widget/auto_vore")
            };
            List<string> buttonTooltips = new List<string>()
            {
                "RV2_Settings_Rules_AllowedToAutoVore".Translate()
            };
            foreach(RV2DesignationDef designation in RV2_Common.VoreDesignations)
            {
                buttons.Add(ContentFinder<Texture2D>.Get(designation.iconPathEnabledAutomatically));
                buttonTooltips.Add(designation.description);
            }
            UIUtility.ButtonImages(inRect, buttons, new List<Action>(), buttonTooltips, null, 24f);
        }

        private void DoRuleRow(Listing_Standard list, int index, RuleEntry entry)
        {
            string ruleLabel = (index + 1) + ": " + entry.Target.PresentationLabel;
            int columnCount = ShouldDisplayPawnExplanation ? 4 : 3;
            float rowHeight = Text.CalcHeight(ruleLabel, list.ColumnWidth / columnCount);
            Rect rowRect = list.GetRect(rowHeight);
            if(!entry.isEnabled)
            {
                Widgets.DrawRectFast(rowRect, Widgets.MenuSectionBGFillColor);
                TooltipHandler.TipRegion(rowRect, "RV2_Settings_RuleDisabled".Translate());
                ruleLabel = ruleLabel.Colorize(Color.grey);
            }
            List<Rect> columns = UIUtility.CreateColumns(rowRect, columnCount, out _, 0, 0, 5f);
            Widgets.Label(columns[0], ruleLabel);
            bool isFirstRule = index == 0;
            DoRuleQuickAccessButtons(columns[1], entry, isFirstRule);
            DoRuleControlButtons(columns[2], index, entry);
            if(ShouldDisplayPawnExplanation)
                DoRulePawnExplanation(columns[3], entry);
        }

        private void DoRuleQuickAccessButtons(Rect inRect, RuleEntry entry, bool isFirstRule = false)
        {
            RuleTarget target = entry.Target;
            VoreRule rule = entry.Rule;
            List<Texture2D> buttons = new List<Texture2D>();
            List<Action> buttonActions = new List<Action>();
            List<string> buttonTooltips = new List<string>();
            //bool displayPreyButtons = target.targetRole == IdentifierRole.Both || target.targetRole == IdentifierRole.Prey;
            //bool displayPredatorButtons = target.targetRole == IdentifierRole.Both || target.targetRole == IdentifierRole.Predator;

            // auto vore
            buttons.Add(GetStateTexture(rule.AllowedInAutoVore));
            buttonTooltips.Add(GetStateTooltip(rule.AllowedInAutoVore));
            // first rule cycles between ON and OFF, all other rules can also COPY
            if(isFirstRule)
            {
                if(rule.AllowedInAutoVore == RuleState.On)
                {
                    buttonActions.Add(delegate ()
                    {
                        rule.AllowedInAutoVore = RuleState.Off;
                        Rules.NotifyStale();
                    });
                }
                else
                {
                    buttonActions.Add(delegate ()
                    {
                        rule.AllowedInAutoVore = RuleState.On;
                        Rules.NotifyStale();
                    });
                }
            }
            else
            {
                buttonActions.Add(delegate ()
                {
                    rule.AllowedInAutoVore = rule.AllowedInAutoVore.Next();
                    Rules.NotifyStale();
                });
            }
            List<RV2DesignationDef> designations = RV2_Common.VoreDesignations;

            foreach(RV2DesignationDef designation in designations)
            {
                // if the current designation does not apply to the rule, add a blank space
                if(!target.AllowsDesignation(designation))
                {
                    buttons.Add(UITextures.BlankButtonTexture);
                    buttonActions.Add(() => { });
                    buttonTooltips.Add("");
                }
                // otherwise do the state button
                else
                {
                    string designationKey = designation.defName;
                    // take the rules current state
                    RuleState currentState = rule.DesignationStates.TryGetValue(designationKey, RuleState.On);
                    buttons.Add(GetStateTexture(currentState));
                    buttonTooltips.Add(GetStateTooltip(currentState));
                    // special handling for first rule, switch between ON and OFF
                    if(isFirstRule)
                    {
                        if(currentState == RuleState.On)
                        {
                            buttonActions.Add(delegate ()
                            {
                                rule.DesignationStates.SetOrAdd(designationKey, RuleState.Off);
                                Rules.NotifyStale();
                            });
                        }
                        else
                        {
                            buttonActions.Add(delegate ()
                            {
                                rule.DesignationStates.SetOrAdd(designationKey, RuleState.On);
                                Rules.NotifyStale();
                            });
                        }
                    }
                    // normal handling for other rules - cycle through enum
                    else
                    {
                        buttonActions.Add(delegate ()
                        {
                            rule.DesignationStates.SetOrAdd(designationKey, currentState.Next());
                            Rules.NotifyStale();
                        });
                    }
                }
            }
            UIUtility.ButtonImages(inRect, buttons, buttonActions, buttonTooltips, null, 24f);
        }

        public static void DoLabeledRuleStateControl(Listing_Standard list, string label, ref RuleState currentState, string tooltip = null)
        {
            string stateTooltip = GetStateTooltip(currentState);
            if(tooltip == null)
            {
                tooltip = stateTooltip;
            }
            else if(stateTooltip != null)
            {
                tooltip += "\n\n" + stateTooltip;
            }

            list.LabeledCheckbox(label, ref currentState, SettingsContainer_Rules.RuleStateIcons, tooltip, 24f);
        }

        public void DoRuleStateControl(Rect inRect, ref RuleState currentState, string tooltip = null)
        {
            UIUtility.Checkbox(inRect, ref currentState, SettingsContainer_Rules.RuleStateIcons, tooltip);
        }

        private static string GetStateTooltip(RuleState state)
        {
            switch(state)
            {
                case RuleState.Copy:
                    return "RV2_Settings_Rules_RuleStateCopy".Translate();
                case RuleState.Off:
                    return "RV2_Settings_Rules_RuleStateOff".Translate();
                case RuleState.On:
                    return "RV2_Settings_Rules_RuleStateOn".Translate();
                default:
                    return null;
            }
        }

        public Texture2D GetStateTexture(RuleState state)
        {
            switch(state)
            {
                case RuleState.Copy:
                    return UITextures.CopyTexture;
                case RuleState.Off:
                    return UITextures.CheckOffTexture;
                case RuleState.On:
                    return UITextures.CheckOnTexture;
                default:
                    return default(Texture2D);
            }
        }

        private void DoRuleControlButtons(Rect inRect, int index, RuleEntry entry)
        {
            RuleTarget target = entry.Target;
            VoreRule rule = entry.Rule;
            if(RV2Log.ShouldLog(false, "Settings"))
                RV2Log.Message($"{index}:current target: {target}", true, "Settings");
            Action moveDown = () => Rules.MoveRuleDown(index);
            Action moveUp = () => Rules.MoveRuleUp(index);
            Action toggleDisabled = () => entry.isEnabled = !entry.isEnabled;
            // the first entry must never be removed or moved!
            bool isFirstEntry = index == 0;
            Action edit = delegate ()
            {
                Find.WindowStack.Add(new Window_RuleEditor(index));
            };
            Action remove = delegate ()
            {
                Rules.Rules.Remove(entry);
                Rules.NotifyStale();
            };
            List<Texture2D> buttons = new List<Texture2D> { UITextures.EditButtonTexture };
            List<Action> buttonActions = new List<Action>() { edit };
            List<string> buttonTooltips = new List<string>() { "RV2_Settings_Edit".Translate() };
            // the first and second rule should not have a MoveUp button
            bool canMoveUp = index > 1;
            bool isLastEntry = index == Rules.Rules.Count - 1;
            bool canMoveDown = !isLastEntry && !isFirstEntry;
            if(!isFirstEntry)
            {
                buttons.Add(UITextures.DisableButtonTexture);
                buttonActions.Add(toggleDisabled);
                if(entry.isEnabled)
                {
                    buttonTooltips.Add("RV2_Settings_DisableRule".Translate());
                }
                else
                {
                    buttonTooltips.Add("RV2_Settings_EnableRule".Translate());
                }
            }
            if(canMoveUp)
            {
                buttons.Add(UITextures.MoveUpButtonTexture);
                buttonActions.Add(moveUp);
                buttonTooltips.Add("RV2_Settings_MoveUp".Translate());
            }
            if(canMoveDown)
            {
                buttons.Add(UITextures.MoveDownButtonTexture);
                buttonActions.Add(moveDown);
                buttonTooltips.Add("RV2_Settings_MoveDown".Translate());
            }
            if(!isFirstEntry)
            {
                buttons.Add(UITextures.RemoveButtonTexture);
                buttonActions.Add(remove);
                buttonTooltips.Add("RV2_Settings_Remove".Translate());
            }
            UIUtility.ButtonImages(inRect, buttons, buttonActions, buttonTooltips, null, 20f);
            //list.ButtonImages(buttons, buttonActions, buttonTooltips, null, (index + 1) + ". " + rule.Key.GetName());
        }

        private void DoRulePawnExplanation(Rect inRect, RuleEntry entry)
        {
            Pawn pawn = RuleTarget.PickedPawn;
            List<Rect> columns = UIUtility.CreateColumns(inRect, 3, out _, 0, 0, 12);
            // ignore first column, the header draws the pawn here, we don't need to re-do that every time
            DoRulePawnExplanationForRole(columns[1], pawn, entry, RuleTargetRole.Predator);
            DoRulePawnExplanationForRole(columns[2], pawn, entry, RuleTargetRole.Prey);
        }

        private void DoRulePawnExplanationForRole(Rect inRect, Pawn pawn, RuleEntry entry, RuleTargetRole role)
        {
            bool doesApply = entry.Target.AppliesTo(pawn, role);
            Texture contentTexture = doesApply ? UITextures.CheckOnTexture : UITextures.CheckOffTexture;
            string tip = doesApply ? "RV2_Settings_Rule_RuleExplanation_DoesApply" : "RV2_Settings_Rule_RuleExplanation_DoesNotApply";
            tip = tip.Translate(pawn.LabelShortCap);
            UIUtility.TextureInCenter(inRect, contentTexture, out Rect contentTextureRect, iconSize);
            TooltipHandler.TipRegion(contentTextureRect, tip);
        }

        #region preset menu
        private void OpenSavePresetMenu()
        {
            Action<string> save = delegate (string content)
            {
                if(string.IsNullOrEmpty(content))
                {
                    return;
                }
                Rules.Presets.SetOrAdd(content, new VoreRulePreset(Rules.Rules));
            };
            Action cancel = () => { };
            Window_CustomTextField textFieldWindow = new Window_CustomTextField("", save, cancel);
            Find.WindowStack.Add(textFieldWindow);
        }

        private void OpenLoadPresetMenu()
        {
            Action<string> load = delegate (string presetName)
            {
                if(RV2Log.ShouldLog(false, "Settings"))
                    RV2Log.Message($"Loading preset {presetName}", "Settings");
                if(Rules.Presets.ContainsKey(presetName))
                {
                    Rules.Presets[presetName].ApplyPreset();
                    Rules.NotifyStale();
                }
            };
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            foreach(string presetName in Rules.Presets.Keys.ToList())
            {
                options.Add(new FloatMenuOption(presetName, () => load(presetName)));
            }
            Find.WindowStack.Add(new FloatMenu(options));
        }

        private void OpenRemovePresetMenu()
        {
            Action<string> remove = delegate (string presetName)
            {
                if(RV2Log.ShouldLog(false, "Settings"))
                    RV2Log.Message($"Removing preset {presetName}", "Settings");
                if(Rules.Presets.ContainsKey(presetName))
                {
                    Rules.Presets.Remove(presetName);
                }
            };
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            foreach(string presetName in Rules.Presets.Keys.ToList())
            {
                options.Add(new FloatMenuOption(presetName, () => remove(presetName)));
            }
            Find.WindowStack.Add(new FloatMenu(options));
        }
        #endregion
    }
}
