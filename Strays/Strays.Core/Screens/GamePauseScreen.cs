using System;
using Microsoft.Xna.Framework;
using Strays.Core.Game.Entities;
using Strays.Core.Game.Progression;
using Strays.Core.Localization;
using Strays.Core.Services;

namespace Strays.Screens;

/// <summary>
/// In-game pause menu with access to party, inventory, factions, and save/load.
/// </summary>
class GamePauseScreen : MenuScreen
{
    private readonly StrayRoster _roster;
    private readonly GameStateService _gameState;
    private readonly FactionReputation _factionReputation;

    /// <summary>
    /// Creates a new game pause screen.
    /// </summary>
    /// <param name="roster">The player's Stray roster.</param>
    /// <param name="gameState">The game state service.</param>
    /// <param name="factionReputation">The faction reputation tracker.</param>
    public GamePauseScreen(StrayRoster roster, GameStateService gameState, FactionReputation factionReputation)
        : base("Paused")
    {
        _roster = roster;
        _gameState = gameState;
        _factionReputation = factionReputation;

        IsPopup = true;

        // Create menu entries
        var partyEntry = new MenuEntry("Party");
        var inventoryEntry = new MenuEntry("Inventory");
        var factionsEntry = new MenuEntry("Factions");
        var saveEntry = new MenuEntry("Save Game");
        var settingsEntry = new MenuEntry("Settings");
        var resumeEntry = new MenuEntry(Resources.Resume);
        var quitEntry = new MenuEntry(Resources.Quit);

        // Hook up event handlers
        partyEntry.Selected += PartyEntrySelected;
        inventoryEntry.Selected += InventoryEntrySelected;
        factionsEntry.Selected += FactionsEntrySelected;
        saveEntry.Selected += SaveEntrySelected;
        settingsEntry.Selected += SettingsEntrySelected;
        resumeEntry.Selected += OnCancel;
        quitEntry.Selected += QuitEntrySelected;

        // Add entries to menu
        MenuEntries.Add(partyEntry);
        MenuEntries.Add(inventoryEntry);
        MenuEntries.Add(factionsEntry);
        MenuEntries.Add(saveEntry);
        MenuEntries.Add(settingsEntry);
        MenuEntries.Add(resumeEntry);
        MenuEntries.Add(quitEntry);
    }

    private void PartyEntrySelected(object? sender, PlayerIndexEventArgs e)
    {
        var partyScreen = new PartyScreen(_roster);
        ScreenManager.AddScreen(partyScreen, ControllingPlayer);
    }

    private void InventoryEntrySelected(object? sender, PlayerIndexEventArgs e)
    {
        var inventoryScreen = new InventoryScreen(
            _roster,
            _gameState.Data.OwnedMicrochips,
            _gameState.Data.OwnedAugmentations,
            _gameState.Data.InventoryItems);
        ScreenManager.AddScreen(inventoryScreen, ControllingPlayer);
    }

    private void FactionsEntrySelected(object? sender, PlayerIndexEventArgs e)
    {
        var factionScreen = new FactionScreen(_factionReputation);
        ScreenManager.AddScreen(factionScreen, ControllingPlayer);
    }

    private void SaveEntrySelected(object? sender, PlayerIndexEventArgs e)
    {
        // Show save slot selection
        var saveScreen = new SaveLoadScreen(_gameState, isSaving: true);
        ScreenManager.AddScreen(saveScreen, ControllingPlayer);
    }

    private void SettingsEntrySelected(object? sender, PlayerIndexEventArgs e)
    {
        var settingsScreen = new SettingsScreen();
        ScreenManager.AddScreen(settingsScreen, ControllingPlayer);
    }

    private void QuitEntrySelected(object? sender, PlayerIndexEventArgs e)
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
            LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(), new StraysMainMenuScreen());
        };
        savePrompt.Cancelled += (s, args) =>
        {
            LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(), new StraysMainMenuScreen());
        };
        ScreenManager.AddScreen(savePrompt, ControllingPlayer);
    }

    public override void Draw(GameTime gameTime)
    {
        // Draw semi-transparent overlay
        var spriteBatch = ScreenManager.SpriteBatch;
        spriteBatch.Begin();

        // Darken background
        var viewport = ScreenManager.GraphicsDevice.Viewport;
        var pixel = new Microsoft.Xna.Framework.Graphics.Texture2D(ScreenManager.GraphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });
        spriteBatch.Draw(pixel, new Rectangle(0, 0, viewport.Width, viewport.Height), Color.Black * 0.6f);
        pixel.Dispose();

        spriteBatch.End();

        base.Draw(gameTime);
    }
}
