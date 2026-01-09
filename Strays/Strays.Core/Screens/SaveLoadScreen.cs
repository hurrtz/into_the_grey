using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Strays.Core.Inputs;
using Strays.Core.Services;
using Strays.ScreenManagers;

namespace Strays.Screens;

/// <summary>
/// Screen for saving or loading game data from multiple slots.
/// </summary>
public class SaveLoadScreen : GameScreen
{
    private readonly GameStateService _gameState;
    private readonly bool _isSaving;

    private SpriteFont? _font;
    private SpriteFont? _smallFont;
    private Texture2D? _pixelTexture;

    private int _selectedSlot = 0;
    private SaveSlotInfo?[] _slotInfos = new SaveSlotInfo?[4]; // 3 manual + 1 auto

    public SaveLoadScreen(GameStateService gameState, bool isSaving)
    {
        _gameState = gameState;
        _isSaving = isSaving;

        IsPopup = true;
        TransitionOnTime = TimeSpan.FromSeconds(0.2);
        TransitionOffTime = TimeSpan.FromSeconds(0.2);
    }

    public override void LoadContent()
    {
        base.LoadContent();

        var content = ScreenManager.Game.Content;
        _font = content.Load<SpriteFont>("Fonts/MenuFont");
        _smallFont = content.Load<SpriteFont>("Fonts/GameFont");

        _pixelTexture = new Texture2D(ScreenManager.GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });

        RefreshSlotInfos();
    }

    public override void UnloadContent()
    {
        base.UnloadContent();
        _pixelTexture?.Dispose();
    }

    private void RefreshSlotInfos()
    {
        for (int i = 0; i < 3; i++)
        {
            _slotInfos[i] = _gameState.GetSaveInfo(i);
        }
        _slotInfos[3] = _gameState.GetAutoSaveInfo();
    }

    public override void HandleInput(GameTime gameTime, InputState input)
    {
        PlayerIndex playerIndex;

        if (input.IsMenuCancel(ControllingPlayer, out playerIndex))
        {
            ExitScreen();
            return;
        }

        if (input.IsMenuUp(ControllingPlayer))
        {
            _selectedSlot = Math.Max(0, _selectedSlot - 1);
        }
        else if (input.IsMenuDown(ControllingPlayer))
        {
            _selectedSlot = Math.Min(3, _selectedSlot + 1);
        }

        if (input.IsMenuSelect(ControllingPlayer, out playerIndex))
        {
            HandleSlotSelection();
        }
    }

    private void HandleSlotSelection()
    {
        if (_isSaving)
        {
            // Can't save to auto-save slot manually
            if (_selectedSlot == 3)
            {
                // Show error message
                return;
            }

            // Check if slot has data - confirm overwrite
            if (_slotInfos[_selectedSlot] != null)
            {
                var confirm = new MessageBoxScreen($"Overwrite Slot {_selectedSlot + 1}?");
                confirm.Accepted += (s, e) =>
                {
                    PerformSave(_selectedSlot);
                };
                ScreenManager.AddScreen(confirm, ControllingPlayer);
            }
            else
            {
                PerformSave(_selectedSlot);
            }
        }
        else
        {
            // Loading
            var info = _slotInfos[_selectedSlot];
            if (info == null)
            {
                // No save in this slot
                return;
            }

            // For auto-save, use special slot number
            int slotToLoad = _selectedSlot == 3 ? 99 : _selectedSlot;

            var confirm = new MessageBoxScreen($"Load {info.DisplayName}?");
            confirm.Accepted += (s, e) =>
            {
                PerformLoad(slotToLoad);
            };
            ScreenManager.AddScreen(confirm, ControllingPlayer);
        }
    }

    private void PerformSave(int slot)
    {
        if (_gameState.Save(slot))
        {
            RefreshSlotInfos();
            // Show success briefly then exit
            ExitScreen();
        }
    }

    private void PerformLoad(int slot)
    {
        if (_gameState.Load(slot))
        {
            // Return to world screen with loaded data
            LoadingScreen.Load(ScreenManager, true, ControllingPlayer, new WorldScreen());
        }
    }

    public override void Draw(GameTime gameTime)
    {
        if (_font == null || _smallFont == null || _pixelTexture == null)
            return;

        var spriteBatch = ScreenManager.SpriteBatch;
        var viewport = ScreenManager.GraphicsDevice.Viewport;

        spriteBatch.Begin();

        // Draw background overlay
        spriteBatch.Draw(_pixelTexture, new Rectangle(0, 0, viewport.Width, viewport.Height), Color.Black * 0.85f);

        // Title
        string title = _isSaving ? "Save Game" : "Load Game";
        var titleSize = _font.MeasureString(title);
        spriteBatch.DrawString(_font, title, new Vector2((viewport.Width - titleSize.X) / 2, 30), Color.White);

        // Draw slots
        int slotWidth = 500;
        int slotHeight = 80;
        int startX = (viewport.Width - slotWidth) / 2;
        int startY = 80;

        for (int i = 0; i < 4; i++)
        {
            var slotRect = new Rectangle(startX, startY + i * (slotHeight + 10), slotWidth, slotHeight);
            bool isSelected = i == _selectedSlot;
            bool isAutoSave = i == 3;

            // Background
            Color bgColor = isSelected ? Color.DarkBlue * 0.7f : Color.DarkSlateGray * 0.5f;
            spriteBatch.Draw(_pixelTexture, slotRect, bgColor);

            // Border
            Color borderColor = isSelected ? Color.Yellow : Color.Gray;
            DrawBorder(spriteBatch, slotRect, borderColor);

            // Slot info
            var info = _slotInfos[i];
            string slotName = isAutoSave ? "Auto Save" : $"Slot {i + 1}";

            if (info != null)
            {
                // Has save data
                spriteBatch.DrawString(_smallFont, slotName, new Vector2(slotRect.X + 10, slotRect.Y + 5), Color.Yellow);
                spriteBatch.DrawString(_smallFont, info.FormattedTimestamp, new Vector2(slotRect.X + slotWidth - 150, slotRect.Y + 5), Color.Gray);

                string summary = info.Summary;
                spriteBatch.DrawString(_smallFont, summary, new Vector2(slotRect.X + 10, slotRect.Y + 25), Color.White);

                string details = $"Play Time: {info.FormattedPlayTime} | Quests: {info.CompletedQuests}";
                spriteBatch.DrawString(_smallFont, details, new Vector2(slotRect.X + 10, slotRect.Y + 45), Color.LightGray);
            }
            else
            {
                // Empty slot
                spriteBatch.DrawString(_smallFont, slotName, new Vector2(slotRect.X + 10, slotRect.Y + 5), Color.Yellow);

                string emptyText = isAutoSave ? "No auto-save" : "Empty Slot";
                var emptySize = _smallFont.MeasureString(emptyText);
                spriteBatch.DrawString(_smallFont, emptyText,
                    new Vector2(slotRect.X + (slotWidth - emptySize.X) / 2, slotRect.Y + (slotHeight - emptySize.Y) / 2),
                    Color.Gray);
            }

            // Can't save to auto-save
            if (_isSaving && isAutoSave && isSelected)
            {
                spriteBatch.DrawString(_smallFont, "(Auto-save only)", new Vector2(slotRect.X + 10, slotRect.Y + 60), Color.Red);
            }
        }

        // Instructions
        string instructions = _isSaving
            ? "[Up/Down] Select Slot | [Enter] Save | [ESC] Cancel"
            : "[Up/Down] Select Slot | [Enter] Load | [ESC] Cancel";
        var instrSize = _smallFont.MeasureString(instructions);
        spriteBatch.DrawString(_smallFont, instructions, new Vector2((viewport.Width - instrSize.X) / 2, viewport.Height - 40), Color.Gray);

        spriteBatch.End();
    }

    private void DrawBorder(SpriteBatch spriteBatch, Rectangle bounds, Color color)
    {
        int thickness = 2;
        spriteBatch.Draw(_pixelTexture!, new Rectangle(bounds.X, bounds.Y, bounds.Width, thickness), color);
        spriteBatch.Draw(_pixelTexture!, new Rectangle(bounds.X, bounds.Y + bounds.Height - thickness, bounds.Width, thickness), color);
        spriteBatch.Draw(_pixelTexture!, new Rectangle(bounds.X, bounds.Y, thickness, bounds.Height), color);
        spriteBatch.Draw(_pixelTexture!, new Rectangle(bounds.X + bounds.Width - thickness, bounds.Y, thickness, bounds.Height), color);
    }
}
