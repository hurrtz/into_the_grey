using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Strays.Core.Game.Data;
using Strays.Core.Game.Progression;
using Strays.Core.Inputs;
using Strays.Core.ScreenManagers;

namespace Strays.Core.Screens;

/// <summary>
/// Screen displaying the in-game bestiary/encyclopedia.
/// </summary>
public class BestiaryScreen : GameScreen
{
    private readonly Bestiary _bestiary;
    private SpriteBatch? _spriteBatch;
    private SpriteFont? _titleFont;
    private SpriteFont? _contentFont;
    private Texture2D? _pixelTexture;

    // UI state
    private int _selectedCategoryIndex = 0;
    private int _selectedEntryIndex = 0;
    private int _scrollOffset = 0;
    private List<BestiaryEntry> _currentEntries = new();
    private BestiaryCategory? _currentCategory;

    // Display modes
    private enum ViewMode { Categories, EntryList, EntryDetails }
    private ViewMode _viewMode = ViewMode.Categories;

    // Animation
    private float _transitionAlpha = 0f;
    private float _selectionPulse = 0f;

    // Layout constants
    private const int SIDEBAR_WIDTH = 280;
    private const int HEADER_HEIGHT = 80;
    private const int ENTRY_HEIGHT = 60;
    private const int MAX_VISIBLE_ENTRIES = 8;
    private const int PADDING = 20;

    // Colors
    private static readonly Color BackgroundColor = new(20, 20, 30);
    private static readonly Color PanelColor = new(30, 30, 45);
    private static readonly Color HeaderColor = new(40, 40, 60);
    private static readonly Color SelectedColor = new(60, 80, 120);
    private static readonly Color TextColor = new(220, 220, 230);
    private static readonly Color DimTextColor = new(150, 150, 160);
    private static readonly Color AccentColor = new(100, 180, 255);

    public BestiaryScreen(Bestiary bestiary)
    {
        _bestiary = bestiary;
        TransitionOnTime = TimeSpan.FromSeconds(0.3);
        TransitionOffTime = TimeSpan.FromSeconds(0.2);
    }

    public override void LoadContent()
    {
        base.LoadContent();

        if (ScreenManager == null) return;

        _spriteBatch = ScreenManager.SpriteBatch;
        _contentFont = ScreenManager.Game.Content.Load<SpriteFont>("Fonts/GameFont");
        _titleFont = ScreenManager.Game.Content.Load<SpriteFont>("Fonts/MenuFont");

        // Create pixel texture
        _pixelTexture = new Texture2D(ScreenManager.GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });

        // Load initial category
        RefreshCurrentCategory();
    }

    public override void UnloadContent()
    {
        _pixelTexture?.Dispose();
        base.UnloadContent();
    }

    private void RefreshCurrentCategory()
    {
        if (_bestiary.Categories.Count == 0) return;

        _currentCategory = _bestiary.Categories[_selectedCategoryIndex];
        _currentEntries = _bestiary.GetEntriesForCategory(_currentCategory.Id).ToList();
        _selectedEntryIndex = 0;
        _scrollOffset = 0;
    }

    public override void HandleInput(InputState input)
    {
        if (input.IsMenuCancel(ControllingPlayer, out _))
        {
            if (_viewMode == ViewMode.EntryDetails)
            {
                _viewMode = ViewMode.EntryList;
            }
            else if (_viewMode == ViewMode.EntryList)
            {
                _viewMode = ViewMode.Categories;
            }
            else
            {
                ExitScreen();
            }

            return;
        }

        if (input.IsMenuSelect(ControllingPlayer, out _))
        {
            if (_viewMode == ViewMode.Categories)
            {
                _viewMode = ViewMode.EntryList;
            }
            else if (_viewMode == ViewMode.EntryList && _currentEntries.Count > 0)
            {
                _viewMode = ViewMode.EntryDetails;
            }

            return;
        }

        // Navigation
        if (input.IsMenuUp(ControllingPlayer))
        {
            NavigateUp();
        }
        else if (input.IsMenuDown(ControllingPlayer))
        {
            NavigateDown();
        }
        else if (input.IsMenuLeft(ControllingPlayer))
        {
            if (_viewMode == ViewMode.EntryList || _viewMode == ViewMode.EntryDetails)
            {
                _viewMode = ViewMode.Categories;
            }
        }
        else if (input.IsMenuRight(ControllingPlayer))
        {
            if (_viewMode == ViewMode.Categories)
            {
                _viewMode = ViewMode.EntryList;
            }
        }
    }

    private void NavigateUp()
    {
        if (_viewMode == ViewMode.Categories)
        {
            _selectedCategoryIndex--;

            if (_selectedCategoryIndex < 0)
            {
                _selectedCategoryIndex = _bestiary.Categories.Count - 1;
            }

            RefreshCurrentCategory();
        }
        else if (_viewMode == ViewMode.EntryList && _currentEntries.Count > 0)
        {
            _selectedEntryIndex--;

            if (_selectedEntryIndex < 0)
            {
                _selectedEntryIndex = _currentEntries.Count - 1;
            }

            UpdateScrollOffset();
        }
    }

    private void NavigateDown()
    {
        if (_viewMode == ViewMode.Categories)
        {
            _selectedCategoryIndex++;

            if (_selectedCategoryIndex >= _bestiary.Categories.Count)
            {
                _selectedCategoryIndex = 0;
            }

            RefreshCurrentCategory();
        }
        else if (_viewMode == ViewMode.EntryList && _currentEntries.Count > 0)
        {
            _selectedEntryIndex++;

            if (_selectedEntryIndex >= _currentEntries.Count)
            {
                _selectedEntryIndex = 0;
            }

            UpdateScrollOffset();
        }
    }

    private void UpdateScrollOffset()
    {
        if (_selectedEntryIndex < _scrollOffset)
        {
            _scrollOffset = _selectedEntryIndex;
        }
        else if (_selectedEntryIndex >= _scrollOffset + MAX_VISIBLE_ENTRIES)
        {
            _scrollOffset = _selectedEntryIndex - MAX_VISIBLE_ENTRIES + 1;
        }
    }

    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

        _transitionAlpha = 1f - TransitionPosition;
        _selectionPulse = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 3) * 0.1f + 0.9f;
    }

    public override void Draw(GameTime gameTime)
    {
        if (_spriteBatch == null || _contentFont == null || _titleFont == null || _pixelTexture == null)
        {
            return;
        }

        var viewport = ScreenManager?.GraphicsDevice.Viewport ?? new Viewport(0, 0, 1280, 720);

        _spriteBatch.Begin();

        // Background
        DrawRect(new Rectangle(0, 0, viewport.Width, viewport.Height), BackgroundColor * _transitionAlpha);

        // Header
        DrawHeader(viewport);

        // Main content area
        var contentArea = new Rectangle(PADDING, HEADER_HEIGHT + PADDING, viewport.Width - PADDING * 2, viewport.Height - HEADER_HEIGHT - PADDING * 2);

        // Sidebar (categories)
        var sidebarRect = new Rectangle(contentArea.X, contentArea.Y, SIDEBAR_WIDTH, contentArea.Height);
        DrawCategories(sidebarRect);

        // Entry list
        var listRect = new Rectangle(contentArea.X + SIDEBAR_WIDTH + PADDING, contentArea.Y, 350, contentArea.Height);
        DrawEntryList(listRect);

        // Entry details
        var detailsRect = new Rectangle(listRect.Right + PADDING, contentArea.Y, contentArea.Right - listRect.Right - PADDING, contentArea.Height);
        DrawEntryDetails(detailsRect);

        // Navigation hints
        DrawNavigationHints(viewport);

        _spriteBatch.End();
    }

    private void DrawHeader(Viewport viewport)
    {
        // Header background
        DrawRect(new Rectangle(0, 0, viewport.Width, HEADER_HEIGHT), HeaderColor * _transitionAlpha);

        // Title
        string title = "BESTIARY";
        var titleSize = _titleFont!.MeasureString(title);
        _spriteBatch!.DrawString(_titleFont, title, new Vector2(PADDING, (HEADER_HEIGHT - titleSize.Y) / 2), AccentColor * _transitionAlpha);

        // Completion stats
        string stats = $"Discovered: {_bestiary.DiscoveredCount}/{_bestiary.TotalEntries} ({_bestiary.CompletionPercent:F1}%)";
        var statsSize = _contentFont!.MeasureString(stats);
        _spriteBatch.DrawString(_contentFont, stats, new Vector2(viewport.Width - statsSize.X - PADDING, (HEADER_HEIGHT - statsSize.Y) / 2), TextColor * _transitionAlpha);

        // Completion bar
        int barWidth = 200;
        int barHeight = 8;
        var barRect = new Rectangle(viewport.Width - barWidth - PADDING - (int)statsSize.X - 20, (HEADER_HEIGHT - barHeight) / 2, barWidth, barHeight);
        DrawRect(barRect, new Color(50, 50, 60) * _transitionAlpha);

        int fillWidth = (int)(barWidth * (_bestiary.CompletionPercent / 100f));
        DrawRect(new Rectangle(barRect.X, barRect.Y, fillWidth, barHeight), AccentColor * _transitionAlpha);
    }

    private void DrawCategories(Rectangle bounds)
    {
        // Panel background
        DrawRect(bounds, PanelColor * _transitionAlpha);

        // Title
        _spriteBatch!.DrawString(_contentFont!, "CATEGORIES", new Vector2(bounds.X + 10, bounds.Y + 10), AccentColor * _transitionAlpha);

        // Category list
        int y = bounds.Y + 40;

        for (int i = 0; i < _bestiary.Categories.Count; i++)
        {
            var category = _bestiary.Categories[i];
            bool isSelected = i == _selectedCategoryIndex;
            bool isActive = _viewMode == ViewMode.Categories && isSelected;

            var entryRect = new Rectangle(bounds.X + 5, y, bounds.Width - 10, 50);

            if (isSelected)
            {
                Color selColor = isActive ? SelectedColor * _selectionPulse : SelectedColor * 0.5f;
                DrawRect(entryRect, selColor * _transitionAlpha);
            }

            // Category color indicator
            DrawRect(new Rectangle(entryRect.X + 5, entryRect.Y + 5, 4, entryRect.Height - 10), category.Color * _transitionAlpha);

            // Category name
            _spriteBatch.DrawString(_contentFont!, category.Name, new Vector2(entryRect.X + 15, entryRect.Y + 5), (isSelected ? Color.White : TextColor) * _transitionAlpha);

            // Completion
            float completion = _bestiary.GetCategoryCompletion(category.Id);
            string compText = $"{completion:F0}%";
            _spriteBatch.DrawString(_contentFont!, compText, new Vector2(entryRect.X + 15, entryRect.Y + 25), DimTextColor * _transitionAlpha);

            y += 55;
        }
    }

    private void DrawEntryList(Rectangle bounds)
    {
        // Panel background
        DrawRect(bounds, PanelColor * _transitionAlpha);

        if (_currentCategory == null || _currentEntries.Count == 0)
        {
            _spriteBatch!.DrawString(_contentFont!, "No entries", new Vector2(bounds.X + 10, bounds.Y + 10), DimTextColor * _transitionAlpha);

            return;
        }

        // Title
        _spriteBatch!.DrawString(_contentFont!, _currentCategory.Name.ToUpper(), new Vector2(bounds.X + 10, bounds.Y + 10), AccentColor * _transitionAlpha);

        // Entry list
        int y = bounds.Y + 40;

        for (int i = _scrollOffset; i < Math.Min(_scrollOffset + MAX_VISIBLE_ENTRIES, _currentEntries.Count); i++)
        {
            var entry = _currentEntries[i];
            bool isSelected = i == _selectedEntryIndex;
            bool isActive = _viewMode != ViewMode.Categories && isSelected;

            var entryRect = new Rectangle(bounds.X + 5, y, bounds.Width - 10, ENTRY_HEIGHT);

            if (isSelected)
            {
                Color selColor = isActive ? SelectedColor * _selectionPulse : SelectedColor * 0.5f;
                DrawRect(entryRect, selColor * _transitionAlpha);
            }

            // Status indicator
            Color statusColor = entry.Status switch
            {
                DiscoveryStatus.Unknown => new Color(60, 60, 70),
                DiscoveryStatus.Sighted => new Color(150, 150, 100),
                DiscoveryStatus.Defeated => new Color(200, 150, 100),
                DiscoveryStatus.Recruited => new Color(100, 200, 100),
                DiscoveryStatus.Mastered => new Color(255, 200, 100),
                _ => Color.Gray
            };
            DrawRect(new Rectangle(entryRect.X + 5, entryRect.Y + 5, 8, entryRect.Height - 10), statusColor * _transitionAlpha);

            // Name (hidden if unknown)
            string name = _bestiary.GetDisplayName(entry.DefinitionId);
            _spriteBatch.DrawString(_contentFont!, name, new Vector2(entryRect.X + 20, entryRect.Y + 5), (isSelected ? Color.White : TextColor) * _transitionAlpha);

            // Status text
            string statusText = entry.Status.ToString();
            _spriteBatch.DrawString(_contentFont!, statusText, new Vector2(entryRect.X + 20, entryRect.Y + 25), DimTextColor * _transitionAlpha);

            // Encounter count
            if (entry.EncounterCount > 0)
            {
                string encounters = $"x{entry.EncounterCount}";
                var encSize = _contentFont!.MeasureString(encounters);
                _spriteBatch.DrawString(_contentFont, encounters, new Vector2(entryRect.Right - encSize.X - 10, entryRect.Y + 15), DimTextColor * _transitionAlpha);
            }

            y += ENTRY_HEIGHT + 5;
        }

        // Scroll indicators
        if (_scrollOffset > 0)
        {
            _spriteBatch.DrawString(_contentFont!, "▲", new Vector2(bounds.X + bounds.Width / 2, bounds.Y + 38), AccentColor * _transitionAlpha);
        }

        if (_scrollOffset + MAX_VISIBLE_ENTRIES < _currentEntries.Count)
        {
            _spriteBatch.DrawString(_contentFont!, "▼", new Vector2(bounds.X + bounds.Width / 2, bounds.Bottom - 20), AccentColor * _transitionAlpha);
        }
    }

    private void DrawEntryDetails(Rectangle bounds)
    {
        // Panel background
        DrawRect(bounds, PanelColor * _transitionAlpha);

        if (_currentEntries.Count == 0 || _selectedEntryIndex >= _currentEntries.Count)
        {
            _spriteBatch!.DrawString(_contentFont!, "Select an entry", new Vector2(bounds.X + 10, bounds.Y + 10), DimTextColor * _transitionAlpha);

            return;
        }

        var entry = _currentEntries[_selectedEntryIndex];
        var definition = StrayDefinitions.Get(entry.DefinitionId);

        int y = bounds.Y + 10;

        // Name
        string name = _bestiary.GetDisplayName(entry.DefinitionId);
        _spriteBatch!.DrawString(_titleFont!, name, new Vector2(bounds.X + 10, y), AccentColor * _transitionAlpha);
        y += 40;

        // Status badge
        DrawRect(new Rectangle(bounds.X + 10, y, 100, 24), GetStatusColor(entry.Status) * _transitionAlpha);
        _spriteBatch.DrawString(_contentFont!, entry.Status.ToString(), new Vector2(bounds.X + 15, y + 2), Color.White * _transitionAlpha);
        y += 35;

        // Description
        string desc = _bestiary.GetDisplayDescription(entry.DefinitionId);
        DrawWrappedText(desc, new Rectangle(bounds.X + 10, y, bounds.Width - 20, 80), TextColor);
        y += 90;

        // Stats section
        if (entry.StatsRevealed && definition != null)
        {
            DrawRect(new Rectangle(bounds.X + 10, y, bounds.Width - 20, 2), AccentColor * 0.3f * _transitionAlpha);
            y += 10;

            _spriteBatch.DrawString(_contentFont!, "STATS", new Vector2(bounds.X + 10, y), AccentColor * _transitionAlpha);
            y += 25;

            var stats = definition.BaseStats;
            DrawStatBar("HP", stats.MaxHp, 200, bounds.X + 10, y, bounds.Width - 20);
            y += 25;
            DrawStatBar("ATK", stats.Attack, 30, bounds.X + 10, y, bounds.Width - 20);
            y += 25;
            DrawStatBar("DEF", stats.Defense, 30, bounds.X + 10, y, bounds.Width - 20);
            y += 25;
            DrawStatBar("SPD", stats.Speed, 30, bounds.X + 10, y, bounds.Width - 20);
            y += 30;
        }
        else if (definition != null)
        {
            _spriteBatch.DrawString(_contentFont!, "[Defeat to reveal stats]", new Vector2(bounds.X + 10, y), DimTextColor * _transitionAlpha);
            y += 30;
        }

        // Abilities section
        if (entry.AbilitiesRevealed && definition != null)
        {
            DrawRect(new Rectangle(bounds.X + 10, y, bounds.Width - 20, 2), AccentColor * 0.3f * _transitionAlpha);
            y += 10;

            _spriteBatch.DrawString(_contentFont!, "ABILITIES", new Vector2(bounds.X + 10, y), AccentColor * _transitionAlpha);
            y += 25;

            foreach (var ability in definition.InnateAbilities.Take(4))
            {
                _spriteBatch.DrawString(_contentFont!, $"• {ability.Replace("_", " ")}", new Vector2(bounds.X + 15, y), TextColor * _transitionAlpha);
                y += 20;
            }

            y += 10;
        }
        else if (definition != null && definition.InnateAbilities.Count > 0)
        {
            _spriteBatch.DrawString(_contentFont!, "[Recruit to reveal abilities]", new Vector2(bounds.X + 10, y), DimTextColor * _transitionAlpha);
            y += 30;
        }

        // Encounter info
        if (entry.EncounterCount > 0)
        {
            DrawRect(new Rectangle(bounds.X + 10, y, bounds.Width - 20, 2), AccentColor * 0.3f * _transitionAlpha);
            y += 10;

            _spriteBatch.DrawString(_contentFont!, "ENCOUNTER DATA", new Vector2(bounds.X + 10, y), AccentColor * _transitionAlpha);
            y += 25;

            _spriteBatch.DrawString(_contentFont!, $"Encountered: {entry.EncounterCount} times", new Vector2(bounds.X + 15, y), TextColor * _transitionAlpha);
            y += 20;
            _spriteBatch.DrawString(_contentFont!, $"Defeated: {entry.DefeatCount} times", new Vector2(bounds.X + 15, y), TextColor * _transitionAlpha);
            y += 20;
            _spriteBatch.DrawString(_contentFont!, $"Recruited: {entry.RecruitCount} times", new Vector2(bounds.X + 15, y), TextColor * _transitionAlpha);
            y += 20;

            if (entry.EncounteredBiomes.Count > 0)
            {
                string biomes = string.Join(", ", entry.EncounteredBiomes.Take(3));
                _spriteBatch.DrawString(_contentFont!, $"Found in: {biomes}", new Vector2(bounds.X + 15, y), DimTextColor * _transitionAlpha);
            }
        }
    }

    private void DrawStatBar(string label, int value, int maxValue, int x, int y, int width)
    {
        // Label
        _spriteBatch!.DrawString(_contentFont!, $"{label}:", new Vector2(x, y), TextColor * _transitionAlpha);

        // Bar background
        int barX = x + 50;
        int barWidth = width - 100;
        DrawRect(new Rectangle(barX, y + 5, barWidth, 12), new Color(40, 40, 50) * _transitionAlpha);

        // Bar fill
        float fill = Math.Min(1f, (float)value / maxValue);
        DrawRect(new Rectangle(barX, y + 5, (int)(barWidth * fill), 12), AccentColor * _transitionAlpha);

        // Value
        _spriteBatch.DrawString(_contentFont!, value.ToString(), new Vector2(barX + barWidth + 5, y), TextColor * _transitionAlpha);
    }

    private void DrawWrappedText(string text, Rectangle bounds, Color color)
    {
        // Simple text wrapping
        var words = text.Split(' ');
        string line = "";
        int y = bounds.Y;

        foreach (var word in words)
        {
            string testLine = line.Length > 0 ? line + " " + word : word;
            var size = _contentFont!.MeasureString(testLine);

            if (size.X > bounds.Width)
            {
                _spriteBatch!.DrawString(_contentFont, line, new Vector2(bounds.X, y), color * _transitionAlpha);
                line = word;
                y += 20;

                if (y > bounds.Bottom - 20) break;
            }
            else
            {
                line = testLine;
            }
        }

        if (line.Length > 0 && y <= bounds.Bottom - 20)
        {
            _spriteBatch!.DrawString(_contentFont!, line, new Vector2(bounds.X, y), color * _transitionAlpha);
        }
    }

    private void DrawNavigationHints(Viewport viewport)
    {
        string hints = _viewMode switch
        {
            ViewMode.Categories => "[↑↓] Navigate  [→/Enter] Select  [Esc] Close",
            ViewMode.EntryList => "[↑↓] Navigate  [Enter] Details  [←/Esc] Back",
            ViewMode.EntryDetails => "[Esc] Back",
            _ => ""
        };

        var hintSize = _contentFont!.MeasureString(hints);
        _spriteBatch!.DrawString(_contentFont, hints, new Vector2((viewport.Width - hintSize.X) / 2, viewport.Height - 30), DimTextColor * _transitionAlpha);
    }

    private Color GetStatusColor(DiscoveryStatus status)
    {
        return status switch
        {
            DiscoveryStatus.Unknown => new Color(60, 60, 70),
            DiscoveryStatus.Sighted => new Color(150, 150, 100),
            DiscoveryStatus.Defeated => new Color(200, 150, 100),
            DiscoveryStatus.Recruited => new Color(100, 200, 100),
            DiscoveryStatus.Mastered => new Color(255, 200, 100),
            _ => Color.Gray
        };
    }

    private void DrawRect(Rectangle rect, Color color)
    {
        _spriteBatch?.Draw(_pixelTexture, rect, color);
    }
}

