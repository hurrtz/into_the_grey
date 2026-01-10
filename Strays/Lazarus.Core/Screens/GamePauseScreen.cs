using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Lazarus.Core;
using Lazarus.Core.Game.Entities;
using Lazarus.Core.Game.Progression;
using Lazarus.Core.Inputs;
using Lazarus.Core.Localization;
using Lazarus.Core.Services;
using Lazarus.ScreenManagers;

namespace Lazarus.Screens;

/// <summary>
/// In-game pause menu with access to party, inventory, factions, and save/load.
/// </summary>
class GamePauseScreen : GameScreen
{
    private readonly StrayRoster _roster;
    private readonly GameStateService _gameState;
    private readonly FactionReputation _factionReputation;

    private SpriteBatch? _spriteBatch;
    private SpriteFont? _font;
    private Texture2D? _pixelTexture;

    private int _selectedIndex;
    private float _selectionPulse;
    private float _transitionAlpha;

    private readonly List<PauseMenuItem> _menuItems = new();

    // Modern UI colors
    private static readonly Color BackgroundColor = new(15, 18, 25, 230);
    private static readonly Color PanelColor = new(25, 30, 40);
    private static readonly Color SelectedColor = new(50, 80, 130);
    private static readonly Color TextColor = new(220, 225, 235);
    private static readonly Color DimTextColor = new(140, 150, 170);
    private static readonly Color AccentColor = new(80, 160, 255);
    private static readonly Color TitleColor = new(80, 160, 255);

    private class PauseMenuItem
    {
        public string Text { get; init; } = "";
        public Action<PlayerIndex>? Action { get; init; }
    }

    /// <summary>
    /// Creates a new game pause screen.
    /// </summary>
    public GamePauseScreen(StrayRoster roster, GameStateService gameState, FactionReputation factionReputation)
    {
        _roster = roster;
        _gameState = gameState;
        _factionReputation = factionReputation;

        IsPopup = true;
        TransitionOnTime = TimeSpan.FromSeconds(0.2);
        TransitionOffTime = TimeSpan.FromSeconds(0.15);

        // Create menu items
        _menuItems.Add(new PauseMenuItem { Text = "Party", Action = OnParty });
        _menuItems.Add(new PauseMenuItem { Text = "Inventory", Action = OnInventory });
        _menuItems.Add(new PauseMenuItem { Text = "Factions", Action = OnFactions });
        _menuItems.Add(new PauseMenuItem { Text = "Save Game", Action = OnSave });
        _menuItems.Add(new PauseMenuItem { Text = "Settings", Action = OnSettings });
        _menuItems.Add(new PauseMenuItem { Text = "Resume", Action = _ => ExitScreen() });
        _menuItems.Add(new PauseMenuItem { Text = "Quit", Action = OnQuit });
    }

    public override void LoadContent()
    {
        base.LoadContent();

        _spriteBatch = ScreenManager.SpriteBatch;
        _font = ScreenManager.Font;

        _pixelTexture = new Texture2D(ScreenManager.GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });
    }

    public override void UnloadContent()
    {
        base.UnloadContent();
        _pixelTexture?.Dispose();
        _pixelTexture = null;
    }

    public override void HandleInput(GameTime gameTime, InputState input)
    {
        base.HandleInput(gameTime, input);

        // Navigate up
        if (input.IsMenuUp(ControllingPlayer))
        {
            _selectedIndex--;
            if (_selectedIndex < 0)
                _selectedIndex = _menuItems.Count - 1;
        }

        // Navigate down
        if (input.IsMenuDown(ControllingPlayer))
        {
            _selectedIndex++;
            if (_selectedIndex >= _menuItems.Count)
                _selectedIndex = 0;
        }

        // Select
        PlayerIndex playerIndex;
        if (input.IsMenuSelect(ControllingPlayer, out playerIndex))
        {
            _menuItems[_selectedIndex].Action?.Invoke(playerIndex);
        }

        // Cancel (resume)
        if (input.IsMenuCancel(ControllingPlayer, out playerIndex))
        {
            ExitScreen();
        }
    }

    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

        _transitionAlpha = TransitionAlpha;
        _selectionPulse += (float)gameTime.ElapsedGameTime.TotalSeconds * 3f;
    }

    public override void Draw(GameTime gameTime)
    {
        if (_spriteBatch == null || _font == null || _pixelTexture == null) return;

        var viewport = ScreenManager.GraphicsDevice.Viewport;

        _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, ScreenManager.GlobalTransformation);

        // Full screen dark overlay
        _spriteBatch.Draw(_pixelTexture, new Rectangle(0, 0, (int)ScreenManager.BaseScreenSize.X, (int)ScreenManager.BaseScreenSize.Y),
            Color.Black * 0.7f * _transitionAlpha);

        // Menu panel
        int panelWidth = 280;
        int panelHeight = 50 + _menuItems.Count * 45;
        int panelX = ((int)ScreenManager.BaseScreenSize.X - panelWidth) / 2;
        int panelY = ((int)ScreenManager.BaseScreenSize.Y - panelHeight) / 2;

        // Panel background
        DrawRect(new Rectangle(panelX - 2, panelY - 2, panelWidth + 4, panelHeight + 4), AccentColor * 0.5f * _transitionAlpha);
        DrawRect(new Rectangle(panelX, panelY, panelWidth, panelHeight), PanelColor * _transitionAlpha);

        // Title
        string title = "PAUSED";
        var titleSize = _font.MeasureString(title);
        _spriteBatch.DrawString(_font, title,
            new Vector2(panelX + (panelWidth - titleSize.X) / 2, panelY + 12),
            TitleColor * _transitionAlpha);

        // Menu items
        int itemY = panelY + 50;
        int itemHeight = 40;

        for (int i = 0; i < _menuItems.Count; i++)
        {
            bool isSelected = i == _selectedIndex;
            var item = _menuItems[i];

            // Selection highlight
            if (isSelected)
            {
                float pulse = 0.8f + 0.2f * (float)Math.Sin(_selectionPulse);
                DrawRect(new Rectangle(panelX + 10, itemY, panelWidth - 20, itemHeight - 5),
                    SelectedColor * pulse * _transitionAlpha);
            }

            // Item text
            var textSize = _font.MeasureString(item.Text);
            Color textColor = isSelected ? Color.White : TextColor;
            _spriteBatch.DrawString(_font, item.Text,
                new Vector2(panelX + (panelWidth - textSize.X) / 2, itemY + 8),
                textColor * _transitionAlpha);

            itemY += 45;
        }

        // Footer hint
        string hint = "[Up/Down] Navigate   [Enter] Select   [Esc] Resume";
        var hintSize = _font.MeasureString(hint);
        _spriteBatch.DrawString(_font, hint,
            new Vector2((ScreenManager.BaseScreenSize.X - hintSize.X) / 2, ScreenManager.BaseScreenSize.Y - 30),
            DimTextColor * 0.6f * _transitionAlpha);

        _spriteBatch.End();
    }

    private void DrawRect(Rectangle rect, Color color)
    {
        if (_pixelTexture != null)
            _spriteBatch!.Draw(_pixelTexture, rect, color);
    }

    private void OnParty(PlayerIndex playerIndex)
    {
        var partyScreen = new PartyScreen(_roster, _gameState);
        ScreenManager.AddScreen(partyScreen, ControllingPlayer);
    }

    private void OnInventory(PlayerIndex playerIndex)
    {
        var inventoryScreen = new InventoryScreen(
            _roster,
            _gameState.Data.OwnedMicrochips,
            _gameState.Data.OwnedAugmentations,
            _gameState.Data.InventoryItems);
        ScreenManager.AddScreen(inventoryScreen, ControllingPlayer);
    }

    private void OnFactions(PlayerIndex playerIndex)
    {
        var factionScreen = new FactionScreen(_factionReputation);
        ScreenManager.AddScreen(factionScreen, ControllingPlayer);
    }

    private void OnSave(PlayerIndex playerIndex)
    {
        var saveScreen = new SaveLoadScreen(SaveLoadMode.Save);
        ScreenManager.AddScreen(saveScreen, ControllingPlayer);
    }

    private void OnSettings(PlayerIndex playerIndex)
    {
        var settingsScreen = new SettingsMenuScreen();
        ScreenManager.AddScreen(settingsScreen, ControllingPlayer);
    }

    private void OnQuit(PlayerIndex playerIndex)
    {
        string message = Resources.QuitQuestion;
        var confirmQuit = new MessageBoxScreen(message);
        confirmQuit.Accepted += ConfirmQuitAccepted;
        ScreenManager.AddScreen(confirmQuit, ControllingPlayer);
    }

    private void ConfirmQuitAccepted(object? sender, PlayerIndexEventArgs e)
    {
        // Offer to save before quitting
        var savePrompt = new MessageBoxScreen("Save before quitting?");
        savePrompt.Accepted += (s, args) =>
        {
            _gameState.Save(_gameState.Data.SaveSlot);
            LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(), new MainMenuScreen());
        };
        savePrompt.Cancelled += (s, args) =>
        {
            LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(), new MainMenuScreen());
        };
        ScreenManager.AddScreen(savePrompt, ControllingPlayer);
    }
}
