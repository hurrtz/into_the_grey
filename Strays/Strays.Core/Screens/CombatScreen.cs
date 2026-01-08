using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Strays.Core.Game.Combat;
using Strays.Core.Game.Data;
using Strays.Core.Game.Entities;
using Strays.Core.Game.World;
using Strays.Core.Inputs;
using Strays.ScreenManagers;

namespace Strays.Screens;

/// <summary>
/// Combat screen for turn-based ATB battles.
/// </summary>
public class CombatScreen : GameScreen
{
    private readonly List<Stray> _partyStrays;
    private readonly List<Stray> _enemyStrays;
    private readonly Encounter? _encounter;
    private readonly Companion? _companion;

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

    public CombatScreen(List<Stray> partyStrays, List<Stray> enemyStrays, Encounter? encounter = null, Companion? companion = null)
    {
        _partyStrays = partyStrays;
        _enemyStrays = enemyStrays;
        _encounter = encounter;
        _companion = companion;

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
        _combatState.Initialize(_partyStrays, _enemyStrays, _encounter);
        _combatState.CombatEnded += OnCombatStateEnded;
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
        // Wait a moment before closing
        System.Threading.Tasks.Task.Delay(1500).ContinueWith(_ =>
        {
            var args = new CombatEndedEventArgs
            {
                Victory = phase == CombatPhase.Victory,
                Defeat = phase == CombatPhase.Defeat,
                Fled = phase == CombatPhase.Fled,
                ExperienceEarned = _combatState.ExperienceEarned,
                RecruitedStray = _combatState.RecruitableStray
            };

            CombatEnded?.Invoke(this, args);
            ExitScreen();
        });
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
                ScreenManager.BaseScreenSize.Y / 2 - 20
            );
            spriteBatch.DrawString(_font, phaseText, phasePos, phaseColor);

            // Show experience if victory
            if (_combatState.Phase == CombatPhase.Victory)
            {
                var expText = $"EXP: {_combatState.ExperienceEarned}";
                var expPos = new Vector2(
                    ScreenManager.BaseScreenSize.X / 2 - _font.MeasureString(expText).X / 2,
                    ScreenManager.BaseScreenSize.Y / 2 + 10
                );
                spriteBatch.DrawString(_font, expText, expPos, Color.Cyan);
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
        var menuX = 60;
        var menuY = (int)ScreenManager.BaseScreenSize.Y - 200;
        var menuWidth = 250;
        var menuHeight = Math.Min(abilities.Count, 6) * 25 + 30;

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
        var currentEnergy = _combatState.ActiveCombatant.CurrentEnergy;
        for (int i = 0; i < Math.Min(abilities.Count, 6); i++)
        {
            var ability = abilities[i];
            var def = ability.Definition;
            var isSelected = i == _combatState.AbilityIndex;
            var canUse = ability.IsReady && def.EnergyCost <= currentEnergy;

            var color = isSelected ? Color.Yellow :
                       !canUse ? Color.Gray : Color.White;
            var prefix = isSelected ? "> " : "  ";

            // Ability name and cost
            var text = $"{prefix}{def.Name}";
            var costText = $"[{def.EnergyCost} EP]";

            // Add cooldown indicator
            if (ability.CurrentCooldown > 0)
            {
                costText = $"[CD:{ability.CurrentCooldown}]";
            }

            var textPos = new Vector2(menuX + 5, menuY + 5 + i * 25);
            spriteBatch.DrawString(_font, text, textPos, color);

            var costPos = new Vector2(menuX + menuWidth - _font.MeasureString(costText).X - 10, textPos.Y);
            spriteBatch.DrawString(_font, costText, costPos, canUse ? Color.Cyan : Color.Gray);
        }

        // Draw energy bar at bottom
        var energyText = $"Energy: {currentEnergy}/{_combatState.ActiveCombatant.MaxEnergy}";
        var energyPos = new Vector2(menuX + 5, menuY + menuHeight - 22);
        spriteBatch.DrawString(_font, energyText, energyPos, Color.Cyan);

        // Draw selected ability description
        if (_combatState.AbilityIndex < abilities.Count)
        {
            var selectedAbility = abilities[_combatState.AbilityIndex];
            var descText = selectedAbility.Definition.Description;
            if (descText.Length > 40)
                descText = descText.Substring(0, 37) + "...";

            var descPos = new Vector2(menuX + menuWidth + 10, menuY + 5);
            spriteBatch.DrawString(_font, descText, descPos, Color.LightGray);

            // Show element and target type
            var elementText = $"Element: {selectedAbility.Definition.Element}";
            var targetText = $"Target: {selectedAbility.Definition.Target}";
            spriteBatch.DrawString(_font, elementText, new Vector2(descPos.X, descPos.Y + 20), GetElementColor(selectedAbility.Definition.Element));
            spriteBatch.DrawString(_font, targetText, new Vector2(descPos.X, descPos.Y + 40), Color.LightGray);
        }
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
