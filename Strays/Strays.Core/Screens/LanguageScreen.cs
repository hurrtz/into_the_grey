using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Strays.Core.Inputs;
using Strays.Core.Localization;
using Strays.ScreenManagers;

namespace Strays.Screens;

/// <summary>
/// Screen for selecting the game language.
/// </summary>
public class LanguageScreen : GameScreen
{
    private SpriteBatch? _spriteBatch;
    private SpriteFont? _titleFont;
    private SpriteFont? _contentFont;
    private Texture2D? _pixelTexture;

    private readonly List<LanguageInfo> _languages;
    private int _selectedIndex;
    private int _currentLanguageIndex;

    private float _transitionAlpha = 0f;
    private float _selectionPulse = 0f;
    private float _scrollOffset = 0f;

    // Animation for language preview
    private GameLanguage _previewLanguage;
    private float _previewTimer = 0f;
    private const float PREVIEW_DELAY = 0.3f;

    // Colors
    private static readonly Color BackgroundColor = new(20, 25, 35);
    private static readonly Color PanelColor = new(35, 40, 55);
    private static readonly Color SelectedColor = new(50, 80, 130);
    private static readonly Color TextColor = new(220, 225, 235);
    private static readonly Color DimTextColor = new(140, 150, 170);
    private static readonly Color AccentColor = new(80, 160, 255);
    private static readonly Color WarningColor = new(255, 180, 80);
    private static readonly Color CurrentColor = new(80, 200, 120);
    private static readonly Color IncompleteColor = new(255, 150, 100);

    // Event when language is selected
    public event EventHandler<GameLanguage>? LanguageSelected;

    /// <summary>
    /// Filters a string to only contain characters the font can render.
    /// </summary>
    private string GetSafeString(string text, SpriteFont font)
    {
        if (string.IsNullOrEmpty(text)) return text;

        var safeChars = new System.Text.StringBuilder();
        foreach (char c in text)
        {
            if (font.Characters.Contains(c) || c == '\n' || c == '\r')
            {
                safeChars.Append(c);
            }
            else
            {
                safeChars.Append('?');
            }
        }
        return safeChars.ToString();
    }

    public LanguageScreen()
    {
        TransitionOnTime = TimeSpan.FromSeconds(0.3);
        TransitionOffTime = TimeSpan.FromSeconds(0.2);

        _languages = LocalizationManager.Instance.GetSupportedLanguages().ToList();

        // Find current language index
        var current = LocalizationManager.Instance.CurrentLanguage;
        _currentLanguageIndex = _languages.FindIndex(l => l.Language == current);
        _selectedIndex = _currentLanguageIndex >= 0 ? _currentLanguageIndex : 0;
        _previewLanguage = _languages[_selectedIndex].Language;
    }

    public override void LoadContent()
    {
        base.LoadContent();

        if (ScreenManager == null)
        {
            return;
        }

        _spriteBatch = ScreenManager.SpriteBatch;
        _contentFont = ScreenManager.Game.Content.Load<SpriteFont>("Fonts/GameFont");
        _titleFont = ScreenManager.Game.Content.Load<SpriteFont>("Fonts/MenuFont");

        _pixelTexture = new Texture2D(ScreenManager.GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });
    }

    public override void UnloadContent()
    {
        _pixelTexture?.Dispose();
        base.UnloadContent();
    }

    public override void HandleInput(GameTime gameTime, InputState input)
    {
        if (input.IsMenuCancel(ControllingPlayer, out _))
        {
            ExitScreen();

            return;
        }

        // Navigation
        if (input.IsMenuUp(ControllingPlayer))
        {
            _selectedIndex--;

            if (_selectedIndex < 0)
            {
                _selectedIndex = _languages.Count - 1;
            }

            _previewTimer = 0f;
        }
        else if (input.IsMenuDown(ControllingPlayer))
        {
            _selectedIndex++;

            if (_selectedIndex >= _languages.Count)
            {
                _selectedIndex = 0;
            }

            _previewTimer = 0f;
        }

        // Selection
        if (input.IsMenuSelect(ControllingPlayer, out _))
        {
            SelectLanguage();
        }
    }

    private void SelectLanguage()
    {
        var selected = _languages[_selectedIndex];

        LocalizationManager.Instance.SetLanguage(selected.Language);
        _currentLanguageIndex = _selectedIndex;

        LanguageSelected?.Invoke(this, selected.Language);
        ExitScreen();
    }

    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        _transitionAlpha = 1f - TransitionPosition;
        _selectionPulse = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 3) * 0.1f + 0.9f;

        // Update preview timer
        _previewTimer += deltaTime;

        if (_previewTimer >= PREVIEW_DELAY)
        {
            _previewLanguage = _languages[_selectedIndex].Language;
        }

        // Smooth scroll to selected item
        float targetScroll = Math.Max(0, _selectedIndex - 4);
        _scrollOffset = MathHelper.Lerp(_scrollOffset, targetScroll, deltaTime * 10f);
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

        // Main panel
        var panelRect = new Rectangle(viewport.Width / 2 - 300, 100, 600, viewport.Height - 180);
        DrawRect(panelRect, PanelColor * _transitionAlpha);

        // Language list
        DrawLanguageList(panelRect);

        // Preview panel (right side)
        var previewRect = new Rectangle(panelRect.Right + 40, 100, viewport.Width - panelRect.Right - 80, viewport.Height - 180);

        if (previewRect.Width > 200)
        {
            DrawPreviewPanel(previewRect);
        }

        // Footer
        DrawFooter(viewport);

        _spriteBatch.End();
    }

    private void DrawHeader(Viewport viewport)
    {
        string title = L.Get("Settings_Language");

        if (string.IsNullOrEmpty(title) || title.StartsWith("["))
        {
            title = "Language / 言語 / Sprache";
        }

        var titleSize = _titleFont!.MeasureString(title);
        _spriteBatch!.DrawString(_titleFont, title,
            new Vector2((viewport.Width - titleSize.X) / 2, 30), AccentColor * _transitionAlpha);
    }

    private void DrawLanguageList(Rectangle bounds)
    {
        int itemHeight = 60;
        int visibleItems = (bounds.Height - 20) / itemHeight;
        int startIndex = (int)_scrollOffset;
        int y = bounds.Y + 10;

        // Column headers
        _spriteBatch!.DrawString(_contentFont!, "Language",
            new Vector2(bounds.X + 20, y), DimTextColor * 0.7f * _transitionAlpha);
        _spriteBatch.DrawString(_contentFont!, "Status",
            new Vector2(bounds.Right - 100, y), DimTextColor * 0.7f * _transitionAlpha);

        y += 30;

        for (int i = startIndex; i < Math.Min(startIndex + visibleItems, _languages.Count); i++)
        {
            var lang = _languages[i];
            bool isSelected = i == _selectedIndex;
            bool isCurrent = i == _currentLanguageIndex;

            // Selection background
            if (isSelected)
            {
                DrawRect(new Rectangle(bounds.X + 5, y, bounds.Width - 10, itemHeight - 5),
                    SelectedColor * _selectionPulse * _transitionAlpha);
            }

            // Current language indicator
            if (isCurrent)
            {
                DrawRect(new Rectangle(bounds.X + 5, y, 4, itemHeight - 5),
                    CurrentColor * _transitionAlpha);
            }

            // Native name (large) - use safe string to avoid font crashes
            string nativeName = GetSafeString(lang.NativeName, _contentFont!);
            Color nameColor = isSelected ? Color.White : TextColor;
            _spriteBatch.DrawString(_contentFont!, nativeName,
                new Vector2(bounds.X + 20, y + 8), nameColor * _transitionAlpha);

            // English name (small, dim)
            string englishName = GetSafeString(lang.EnglishName, _contentFont!);
            _spriteBatch.DrawString(_contentFont!, englishName,
                new Vector2(bounds.X + 20, y + 32), DimTextColor * 0.7f * _transitionAlpha);

            // Status indicator
            string status;
            Color statusColor;

            if (isCurrent)
            {
                status = "*";
                statusColor = CurrentColor;
            }
            else if (!lang.IsComplete)
            {
                status = $"{lang.CompletionPercent:F0}%";
                statusColor = IncompleteColor;
            }
            else
            {
                status = "";
                statusColor = DimTextColor;
            }

            if (!string.IsNullOrEmpty(status))
            {
                _spriteBatch.DrawString(_contentFont!, status,
                    new Vector2(bounds.Right - 80, y + 18), statusColor * _transitionAlpha);
            }

            y += itemHeight;
        }

        // Scroll indicators
        if (startIndex > 0)
        {
            _spriteBatch.DrawString(_contentFont!, "^",
                new Vector2(bounds.X + bounds.Width / 2 - 10, bounds.Y + 35),
                DimTextColor * (float)(0.5f + 0.5f * Math.Sin(DateTime.Now.Ticks / 10000000.0 * 3)) * _transitionAlpha);
        }

        if (startIndex + visibleItems < _languages.Count)
        {
            _spriteBatch.DrawString(_contentFont!, "v",
                new Vector2(bounds.X + bounds.Width / 2 - 10, bounds.Bottom - 25),
                DimTextColor * (float)(0.5f + 0.5f * Math.Sin(DateTime.Now.Ticks / 10000000.0 * 3)) * _transitionAlpha);
        }
    }

    private void DrawPreviewPanel(Rectangle bounds)
    {
        DrawRect(bounds, PanelColor * _transitionAlpha);

        int y = bounds.Y + 20;

        // Preview title
        _spriteBatch!.DrawString(_contentFont!, "Preview",
            new Vector2(bounds.X + 20, y), DimTextColor * _transitionAlpha);
        y += 40;

        // Show sample strings in the preview language
        var previewInfo = _languages.FirstOrDefault(l => l.Language == _previewLanguage);

        if (previewInfo == null)
        {
            return;
        }

        // Temporarily switch language for preview
        var currentLang = LocalizationManager.Instance.CurrentLanguage;
        LocalizationManager.Instance.SetLanguage(_previewLanguage);

        // Sample UI strings
        var samples = new[]
        {
            ("Menu_NewGame", "New Game"),
            ("Menu_Continue", "Continue"),
            ("Menu_Settings", "Settings"),
            ("Combat_Attack", "Attack"),
            ("Combat_Victory", "Victory!"),
            ("Stray_Recruit", "Recruit"),
            ("Item_Use", "Use")
        };

        foreach (var (key, fallback) in samples)
        {
            string text = L.Get(key);

            if (string.IsNullOrEmpty(text) || text.StartsWith("["))
            {
                text = fallback;
            }

            // Use safe string to avoid font crashes with non-ASCII characters
            string safeText = GetSafeString(text, _contentFont!);
            _spriteBatch.DrawString(_contentFont!, safeText,
                new Vector2(bounds.X + 20, y), TextColor * _transitionAlpha);
            y += 35;
        }

        // Restore current language
        LocalizationManager.Instance.SetLanguage(currentLang);

        // Warning for incomplete translations
        if (!previewInfo.IsComplete)
        {
            y = bounds.Bottom - 60;
            DrawRect(new Rectangle(bounds.X + 10, y - 5, bounds.Width - 20, 50),
                WarningColor * 0.2f * _transitionAlpha);

            string warning = $"Translation: {previewInfo.CompletionPercent:F0}% complete";
            _spriteBatch.DrawString(_contentFont!, warning,
                new Vector2(bounds.X + 20, y), WarningColor * _transitionAlpha);

            y += 25;
            string note = "Missing text will show in English";
            _spriteBatch.DrawString(_contentFont!, note,
                new Vector2(bounds.X + 20, y), DimTextColor * _transitionAlpha);
        }
    }

    private void DrawFooter(Viewport viewport)
    {
        string hints = "[Up/Down] Select  [Enter] Confirm  [Esc] Cancel";
        var hintSize = _contentFont!.MeasureString(hints);

        _spriteBatch!.DrawString(_contentFont, hints,
            new Vector2((viewport.Width - hintSize.X) / 2, viewport.Height - 35),
            DimTextColor * _transitionAlpha);
    }

    private void DrawRect(Rectangle rect, Color color)
    {
        _spriteBatch?.Draw(_pixelTexture, rect, color);
    }
}
