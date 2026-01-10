using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Strays.Core.Inputs;
using Strays.ScreenManagers;

namespace Strays.Screens;

// Local pixel texture for drawing primitives

/// <summary>
/// Mode for the save/load screen.
/// </summary>
public enum SaveLoadMode
{
    Save,
    Load
}

/// <summary>
/// Preview data for a save slot.
/// </summary>
public class SaveSlotPreview
{
    /// <summary>
    /// Slot number.
    /// </summary>
    public int SlotNumber { get; init; }

    /// <summary>
    /// Whether the slot has data.
    /// </summary>
    public bool HasData { get; init; }

    /// <summary>
    /// Player/party name.
    /// </summary>
    public string PlayerName { get; init; } = "";

    /// <summary>
    /// Current story chapter.
    /// </summary>
    public string Chapter { get; init; } = "";

    /// <summary>
    /// Current biome.
    /// </summary>
    public string Biome { get; init; } = "";

    /// <summary>
    /// Average party level.
    /// </summary>
    public int Level { get; init; }

    /// <summary>
    /// Total play time.
    /// </summary>
    public TimeSpan PlayTime { get; init; }

    /// <summary>
    /// When this save was made.
    /// </summary>
    public DateTime SaveDate { get; init; }

    /// <summary>
    /// Number of Strays recruited.
    /// </summary>
    public int StraysRecruited { get; init; }

    /// <summary>
    /// Completion percentage.
    /// </summary>
    public float CompletionPercent { get; init; }

    /// <summary>
    /// Primary party member names.
    /// </summary>
    public List<string> PartyMembers { get; init; } = new();

    /// <summary>
    /// Current story flag summary.
    /// </summary>
    public string StoryStatus { get; init; } = "";
}

/// <summary>
/// Enhanced save/load screen with multiple slots and previews.
/// </summary>
public class SaveLoadScreen : GameScreen
{
    /// <summary>
    /// Number of save slots.
    /// </summary>
    public const int SLOT_COUNT = 6;

    private readonly SaveLoadMode _mode;
    private readonly List<SaveSlotPreview> _slots;
    private int _selectedSlot;
    private int _scrollOffset;
    private const int VISIBLE_SLOTS = 4;

    private bool _confirmingAction;
    private string _confirmMessage = "";

    private float _selectionPulse;
    private float _fadeIn;

    private Texture2D? _pixelTexture;

    private readonly Color _backgroundColor = new(15, 20, 30);
    private readonly Color _panelColor = new(25, 35, 50);
    private readonly Color _selectedColor = new(45, 65, 95);
    private readonly Color _emptySlotColor = new(35, 45, 60);
    private readonly Color _textColor = new(200, 210, 220);
    private readonly Color _accentColor = new(80, 180, 230);
    private readonly Color _warningColor = new(230, 160, 80);

    /// <summary>
    /// Event fired when save/load is completed.
    /// </summary>
    public event EventHandler<int>? ActionCompleted;

    /// <summary>
    /// Event fired when screen is cancelled.
    /// </summary>
    public event EventHandler? Cancelled;

    public SaveLoadScreen(SaveLoadMode mode)
    {
        _mode = mode;
        _slots = new List<SaveSlotPreview>();

        LoadSlotPreviews();
    }

    public override void LoadContent()
    {
        base.LoadContent();

        // Create a 1x1 white pixel texture for drawing primitives
        _pixelTexture = new Texture2D(ScreenManager.GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });
    }

    public override void UnloadContent()
    {
        base.UnloadContent();

        _pixelTexture?.Dispose();
        _pixelTexture = null;
    }

    /// <summary>
    /// Loads preview data for all save slots.
    /// </summary>
    private void LoadSlotPreviews()
    {
        _slots.Clear();

        for (int i = 0; i < SLOT_COUNT; i++)
        {
            _slots.Add(LoadSlotPreview(i));
        }
    }

    /// <summary>
    /// Loads preview data for a specific slot.
    /// </summary>
    private SaveSlotPreview LoadSlotPreview(int slot)
    {
        var savePath = GetSavePath(slot);

        if (!File.Exists(savePath))
        {
            return new SaveSlotPreview
            {
                SlotNumber = slot,
                HasData = false
            };
        }

        try
        {
            var json = File.ReadAllText(savePath);
            var saveData = JsonSerializer.Deserialize<SaveGameData>(json);

            if (saveData == null)
            {
                return CreateEmptySlot(slot);
            }

            return new SaveSlotPreview
            {
                SlotNumber = slot,
                HasData = true,
                PlayerName = saveData.PlayerName ?? "Unknown",
                Chapter = saveData.Chapter ?? "Act I",
                Biome = saveData.Biome ?? "Fringe",
                Level = saveData.Level,
                PlayTime = TimeSpan.FromSeconds(saveData.PlayTimeSeconds),
                SaveDate = saveData.SaveDate,
                StraysRecruited = saveData.StraysRecruited,
                CompletionPercent = saveData.CompletionPercent,
                PartyMembers = saveData.PartyMembers ?? new List<string>(),
                StoryStatus = saveData.StoryStatus ?? ""
            };
        }
        catch
        {
            return CreateEmptySlot(slot);
        }
    }

    private SaveSlotPreview CreateEmptySlot(int slot)
    {
        return new SaveSlotPreview
        {
            SlotNumber = slot,
            HasData = false
        };
    }

    /// <summary>
    /// Gets the save file path for a slot.
    /// </summary>
    private string GetSavePath(int slot)
    {
        var saveDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Strays", "Saves");

        return Path.Combine(saveDir, $"save_{slot}.json");
    }

    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        _selectionPulse += deltaTime * 3f;

        if (_fadeIn < 1f)
        {
            _fadeIn = Math.Min(1f, _fadeIn + deltaTime * 4f);
        }
    }

    public override void HandleInput(GameTime gameTime, InputState input)
    {
        if (_confirmingAction)
        {
            HandleConfirmInput(input);
            return;
        }

        // Navigation
        if (input.IsNewKeyPress(Keys.Up, ControllingPlayer, out _) ||
            input.IsNewKeyPress(Keys.W, ControllingPlayer, out _))
        {
            _selectedSlot = Math.Max(0, _selectedSlot - 1);
            UpdateScroll();
        }
        else if (input.IsNewKeyPress(Keys.Down, ControllingPlayer, out _) ||
                 input.IsNewKeyPress(Keys.S, ControllingPlayer, out _))
        {
            _selectedSlot = Math.Min(_slots.Count - 1, _selectedSlot + 1);
            UpdateScroll();
        }

        // Selection
        if (input.IsNewKeyPress(Keys.Enter, ControllingPlayer, out _) ||
            input.IsNewKeyPress(Keys.Space, ControllingPlayer, out _))
        {
            AttemptAction();
        }

        // Cancel
        if (input.IsNewKeyPress(Keys.Escape, ControllingPlayer, out _) ||
            input.IsNewKeyPress(Keys.Back, ControllingPlayer, out _))
        {
            Cancelled?.Invoke(this, EventArgs.Empty);
            ExitScreen();
        }

        // Quick slot selection (1-6)
        for (int i = 0; i < SLOT_COUNT; i++)
        {
            if (input.IsNewKeyPress(Keys.D1 + i, ControllingPlayer, out _))
            {
                _selectedSlot = i;
                UpdateScroll();
            }
        }
    }

    private void HandleConfirmInput(InputState input)
    {
        if (input.IsNewKeyPress(Keys.Y, ControllingPlayer, out _) ||
            input.IsNewKeyPress(Keys.Enter, ControllingPlayer, out _))
        {
            PerformAction();
            _confirmingAction = false;
        }
        else if (input.IsNewKeyPress(Keys.N, ControllingPlayer, out _) ||
                 input.IsNewKeyPress(Keys.Escape, ControllingPlayer, out _))
        {
            _confirmingAction = false;
        }
    }

    private void UpdateScroll()
    {
        if (_selectedSlot < _scrollOffset)
        {
            _scrollOffset = _selectedSlot;
        }
        else if (_selectedSlot >= _scrollOffset + VISIBLE_SLOTS)
        {
            _scrollOffset = _selectedSlot - VISIBLE_SLOTS + 1;
        }
    }

    private void AttemptAction()
    {
        var slot = _slots[_selectedSlot];

        if (_mode == SaveLoadMode.Load)
        {
            if (!slot.HasData)
            {
                // Can't load empty slot
                return;
            }

            _confirmMessage = $"Load save from Slot {slot.SlotNumber + 1}?\nUnsaved progress will be lost.";
            _confirmingAction = true;
        }
        else // Save mode
        {
            if (slot.HasData)
            {
                _confirmMessage = $"Overwrite save in Slot {slot.SlotNumber + 1}?\nExisting data will be replaced.";
            }
            else
            {
                _confirmMessage = $"Save game to Slot {slot.SlotNumber + 1}?";
            }

            _confirmingAction = true;
        }
    }

    private void PerformAction()
    {
        ActionCompleted?.Invoke(this, _selectedSlot);
        ExitScreen();
    }

    public override void Draw(GameTime gameTime)
    {
        var spriteBatch = ScreenManager.SpriteBatch;
        var viewport = ScreenManager.GraphicsDevice.Viewport;
        var font = ScreenManager.Font;

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

        float alpha = _fadeIn;

        // Background
        DrawFilledRectangle(spriteBatch, viewport.Bounds, _backgroundColor * alpha);

        // Title
        string title = _mode == SaveLoadMode.Save ? "SAVE GAME" : "LOAD GAME";
        var titleSize = font.MeasureString(title) * 1.5f;
        var titlePos = new Vector2((viewport.Width - titleSize.X) / 2, 40);
        spriteBatch.DrawString(font, title, titlePos, _accentColor * alpha, 0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0f);

        // Slot list
        int panelWidth = 700;
        int panelHeight = 130;
        int startX = (viewport.Width - panelWidth) / 2;
        int startY = 120;
        int spacing = 10;

        for (int i = 0; i < VISIBLE_SLOTS && i + _scrollOffset < _slots.Count; i++)
        {
            var slot = _slots[i + _scrollOffset];
            int y = startY + i * (panelHeight + spacing);
            bool isSelected = i + _scrollOffset == _selectedSlot;

            DrawSlotPanel(spriteBatch, font, slot, new Rectangle(startX, y, panelWidth, panelHeight), isSelected, alpha);
        }

        // Scroll indicators
        if (_scrollOffset > 0)
        {
            spriteBatch.DrawString(font, "^ More", new Vector2(startX + panelWidth / 2 - 30, startY - 25), _textColor * alpha * 0.6f);
        }

        if (_scrollOffset + VISIBLE_SLOTS < _slots.Count)
        {
            float bottomY = startY + VISIBLE_SLOTS * (panelHeight + spacing);
            spriteBatch.DrawString(font, "v More", new Vector2(startX + panelWidth / 2 - 30, bottomY), _textColor * alpha * 0.6f);
        }

        // Instructions
        string instructions = _mode == SaveLoadMode.Save
            ? "[Enter] Save  [Esc] Cancel  [1-6] Quick Select"
            : "[Enter] Load  [Esc] Cancel  [1-6] Quick Select";
        var instructSize = font.MeasureString(instructions);
        spriteBatch.DrawString(font, instructions, new Vector2((viewport.Width - instructSize.X) / 2, viewport.Height - 50), _textColor * alpha * 0.7f);

        // Confirmation dialog
        if (_confirmingAction)
        {
            DrawConfirmDialog(spriteBatch, font, viewport, alpha);
        }

        spriteBatch.End();
    }

    private void DrawSlotPanel(SpriteBatch spriteBatch, SpriteFont font, SaveSlotPreview slot, Rectangle bounds, bool isSelected, float alpha)
    {
        // Panel background
        Color bgColor = slot.HasData
            ? (isSelected ? _selectedColor : _panelColor)
            : _emptySlotColor;

        if (isSelected)
        {
            float pulse = (float)(Math.Sin(_selectionPulse) * 0.5f + 0.5f);
            bgColor = Color.Lerp(bgColor, _accentColor * 0.3f, pulse * 0.3f);
        }

        DrawFilledRectangle(spriteBatch, bounds, bgColor * alpha);

        // Selection border
        if (isSelected)
        {
            DrawRectangleBorder(spriteBatch, bounds, _accentColor * alpha, 2);
        }

        // Slot number
        string slotLabel = $"Slot {slot.SlotNumber + 1}";
        spriteBatch.DrawString(font, slotLabel, new Vector2(bounds.X + 15, bounds.Y + 10), _accentColor * alpha);

        if (!slot.HasData)
        {
            // Empty slot
            string emptyText = _mode == SaveLoadMode.Save ? "- Empty Slot -" : "- No Data -";
            var emptySize = font.MeasureString(emptyText);
            var emptyPos = new Vector2(bounds.X + (bounds.Width - emptySize.X) / 2, bounds.Y + (bounds.Height - emptySize.Y) / 2);
            spriteBatch.DrawString(font, emptyText, emptyPos, _textColor * alpha * 0.5f);

            return;
        }

        // Has save data
        int leftColumn = bounds.X + 15;
        int rightColumn = bounds.X + bounds.Width / 2 + 20;
        int row1 = bounds.Y + 35;
        int row2 = bounds.Y + 55;
        int row3 = bounds.Y + 75;
        int row4 = bounds.Y + 95;

        // Left column
        spriteBatch.DrawString(font, slot.PlayerName, new Vector2(leftColumn, row1), _textColor * alpha);
        spriteBatch.DrawString(font, $"Level {slot.Level}  •  {slot.Chapter}", new Vector2(leftColumn, row2), _textColor * alpha * 0.8f);
        spriteBatch.DrawString(font, $"Location: {slot.Biome}", new Vector2(leftColumn, row3), _textColor * alpha * 0.7f);

        // Party members
        if (slot.PartyMembers.Count > 0)
        {
            string party = string.Join(", ", slot.PartyMembers.Take(3));

            if (slot.PartyMembers.Count > 3)
            {
                party += $" +{slot.PartyMembers.Count - 3}";
            }

            spriteBatch.DrawString(font, $"Party: {party}", new Vector2(leftColumn, row4), _textColor * alpha * 0.6f);
        }

        // Right column
        string playTime = FormatPlayTime(slot.PlayTime);
        spriteBatch.DrawString(font, $"Time: {playTime}", new Vector2(rightColumn, row1), _textColor * alpha * 0.8f);
        spriteBatch.DrawString(font, $"Strays: {slot.StraysRecruited}", new Vector2(rightColumn, row2), _textColor * alpha * 0.8f);
        spriteBatch.DrawString(font, $"Completion: {slot.CompletionPercent:F1}%", new Vector2(rightColumn, row3), _textColor * alpha * 0.8f);

        // Save date
        string dateStr = slot.SaveDate.ToString("MMM dd, yyyy  HH:mm");
        spriteBatch.DrawString(font, dateStr, new Vector2(rightColumn, row4), _textColor * alpha * 0.6f);

        // Story status indicator
        if (!string.IsNullOrEmpty(slot.StoryStatus))
        {
            var statusSize = font.MeasureString(slot.StoryStatus);
            spriteBatch.DrawString(font, slot.StoryStatus, new Vector2(bounds.Right - statusSize.X - 15, bounds.Y + 10), _warningColor * alpha * 0.8f);
        }
    }

    private void DrawConfirmDialog(SpriteBatch spriteBatch, SpriteFont font, Viewport viewport, float alpha)
    {
        // Dim overlay
        DrawFilledRectangle(spriteBatch, viewport.Bounds, Color.Black * 0.7f * alpha);

        // Dialog box
        int dialogWidth = 450;
        int dialogHeight = 180;
        var dialogBounds = new Rectangle(
            (viewport.Width - dialogWidth) / 2,
            (viewport.Height - dialogHeight) / 2,
            dialogWidth,
            dialogHeight);

        DrawFilledRectangle(spriteBatch, dialogBounds, _panelColor * alpha);
        DrawRectangleBorder(spriteBatch, dialogBounds, _accentColor * alpha, 2);

        // Title
        string dialogTitle = _mode == SaveLoadMode.Save ? "Confirm Save" : "Confirm Load";
        var titleSize = font.MeasureString(dialogTitle);
        spriteBatch.DrawString(font, dialogTitle, new Vector2(dialogBounds.X + (dialogWidth - titleSize.X) / 2, dialogBounds.Y + 20), _accentColor * alpha);

        // Message
        var lines = _confirmMessage.Split('\n');
        int lineY = dialogBounds.Y + 55;

        foreach (var line in lines)
        {
            var lineSize = font.MeasureString(line);
            spriteBatch.DrawString(font, line, new Vector2(dialogBounds.X + (dialogWidth - lineSize.X) / 2, lineY), _textColor * alpha);
            lineY += 22;
        }

        // Options
        string options = "[Y] Yes    [N] No";
        var optSize = font.MeasureString(options);
        spriteBatch.DrawString(font, options, new Vector2(dialogBounds.X + (dialogWidth - optSize.X) / 2, dialogBounds.Bottom - 40), _textColor * alpha * 0.8f);
    }

    private string FormatPlayTime(TimeSpan time)
    {
        if (time.TotalHours >= 1)
        {
            return $"{(int)time.TotalHours}h {time.Minutes}m";
        }

        return $"{time.Minutes}m {time.Seconds}s";
    }

    private void DrawFilledRectangle(SpriteBatch spriteBatch, Rectangle bounds, Color color)
    {
        if (_pixelTexture == null) return;
        spriteBatch.Draw(_pixelTexture, bounds, color);
    }

    private void DrawRectangleBorder(SpriteBatch spriteBatch, Rectangle bounds, Color color, int thickness)
    {
        if (_pixelTexture == null) return;
        var pixel = _pixelTexture;

        // Top
        spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, bounds.Width, thickness), color);
        // Bottom
        spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Bottom - thickness, bounds.Width, thickness), color);
        // Left
        spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, thickness, bounds.Height), color);
        // Right
        spriteBatch.Draw(pixel, new Rectangle(bounds.Right - thickness, bounds.Y, thickness, bounds.Height), color);
    }
}

/// <summary>
/// Serializable save game data structure.
/// </summary>
public class SaveGameData
{
    public string? PlayerName { get; set; }
    public string? Chapter { get; set; }
    public string? Biome { get; set; }
    public int Level { get; set; }
    public double PlayTimeSeconds { get; set; }
    public DateTime SaveDate { get; set; }
    public int StraysRecruited { get; set; }
    public float CompletionPercent { get; set; }
    public List<string>? PartyMembers { get; set; }
    public string? StoryStatus { get; set; }

    // Additional game state data
    public Dictionary<string, bool>? StoryFlags { get; set; }
    public Dictionary<string, int>? Counters { get; set; }
    public List<string>? QuestLog { get; set; }
    public Dictionary<string, int>? Inventory { get; set; }
    public int Currency { get; set; }
    public Dictionary<string, int>? FactionReputation { get; set; }
}
