using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Lazarus.Core.Game.Combat;
using Lazarus.Core.Game.Data;
using Lazarus.Core.Game.Entities;
using Lazarus.Core.Game.World;
using Lazarus.Core.Inputs;
using Lazarus.Core.Services;
using Lazarus.ScreenManagers;

namespace Lazarus.Screens;

/// <summary>
/// Combat screen for turn-based ATB battles.
/// </summary>
public class CombatScreen : GameScreen
{
    private readonly List<Kyn> _partyKyns;
    private readonly List<Kyn> _enemyKyns;
    private readonly Encounter? _encounter;
    private readonly Companion? _companion;
    private readonly GameStateService? _gameState;

    private CombatState _combatState = null!;
    private Texture2D? _pixelTexture;
    private SpriteFont? _font;

    // Companion intervention
    private float _companionInterventionTimer = 0f;
    private const float CompanionInterventionChance = 0.15f; // 15% chance per second
    private Random _random = new();

    // Input handling
    private KeyboardState _previousKeyboardState;

    /// <summary>
    /// Event fired when combat ends.
    /// </summary>
    public event EventHandler<CombatEndedEventArgs>? CombatEnded;

    public CombatScreen(List<Kyn> partyKyns, List<Kyn> enemyKyns, Encounter? encounter = null, Companion? companion = null, GameStateService? gameState = null)
    {
        _partyKyns = partyKyns;
        _enemyKyns = enemyKyns;
        _encounter = encounter;
        _companion = companion;
        _gameState = gameState;

        IsPopup = true; // Draw on top of world screen
        TransitionOnTime = TimeSpan.FromSeconds(0.3);
        TransitionOffTime = TimeSpan.FromSeconds(0.3);
    }

    public override void LoadContent()
    {
        base.LoadContent();

        // Create pixel texture
        _pixelTexture = new Texture2D(ScreenManager.GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });

        _font = ScreenManager.Font;

        // Initialize combat state
        _combatState = new CombatState();
        _combatState.Initialize(_partyKyns, _enemyKyns, _encounter);
        _combatState.CombatEnded += OnCombatStateEnded;
        _combatState.CompanionIntervened += OnCompanionIntervened;

        // Set companion-related combat state
        if (_companion != null)
        {
            _combatState.CompanionPresent = _companion.IsPresent;
            _combatState.GravitationStage = _companion.GravitationStage;
        }
        else
        {
            _combatState.CompanionPresent = false;
        }
    }

    public override void UnloadContent()
    {
        base.UnloadContent();
        _pixelTexture?.Dispose();
    }

    public override void HandleInput(GameTime gameTime, InputState input)
    {
        if (input == null)
            return;

        var keyboardState = Keyboard.GetState();

        switch (_combatState.Phase)
        {
            case CombatPhase.SelectingAction:
                HandleActionSelection(keyboardState);
                break;

            case CombatPhase.SelectingAbility:
                HandleAbilitySelection(keyboardState);
                break;

            case CombatPhase.SelectingTarget:
                HandleTargetSelection(keyboardState);
                break;
        }

        _previousKeyboardState = keyboardState;
    }

    private void HandleActionSelection(KeyboardState keyboardState)
    {
        var actions = _combatState.GetAvailableActions();

        // Navigate actions
        if (IsKeyPressed(keyboardState, Keys.Up) || IsKeyPressed(keyboardState, Keys.W))
        {
            _combatState.ActionIndex = (_combatState.ActionIndex - 1 + actions.Count) % actions.Count;
        }
        if (IsKeyPressed(keyboardState, Keys.Down) || IsKeyPressed(keyboardState, Keys.S))
        {
            _combatState.ActionIndex = (_combatState.ActionIndex + 1) % actions.Count;
        }

        // Select action
        if (IsKeyPressed(keyboardState, Keys.Enter) || IsKeyPressed(keyboardState, Keys.Space))
        {
            var selectedAction = actions[_combatState.ActionIndex];
            var actionType = selectedAction switch
            {
                "Attack" => CombatActionType.Attack,
                "Abilities" => CombatActionType.Ability,
                "Defend" => CombatActionType.Defend,
                "Flee" => CombatActionType.Flee,
                _ => CombatActionType.Attack
            };
            _combatState.SelectAction(actionType);
        }
    }

    private void HandleAbilitySelection(KeyboardState keyboardState)
    {
        var abilities = _combatState.GetAvailableAbilities();
        if (abilities.Count == 0)
        {
            _combatState.CancelAbilitySelection();
            return;
        }

        // Navigate abilities
        if (IsKeyPressed(keyboardState, Keys.Up) || IsKeyPressed(keyboardState, Keys.W))
        {
            _combatState.AbilityIndex = (_combatState.AbilityIndex - 1 + abilities.Count) % abilities.Count;
        }
        if (IsKeyPressed(keyboardState, Keys.Down) || IsKeyPressed(keyboardState, Keys.S))
        {
            _combatState.AbilityIndex = (_combatState.AbilityIndex + 1) % abilities.Count;
        }

        // Select ability
        if (IsKeyPressed(keyboardState, Keys.Enter) || IsKeyPressed(keyboardState, Keys.Space))
        {
            _combatState.SelectAbility(_combatState.AbilityIndex);
        }

        // Cancel
        if (IsKeyPressed(keyboardState, Keys.Escape) || IsKeyPressed(keyboardState, Keys.Back))
        {
            _combatState.CancelAbilitySelection();
        }
    }

    private void HandleTargetSelection(KeyboardState keyboardState)
    {
        // Navigate targets
        if (IsKeyPressed(keyboardState, Keys.Up) || IsKeyPressed(keyboardState, Keys.W))
        {
            _combatState.CycleTarget(-1);
        }
        if (IsKeyPressed(keyboardState, Keys.Down) || IsKeyPressed(keyboardState, Keys.S))
        {
            _combatState.CycleTarget(1);
        }

        // Confirm target
        if (IsKeyPressed(keyboardState, Keys.Enter) || IsKeyPressed(keyboardState, Keys.Space))
        {
            _combatState.ConfirmTarget();
        }

        // Cancel
        if (IsKeyPressed(keyboardState, Keys.Escape) || IsKeyPressed(keyboardState, Keys.Back))
        {
            _combatState.CancelTargetSelection();
        }
    }

    private bool IsKeyPressed(KeyboardState current, Keys key)
    {
        return current.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key);
    }

    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

        if (otherScreenHasFocus)
            return;

        // Update combat state
        _combatState.Update(gameTime);

        // Check for companion intervention
        if (_companion != null && _companion.IsPresent && _combatState.Phase == CombatPhase.Running)
        {
            CheckCompanionIntervention(gameTime);
        }
    }

    private void CheckCompanionIntervention(GameTime gameTime)
    {
        _companionInterventionTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_companionInterventionTimer >= 1f)
        {
            _companionInterventionTimer = 0f;

            if (_random.NextDouble() < CompanionInterventionChance)
            {
                // Companion intervenes with Gravitation
                var stage = _companion!.GravitationStage;
                var allyTargetChance = stage.GetAllyTargetChance();
                var targetsAlly = _random.NextDouble() < allyTargetChance;

                _combatState.TriggerGravitation(stage, targetsAlly);
            }
        }
    }

    private void OnCombatStateEnded(object? sender, CombatPhase phase)
    {
        // Fire event immediately - caller (WorldScreen) will show VictoryScreen
        var args = new CombatEndedEventArgs
        {
            Victory = phase == CombatPhase.Victory,
            Defeat = phase == CombatPhase.Defeat,
            Fled = phase == CombatPhase.Fled,
            ExperienceEarned = _combatState.ExperienceEarned,
            CurrencyEarned = _combatState.CurrencyEarned,
            TelemetryUnitsEarned = _combatState.TelemetryUnitsEarned,
            RecruitedKyn = _combatState.RecruitableKyn
        };

        CombatEnded?.Invoke(this, args);
        ExitScreen();
    }

    private void OnCompanionIntervened(object? sender, GravitationInterventionEventArgs e)
    {
        // Record use and check for escalation
        bool escalated = false;
        if (_gameState != null)
        {
            escalated = _gameState.RecordGravitationUse();

            // Update combat state with new stage if escalated
            if (escalated)
            {
                _combatState.GravitationStage = _gameState.GravitationStage;
            }

            // Check for companion departure at Critical stage hitting ally
            if (e.HitAlly && e.Stage == GravitationStage.Critical && e.Target != null)
            {
                var targetKyn = e.Target.Kyn;
                if (_gameState.CheckCriticalGravitationDeparture(targetKyn.CurrentHp, targetKyn.MaxHp))
                {
                    // Companion departure triggered!
                    _combatState.CompanionPresent = false;
                    System.Diagnostics.Debug.WriteLine($"[Combat] COMPANION DEPARTURE TRIGGERED! {targetKyn.DisplayName} nearly killed!");
                }
            }
        }

        // Show intervention message based on stage
        string companionName = _companion?.Name ?? "Companion";
        string message;

        if (e.HitAlly)
        {
            message = e.Stage switch
            {
                GravitationStage.Unstable => $"{companionName}'s Gravitation wavers... hits ally!",
                GravitationStage.Dangerous => $"{companionName} loses control! Ally targeted!",
                GravitationStage.Critical => $"{companionName} CAN'T STOP! ALLY HIT!",
                _ => $"{companionName} uses Gravitation... but hits wrong target!"
            };
        }
        else
        {
            message = e.Stage switch
            {
                GravitationStage.Normal => $"{companionName} uses Gravitation!",
                GravitationStage.Unstable => $"{companionName}'s unstable Gravitation strikes!",
                GravitationStage.Dangerous => $"{companionName} unleashes dangerous Gravitation!",
                GravitationStage.Critical => $"{companionName}'s CRITICAL Gravitation!!",
                _ => $"{companionName} uses Gravitation!"
            };
        }

        // Log the intervention
        System.Diagnostics.Debug.WriteLine($"[Combat] {message} (Damage: {e.DamagePercent * 100}%)");

        // Log escalation warning
        if (escalated)
        {
            System.Diagnostics.Debug.WriteLine($"[Combat] WARNING: Gravitation has escalated to {_gameState?.GravitationStage}!");
        }
    }

    public override void Draw(GameTime gameTime)
    {
        var spriteBatch = ScreenManager.SpriteBatch;

        // Draw semi-transparent background
        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            null, null, null,
            ScreenManager.GlobalTransformation
        );

        // Background overlay
        var screenRect = new Rectangle(0, 0, (int)ScreenManager.BaseScreenSize.X, (int)ScreenManager.BaseScreenSize.Y);
        spriteBatch.Draw(_pixelTexture, screenRect, Color.Black * 0.8f);

        // Draw combat arena background
        var arenaRect = new Rectangle(50, 50, (int)ScreenManager.BaseScreenSize.X - 100, (int)ScreenManager.BaseScreenSize.Y - 150);
        spriteBatch.Draw(_pixelTexture, arenaRect, new Color(30, 30, 40));

        // Draw combatants
        DrawCombatants(spriteBatch);

        // Draw UI
        DrawCombatUI(spriteBatch);

        // Draw action result
        if (_combatState.LastResult != null)
        {
            DrawActionResult(spriteBatch, _combatState.LastResult);
        }

        spriteBatch.End();
    }

    private void DrawCombatants(SpriteBatch spriteBatch)
    {
        // Draw party combatants
        foreach (var combatant in _combatState.Party)
        {
            bool isSelected = combatant == _combatState.ActiveCombatant;
            combatant.Draw(spriteBatch, _pixelTexture!, _font, isSelected);
        }

        // Draw enemy combatants
        foreach (var combatant in _combatState.Enemies)
        {
            bool isTargeted = combatant == _combatState.TargetedCombatant;
            combatant.Draw(spriteBatch, _pixelTexture!, _font, isTargeted);
        }
    }

    private void DrawCombatUI(SpriteBatch spriteBatch)
    {
        if (_font == null || _pixelTexture == null)
            return;

        // Draw action menu during selection
        if (_combatState.Phase == CombatPhase.SelectingAction && _combatState.ActiveCombatant != null)
        {
            DrawActionMenu(spriteBatch);
        }

        // Draw ability menu during ability selection
        if (_combatState.Phase == CombatPhase.SelectingAbility && _combatState.ActiveCombatant != null)
        {
            DrawAbilityMenu(spriteBatch);
        }

        // Draw target indicator during targeting
        if (_combatState.Phase == CombatPhase.SelectingTarget)
        {
            var targetText = _combatState.SelectedAbility != null
                ? $"Select Target for {_combatState.SelectedAbility.Definition.Name} (Enter/Esc)"
                : "Select Target (Enter to confirm, Esc to cancel)";
            var targetPos = new Vector2(
                ScreenManager.BaseScreenSize.X / 2 - _font.MeasureString(targetText).X / 2,
                ScreenManager.BaseScreenSize.Y - 30
            );
            spriteBatch.DrawString(_font, targetText, targetPos, Color.Yellow);
        }

        // Draw combat phase indicator
        var phaseText = _combatState.Phase switch
        {
            CombatPhase.Victory => "VICTORY!",
            CombatPhase.Defeat => "DEFEAT...",
            CombatPhase.Fled => "Escaped!",
            _ => ""
        };

        if (!string.IsNullOrEmpty(phaseText))
        {
            var phaseColor = _combatState.Phase == CombatPhase.Victory ? Color.Gold :
                            _combatState.Phase == CombatPhase.Defeat ? Color.Red : Color.White;
            var phasePos = new Vector2(
                ScreenManager.BaseScreenSize.X / 2 - _font.MeasureString(phaseText).X / 2,
                ScreenManager.BaseScreenSize.Y / 2 - 40
            );
            spriteBatch.DrawString(_font, phaseText, phasePos, phaseColor);

            // Show experience, currency and TU if victory
            if (_combatState.Phase == CombatPhase.Victory)
            {
                // Show EXP and Currency on same line
                var rewardsText = $"EXP: +{_combatState.ExperienceEarned}   Currency: +{_combatState.CurrencyEarned}";
                var rewardsPos = new Vector2(
                    ScreenManager.BaseScreenSize.X / 2 - _font.MeasureString(rewardsText).X / 2,
                    ScreenManager.BaseScreenSize.Y / 2 - 10
                );
                spriteBatch.DrawString(_font, rewardsText, rewardsPos, Color.Cyan);

                // Show TU earned
                if (_combatState.TelemetryUnitsEarned > 0)
                {
                    var tuText = $"TU: +{_combatState.TelemetryUnitsEarned}";
                    var tuPos = new Vector2(
                        ScreenManager.BaseScreenSize.X / 2 - _font.MeasureString(tuText).X / 2,
                        ScreenManager.BaseScreenSize.Y / 2 + 15
                    );
                    spriteBatch.DrawString(_font, tuText, tuPos, Color.LimeGreen);
                }

                // Show chip level-ups
                if (_combatState.ChipsLeveledUp.Count > 0)
                {
                    var levelUpY = ScreenManager.BaseScreenSize.Y / 2 + 45;
                    foreach (var (kynName, chipName, newLevel) in _combatState.ChipsLeveledUp)
                    {
                        var levelUpText = $"{kynName}'s {chipName} -> {newLevel}!";
                        var levelUpPos = new Vector2(
                            ScreenManager.BaseScreenSize.X / 2 - _font.MeasureString(levelUpText).X / 2,
                            levelUpY
                        );
                        spriteBatch.DrawString(_font, levelUpText, levelUpPos, Color.Yellow);
                        levelUpY += 20;
                    }
                }
            }
        }

        // Draw companion indicator
        if (_companion != null && _companion.IsPresent)
        {
            var companionText = $"{_companion.Name} watches...";
            var companionPos = new Vector2(10, ScreenManager.BaseScreenSize.Y - 60);
            spriteBatch.DrawString(_font, companionText, companionPos, Color.Orange);
        }
    }

    private void DrawActionMenu(SpriteBatch spriteBatch)
    {
        if (_font == null || _pixelTexture == null)
            return;

        var actions = _combatState.GetAvailableActions();
        var menuX = 60;
        var menuY = (int)ScreenManager.BaseScreenSize.Y - 100;
        var menuWidth = 120;
        var menuHeight = actions.Count * 25 + 10;

        // Menu background
        var menuRect = new Rectangle(menuX, menuY, menuWidth, menuHeight);
        spriteBatch.Draw(_pixelTexture, menuRect, new Color(40, 40, 60));

        // Menu border
        var borderRect = new Rectangle(menuX - 2, menuY - 2, menuWidth + 4, menuHeight + 4);
        spriteBatch.Draw(_pixelTexture, new Rectangle(borderRect.X, borderRect.Y, borderRect.Width, 2), Color.White);
        spriteBatch.Draw(_pixelTexture, new Rectangle(borderRect.X, borderRect.Y, 2, borderRect.Height), Color.White);
        spriteBatch.Draw(_pixelTexture, new Rectangle(borderRect.X, borderRect.Bottom - 2, borderRect.Width, 2), Color.White);
        spriteBatch.Draw(_pixelTexture, new Rectangle(borderRect.Right - 2, borderRect.Y, 2, borderRect.Height), Color.White);

        // Menu title
        var title = _combatState.ActiveCombatant?.Name ?? "Action";
        spriteBatch.DrawString(_font, title, new Vector2(menuX + 5, menuY - 20), Color.Cyan);

        // Actions
        for (int i = 0; i < actions.Count; i++)
        {
            var action = actions[i];
            var isSelected = i == _combatState.ActionIndex;
            var color = isSelected ? Color.Yellow : Color.White;
            var prefix = isSelected ? "> " : "  ";

            var textPos = new Vector2(menuX + 5, menuY + 5 + i * 25);
            spriteBatch.DrawString(_font, prefix + action, textPos, color);
        }

        // Draw energy bar
        if (_combatState.ActiveCombatant != null)
        {
            var energyText = $"EP: {_combatState.ActiveCombatant.CurrentEnergy}/{_combatState.ActiveCombatant.MaxEnergy}";
            var energyPos = new Vector2(menuX + 5, menuY + menuHeight + 5);
            spriteBatch.DrawString(_font, energyText, energyPos, Color.Cyan);
        }
    }

    private void DrawAbilityMenu(SpriteBatch spriteBatch)
    {
        if (_font == null || _pixelTexture == null || _combatState.ActiveCombatant == null)
            return;

        var abilities = _combatState.GetAvailableAbilities();
        var combatant = _combatState.ActiveCombatant;
        var menuX = 60;
        var menuY = (int)ScreenManager.BaseScreenSize.Y - 220;
        var menuWidth = 280;
        var menuHeight = Math.Min(abilities.Count, 6) * 28 + 50;

        // Menu background
        var menuRect = new Rectangle(menuX, menuY, menuWidth, menuHeight);
        spriteBatch.Draw(_pixelTexture, menuRect, new Color(40, 40, 60));

        // Menu border
        var borderRect = new Rectangle(menuX - 2, menuY - 2, menuWidth + 4, menuHeight + 4);
        spriteBatch.Draw(_pixelTexture, new Rectangle(borderRect.X, borderRect.Y, borderRect.Width, 2), Color.White);
        spriteBatch.Draw(_pixelTexture, new Rectangle(borderRect.X, borderRect.Y, 2, borderRect.Height), Color.White);
        spriteBatch.Draw(_pixelTexture, new Rectangle(borderRect.X, borderRect.Bottom - 2, borderRect.Width, 2), Color.White);
        spriteBatch.Draw(_pixelTexture, new Rectangle(borderRect.Right - 2, borderRect.Y, 2, borderRect.Height), Color.White);

        // Menu title
        var title = "Abilities (Esc to cancel)";
        spriteBatch.DrawString(_font, title, new Vector2(menuX + 5, menuY - 20), Color.Cyan);

        // Abilities list
        var currentEnergy = combatant.CurrentEnergy;
        for (int i = 0; i < Math.Min(abilities.Count, 6); i++)
        {
            var ability = abilities[i];
            var def = ability.Definition;
            var isSelected = i == _combatState.AbilityIndex;
            var isOverheated = combatant.IsAbilityOverheated(def.Id);
            var canUse = ability.IsReady && def.EnergyCost <= currentEnergy && !isOverheated;

            var color = isOverheated ? Color.OrangeRed :
                       isSelected ? Color.Yellow :
                       !canUse ? Color.Gray : Color.White;
            var prefix = isSelected ? "> " : "  ";

            // Ability name
            var text = $"{prefix}{def.Name}";
            var textPos = new Vector2(menuX + 5, menuY + 5 + i * 28);
            spriteBatch.DrawString(_font, text, textPos, color);

            // Energy cost
            var costText = $"{def.EnergyCost}E";
            if (ability.CurrentCooldown > 0)
            {
                costText = $"CD:{ability.CurrentCooldown}";
            }
            var costPos = new Vector2(menuX + 160, textPos.Y);
            spriteBatch.DrawString(_font, costText, costPos, def.EnergyCost <= currentEnergy ? Color.Cyan : Color.Gray);

            // Heat indicator for chip abilities
            var (heatCurrent, heatMax) = combatant.GetAbilityHeat(def.Id);
            if (heatMax > 0)
            {
                // Draw mini heat bar
                var heatBarX = (int)(menuX + 210);
                var heatBarY = (int)(textPos.Y + 4);
                var heatBarWidth = 50;
                var heatBarHeight = 8;

                // Background
                spriteBatch.Draw(_pixelTexture, new Rectangle(heatBarX, heatBarY, heatBarWidth, heatBarHeight), Color.DarkGray);

                // Fill
                var heatPercent = heatCurrent / heatMax;
                var heatColor = heatPercent >= 1f ? Color.Red :
                               heatPercent > 0.7f ? Color.OrangeRed :
                               heatPercent > 0.4f ? Color.Orange : Color.Yellow;
                spriteBatch.Draw(_pixelTexture, new Rectangle(heatBarX, heatBarY, (int)(heatBarWidth * heatPercent), heatBarHeight), heatColor);

                // Overheat indicator
                if (isOverheated)
                {
                    spriteBatch.DrawString(_font, "!", new Vector2(heatBarX + heatBarWidth + 2, textPos.Y), Color.Red);
                }
            }
        }

        // Draw energy bar at bottom
        var energyBarY = menuY + menuHeight - 40;
        DrawEnergyBar(spriteBatch, menuX + 5, energyBarY, menuWidth - 10, combatant.CurrentEnergy, combatant.MaxEnergy);

        // Draw selected ability description
        if (_combatState.AbilityIndex < abilities.Count)
        {
            var selectedAbility = abilities[_combatState.AbilityIndex];
            var descText = selectedAbility.Definition.Description;
            if (descText.Length > 35)
                descText = descText.Substring(0, 32) + "...";

            var descPos = new Vector2(menuX + menuWidth + 10, menuY + 5);
            spriteBatch.DrawString(_font, descText, descPos, Color.LightGray);

            // Show element and target type
            var elementText = $"Element: {selectedAbility.Definition.Element}";
            var targetText = $"Target: {selectedAbility.Definition.Target}";
            spriteBatch.DrawString(_font, elementText, new Vector2(descPos.X, descPos.Y + 20), GetElementColor(selectedAbility.Definition.Element));
            spriteBatch.DrawString(_font, targetText, new Vector2(descPos.X, descPos.Y + 40), Color.LightGray);

            // Show heat info for chip abilities
            var (heat, maxHeat) = combatant.GetAbilityHeat(selectedAbility.Definition.Id);
            if (maxHeat > 0)
            {
                var heatText = $"Heat: {(int)heat}/{(int)maxHeat}";
                var heatColor = combatant.IsAbilityOverheated(selectedAbility.Definition.Id) ? Color.Red : Color.Orange;
                spriteBatch.DrawString(_font, heatText, new Vector2(descPos.X, descPos.Y + 60), heatColor);
            }
        }
    }

    private void DrawEnergyBar(SpriteBatch spriteBatch, int x, int y, int width, int current, int max)
    {
        if (_pixelTexture == null || _font == null)
            return;

        // Background
        spriteBatch.Draw(_pixelTexture, new Rectangle(x, y, width, 16), new Color(20, 20, 40));

        // Fill
        var fillWidth = max > 0 ? (int)(width * ((float)current / max)) : 0;
        var energyColor = current > max * 0.5f ? Color.DeepSkyBlue :
                         current > max * 0.25f ? Color.DodgerBlue : Color.Blue;
        spriteBatch.Draw(_pixelTexture, new Rectangle(x, y, fillWidth, 16), energyColor);

        // Border
        spriteBatch.Draw(_pixelTexture, new Rectangle(x, y, width, 2), Color.White * 0.5f);
        spriteBatch.Draw(_pixelTexture, new Rectangle(x, y + 14, width, 2), Color.White * 0.5f);
        spriteBatch.Draw(_pixelTexture, new Rectangle(x, y, 2, 16), Color.White * 0.5f);
        spriteBatch.Draw(_pixelTexture, new Rectangle(x + width - 2, y, 2, 16), Color.White * 0.5f);

        // Text
        var text = $"EP: {current}/{max}";
        var textPos = new Vector2(x + width / 2 - _font.MeasureString(text).X / 2, y);
        spriteBatch.DrawString(_font, text, textPos, Color.White);
    }

    private Color GetElementColor(Element element)
    {
        return element switch
        {
            Element.Electric => Color.Yellow,
            Element.Fire => Color.OrangeRed,
            Element.Ice => Color.LightBlue,
            Element.Toxic => Color.Purple,
            Element.Kinetic => Color.Gray,
            Element.Psionic => Color.Magenta,
            Element.Corruption => Color.DarkMagenta,
            _ => Color.White
        };
    }

    private void DrawActionResult(SpriteBatch spriteBatch, CombatActionResult result)
    {
        if (_font == null || _pixelTexture == null)
            return;

        var message = result.Message;
        var messagePos = new Vector2(
            ScreenManager.BaseScreenSize.X / 2 - _font.MeasureString(message).X / 2,
            80
        );

        // Background for message
        var bgRect = new Rectangle(
            (int)messagePos.X - 10,
            (int)messagePos.Y - 5,
            (int)_font.MeasureString(message).X + 20,
            30
        );
        spriteBatch.Draw(_pixelTexture, bgRect, Color.Black * 0.7f);

        // Message text
        var color = result.Action.Type == CombatActionType.Gravitation ? Color.Magenta :
                   result.WasCritical ? Color.Yellow :
                   result.CausedDefeat ? Color.Red : Color.White;

        spriteBatch.DrawString(_font, message, messagePos, color);
    }
}
